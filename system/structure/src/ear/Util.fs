namespace EA.EAR
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
    open Logary
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "EAR"; "EAR"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    Logary.Message.eventFormat (Verbose, "Module Enter")|> Logger.logSimple moduleLogger


    let inputStuff:LoadMasterFile = (fun opts->
      Message.eventFormat (Debug, "inputStuff: Enter") |> Logger.logSimple moduleLogger

      Message.eventFormat (Debug, "inputStuff: Exit") |> Logger.logSimple moduleLogger
      (opts, {MasterModelText=[||]})
    )

    let doStuff:RunTransformsAndFilters = (fun (opts, masterModel)->
        Message.eventFormat (Debug, "doStuff: Exit") |> Logger.logSimple moduleLogger
        Message.eventFormat (Debug, "doStuff: Exit") |> Logger.logSimple moduleLogger
        (opts, {MasterModelText=[||]})
    )

    let outputStuff:WriteOutModelReportType = (fun (opts, transformedModel)->
        Message.eventFormat (Debug, "outputtuff: Enter") |> Logger.logSimple moduleLogger

        Message.eventFormat (Debug, "outputStuff: Exit") |> Logger.logSimple moduleLogger
        0 //  it's always successful as far as the O/S is concerned
    )

    #nowarn "0067"
    let newMain (argv:string[]) (compilerCancelationToken:System.Threading.CancellationTokenSource) (manualResetEvent:System.Threading.ManualResetEventSlim) =
        Logary.Message.eventFormat (Verbose, "newMain Enter")|> Logger.logSimple moduleLogger
        try
          let r=loadEARConfigFromCommandLine argv |> inputStuff |> doStuff |> outputStuff

          // I'm done (since I'm a single-threaded function, I know this)
          // take a few seconds to catch up, then you may run until you quit
          compilerCancelationToken.Token.WaitHandle.WaitOne(3000) |> ignore
          manualResetEvent.Set()
          Logary.Message.eventFormat (Verbose, "newMain Exit Normal Path")|> Logger.logSimple moduleLogger
          0
        with
            | :? UserNeedsHelp as hex ->
                defaultEARBaseOptions.PrintThis
                Logary.Message.eventFormat (Debug, "newMain Exit User Need Help")|> Logger.logSimple moduleLogger
                manualResetEvent.Set()
                0
            | ex ->
                System.Console.WriteLine ("Program terminated abnormally " + ex.Message)
                System.Console.WriteLine (ex.StackTrace)
                if ex.InnerException = null
                    then
                        Logary.Message.eventFormat (Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        manualResetEvent.Set()
                        1
                    else
                        System.Console.WriteLine("---   Inner Exception   ---")
                        System.Console.WriteLine (ex.InnerException.Message)
                        System.Console.WriteLine (ex.InnerException.StackTrace)
                        Logary.Message.eventFormat (Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        manualResetEvent.Set()
                        1
    // For folks on anal mode, log the module being exited.  NounVerb Proper Case
    Logary.Message.eventFormat (Verbose, "Module Exit")|> Logger.logSimple moduleLogger