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
    //open EA.Core
    open Logary // needed at bottom to give right "Level" lookup for logging
    // * Opens have gotten fewer. Still working through my code-build-debug cycle
    // * Have yet to use the debugger (!)

    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Compiler"; "EA"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    // We need this type constructor to cross the DLL boundary to the library
    let compile (localCompilationUnits:CompilationUnitType[]):CompilationResultType =
      let translatedInput:EA.Core.Util.CompilationUnitType[] =
        localCompilationUnits|>Array.map(fun x->{Info=x.Info;FileContents=x.FileContents})
      let compileResult=EA.Core.Util.Compile(translatedInput)
      let translatedResult={MasterModelText=compileResult.MasterModelText}
      translatedResult

    let inputStuff:GetCompileDataType = (fun opts->
      logEvent Logary.Debug "Method inputStuff beginning....." moduleLogger
      let didTheUserProvideAnyCLIFiles=opts.FileListFromCommandLine.Length>0 
      let isThereAFileStreamGBeingPipedToUs=(opts.IncomingStream |> Seq.length)>0
      let areKeystrokesQueuedUpAndWaitingToBeProcessed = try System.Console.KeyAvailable with |_->false
      logEvent Verbose ("Method inputStuff FILES REFERENCED ON CLI: " + opts.FileListFromCommandLine.Length.ToString()) moduleLogger
      let incomingPipedFileContents = opts.IncomingStream |>Seq.toArray
      let streamFileParm={Info=getFakeFileInfo(); FileContents=incomingPipedFileContents}
      let loadAllCLIFiles=
        let incomingCLIContents = 
            opts.FileListFromCommandLine |>
                Array.map(fun x->
                let (fileName:string),(fileInfo:System.IO.FileInfo)=x
                let fileContents=
                    try System.IO.File.ReadAllLines(fileName) with |_->[||]
                logEvent Verbose ("Method inputStuff file " + fileName + " line count = " + fileContents.Length.ToString()) moduleLogger
                (fileInfo,fileContents)
                )
        let collapsedMap=incomingCLIContents |> Array.map(fun (x,y)->{Info=x;FileContents=y})
        collapsedMap
      let compilationUnitsToReturn =
          match isThereAFileStreamGBeingPipedToUs, didTheUserProvideAnyCLIFiles
            with
                | true, true->
                    logEvent Verbose "Method inputStuff FileStream: YES, Extra CLI files: YES." moduleLogger
                    let ret = loadAllCLIFiles |> Array.append [|streamFileParm|]
                    ret
                | true,false->
                    logEvent Verbose "Method inputStuff FileStream: YES, Extra CLI files: NO." moduleLogger
                    [|streamFileParm|]
                | false,true->
                    logEvent Verbose "Method inputStuff FileStream: NO, Extra CLI files: YES." moduleLogger
                    loadAllCLIFiles
                | false,false->
                    logEvent Verbose "Method inputStuff FileStream: NO, Extra CLI files: NO." moduleLogger
                    [||]
            
      logEvent Logary.Debug "..... Method inputStuff ending. Normal Path." moduleLogger
      (opts, compilationUnitsToReturn)
    )

    let doStuff:RunCompilationType = (fun (opts, compilationUnitArray)->
      logEvent Logary.Debug ("Method doStuff beginning..... " + compilationUnitArray.Length.ToString() + " Compilation Units coming in with " + (compilationUnitArray |> Seq.sumBy(fun x->x.FileContents.Length)).ToString() + " total lines") moduleLogger
      let ret=compile(compilationUnitArray)
      //let primitives=compilationUnitArray|>Array.map(fun x->(x.Info, x.FileContents))
      //let ret2:string[]=EA.Core.Util.Compile(primitives)
      //let ret={MasterModelText=ret2}
      //let newin:EA.Core.Util.CompilationUnitType[]=compilationUnitArray|>Array.map(fun x->{Info=x.Info;FileContents=x.FileContents})
      //let foo:EA.Core.Util.CompilationResultType=EA.Core.Util.CompileFunction(newin)
      //logEvent Logary.Debug ("Foo ") moduleLogger
      ////logEvent Verbose ("FOO = " + foo.MasterModelText.Length.ToString() + " lines in master model" ) moduleLogger
      logEvent Logary.Debug ("..... Method doStuff ending. Normal Path. " + ret.MasterModelText.Length.ToString() + " lines in master model" ) moduleLogger
      (opts,ret)
    )

    let outputStuff:WriteOutCompiledModelType = (fun (opts, transformedModel)->
      logEvent Logary.Debug "Method outputStuff beginning....." moduleLogger
      // Our first test -- actually part of prod code -- should compile the results again with same result
      // If the loop-through check fails, we're not writing out
      let initialOutput=transformedModel
      let newFakeInfo=getFakeFileInfo()
      let newParm:CompilationUnitType[]=[|{Info=newFakeInfo; FileContents=initialOutput.MasterModelText}|]
      let opts,secondTimeThrough=(opts,newParm) |> doStuff
      if initialOutput<>secondTimeThrough
        then
            failwith "MODEL LOOPBACK FAILURE"
        else
            transformedModel.MasterModelText |> Array.iteri(fun i x->
              Console.Error.WriteLine(x)
              )
      logEvent Logary.Debug "..... Method outputStuff ending. Normal Path." moduleLogger
      0 //  it's always successful as far as the O/S is concerned
    )

    let newMain (argv:string[]) (compilerCancelationToken:System.Threading.CancellationTokenSource) (manualResetEvent:System.Threading.ManualResetEventSlim) (incomingStream:seq<string>) (ret:int byref) =
        try
          logEvent Verbose "Method newMain beginning....." moduleLogger
          //logEvent Logary.Debug ("Method newMain incomingStuff lineCount = " + (incomingStream |> Seq.length).ToString()) moduleLogger

          // Error is the new Out. Write here so user can pipe places
          //Console.Error.WriteLine "I am here. yes."
         // incomingStream |> Seq.iter(fun x->Console.Error.Write(x))
          let mutable ret=loadEAConfigFromCommandLine argv incomingStream |> inputStuff |> doStuff |> outputStuff
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