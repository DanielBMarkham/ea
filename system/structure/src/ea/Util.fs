namespace EA.Compiler
module Util=

open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
open EA.Types
open EA.Lenses
open EA.Persist
open EA.Utilities

open Logary

let moduleLogger = logary.getLogger (PointName [| "EA"; "Compiler"; "Util" |])

let inputStuff:GetCompileDataType = (fun opts->
    Logary.Message.eventFormat (Verbose, "inputStuff: Enter", "adam")
        |> Logger.logSimple moduleLogger

    //match opts.ConfigBase.Verbosity.parameterValue with
    //    | Debug|Verbose->testingLogger.debug(eventX "Entering inputStuff")
    //    |_->()
    //testingLogger.debug(eventX "asdfsadf")
    //match opts.ConfigBase.Verbosity.parameterValue with
    //    |_->testingLogger.debug(eventX "Leaving inputStuff")
    //    |_->()
    Logary.Message.eventFormat (Verbose, "inputStuff: Exit", "adam")
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
    //testingLogger.verbose(eventX "Entering newMain")
    Message.eventFormat (Info, "{userName} logged in", "adam")
    |> Logger.logSimple applicationLogger
    try
        let ret=loadEAConfigFromCommandLine argv |> inputStuff  |> doStuff |> outputStuff
        //testingLogger.verbose(eventX "Leaving newMain through normal flow")
        // wait 3 seconds then tell the logging thread to resume whatever it's doing
        //testingLogger.info(eventX "MOO")
        printfn("here we are\n")
        //testingLogger.info(eventX "MOO")
        //testingLogger.fatal(eventX "COW")
        //testingLogger.error(eventX "LIKES")
        //testingLogger.warn(eventX "GRASS")
        //testingLogger.info(eventX "AND")
        //testingLogger.debug(eventX "POPCICLES")
        // I'm done (since I'm a single-threaded function, I know this)
        // take a few seconds to catch up, then you may run until you quit
        compilerCancelationToken.Token.WaitHandle.WaitOne(3000)
        printfn("here we go\n")
        manualResetEvent.Set()
        0
    with
        | :? UserNeedsHelp as hex ->
            defaultEABaseOptions.PrintThis
            //testingLogger.verbose(eventX "Leaving newMain because of UserHelp exception")
            
            0
        | ex ->
            System.Console.WriteLine ("Program terminated abnormally " + ex.Message)
            System.Console.WriteLine (ex.StackTrace)
            if ex.InnerException = null
                then
                    //testingLogger.verbose(eventX "Leaving newMain because of System Exception")
                    1
                else
                    System.Console.WriteLine("---   Inner Exception   ---")
                    System.Console.WriteLine (ex.InnerException.Message)
                    System.Console.WriteLine (ex.InnerException.StackTrace)
                    //testingLogger.verbose(eventX "Leaving newMain because of System Exception")
                    1