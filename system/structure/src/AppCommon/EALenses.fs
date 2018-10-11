namespace EA
  module Lenses=
    open System
    open Logary
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    //Yes, I'm repeating several modules in my include list, in seemingly-random order. Don't touch it, moron!
    open Logary
    open Logary.Configuration
    open Logary.Targets
    open Logary.Configuration
    open Logary.Configuration.Transformers
    open Expecto
    open Logary
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Lenses"; "EALenses" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    Logary.Message.eventFormat (Verbose, "Module Enter")|> Logger.logSimple moduleLogger
    Console.WriteLine "whoa"
    // For folks on anal mode, log the module being exited.  NounVerb Proper Case
    Logary.Message.eventFormat (Verbose, "Module Exit")|> Logger.logSimple moduleLogger
