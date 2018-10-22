namespace EA
  module Types=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open Logary

    type LogEventParms=LogLevel*string*Logary.Logger

    val logEvent:LogLevel->string->Logary.Logger->unit 
    val logary:Logary.Configuration.LogManager
    
    type CompilationUnitType = {
        Info:System.IO.FileInfo
        FileContents:string[]
    }
    type CompilationResultType = {
        MasterModelText:string[]
        }
    
