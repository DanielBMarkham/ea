namespace EA.Compiler
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
    open EA.Compiler.Util
    //Yes, I'm repeating several modules in my include list, in seemingly-random order. Don't touch it, moron!
    open Logary
    open Logary.Configuration
    open Logary.Targets
    open Logary.Configuration
    open Logary.Configuration.Transformers
    open Expecto
    open Logary
    open Logary.Facade.Literals
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Compiler"; "EA"; "Main" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    Message.eventFormat (Verbose, "Module Enter")|> Logger.logSimple moduleLogger
    Console.WriteLine "whoa"

    /// This file should only
    /// Handle bare-metal O/S stuff
    /// Threading and pipes. Nothing else
    /// It answers the question: can you run at all?
    [<EntryPoint>]
    let main argv =
        // Swap stdout and sterr, since nobody seems to write to the correct place
        Message.eventFormat (Verbose, "main Enter")|> Logger.logSimple moduleLogger
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
        Message.eventFormat (Verbose, "main Exit Normal Path")|> Logger.logSimple moduleLogger
        ret
