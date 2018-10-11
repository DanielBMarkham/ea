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
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Test"; "EAETest"; "Main" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    Logary.Message.eventFormat (Info, "Module Enter")|> Logger.logSimple moduleLogger
    Console.WriteLine "whoa"

    /// This file should only
    /// Handle bare-metal O/S stuff
    /// Threading and pipes. Nothing else
    /// It answers the question: can you run at all?
    [<EntryPoint>]
    let main argv =
        // Swap stdout and sterr, since nobody seems to write to the correct place
        Logary.Message.eventFormat (Info, "main Enter")|> Logger.logSimple moduleLogger
        let oldStdout=System.Console.Out
        let oldStdin=System.Console.In
        let oldStdErr=System.Console.Error
        System.Console.SetOut oldStdErr
        System.Console.SetError oldStdout
        use mre = new System.Threading.ManualResetEventSlim(false)
        use sub = Console.CancelKeyPress.Subscribe (fun _ -> mre.Set())
        let cts = new CancellationTokenSource()
        Console.Error.WriteLine "Press Ctrl-C to terminate program"
        let ret=newMain argv cts mre
        mre.Wait(cts.Token)

        System.Console.SetError oldStdErr
        System.Console.SetOut oldStdout
        Logary.Message.eventFormat (Info, "main Exit Normal Path")|> Logger.logSimple moduleLogger
        ret



    // let logger = Log.create "MyTests"
    // [<EntryPoint>]
    // let main argv =
    //     Tests.runTestsInAssembly defaultConfig argv
