namespace EA.EAR.Test
  open Logary.Adapters.Facade
  module Util=
    open System
    open EA.Types
    open EA.EAR.Sample
    open Logary
    open LogaryFacadeAdapter
    let moduleLogger = logary.getLogger (Logary.PointName [| "EA"; "EAR"; "Test"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    #nowarn "0067"
    let newMain (argv:string[]) (compilerCancelationToken:System.Threading.CancellationTokenSource) (manualResetEvent:System.Threading.ManualResetEventSlim) =
        try
            logEvent Verbose "Method newMain beginning....." moduleLogger
            let ret=Expecto.Tests.runTestsInAssembly Expecto.Impl.ExpectoConfig.defaultConfig argv
            //let ret=0 //oadEAConfigFromCommandLine argv |> inputStuff  |> doStuff |> outputStuff

            // I'm done (since I'm a single-threaded function, I know this)
            // take a few seconds to catch up, then you may run until you quit
            logEvent Verbose "..... Method newMain edning. Normal Path." moduleLogger
            compilerCancelationToken.Token.WaitHandle.WaitOne(3000) |> ignore
            manualResetEvent.Set()
            0
        with
            | :? System.Exception as hex-> //UserNeedsHelp as hex ->
                logEvent Debug "..... Method newMain edning. User requested help." moduleLogger
                defaultEABaseOptions.PrintThis
                0
            | ex ->
                logEvent Error "..... Method newMain edning. Exception." moduleLogger
                logEvent Error ("Program terminated abnormally " + ex.Message)
                logEvent Error (ex.StackTrace)
                if ex.InnerException = null
                    then
                        //logEvent Error 
                        //Logary.Message.eventFormat (Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        1
                    else
                        System.Console.WriteLine("---   Inner Exception   ---")
                        System.Console.WriteLine (ex.InnerException.Message)
                        System.Console.WriteLine (ex.InnerException.StackTrace)
                        //Logary.Message.eventFormat (Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        1
    logEvent Verbose "....Module exit" moduleLogger