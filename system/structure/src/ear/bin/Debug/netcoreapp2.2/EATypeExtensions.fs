module EATypeExtensions
open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
//open Logary
open System

//open Expecto
//open Expecto.Logging
//open Expecto.Logging.Message
//open System
open Logary // normal usage
open Logary.Message // normal usage
open Logary.Configuration // conf
//open Logary
//open Logary.Configuration


//let testingLogger = Logary.Log.create "EA_TEST"
//let appLogger = Logary.Facade.Log.create "EA"

//let logary = 
//    Config.create "EA" "localhost"
//    |> Config.target (LiterateConsole.create LiterateConsole.empty "console")
//    |> Config.processing (Events.events |> Events.sink ["console";])
//    |> Config.build
//    |> Hopac.Hopac.run



// DATA types
type EAConfigType =
    {
    ConfigBase:ConfigBase
    }
    with member this.PrintThis() =
        //testingLogger.info(
        //    eventX "EasyAMConfig Parameters Provided"
        //)
        System.Console.WriteLine "EasyAMConfig Parameters Provided"

type EARConfigType =
    {
    ConfigBase:ConfigBase
    }
    with member this.PrintThis() =
        //testingLogger.info(
        //    eventX "EARConfig Parameters Provided"
        //)
        System.Console.WriteLine "EasyAMConfig Parameters Provided"

type CompilationUnitType = {
    Info:System.IO.FileInfo
    FileContents:string[]
}
type CompilationResultType = {
    MasterModelText:string[]
    }

// FUNCTION TYPES
/// Process any args you can from the command line
/// Get rid of any junk
type GetEAProgramConfigType=string []->EAConfigType
/// Responsible only for getting a list of strings and associated files to compile. Nothing else
/// Any failure results in an empty string and an INFO message back to the caller (but okay result)
type GetCompileDataType=EAConfigType->(EAConfigType * CompilationUnitType[])

// Main compilation happens here. It can fail but it can't crash, so no (direct) IO
type RunCompilationType=(EAConfigType * CompilationUnitType[])->(EAConfigType * CompilationResultType)

/// Final stage. Writes out model to persistent storage. It can fail, but it doesn't matter,
/// since any failure in simple IO would prevent us from telling anybody
type WriteOutCompiledModelType=(EAConfigType * CompilationResultType)->int


/// Loading config parms for any apps
/// The only function bodies that go into
/// Our shared APP Types file are those functions
/// That are required to cooredinate/stay the same
/// Between all the apps in the set. Loading configs
/// Is one of those functions. (You want consistency and interop)
let defaultVerbosity  =
    {
        commandLineParameterSymbol="V"
        commandLineParameterName="Verbosity"
        parameterHelpText=[|"/V:[0-9]           -> Amount of trace info to report. 0=none, 5=normal, 9=max."|]
        parameterValue=Verbosity.Info
    }
let EAProgramHelp = [|"EasyAM. An analysis compiler. It compiles freeform notes to useful project stuff."|]
//createNewBaseOptions programName programTagLine programHelpText verbose
let defaultEABaseOptions = createNewBaseOptions "ea" "The world's first analysis compiler" EAProgramHelp defaultVerbosity
let loadEAConfigFromCommandLine:GetEAProgramConfigType = (fun args->
    //appLogger.verbose(eventX "Entering loadEAConfigFromCommandLine")
    if args.Length>0 && (args.[0]="?"||args.[0]="/?"||args.[0]="-?"||args.[0]="--?"||args.[0]="help"||args.[0]="/help"||args.[0]="-help"||args.[0]="--help") then raise (UserNeedsHelp args.[0]) else
    let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
    let newConfigBase = {defaultEABaseOptions with Verbosity=defaultVerbosity}
    let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
    //appLogger.verbose(eventX "Leaving loadEAConfigFromCommandLine")
    {ConfigBase = newConfigBase}
    )

/// Process any args you can from the command line
/// Get rid of any junk
type GetEARProgramConfigType=string []->EARConfigType
/// Load the master file
/// Any failure results in an empty string and an INFO message back to the caller (but okay result)
type LoadMasterFile=EARConfigType->(EARConfigType * CompilationResultType)

/// Do the slicing and dicing
/// Note that the model comes in and leaves in the same format
type RunTransformsAndFilters=(EARConfigType * CompilationResultType)->(EARConfigType * CompilationResultType)

/// Take whatever model we now have and send it wherever it's supposed to go
type WriteOutModelReportType=(EARConfigType * CompilationResultType)->int



// A couple of prototype EAR types to begin thinking about what goes here
let EARProgramHelp = [|"EasyAM Reporting (EAR). Takes EasyAM master files and translates output."|]
//createNewBaseOptions programName programTagLine programHelpText verbose
let defaultEARBaseOptions = createNewBaseOptions "ear" "The reporting engine for EasyAM" EARProgramHelp defaultVerbosity
let loadEARConfigFromCommandLine:GetEARProgramConfigType = (fun args->
    if args.Length>0 && (args.[0]="?"||args.[0]="/?"||args.[0]="-?"||args.[0]="--?"||args.[0]="help"||args.[0]="/help"||args.[0]="-help"||args.[0]="--help") then raise (UserNeedsHelp args.[0]) else
    let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
    let newConfigBase = {defaultEABaseOptions with Verbosity=defaultVerbosity}
    let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
    {ConfigBase = newConfigBase}
    )
