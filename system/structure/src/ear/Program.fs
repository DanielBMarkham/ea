namespace EA.EAR
  module Main=
    open System
    open System.Threading
    open EA.Types
    open EA.EAR.Util
    open Logary
    let moduleLogger = logary.getLogger (PointName [| "EA"; "EAR"; "EAR"; "Program" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    /// This file should only
    /// Handle bare-metal O/S stuff
    /// Threading and pipes. Nothing else
    /// It answers the question: can you run at all?
    [<EntryPoint>]
    let main argv =
        logEvent Verbose "Method main beginning....." moduleLogger
        use mre = new System.Threading.ManualResetEventSlim(false)
        use sub = Console.CancelKeyPress.Subscribe (fun _ -> mre.Set())
        let cts = new CancellationTokenSource()
        Console.Out.WriteLine "Press Ctrl-C to terminate program"
        // As long as we're a single-thread on a console app, we can
        // use a mutable cell for a return value, allowing whatever
        // threading mechanis m we use for everything else the job of returning an int back
        // to send to the O/S (In other words, this is a special case,
        // if you start spinning threads off directly in main or newMain,
        // this will not work.)
        let mutable ret=0
        newMain argv cts mre &ret
        mre.Wait(cts.Token)
        ret
    // Can't log the module exit. There's nothing to log to