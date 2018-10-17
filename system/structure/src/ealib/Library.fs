namespace EA.Core
    module Main=
        open System
        open SystemTypeExtensions
        open SystemUtilities
        open CommandLineHelper
        open EA.Types
        open EA.Lenses
        open EA.Persist
        open Logary // needed at bottom to give right "Level" lookup for logging
        // Tag-list for the logger is namespace, project name, file name
        let moduleLogger = logary.getLogger (PointName [| "EA"; "Core"; "EALib"; "Library" |])
        // For folks on anal mode, log the module being entered.  NounVerb Proper Case
        logEvent Verbose "Module enter...." moduleLogger

        Console.WriteLine "whoa"
        // For folks on anal mode, log the module being exited.  NounVerb Proper Case
        Logary.Message.eventFormat (Verbose, "Module Exit")|> Logger.logSimple moduleLogger

        module Say =
            let hello name =
                printfn "Hello %s" name
    
        logEvent Verbose "....Module exit" moduleLogger



//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
