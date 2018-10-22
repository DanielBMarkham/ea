namespace EA.Test
  module Main=
    open System
    open System.Threading
    open Logary
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Utilities
    open EA.Test.Util
    //Yes, I'm repeating several modules in my include list, in seemingly-random order. Don't touch it, moron!
    open Logary
    open Logary.Configuration
    open Logary.Targets
    open Logary.Configuration
    open Logary.Configuration.Transformers
    open Expecto
    open Logary
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Test"; "EATest"; "Main" |])
    // folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    /// This file should only
    /// Handle bare-metal O/S stuff
    /// Threading and pipes. Nothing else
    /// It answers the question: can you run at all?
    [<EntryPoint>]
    let main argv =
        let incomingStream =
            try
                if System.Console.IsInputRedirected
                    then
                        let mutable peekCharacter=(-1)
                        try 
                          if System.Console.KeyAvailable
                            then
                                peekCharacter<-System.Console.In.Peek()
                                let ret= System.Console.In.ReadToEnd()
                                logEvent Debug ("Peek character = " + peekCharacter.ToString()) moduleLogger
                                logEvent Debug ("Method main incoming stream data length = " + ret.Length.ToString()) moduleLogger
                                ret
                            else
                                ""
                        with |_ ->""
                    else ""
              with |_->""
        EA.Types.turnOnLogging()
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