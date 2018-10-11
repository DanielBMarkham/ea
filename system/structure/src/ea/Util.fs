namespace EA.Compiler
module Util=

open Logary
open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
open EA.Types
open EA.Lenses
open EA.Persist
open EA.Utilities
//Yes, I'm repeating several modules in my include list, in seemingly-random order. Don't touch it, moron!
open Logary
open Logary.Configuration
open Logary.Targets
open Logary.Configuration
open Logary.Configuration.Transformers
open Expecto
// Tag-list for the logger is namespace, project name, file name
let moduleLogger = logary.getLogger (PointName [| "EA"; "Compiler"; "EA"; "Util" |])
// For folks on anal mode, log the module being entered.  NounVerb Proper Case
Logary.Message.eventFormat (Info, "Module Enter")|> Logger.logSimple moduleLogger


let inputStuff:GetCompileDataType = (fun opts->
    Message.eventFormat (Verbose, "inputStuff: Enter", "adam") |> Logger.logSimple moduleLogger

    //match opts.ConfigBase.Verbosity.parameterValue with
    //    | Debug|Verbose->testingLogger.debug(eventX "Entering inputStuff")
    //    |_->()
    //testingLogger.debug(eventX "asdfsadf")
    //match opts.ConfigBase.Verbosity.parameterValue with
    //    |_->testingLogger.debug(eventX "Leaving inputStuff")
    //    |_->()
    Message.eventFormat (Verbose, "inputStuff: Exit", "adam")
        |> Logger.logSimple moduleLogger
    Message.eventFormat (Info, "inputStuff: Exit", "adam")
        |> Logger.logSimple moduleLogger
    (opts, [||])
)
let doStuff:RunCompilationType = (fun (opts, compilationUnits)->
    Logary.Message.eventFormat (Verbose, "doStuff: Enter")
        |> Logger.logSimple moduleLogger
    Logary.Message.eventFormat (Verbose, "doStuff: Exit")
        |> Logger.logSimple moduleLogger
    (opts, {MasterModelText=[||]})
)

let outputStuff:WriteOutCompiledModelType = (fun (opts, masterModel)->
    Logary.Message.eventFormat (Verbose, "outputStuff: Enter")
        |> Logger.logSimple moduleLogger
    Logary.Message.eventFormat (Verbose, "outputStuff: Exit")
        |> Logger.logSimple moduleLogger
    0 //  it's always successful as far as the O/S is concerned
)

#nowarn "0067"
let newMain (argv:string[]) (compilerCancelationToken:System.Threading.CancellationTokenSource) (manualResetEvent:System.Threading.ManualResetEventSlim) =
    Logary.Message.eventFormat (Info, "newMain Enter")|> Logger.logSimple moduleLogger
    try
        let ret=loadEAConfigFromCommandLine argv |> inputStuff  |> doStuff |> outputStuff

        // I'm done (since I'm a single-threaded function, I know this)
        // take a few seconds to catch up, then you may run until you quit
        compilerCancelationToken.Token.WaitHandle.WaitOne(3000)
        manualResetEvent.Set()
        Logary.Message.eventFormat (Info, "newMain Exit Normal Path")|> Logger.logSimple moduleLogger
        0
    with
        | :? UserNeedsHelp as hex ->
            defaultEABaseOptions.PrintThis
            Logary.Message.eventFormat (Info, "newMain Exit User Need Help")|> Logger.logSimple moduleLogger
            0
        | ex ->
            System.Console.WriteLine ("Program terminated abnormally " + ex.Message)
            System.Console.WriteLine (ex.StackTrace)
            if ex.InnerException = null
                then
                    Logary.Message.eventFormat (Info, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                    1
                else
                    System.Console.WriteLine("---   Inner Exception   ---")
                    System.Console.WriteLine (ex.InnerException.Message)
                    System.Console.WriteLine (ex.InnerException.StackTrace)
                    Logary.Message.eventFormat (Info, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                    1
// For folks on anal mode, log the module being exited.  NounVerb Proper Case
Logary.Message.eventFormat (Info, "Module Exit")|> Logger.logSimple moduleLogger
