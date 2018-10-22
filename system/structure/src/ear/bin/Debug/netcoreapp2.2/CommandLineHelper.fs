module CommandLineHelper
    open SystemTypeExtensions
    open SystemUtilities
    open System.IO
    open Newtonsoft.Json

    /// Throw this anywhere to have outer shell catch and try to help user
    exception UserNeedsHelp of string
    // I modified some of these from the Logary help file
    // I liked them
    /// How much extra logging detail to get from the apps
    type Verbosity =
        /// Never tell me about anything no matter what. STFU
        | Silent
        /// Reserved for things that make the service/process crash. I cannot work at all. Alert humans!
        | Fatal
        /// Edge cases. IO failed. I did not plan for this. Alert humans!
        | Error
        /// Temp glitches. Lots of retries. Unusual things but we're good. It this happens a lot, people may be to be alerted
        | Warn
        /// Events and gauges for company-revelvant sutff. users sign in, sign up, integration has to retry, etc.
        | Info
        ///  Log all metrics here. Anything a coder might need from far away to figure out what's going on
        | Debug
        ///  Super-anal logging. Anything conceivably anybody might need anywhere. Assume no sources code or understanding of app
        | Verbose
        static member ToList()=[Silent;Fatal;Error;Warn;Info;Debug;Verbose]
        override self.ToString() =
            match self with
            | Silent->"Silent"
            | Fatal->"Fatal"
            | Error->"Error"
            | Warn->"Warn"
            | Info->"Info"
            | Debug->"Debug"
            | Verbose->"Verbose"
        static member FromString(str) =
            match str with
            | "Silent"|"S"|"1"->Silent
            | "Fatal"|"F"|"2"->Fatal
            | "Error"|"E"|"3"->Error
            | "Warn"|"W"|"4"->Warn
            | "Info"|"I"|"5"->Info
            | "Debug"|"D"|"6"->Debug
            | "Verbose"|"V"|"7"->Verbose
            |_->Error // If I don't know what you want, I'm only reporting Errors or better

    //
    // Program Command Line Config Settings
    //
    let getMatchingParameters (args:string []) (symbol:string) = 
        args |> Array.filter(fun x->
                    let argParms = x.Split([|':'|],2)
                    let parmName = (argParms.[0]).Substring(1).ToUpper()
                    if argParms.Length > 0 then parmName=symbol.ToUpper() else false
                    )
    let getValuePartOfMostRelevantCommandLineMatch (args:string []) (symbol:string) =
        let matchingParms = getMatchingParameters args symbol
        if matchingParms.Length > 0
            then
                // if there are multiple entries, last one overrides the rest
                let commandLineParm = matchingParms.[matchingParms.Length-1]
                let parmSections=commandLineParm.Split([|':'|], 2)
                if parmSections.Length<2 then Some "" else Some parmSections.[1]
            else
                None
    //type FileParm = string*System.IO.FileInfo option
    type FileParm = 
        {
            FileName:string
            FileInfoOption:System.IO.FileInfo option
        }
        member x.LoadJsonDataOrCreateJsonFileIfMissing<'a> defaultJsonData =
            let fileContents= 
                if (x.FileInfoOption.IsNone) || (File.Exists(x.FileInfoOption.Value.FullName)=false)
                    then
                        System.IO.File.CreateText(x.FileName) |> ignore
                        JsonConvert.SerializeObject(defaultJsonData)
                    else System.IO.File.ReadAllText(x.FileInfoOption.Value.FullName)
            let fileData = 
                if fileContents="" then defaultJsonData else JsonConvert.DeserializeObject<'a>(fileContents)
            fileData
    type DirectoryParm = 
        {
            DirectoryName:string
            DirectoryInfoOption:System.IO.DirectoryInfo option
        }
    type SortOrder = Ascending | Descending
                        static member ToList()=[Ascending;Descending]
                        override this.ToString()=
                            match this with
                                | Ascending->"Ascending"
                                | Descending->"Descending"
                        static member TryParse(stringToParse:string) =
                            match stringToParse with
                                |"a"|"asc"|"ascending"|"A"|"ASC"|"Ascending"|"Asc"|"ASCENDING"->true,SortOrder.Ascending
                                |"d"|"desc"|"descending"|"D"|"DESC"|"Descending"|"Desc"|"DESCENDING"->true,SortOrder.Descending
                                |_->false, SortOrder.Ascending
                        static member Parse(stringToParse:string) =
                            match stringToParse with
                                |"a"|"asc"|"ascending"|"A"|"ASC"|"Ascending"|"Asc"|"ASCENDING"->SortOrder.Ascending
                                |"d"|"desc"|"descending"|"D"|"DESC"|"Descending"|"Desc"|"DESCENDING"->SortOrder.Descending
                                |_->raise(new System.ArgumentOutOfRangeException("Sort Order","The string value provided for Sort Order is not in the Sort Order enum"))

    /// Parameterized type to allow command-line argument processing without a lot of extra coder work
    /// Instantiate the type with the type of value you want. Make a default entry in case nothing is found
    /// Then call the populate method. Will pull from args and return a val and args with the found value (if any consumed)
    type ConfigEntryType<'A> =
        {
            commandLineParameterSymbol:string
            commandLineParameterName:string
            parameterHelpText:string[]
            parameterValue:'A
        } with
            member this.printVal =
                printfn "%s: %s" this.commandLineParameterName (this.parameterValue.ToString())
            member this.printHelp =
                printfn "%s" this.commandLineParameterName
                this.parameterHelpText |> Seq.iter(System.Console.WriteLine)
            member this.swapInNewValue x =
                {this with parameterValue=x}
            static member populateValueFromCommandLine ((defaultConfig:ConfigEntryType<Verbosity>), (args:string[])):ConfigEntryType<Verbosity>  =
                let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
                if parmValue.IsSome
                    then
                        //let parsedNumValue = System.Int32.Parse("0" + parmValue.Value)
                        let parsedVerbosityValue = Verbosity.FromString(parmValue.Value)
                        defaultConfig.swapInNewValue parsedVerbosityValue
                    else
                        defaultConfig
            static member populateValueFromCommandLine ((defaultConfig:ConfigEntryType<string>), (args:string[])):ConfigEntryType<string>  =
                let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
                if parmValue.IsSome
                    then
                        defaultConfig.swapInNewValue parmValue.Value
                    else
                        defaultConfig
            static member populateValueFromCommandLine ((defaultConfig:ConfigEntryType<DirectoryParm>), (args:string[])):ConfigEntryType<DirectoryParm>  =
                let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
                if parmValue.IsSome
                    then
                        if System.IO.Directory.Exists(parmValue.Value)
                            then
                                let tempDirectoryInfoOption = Some(System.IO.DirectoryInfo(parmValue.Value))
                                defaultConfig.swapInNewValue ({DirectoryName=parmValue.Value; DirectoryInfoOption=tempDirectoryInfoOption})
                            else defaultConfig.swapInNewValue ({DirectoryName=parmValue.Value; DirectoryInfoOption=Option.None})
                    else
                        defaultConfig
            static member populateValueFromCommandLine ((defaultConfig:ConfigEntryType<FileParm>), (args:string[])):ConfigEntryType<FileParm>  =
                let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
                if parmValue.IsSome
                    then
                        if System.IO.File.Exists(parmValue.Value)
                            then
                                let tempFileInfoOption = Some(System.IO.FileInfo(parmValue.Value))
                                defaultConfig.swapInNewValue ({FileName=parmValue.Value; FileInfoOption=tempFileInfoOption})
                            else
                                defaultConfig.swapInNewValue ({FileName=parmValue.Value; FileInfoOption=Option.None})
                    else
                        defaultConfig
            static member populateValueFromCommandLine ((defaultConfig:ConfigEntryType<bool>), (args:string[])):ConfigEntryType<bool> =
                let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
                if parmValue.IsSome
                    then
                        if parmValue.Value.ToUpper() = "FALSE" || parmValue.Value = "0" || parmValue.Value.ToUpper() = "F" || parmValue.Value.ToUpper() = "NO"
                            then
                                defaultConfig.swapInNewValue false
                            else
                                defaultConfig.swapInNewValue true
                    else
                        defaultConfig
            static member populateValueFromCommandLine ((defaultConfig:ConfigEntryType<int>), (args:string[])):ConfigEntryType<int>  =
                let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
                if parmValue.IsSome
                    then
                        let parmInt = System.Int32.Parse("0" + parmValue.Value)
                        defaultConfig.swapInNewValue parmInt
                    else
                        defaultConfig
            static member populateValueFromCommandLine ((defaultConfig:ConfigEntryType<System.Uri>), (args:string[])):ConfigEntryType<System.Uri>  =
                let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
                if parmValue.IsSome
                    then
                        defaultConfig.swapInNewValue (new System.Uri(parmValue.Value))
                    else
                        defaultConfig
            static member populateValueFromCommandLine ((defaultConfig:ConfigEntryType<System.DateTime>), (args:string[])):ConfigEntryType<System.DateTime>  =
                let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
                if parmValue.IsSome
                    then
                        defaultConfig.swapInNewValue (System.DateTime.Parse(parmValue.Value))
                    else
                        defaultConfig
            static member populateValueFromCommandLine ((defaultConfig:ConfigEntryType<SortOrder>), (args:string[])):ConfigEntryType<SortOrder>  =
                let parmValue = getValuePartOfMostRelevantCommandLineMatch args defaultConfig.commandLineParameterSymbol
                let newVal=if parmValue.IsNone then defaultConfig.parameterValue else
                            let tp=SortOrder.TryParse parmValue.Value
                            if fst tp=true then snd tp else defaultConfig.parameterValue
                defaultConfig.swapInNewValue newVal
    /// A type so that programs can report what they're doing as they do it
    // This was the programmer can decide what to do with it instead of the OS
    [<NoComparison>]
    type InterimProgressType =
        {
            Items:System.Collections.Generic.Dictionary<string, System.Text.StringBuilder>
        } with
        member this.AddItem key (vl:string) =
            let lookup =
                if this.Items.ContainsKey key then this.Items.Item(key)
                    else
                        let newItem = new System.Text.StringBuilder(65535)
                        this.Items.Add(key,newItem)
                        newItem
            lookup.Append("\r\n" + vl) |> ignore
        member this.GetItem key  =
            if this.Items.ContainsKey key
                then
                    this.Items.Item(key).ToString()
                else
                    ""
    // All programs have at least this configuration on the command line
    [<NoComparison>]
    type ConfigBase =
        {
            ProgramName:string
            ProgramTagLine:string
            ProgramHelpText:string[]
            Verbosity:ConfigEntryType<Verbosity>
            InterimProgress:InterimProgressType
        }
        member this.PrintProgramDescription =
            this.ProgramHelpText |> Seq.iter(System.Console.WriteLine)
        member this.PrintThis =
            printfn "%s" this.ProgramName
            this.ProgramHelpText |> Seq.iter(System.Console.WriteLine)

    let directoryExists (dir:ConfigEntryType<DirectoryParm>) = dir.parameterValue.DirectoryInfoOption.IsSome
    let fileExists (dir:ConfigEntryType<FileParm>) = dir.parameterValue.FileInfoOption.IsSome




    /// Prints out the options for the command before it runs. Detail level is based on verbosity setting
    let commandLinePrintWhileEnter (opts:ConfigBase) fnPrintMe =
                // Entering program command line report
            let nowString = string System.DateTime.Now
            match opts.Verbosity.parameterValue with
                | Silent|Fatal|Error|Warn ->
                    ()
                | Info->
                    printfn "%s. %s" opts.ProgramName opts.ProgramTagLine
                    printfn "Begin: %s" (nowString)
                    printfn "Verbosity: Normal"
                | Debug |Verbose->
                    printfn "%s. %s" opts.ProgramName opts.ProgramTagLine
                    printfn "Begin: %s" (nowString)
                    fnPrintMe()

    /// Exiting program command line report. Detail level is based on verbosity setting
    let commandLinePrintWhileExit (baseOptions:ConfigBase) =
        let nowString = string System.DateTime.Now
        match baseOptions.Verbosity.parameterValue with
            | Silent|Fatal|Error|Warn|Info ->
                ()
            | Verbosity.Debug|Verbose ->
                printfn "End:   %s" (nowString)
                ()

    let defaultVerbosity  =
        {
            commandLineParameterSymbol="V"
            commandLineParameterName="Verbosity"
            parameterHelpText=[|"/V:[1-7]           -> Amount of trace info to report. Silent|Fatal|Error|Warn|Info|Debug|Verbose"|]
            parameterValue=Info
        }

    let createNewBaseOptions programName programTagLine programHelpText verbose =
        {
            ProgramName = programName
            ProgramTagLine = programTagLine
            ProgramHelpText=programHelpText
            Verbosity = verbose
            InterimProgress = {Items=new System.Collections.Generic.Dictionary<string, System.Text.StringBuilder>()}
        }

    let createNewConfigEntry commandlineSymbol commandlineParameterName parameterHelpText initialValue =
        {
            commandLineParameterSymbol=commandlineSymbol
            commandLineParameterName=commandlineParameterName
            parameterHelpText=parameterHelpText
            parameterValue=initialValue
        }
