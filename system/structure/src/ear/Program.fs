﻿namespace EA.EAR
  module Main=
    open System
    open System.Threading
    open EA.EAR.Util

    /// This file should only
    /// Handle bare-metal O/S stuff
    /// Threading and pipes. Nothing else
    /// It answers the question: can you run at all?
    [<EntryPoint>]
    let main argv =
        // Swap stdout and sterr, since nobody seems to write to the correct place
        let oldStdout=System.Console.Out
        let oldStdin=System.Console.In
        let oldStdErr=System.Console.Error
        System.Console.SetOut oldStdErr
        System.Console.SetError oldStdout

        use mre = new System.Threading.ManualResetEventSlim(false)
        use sub = Console.CancelKeyPress.Subscribe (fun _ -> mre.Set())
        let cts = new CancellationTokenSource()
        let ret=newMain argv cts mre
        //Console.Error.WriteLine "Press Ctrl-C to terminate program"
        mre.Wait(cts.Token)

        System.Console.SetError oldStdErr
        System.Console.SetOut oldStdout
        ret