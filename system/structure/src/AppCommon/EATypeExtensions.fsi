namespace EA
  module Types=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open Logary
    // * Started using my first FSI Signature file
    // * The Types problem and dlls - differences from Microservices
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

