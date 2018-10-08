module EATypeExtensions
open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
open System

open Expecto
open Expecto.Logging
open Expecto.Logging.Message

let appLogger = Log.create "EA"


// DATA types
type EAConfigType =
    {
    ConfigBase:ConfigBase
    }
    with member this.PrintThis() =
        printfn "EasyAMConfig Parameters Provided"

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
type GetCompileDataType=EAConfigType->CompilationUnitType[]

// Main compilation happens here. It can fail but it can't crash, so no (direct) IO
type RunCompilationType=EAConfigType->CompilationUnitType[]->CompilationResultType

/// Final stage. Writes out model to persistent storage. It can fail, but it doesn't matter,
/// since any failure in simple IO would prevent us from telling anybody
type WriteOutCompiledModelType=EAConfigType->CompilationUnitType[]->CompilationResultType->int


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
let EAProgramHelp = [|"This is an example program for talking about option types."|]
//createNewBaseOptions programName programTagLine programHelpText verbose
let defaultEABaseOptions = createNewBaseOptions "ea" "The world's first analysis compiler" EAProgramHelp defaultVerbosity
let loadEAConfigFromCommandLine:GetEAProgramConfigType = (fun args->
    if args.Length>0 && (args.[0]="?"||args.[0]="/?"||args.[0]="-?"||args.[0]="--?"||args.[0]="help"||args.[0]="/help"||args.[0]="-help"||args.[0]="--help") then raise (UserNeedsHelp args.[0]) else
    let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
    let newConfigBase = {defaultEABaseOptions with Verbosity=defaultVerbosity}
    let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
    {ConfigBase = newConfigBase}
    )