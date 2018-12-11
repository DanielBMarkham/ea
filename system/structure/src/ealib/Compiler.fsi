namespace EA.Core
  module Compiler=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Core.Util
    open EA.Core.Tokens
    open Logary
    
    // Since this is a shared DLL, I think we need to set the logger from the client-end
    val setLogger:(LogLevel->string->unit)->unit

    
    // Right now, the idea is that CompileResult is the 'done' type, used when
    // we're completely done on this end. It contains a stream of compiled lines
    // plus any kind of meta/set-based information
    // CompilationStream is the lines as they move around getting worked on
    type CompilationStream = CompilationLine []
    // This will evolve, probably as the compiler matures
    type CompilationResultType = {
      Results:CompilationStream
      }
    // helpers/lenses
    val removeFileMarkersFromStream:CompilationStream->CompilationStream
    val removeFileMarkersFromResult:CompilationResultType->CompilationResultType
    // MAIN FUNCTION FOR THE LIBRARY
    val Compile:CompilationUnitType[]->CompilationResultType
    
    // spec
    type TranslateIncomingIntoOneStream=CompilationUnitType[]->CompilationStream
    type IdentifyCompileStreamByCommandType=CompilationStream->CompilationStream
    type TranslateCommandStreamIntoLineType=CompilationStream->CompilationStream
    // implementation
    val translateIncomingIntoOneStream:TranslateIncomingIntoOneStream
    val matchLineToCommandType:IdentifyCompileStreamByCommandType
    val matchLineWithCommandToLineType:TranslateCommandStreamIntoLineType
