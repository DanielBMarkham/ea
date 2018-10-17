﻿namespace EA.Compiler
  module Main=
    open System
    open System.Threading
    open EA.Types
    open EA.Compiler.Util
    open Logary

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
        logEvent Verbose "Method main beginning....." moduleLogger
        use mre = new System.Threading.ManualResetEventSlim(false)
        use sub = Console.CancelKeyPress.Subscribe (fun _ -> mre.Set())

        let cts = new CancellationTokenSource()
        //use newErrorTextWriter=System.IO.TextWriter.Synchronized(Console.Error)
        //Console.SetError(newErrorTextWriter)

        //Console.Out.WriteLine "Press Ctrl-C to terminate program"
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