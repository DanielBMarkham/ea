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

    type EAConfigType =
        {
        ConfigBase:ConfigBase
        FileListFromCommandLine:(string*System.IO.FileInfo)[]
        IncomingStream:seq<string>
        }
        with member PrintThis:unit->unit

    type EARConfigType =
        {
        ConfigBase:ConfigBase
        FileListFromCommandLine:(string*System.IO.FileInfo)[]
        IncomingStream:seq<string>
        }
        with member PrintThis:unit->unit

    val defaultEAConfig:EAConfigType
    val defaultEARConfig:EARConfigType

    val logEvent:LogLevel->string->Logary.Logger->unit
    val logary:Logary.Configuration.LogManager

 