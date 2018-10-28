namespace EA
  module Types=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open Logary
    open System.Data
    //open System.Web.Configuration


    // Logging all the things!
    // Logging code must come before anything else in order to use logging
    // let incomingStuff:string=pipedStreamIncoming()
    // Need to know this when logging


    let oldStdout=System.Console.Out
    let oldStdErr=System.Console.Error
    let mutable CommandLineArgumentsHaveBeenProcessed=false
    type LogEventParms=LogLevel*string*Logary.Logger
    let loggingBacklog = new System.Collections.Generic.Queue<LogEventParms>(4096)
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
        System.Console.SetOut oldStdErr
        System.Console.SetError oldStdout
        loggingBacklog|>Seq.iter(fun x-> 
            let lvl, msg, lggr = x
            logEvent lvl msg lggr)
    logEvent Verbose "Module enter...." moduleLogger

    // DATA types
    type EAConfigType =
        {
        ConfigBase:ConfigBase
        FileListFromCommandLine:(string*System.IO.FileInfo)[]
        IncomingStream:seq<string>
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

    /// The Compilation Unit (files coming in) type used by clients of EALib
    type EACompilationUnitType = {
        Info:System.IO.FileInfo
        FileContents:string[]
    }
    /// The compilation Result that's owned by clients of EALib
    type EACompilationResultType = {
        MasterModelText:string[]
        }


    // FUNCTION TYPES
    /// Process any args you can from the command line
    /// Get rid of any junk
    type GetEAProgramConfigType=string [] -> seq<string>->EAConfigType
    /// Process any args you can from the command line
    /// Get rid of any junk

    /// Responsible only for getting a list of strings and associated files to compile. Nothing else
    /// Any failure results in an empty string and an INFO message back to the caller (but okay result)
    type GetCompileDataType=EAConfigType->(EAConfigType * EACompilationUnitType[])

    // Main compilation happens here. It can fail but it can't crash, so no (direct) IO
    type RunCompilationType=(EAConfigType * EACompilationUnitType[])->(EAConfigType * EACompilationResultType)

    type CompileType=EACompilationUnitType[]->EACompilationResultType

    /// Final stage. Writes out model to persistent storage. It can fail, but it doesn't matter,
    /// since any failure in simple IO would prevent us from telling anybody
    type WriteOutCompiledModelType=(EAConfigType * EACompilationResultType)->int


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
    let loadEAConfigFromCommandLine:GetEAProgramConfigType = (fun args incomingStream->
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
      // Go through the arg list. If it's a file, add to list.
      // If it's a directory, add files in the directory to the list
      let fileList=args |> Array.filter(fun x->
        let newFile=System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,x)
        //logEvent Verbose ("Method loadEAConfigFromCommandLine file exists: " + newFile + " = " + System.IO.File.Exists(newFile).ToString()) moduleLogger
        System.IO.File.Exists(newFile)
        )
      //logEvent Verbose ("Method loadEAConfigFromCommandLine fileList length: " + fileList.Length.ToString()) moduleLogger
      let directoriesList=args |> Array.filter(fun x->System.IO.Directory.Exists(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,x)))
      //logEvent Verbose ("Method loadEAConfigFromCommandLine directories length: " + directoriesList.Length.ToString()) moduleLogger
      let filesFromDirectories=directoriesList |> Array.map(fun x->System.IO.Directory.GetFiles(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,x))) |> Array.concat
      let filesReferenced=fileList|>Array.append filesFromDirectories
      let newFilesReferncedFromTheCommandLine = filesReferenced |> Array.map(fun x->
            let newFile=System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,x)
            (newFile, System.IO.FileInfo(newFile))
        )
      logEvent Verbose "..... Method loadEAConfigFromCommandLine ending. Normal Path." moduleLogger
      //logEvent Debug ("incomingStuff = " + pipedStreamIncoming()) moduleLogger
      turnOnLogging()
      {ConfigBase = newConfigBase; IncomingStream=incomingStream; FileListFromCommandLine=newFilesReferncedFromTheCommandLine}
    )
    /// Process any args you can from the command line
    /// Get rid of any junk
    type GetEARProgramConfigType=string []->EARConfigType
    /// Load the master file
    /// Any failure results in an empty string and an INFO message back to the caller (but okay result)
    type LoadMasterFile=EARConfigType->(EARConfigType * EACompilationResultType)

    /// Do the slicing and dicing
    /// Note that the model comes in and leaves in the same format
    type RunTransformsAndFilters=(EARConfigType * EACompilationResultType)->(EARConfigType * EACompilationResultType)

    /// Take whatever model we now have and send it wherever it's supposed to go
    type WriteOutModelReportType=(EARConfigType * EACompilationResultType)->int



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

    type EasyAMLineTypes =
        |FileBegin
        |FileEnd
        |EmptyLine
        |FreeFormText
        |CompilerSectionDirective
        |CompilerNamespaceDirective
        |CompilerTagDirective
        |CompilerSectionDirectiveWithItem
        |CompilerNamespaceDirectiveWithItem
        |CompilerTagDirectiveWithItem
        |CompilerJoinTypeWithItem
        |NewSectionItem
        |NewJoinedItem
        |NewItemJoinCombination

    type CompilerRuleType =
        |FileBeginType of AllowedNextLinesType
        |FileEndType of AllowedNextLinesType
        |EmptyLineType of AllowedNextLinesType
        |FreeFormTextType of AllowedNextLinesType
        |CompilerSectionDirectiveType of AllowedNextLinesType
        |CompilerNamespaceDirectiveType of AllowedNextLinesType
        |CompilerTagDirectiveType of AllowedNextLinesType
        |CompilerSectionDirectiveWithItemType of AllowedNextLinesType
        |CompilerNamespaceDirectiveWithItemType of AllowedNextLinesType
        |CompilerTagDirectiveWithItemType of AllowedNextLinesType
        |CompilerJoinTypeWithItemType of AllowedNextLinesType
        |NewSectionItemType of AllowedNextLinesType
        |NewJoinedItemType of AllowedNextLinesType
        |NewItemJoinCombinationType of AllowedNextLinesType
    and AllowedNextLinesType = AllowedNextLines of (EasyAMLineTypes list)
    
    let makeRule (ruleName) (nextLinesAllowed:EasyAMLineTypes list) = (ruleName)(AllowedNextLines(nextLinesAllowed))
    let fileBeginRule = makeRule FileBeginType [FileEnd; EmptyLine; FreeFormText; CompilerSectionDirective; NewSectionItem]
    let fileEndRule = makeRule FileEndType []
    let emptyLineRule= makeRule EmptyLineType [FileEnd; EmptyLine;FreeFormText;CompilerSectionDirective;NewSectionItem]
    let freeformTextRule= makeRule FreeFormTextType [FileEnd; EmptyLine; FreeFormText; CompilerSectionDirective; NewSectionItem]
    let compilerSectionDirectiveRule = makeRule CompilerSectionDirectiveType [FileEnd; EmptyLine; FreeFormText; CompilerSectionDirective; NewSectionItem]
    let newSectionItemRule = makeRule NewSectionItemType [FileEnd; EmptyLine; FreeFormText; CompilerSectionDirective; NewSectionItem]

    let CompilerRules =
        [
            fileBeginRule
            ;fileEndRule
            ;emptyLineRule
            ;freeformTextRule
            ;compilerSectionDirectiveRule
            ;newSectionItemRule
        ]
    let isThisLineAllowedNext (previousLineType:EasyAMLineTypes) (lineTypeToTest:EasyAMLineTypes)=
      let rulesThatApply=
        CompilerRules|>List.filter(fun x->
          match x with 
            | previousLineType->true 
            |_-> false
          ) 

      rulesThatApply |> List.exists (fun x->
        match x with
          | currentLineType->true
          |_->false
        )

    // For folks on anal mode, log the module being exited. NounVerb Proper Case
    logEvent Verbose "....Module exit" moduleLogger
