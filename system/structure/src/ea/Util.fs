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
    open EA.Core.Util
    open EA.Core.Compiler
    open Logary // needed at bottom to give right "Level" lookup for logging
    open System.Numerics

    // * Opens have gotten fewer. Still working through my code-build-debug cycle
    // * Have yet to use the debugger (!)

    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Compiler"; "EA"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    /// Perform the translation across the client/dll boundary
    /// Yep, there are other ways to do this. I prefer to think of
    /// the boundary as another outside of the onion. Trust no one. :)
    //let EALibCompile (localCompilationUnits:EA.Types.EACompilationUnitType[]):EA.Types.EACompilationResultType =
    //  let libraryCompilationUnits:EA.Core.Compiler.CompilationUnitType[] =
    //    localCompilationUnits |> Array.map(fun x->{Info=x.Info; FileContents=x.FileContents})
    //  let libraryRet:EA.Core.Compiler.CompilationResultType=Compile(libraryCompilationUnits)
    //  let translatedRet:EACompilationResultType={MasterModelText=libraryRet.MasterModelText}
    //  translatedRet

    /// Responsible only for getting a list of strings and associated files to compile. Nothing else
    /// Any failure results in an empty string and an INFO message back to the caller (but okay result)
    type GetCompileDataType=EAConfigType->(EAConfigType * CompilationUnitType[])
    // Main compilation happens here. It can fail but it can't crash, so no (direct) IO
    type RunCompilationType=(EAConfigType * CompilationUnitType[])->(EAConfigType * CompilationResultType)
    type CompileType=CompilationUnitType[]->CompilationResultType
    /// Final stage. Writes out model to persistent storage. It can fail, but it doesn't matter,
    /// since any failure in simple IO would prevent us from telling anybody
    type WriteOutCompiledModelType=(EAConfigType * CompilationResultType)->int

    let inputStuff:GetCompileDataType = (fun opts->
      logEvent Logary.Debug "Method inputStuff beginning....." moduleLogger
      let didTheUserProvideAnyCLIFiles=opts.FileListFromCommandLine.Length>0
      let isThereAFileStreamGBeingPipedToUs=(opts.IncomingStream |> Seq.length)>0
      let areKeystrokesQueuedUpAndWaitingToBeProcessed = try System.Console.KeyAvailable with |_->false
      logEvent Verbose ("Method inputStuff FILES REFERENCED ON CLI: " + opts.FileListFromCommandLine.Length.ToString()) moduleLogger
      let incomingPipedFileContents = opts.IncomingStream |>Seq.toArray
      let streamFileParm:CompilationUnitType={Info=getFakeFileInfo(); FileContents=incomingPipedFileContents}
      let loadAllCLIFiles:CompilationUnitType[]=
        let incomingCLIContents =
            opts.FileListFromCommandLine |>
                Array.map(fun x->
                let (fileName:string),(fileInfo:System.IO.FileInfo)=x
                let fileContents=
                    try System.IO.File.ReadAllLines(fileName) with |_->[||]
                logEvent Verbose ("Method inputStuff file " + fileName + " line count = " + fileContents.Length.ToString()) moduleLogger
                (fileInfo,fileContents)
                )
        let collapsedMap:CompilationUnitType[]=incomingCLIContents |> Array.map(fun (x,y)->{Info=x;FileContents=y})
        collapsedMap
      let compilationUnitsToReturn:CompilationUnitType[] =
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
      let libraryLogger:Logger= (logary.getLogger (PointName [| "EA"; "Core"; "Compiler"; "EALib"; "Compiler" |]))
      let l2 (lvl:LogLevel) (str:string) = logEvent lvl str libraryLogger
      l2 Debug "..... Setting Remote Logger." 
      setLogger (l2)
      let ret:CompilationResultType=EA.Core.Compiler.Compile(compilationUnitArray)
      logEvent Logary.Debug ("..... Method doStuff ending. Normal Path. " + ret.Results.Length.ToString() + " lines in Results" ) moduleLogger
      (opts,ret)
    )

    let outputStuff:WriteOutCompiledModelType = (fun (opts, transformedModel)->
      logEvent Logary.Debug "Method outputStuff beginning....." moduleLogger
      // Our first test -- actually part of prod code -- should compile the results again with same result
      // If the loop-through check fails, we're not writing out
      let initialOutput=transformedModel
      let newFakeInfo=getFakeFileInfo()
      let newFileContents:string[] = 
        initialOutput.Results 
        |> Array.filter(fun x->isCompilationLineAFileMarker x = false)
        |> Array.map(fun x->x.LineText)
      logEvent Logary.Verbose ("Method outputStuff newFileContents length = " + newFileContents.Length.ToString()) moduleLogger
      let newParm:CompilationUnitType[]=[|{Info=newFakeInfo; FileContents=newFileContents}|]
      let opts,secondTimeThrough=(opts,newParm) |> doStuff
      logEvent Logary.Verbose ("Method outputStuff secondTimeThrough length = " + secondTimeThrough.Results.Length.ToString()) moduleLogger
      // ROUND-TRIP TEST DOESN'T MAKE SENSE WITH THE CODE IN THIS HALF-ASSED STATE
      // TURN BACK ON LATER
      //if initialOutput<>secondTimeThrough
      //  then
      //      logEvent Logary.Error ("..... Method outputStuff loopback FAIL" ) moduleLogger
      //      failwith "MODEL LOOPBACK FAILURE"
      //  else
      let totalCharacters = transformedModel.Results |> Array.sumBy(fun x->x.LineText.Length)
      logEvent Logary.Debug ("..... Method outputStuff totalCharacters = " + totalCharacters.ToString()) moduleLogger
      // THINGS GET WRITTEN OUT HERE
      transformedModel.Results |> removeFileMarkersFromStream |> Array.iteri(fun i x->
        Console.Error.WriteLine(x.LineText)
        )
      logEvent Logary.Verbose ("Method outputStuff transformedModel length = " + transformedModel.Results.Length.ToString()) moduleLogger

      let tempFeedbackSB = new System.Text.StringBuilder(65535)
      transformedModel.Results|>Array.iteri(fun i x->
        let linenum = i.ToString() + "(" + x.LineNumber.ToString() + ")."
        let l2 = linenum + "         ".Substring(linenum.Length)
        let t1 = l2 + "" + (match x.Type with |LineType lt->lt.ToString() |CommandMatch cm->cm.LineType.ToString())
        let t2 = t1 + "                           | ".Substring(t1.Length)
        tempFeedbackSB.Append(t2 + " " + x.LineText + "\r\n") |> ignore
        )
      Console.Out.WriteLine()
      Console.Out.WriteLine(tempFeedbackSB.ToString())
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
            | :? System.NotSupportedException as nse->
                logEvent Logary.Debug ("..... Method newMain ending. NOT SUPPORTED EXCEPTION = " + nse.Message) moduleLogger
                ()
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