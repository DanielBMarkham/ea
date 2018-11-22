  namespace EA.EAR
  module Main=
    open System
    open System.Diagnostics
    open System.Threading
    open EA.Types
    open EA.EAR.Util
    open Logary
    open System
    let bufferToHandleFilesBeingPipedInFromTheCommandLine = new System.Text.StringBuilder()
    let floodWorker((stream:System.IO.Stream), (buffer:byte [] ref), (ct:System.Threading.CancellationToken), (readSize:int ref)) =
      async {
        // One of three things will happen here
        // 1. The Motherhumping OS will hang the console doing nothing. The thread will hang and be killed by the Manager
        // 2. The OS is sending us a file smaller than our buffer length. We read it, update our string builder, and set the size to 0
        // 3. The OS is sending us a file larger than our buffer length. We read  it, update our string builder, and set the size cell to bytes read
        // No matter what, we go away. It's only through the readSize and builder refs that we can send anything back
        let readSizeAsync= 
          try stream.Read(!buffer, 0, (!buffer).Length)
          // for all the threads left hanging when the program moves on, there's nothing to write to. Eat that error. Too late for data now anyway.
          with | :? NotSupportedException as nse->(-1)
        // we only wanna do something if there's a non-zero response.
        // Only update the readSize cell if it's >0 otherwise you'll just keep overwriting the buffer full=0 message
        // Also only update it if it's still (-1). Otherwise you'll overwrite zeros on the actual size
        if readSizeAsync>(0) // -1 is nothing happened. 0 is end-of-stream
            then
                // We've got a live one here, Joe!
                (logEvent Verbose ("INCOMING STREAM. WORKER GOT IT. SIZE = " + readSizeAsync.ToString()) moduleLogger) |> ignore
                bufferToHandleFilesBeingPipedInFromTheCommandLine.Append(Console.InputEncoding.GetString(!buffer, 0, readSizeAsync)) |> ignore 
                readSize:=readSizeAsync
                // Try reading again to the end of the stream. Couldn't hurt, right?
                try
                  let newBuffer=Array.zeroCreate 65535
                  let rec readRest() =
                    match stream.Read(newBuffer, 0, newBuffer.Length) with 
                      | readCount when readCount >0 ->
                        bufferToHandleFilesBeingPipedInFromTheCommandLine.Append(Console.InputEncoding.GetString(newBuffer, 0, readCount)) |> ignore 
                        readRest()
                      |_ ->()
                  readRest()
                with |ex->
                  (logEvent Verbose ("INCOMING STREAM. WORKER SECOND READ FAIL " + ex.Message) moduleLogger) |> ignore
            else ()
      }
    let floodManager((stream:System.IO.Stream), (ct:System.Threading.CancellationToken), (buffer:byte [] ref),  (readSize:int ref)) =
      async {
          while(true) do
            // Infinite loop. Just keep pounding out threads to check Console.In
            let floodWorkerSetup = floodWorker(stream,buffer,ct,readSize)
            Async.Start(floodWorkerSetup,ct)
          // Should never do any of the following code, since our "normal" flow is timing out
          if (!buffer).Length>0
            then
              (logEvent Debug ("WHY AM I HERE? GOT IT MANAGER. SIZE = " + (!buffer).Length.ToString()) moduleLogger) |> ignore
            else ()
          ()
      }
    let floodConsoleToFindIncomingStreams =
      use stream = Console.OpenStandardInput()// .In:>System.IO.Stream //.OpenStandardInput(65535)
      let buffer = ref (Array.zeroCreate 65535) // need a place to buffer things (for debugging, not execution)
      let readSize=ref (-1) // need a universal flag for how much came in (for debugging, not execution)
      // I technically don't need this, but it's nice to have around in case for some reason I want 
      // to zap the whole thing manually instead of using a timeout
      let ct = new System.Threading.CancellationToken()
      let floodManagerSetup = floodManager(stream, ct, buffer, readSize)
      try
        // Whatever your timeout here, it should be long enough to get the 
        // file that's being piped in. There's no right or wrong answer
        Async.RunSynchronously(floodManagerSetup,500,ct)
      with
        // This is where we naturally end. But there's nothing for us to use yet
        // The thread pool must be allowed to spin down first
        | :? TimeoutException as te->
          (logEvent Verbose ("Method floodConsoleToFindIncomingStreams ending normally by timing out " ) moduleLogger) |> ignore
        // We'll also get errors that the stream can't read because blocking. Not here, but as threads come back and there's nowhere to come back to
        // this is for example purposes only. I think the system catches these, since we're long gone
        | :? NotSupportedException as nse->()
        |ex->
          (logEvent Debug ("FILE STREAM COMING IN BUT SOME ERROR = " + ex.Message) moduleLogger)
        // Some oddball break while we were working. Need to decide whether to continue or not
        //| :? ThreadInterruptedException as tie->
        //    (logEvent Debug "FILE STREAM COMING IN BUT THREADS DIED" moduleLogger)


    /// This file should only
    /// Handle bare-metal O/S stuff
    /// Threading and pipes. Nothing else
    /// It answers the question: can you run at all?
    [<EntryPoint>]
    let main argv =
      // if there's a file coming from the O/S, get all of it, right now
      // downstream they'll exepct a sequence, (a list comprehension even better) 
      // and those are the best thing to
      // use, but we need to free up resources to clarify any threading issues
      // so we'll fake out a seq for now

      // NOTES ABOUT PIPING THINGS AROUND LIKE YOU'RE IN LINUX
      // It's always redirected (in git bash at least) And KeyAvailable always crashes
      // console.in.peek hangs when nothing's coming in
      // That's because some processes open the pipe but never write anything to it.
      // Thanks, guys!      
      // You would think something like this would work to get piped-in streams. You would be wrong
      // stream.ReadTimeout<-250
      
      
      logEvent Verbose "Method main beginning....." moduleLogger
      use mre = new System.Threading.ManualResetEventSlim(false)
      use sub = Console.CancelKeyPress.Subscribe (fun _ -> mre.Set())

      let cts = new CancellationTokenSource()
      let incomingFileStream= bufferToHandleFilesBeingPipedInFromTheCommandLine.ToString().Split([|"\r\n";"\n";"\r"|], StringSplitOptions.None)

      //Console.Out.WriteLine "Press Ctrl-C to terminate program"
      // As long as we're a single-thread on a console app, we can
      // use a mutable cell for a return value, allowing whatever
      // threading mechanis m we use for everything else the job of returning an int back
      // to send to the O/S (In other words, this is a special case,
      // if you start spinning threads off directly in main or newMain,
      // this will not work.)
      // * Mutable variables FTW! I think.
      let mutable ret=0
      // This needs to be a sequence since it represents possibly realtime streams of strings arriving
      let foo=incomingFileStream:>seq<string>
      let incomingFileStreams =
          if ( foo |> Seq.length >0) then foo else Seq.empty
      newMain argv cts mre (incomingFileStreams) &ret
      mre.Wait(cts.Token)
      ret
  // Can't log the module exit. There's nothing to log to