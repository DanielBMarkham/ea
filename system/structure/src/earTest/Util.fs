namespace EA.EAR.Test
  module Util=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Core
    open EA.Core.Util
    open EA.Core.Compiler

    open Logary // needed at bottom to give right "Level" lookup for logging
    let moduleLogger = logary.getLogger (Logary.PointName [| "EA"; "EAR"; "Test"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    /// Perform the translation across the client/dll boundary
    /// Yep, there are other ways to do this. I prefer to think of
    /// the boundary as another outside of the onion. Trust no one. :)
    let EALibCompile (localCompilationUnits:CompilationUnitType[]):CompilationResultType =
      let libraryCompilationUnits:CompilationUnitType[] =
        localCompilationUnits |> Array.map(fun x->{Info=x.Info; FileContents=x.FileContents})
      let libraryRet:CompilationResultType=Compile(libraryCompilationUnits)
      let translatedRet:CompilationResultType={Results=[||]}
      translatedRet


    let newMain (argv:string[]) (compilerCancelationToken:System.Threading.CancellationTokenSource) (manualResetEvent:System.Threading.ManualResetEventSlim) (ret:int byref) =
        try
            logEvent Verbose "Method newMain beginning....." moduleLogger
            // Error is the new Out. Write here so user can pipe places
            //Console.Error.WriteLine "I am here. yes."
            // Returns a non-zero if any tests failed
            ret<-Expecto.Tests.runTestsInAssembly Expecto.Impl.ExpectoConfig.defaultConfig argv

            // I'm done (since I'm a single-threaded function, I know this)
            // take a few seconds to catch up, then you may run until you quit
            logEvent Verbose "..... Method newMain ending. Normal Path." moduleLogger
            compilerCancelationToken.Token.WaitHandle.WaitOne(3000) |> ignore
            manualResetEvent.Set()
            ()
        with
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



//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger

//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
