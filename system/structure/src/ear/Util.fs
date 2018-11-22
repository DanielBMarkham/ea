namespace EA.EAR
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
    open EA.Core.Util
    open EA.Core.Compiler
    open Logary // needed at bottom to give right "Level" lookup for logging
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "EAR"; "EAR"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    Logary.Message.eventFormat (Verbose, "Module Enter")|> Logger.logSimple moduleLogger

    /// Load the master file
    /// Any failure results in an empty string and an INFO message back to the caller (but okay result)
    type LoadMasterFile=EARConfigType->(EARConfigType * CompilationResultType)

    /// Do the slicing and dicing
    /// Note that the model comes in and leaves in the same format
    type RunTransformsAndFilters=(EARConfigType * CompilationResultType)->(EARConfigType * CompilationResultType)

    /// Take whatever model we now have and send it wherever it's supposed to go
    type WriteOutModelReportType=(EARConfigType * CompilationResultType)->int




    let inputStuff:LoadMasterFile = (fun opts->
      logEvent Debug "Method inputStuff beginning....." moduleLogger
      logEvent Debug "..... Method inputStuff ending. Normal Path." moduleLogger
      let newFakeInfo=getFakeFileInfo()
      let newFileContents:string[] = [|""|] // FAKE FOR NOW
        //initialOutput.Results 
        //|> Array.filter(fun x->isCompilationLineAFileMarker x = false)
        //|> Array.map(fun x->x.LineText)
      let newParm:CompilationUnitType[]=[|{Info=newFakeInfo; FileContents=newFileContents}|]
      let secondTimeThrough=newParm |> EA.Core.Compiler.Compile
      (opts, {Results=[||]})
    )

    let doStuff:RunTransformsAndFilters = (fun (opts, masterModel)->
      logEvent Debug "Method doStuff beginning....." moduleLogger
      logEvent Debug "..... Method doStuff ending. Normal Path." moduleLogger
      let newFakeInfo=getFakeFileInfo()
      let newFileContents:string[] = [|""|] // FAKE FOR NOW
        //initialOutput.Results 
        //|> Array.filter(fun x->isCompilationLineAFileMarker x = false)
        //|> Array.map(fun x->x.LineText)
      let newParm:CompilationUnitType[]=[|{Info=newFakeInfo; FileContents=newFileContents}|]
      let secondTimeThrough=newParm|> EA.Core.Compiler.Compile
      (opts, {Results=[||]})
    )

    let outputStuff:WriteOutModelReportType = (fun (opts, transformedModel)->
      logEvent Debug "Method outputStuff beginning....." moduleLogger
      logEvent Debug "..... Method outputStuff ending. Normal Path." moduleLogger
      0 //  it's always successful as far as the O/S is concerned
    )

    let newMain (argv:string[]) (compilerCancelationToken:System.Threading.CancellationTokenSource) (manualResetEvent:System.Threading.ManualResetEventSlim) (incomingStream:seq<string>) (ret:int byref) =
        try
          logEvent Verbose "Method newMain beginning....." moduleLogger
          // Error is the new Out. Write here so user can pipe places
          Console.Error.WriteLine "I am here. yes."
          let mutable ret=loadEARConfigFromCommandLine argv incomingStream |> inputStuff |> doStuff |> outputStuff

          // I'm done (since I'm a single-threaded function, I know this)
          // take a few seconds to catch up, then you may run until you quit
          logEvent Verbose "..... Method newMain ending. Normal Path." moduleLogger
          compilerCancelationToken.Token.WaitHandle.WaitOne(3000) |> ignore
          manualResetEvent.Set()
          ()
        with
            | :? UserNeedsHelp as hex ->
                defaultEARBaseOptions.PrintThis
                logEvent Logary.Debug "..... Method newMain ending. User requested help." moduleLogger
                manualResetEvent.Set()
                ()
            | ex ->
                logEvent Error "..... Method newMain ending. Exception." moduleLogger
                logEvent Error ("Program terminated abnormally " + ex.Message) moduleLogger
                logEvent Error (ex.StackTrace) moduleLogger
                if ex.InnerException = null
                    then
                        Logary.Message.eventFormat (Logary.Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        ret<-1
                        manualResetEvent.Set()
                        ()
                    else
                        System.Console.WriteLine("---   Inner Exception   ---")
                        System.Console.WriteLine (ex.InnerException.Message)
                        System.Console.WriteLine (ex.InnerException.StackTrace)
                        Logary.Message.eventFormat (Logary.Debug, "newMain Exit System Exception")|> Logger.logSimple moduleLogger
                        ret<-1
                        manualResetEvent.Set()
                        ()
    // For folks on anal mode, log the module being exited.  NounVerb Proper Case
    logEvent Verbose "....Module exit" moduleLogger
//logEvent Debug "Method XXXXX beginning....." moduleLogger
//logEvent Debug "..... Method XXXXX ending. Normal Path." moduleLogger
