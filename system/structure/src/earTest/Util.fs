namespace EA.EAR.Test
  module Util=
    open System
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
    open Logary
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "EAR"; "Test"; "EARTest"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    Logary.Message.eventFormat (Verbose, "Module Enter")|> Logger.logSimple moduleLogger
    Console.WriteLine "whoa"

    #nowarn "0067"
    let newMain (argv:string[]) (compilerCancelationToken:System.Threading.CancellationTokenSource) (manualResetEvent:System.Threading.ManualResetEventSlim) =
        Logary.Message.eventFormat (Verbose, "newMain Enter")|> Logger.logSimple moduleLogger
        try
            Logary.Adapters.Facade.LogaryFacadeAdapter.initialise<Expecto.Logging.Logger> logary
            Tests.runTestsInAssembly defaultConfig argv
            let ret=0 //oadEAConfigFromCommandLine argv |> inputStuff  |> doStuff |> outputStuff

            // I'm done (since I'm a single-threaded function, I know this)
            // take a few seconds to catch up, then you may run until you quit
            compilerCancelationToken.Token.WaitHandle.WaitOne(3000) |> ignore
            manualResetEvent.Set()
            Logary.Message.eventFormat (Verbose, "newMain Exit Normal Path")|> Logger.logSimple moduleLogger
            0
        with
            | :? UserNeedsHelp as hex ->
                defaultEABaseOptions.PrintThis
                Logary.Message.eventFormat (Debug, "newMain Exit User Need Help")|> Logger.logSimple moduleLogger
                0
            | ex ->
                System.Console.WriteLine ("Program terminated abnormally " + ex.Message)
                System.Console.WriteLine (ex.StackTrace)
                if ex.InnerException = null
                    then
                        Logary.Message.eventFormat (Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        1
                    else
                        System.Console.WriteLine("---   Inner Exception   ---")
                        System.Console.WriteLine (ex.InnerException.Message)
                        System.Console.WriteLine (ex.InnerException.StackTrace)
                        Logary.Message.eventFormat (Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        1


    // For folks on anal mode, log the module being exited.  NounVerb Proper Case
    Logary.Message.eventFormat (Verbose, "Module Exit")|> Logger.logSimple moduleLogger
