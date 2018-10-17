namespace EA.Compiler
  module Util=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Utilities
    open EA.Core
    open Logary // needed at bottom to give right "Level" lookup for logging

    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Compiler"; "EA"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger


    let inputStuff:GetCompileDataType = (fun opts->
      logEvent Debug "Method inputStuff beginning....." moduleLogger
      logEvent Debug "..... Method inputStuff ending. Normal Path." moduleLogger
      (opts, [||])
    )

    let doStuff:RunCompilationType = (fun (opts, masterModel)->
      logEvent Debug "Method doStuff beginning....." moduleLogger
      logEvent Debug "..... Method doStuff ending. Normal Path." moduleLogger
      // Console.Error.WriteLine()
      // Console.Error.WriteLine(incomingStuff)
      (opts, {MasterModelText=[||]})
    )

    let outputStuff:WriteOutCompiledModelType = (fun (opts, transformedModel)->
      logEvent Debug "Method outputStuff beginning....." moduleLogger
      Console.Error.Write(EA.Types.pipedStreamIncoming)
      logEvent Debug "..... Method outputStuff ending. Normal Path." moduleLogger
      0 //  it's always successful as far as the O/S is concerned
    )

    #nowarn "0067"
    let newMain (argv:string[]) (compilerCancelationToken:System.Threading.CancellationTokenSource) (manualResetEvent:System.Threading.ManualResetEventSlim) (ret:int byref) =
        try
          logEvent Verbose "Method newMain beginning....." moduleLogger
          let incomingStream=EA.Types.pipedStreamIncoming
          logEvent Debug ("incomingStuff = " + incomingStream) moduleLogger

          // Error is the new Out. Write here so user can pipe places
          //Console.Error.WriteLine "I am here. yes."
          let mutable ret=loadEAConfigFromCommandLine argv |> inputStuff |> doStuff |> outputStuff
          // I'm done (since I'm a single-threaded function, I know this)
          // take a few seconds to catch up, then you may run until you quit
          logEvent Verbose "..... Method newMain ending. Normal Path." moduleLogger
          compilerCancelationToken.Token.WaitHandle.WaitOne(3000) |> ignore
          manualResetEvent.Set()
          ()
        with
            | :? UserNeedsHelp as hex ->
                defaultEARBaseOptions.PrintThis
                logEvent Debug "..... Method newMain ending. User requested help." moduleLogger
                manualResetEvent.Set()
                ()
            | ex ->
                logEvent Error "..... Method newMain ending. Exception." moduleLogger
                logEvent Error ("Program terminated abnormally " + ex.Message)
                logEvent Error (ex.StackTrace)
                if ex.InnerException = null
                    then
                        Logary.Message.eventFormat (Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        ret<-1
                        manualResetEvent.Set()
                        ()
                    else
                        System.Console.WriteLine("---   Inner Exception   ---")
                        System.Console.WriteLine (ex.InnerException.Message)
                        System.Console.WriteLine (ex.InnerException.StackTrace)
                        Logary.Message.eventFormat (Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        ret<-1
                        manualResetEvent.Set()
                        ()
    // For folks on anal mode, log the module being exited.  NounVerb Proper Case
    logEvent Verbose "....Module exit" moduleLogger
//logEvent Debug "Method XXXXX beginning....." moduleLogger
//logEvent Debug "..... Method XXXXX ending. Normal Path." moduleLogger