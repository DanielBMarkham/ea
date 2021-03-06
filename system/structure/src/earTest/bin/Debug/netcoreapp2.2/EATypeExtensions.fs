namespace EA
  module Types=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA
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


    let ManageBucket      = {Name=Manage;       Category=ServicesOfferedAndValues;  Focus=ChangeMentalModelBasedOnInteractions;               WorkType=EmergentDiscoverRealityAndDecideStrategy;    BucketColor=HexColor("ffffff")}
    let UnderstandBucket  = {Name=Understand;   Category=Values;                    Focus=ChangeMentalModelBasedOnInteractions;               WorkType=DirectDecideAndChangeReality;                BucketColor=HexColor("ffffff")}
    let ExecuteBucket     = {Name=Execute;      Category=Values;                    Focus=AdjustHowMentalModelAndInteractionsWorkTogether;    WorkType=EmergentDiscoverRealityAndDecideStrategy;    BucketColor=HexColor("ffffff")}
    let InstantiateBucket = {Name=Instantiate;  Category=Values;                    Focus=ChangeInteractionsBasedOnMentalModel;               WorkType=DirectDecideAndChangeReality;                BucketColor=HexColor("ffffff")}
    let DeliverBucket     = {Name=Deliver;      Category=ServicesOfferedAndValues;  Focus=ChangeInteractionsBasedOnMentalModel;               WorkType=EmergentDiscoverRealityAndDecideStrategy;    BucketColor=HexColor("ffffff")}
    let OptimizeBucket    = {Name=Optimize;     Category=ServicesOffered;           Focus=ChangeInteractionsBasedOnMentalModel;               WorkType=DirectDecideAndChangeReality;                BucketColor=HexColor("ffffff")}
    let PlanBucket        = {Name=Plan;         Category=ServicesOffered;           Focus=AdjustHowMentalModelAndInteractionsWorkTogether;    WorkType=EmergentDiscoverRealityAndDecideStrategy;    BucketColor=HexColor("ffffff")}
    let GuessBucket       = {Name=Guess;        Category=ServicesOffered;           Focus=ChangeMentalModelBasedOnInteractions;               WorkType=DirectDecideAndChangeReality;                BucketColor=HexColor("ffffff")}
    let BucketList =
      [
        ManageBucket;
        UnderstandBucket;
        ExecuteBucket;
        InstantiateBucket;
        DeliverBucket;
        OptimizeBucket;
        PlanBucket;
        GuessBucket
      ]
    let defaultLocationPointer =
      {
        Genre=Business
        AbstractionLevel=Abstract
        Bucket=PlanBucket
        TemporaalIndicator=ToBe
        Tags=[||]
        Namespace=[||]
      }

    
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
        FileListFromCommandLine:(string*System.IO.FileInfo)[]
        IncomingStream:seq<string>
        }
        with member this.PrintThis() =()
            //testingLogger.info(
            //    eventX "EasyAMConfig Parameters Provided"
            //)

    // FUNCTION TYPES
    /// Process any args you can from the command line
    /// Get rid of any junk
    type GetEAProgramConfigType=string [] -> seq<string>->EAConfigType
    /// Process any args you can from the command line
    /// Get rid of any junk



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
    let defaultEAConfig:EAConfigType ={ConfigBase = {defaultEABaseOptions with Verbosity=defaultVerbosity}; IncomingStream=[||]; FileListFromCommandLine=[||]}
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
    type GetEARProgramConfigType=string [] -> seq<string>->EARConfigType
    // A couple of prototype EAR types to begin thinking about what goes here
    let EARProgramHelp = [|"EasyAM Reporting (EAR). Takes EasyAM master files and translates output."|]
    //createNewBaseOptions programName programTagLine programHelpText verbose
    let defaultEARBaseOptions = createNewBaseOptions "ear" "The reporting engine for EasyAM" EARProgramHelp defaultVerbosity
    let defaultEARConfig:EARConfigType = {ConfigBase = {defaultEARBaseOptions with Verbosity=defaultVerbosity}; IncomingStream=[||]; FileListFromCommandLine=[||]}
    let loadEARConfigFromCommandLine:GetEARProgramConfigType = (fun args incomingStream->
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
        logEvent Verbose "..... Method loadEARConfigFromCommandLine ending. Normal Path." moduleLogger
        turnOnLogging()
        {ConfigBase = newConfigBase; IncomingStream=incomingStream; FileListFromCommandLine=newFilesReferncedFromTheCommandLine}
        )


    // For folks on anal mode, log the module being exited. NounVerb Proper Case
    logEvent Verbose "....Module exit" moduleLogger
