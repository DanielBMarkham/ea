namespace EA.Compiler
  module Main=
    open System
    open System.Diagnostics
    open System.Threading
    open EA.Types
    open EA.Compiler.Util
    open Logary
    open System

    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Compiler"; "EA"; "Main" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case

    logEvent Verbose "Module enter...." moduleLogger

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
      // * Piping continued to be fun. Worked on it right up until last tests were passing
      // * Each environment acted differently. Yay!
      let incomingFileStream=
        try
          if System.Console.IsInputRedirected && System.Console.KeyAvailable && System.Console.In.Peek() <>(-1) // && System.Console.KeyAvailable
              then
                let getAll=Seq.initInfinite(fun _ -> Console.ReadLine()) |>  Seq.takeWhile (fun line -> line <> null)
                let newVal=getAll |> Seq.toArray |> Array.toSeq
                newVal
              else
                []:>seq<string>
          //with |ex->[ex.Message]:>seq<string>
        with |ex->[]:>seq<string>
      logEvent Verbose "Method main beginning....." moduleLogger
      use mre = new System.Threading.ManualResetEventSlim(false)
      use sub = Console.CancelKeyPress.Subscribe (fun _ -> mre.Set())

      let cts = new CancellationTokenSource()

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
      let incomingFileStreams =
          if (incomingFileStream |> Seq.length >0) then incomingFileStream else Seq.empty
      newMain argv cts mre (incomingFileStreams) &ret
      mre.Wait(cts.Token)
      ret
  // Can't log the module exit. There's nothing to log to