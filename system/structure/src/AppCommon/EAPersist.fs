namespace EA
  module Persist=
    open System
    open Logary
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Utilities
    //Yes, I'm repeating several modules in my include list, in seemingly-random order. Don't touch it, moron!
    open Logary
    open Logary.Configuration
    open Logary.Targets
    open Logary.Configuration
    open Logary.Configuration.Transformers
    open Expecto
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Persist"; "EAPersist" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    Logary.Message.eventFormat (Info, "Module Enter")|> Logger.logSimple moduleLogger
    Console.WriteLine "whoa"
    // For folks on anal mode, log the module being exited.  NounVerb Proper Case
    Logary.Message.eventFormat (Info, "Module Exit")|> Logger.logSimple moduleLogger
