
namespace EA
  module Types=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open Logary
    // Logging all the things!
    // Logging code must come before anything else in order to use logging
    let oldStdout=System.Console.Out
    let oldStdin=System.Console.In
    let oldStdErr=System.Console.Error
    System.Console.SetOut oldStdErr
    System.Console.SetError oldStdout
    // Need to know this when logging
    let mutable CommandLineArgumentsHaveBeenProcessed=false
    type LogEventParms=LogLevel*string*Logary.Logger
    let loggingBacklog = new System.Collections.Generic.Queue<LogEventParms>()
    let logary =
        Logary.Configuration.Config.create "EA.Logs" "localhost"
        |> Logary.Configuration.Config.targets [ Logary.Targets.LiterateConsole.create Logary.Targets.LiterateConsole.empty "console" ]
        |> Logary.Configuration.Config.loggerMinLevel "" Logary.LogLevel.Debug
        |> Logary.Configuration.Config.processing (Logary.Configuration.Events.events |> Logary.Configuration.Events.sink ["console";])
        |> Logary.Configuration.Config.build
        |> Hopac.Hopac.run
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Types"; "EATypeExtensions" |])
    // DIDNT NEED. WILD GOOSE CHASE USING THIS FROM HELP FILES
    //Logary.Adapters.Facade.LogaryFacadeAdapter.initialise<Expecto.Logging.Logger> logary
    
    /// ErrorLevel, Message to display, and logger to send it to
    let logEvent (lvl:LogLevel) msg lggr =
        if CommandLineArgumentsHaveBeenProcessed
            then Logary.Message.eventFormat (lvl, msg)|> Logger.logSimple lggr
            else loggingBacklog.Enqueue(lvl, msg, lggr)
    let turnOnLogging() =
        CommandLineArgumentsHaveBeenProcessed<-true
        loggingBacklog|>Seq.iter(fun x-> 
            let lvl, msg, lggr = x
            logEvent lvl msg lggr)
    logEvent Verbose "Module enter...." moduleLogger

    //let applicationLogger = logary.getLogger (PointName [| "EA"; "Program"; "main" |])
    //let testingLogger = logary.getLogger (PointName [| "EA_TEST"; "Program"; "main" |])

    // DATA types
    type EAConfigType =
        {
        ConfigBase:ConfigBase
        }
        with member this.PrintThis() =()
            //testingLogger.info(
            //    eventX "EasyAMConfig Parameters Provided"
            //)
    type EARConfigType =
        {
        ConfigBase:ConfigBase
        }
        with member this.PrintThis() =()
            //testingLogger.info(
            //    eventX "EasyAMConfig Parameters Provided"
            //)



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
    /// Process any args you can from the command line
    /// Get rid of any junk

    /// Responsible only for getting a list of strings and associated files to compile. Nothing else
    /// Any failure results in an empty string and an INFO message back to the caller (but okay result)
    type GetCompileDataType=EAConfigType->(EAConfigType * CompilationUnitType[])

    // Main compilation happens here. It can fail but it can't crash, so no (direct) IO
    type RunCompilationType=(EAConfigType * CompilationUnitType[])->(EAConfigType * CompilationResultType)

    /// Final stage. Writes out model to persistent storage. It can fail, but it doesn't matter,
    /// since any failure in simple IO would prevent us from telling anybody
    type WriteOutCompiledModelType=(EAConfigType * CompilationResultType)->int


    type Verbosity with
      member this.ToLogLevel()=
        match this with
          | Verbosity.Silent->LogLevel.Fatal // Silent is fatal but eat everything. don't throw
          | Verbosity.Fatal->LogLevel.Fatal
          | Verbosity.Error->LogLevel.Error
          | Verbosity.Warn->LogLevel.Warn
          | Verbosity.Info->LogLevel.Info
          | Verbosity.Debug->LogLevel.Debug
          | Verbosity.Verbose->LogLevel.Verbose
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
            parameterHelpText=[|"/V:[1-7]           -> Amount of trace info to report.";"1=Silent, 2=Fatal, 3=Error, 4=Warn, 5=Info, 6=Debug, 7=Verbose"|]
            parameterValue=Verbosity.Info
        }
    let EAProgramHelp = [|"EasyAM. An analysis compiler. It compiles freeform notes to useful project stuff."|]
    //createNewBaseOptions programName programTagLine programHelpText verbose
    let defaultEABaseOptions = createNewBaseOptions "ea" "The world's first analysis compiler" EAProgramHelp defaultVerbosity
    let loadEAConfigFromCommandLine:GetEAProgramConfigType = (fun args->
        logEvent Verbose "Method loadEAConfigFromCommandLine beginning....." moduleLogger
        if args.Length>0 && (args.[0]="?"||args.[0]="/?"||args.[0]="-?"||args.[0]="--?"||args.[0]="help"||args.[0]="/help"||args.[0]="-help"||args.[0]="--help") then raise (UserNeedsHelp args.[0]) else
        let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
        let newConfigBase = {defaultEABaseOptions with Verbosity=defaultVerbosity}
        let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
        if newVerbosity.parameterValue<>defaultVerbosity.parameterValue
          then
            logEvent Info ("New Verbosity set in loadEAConfigFromCommandLine: " + newVerbosity.parameterValue.ToString()) moduleLogger
            logary.switchLoggerLevel ("", newVerbosity.parameterValue.ToLogLevel())
          else ()
        logEvent Verbose "..... Method loadEAConfigFromCommandLine ending. Normal Path." moduleLogger
        turnOnLogging()
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
        logEvent Verbose "Method loadEARConfigFromCommandLine beginning....." moduleLogger
        if args.Length>0 && (args.[0]="?"||args.[0]="/?"||args.[0]="-?"||args.[0]="--?"||args.[0]="help"||args.[0]="/help"||args.[0]="-help"||args.[0]="--help") then raise (UserNeedsHelp args.[0]) else
        let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
        let newConfigBase = {defaultEARBaseOptions with Verbosity=defaultVerbosity}
        let newVerbosity =ConfigEntryType<_>.populateValueFromCommandLine(defaultVerbosity, args)
        if newVerbosity.parameterValue<>defaultVerbosity.parameterValue
          then
            logEvent Info ("New Verbosity set in loadEARConfigFromCommandLine: " + newVerbosity.parameterValue.ToString()) moduleLogger
            logary.switchLoggerLevel ("", newVerbosity.parameterValue.ToLogLevel())
          else ()
        logEvent Verbose "..... Method loadEARConfigFromCommandLine ending. Normal Path." moduleLogger
        turnOnLogging()
        {ConfigBase = newConfigBase}
        )

    // For folks on anal mode, log the module being exited. NounVerb Proper Case
    logEvent Verbose "....Module exit" moduleLogger
