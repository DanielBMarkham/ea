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
    // FOR PROGRAM.FS FILES
    // Swap stdout and sterr, since nobody seems to write to the correct place
    // YOU CANNOT LOG OR DO ANYTHING BEFORE THIS
    // All program output goes to Console.Error. This allows unix piping
    // while also allowing other folks to write out to the console
    let oldStdout=System.Console.Out
    let oldStdin=System.Console.In
    let oldStdErr=System.Console.Error
    System.Console.SetOut oldStdErr
    System.Console.SetError oldStdout


    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Compiler"; "EA"; "Main" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    Message.eventFormat (Verbose, "Module Enter")|> Logger.logSimple moduleLogger
    Console.Error.WriteLine "whoa"

    /// This file should only
    /// Handle bare-metal O/S stuff
    /// Threading and pipes. Nothing else
    /// It answers the question: can you run at all?
    [<EntryPoint>]
    let main argv =

        Message.eventFormat (Verbose, "main Enter")|> Logger.logSimple moduleLogger

        use mre = new System.Threading.ManualResetEventSlim(false)
        use sub = Console.CancelKeyPress.Subscribe (fun _ -> mre.Set())
        let cts = new CancellationTokenSource()
        let ret=newMain argv cts mre
        Console.Error.WriteLine "This is your file! Isn't it exciting!"
        mre.Wait(cts.Token)



        // Switch the Consoles back
        System.Console.SetError oldStdErr
        System.Console.SetOut oldStdout
        Message.eventFormat (Verbose, "main Exit Normal Path")|> Logger.logSimple moduleLogger
        ret
