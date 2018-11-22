namespace EA.Core
  module Compiler=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Core.Util
    open EA.Core.Tokens
    open Logary
    
    // Since this is a shared DLL, I think we need to set the logger from the client-end
    val setLogger:(LogLevel->string->unit)->unit
    // Couldn't decide for separate types or all Compilation Line
    type LineIdentification =
      /// We've identified the command on the line but we don't know what the line is
      |CommandMatch of LineMatcherType
      /// We've taken the command we identified and the context and figured out what kind of line it is
      |LineType of EasyAMLineTypes
    type CompilationLine =
      {
      ShortFileName:string
      FullFileName:string
      CompilationUnitNumber:int
      LineNumber:int 
      Type:LineIdentification
      LineText:string
      TextStartColumn:int
      }
      member LineTypeDescription:string
    // helpers/lenses
    val isCompilationLineACommand:CompilationLine->bool
    val isCompilationLineALineType:CompilationLine->bool
    val isCompilationLineAFileBegin:CompilationLine->bool
    val isCompilationLineAFileEnd:CompilationLine->bool
    val isCompilationLineAFileMarker:CompilationLine->bool 
    val isCompilationLineFreeFormText:CompilationLine->bool

    val isCompilationLineTypeUnprocessed:CompilationLine->bool
    val isCompilationLineTypeFileBegin:CompilationLine->bool
    val isCompilationLineTypeFileEnd:CompilationLine->bool
    val isCompilationLineTypeNewSectionItem:CompilationLine->bool
    val isCompilationLineTypeNewJoinedItem:CompilationLine->bool
    val isCompilationLineTypeCompilerJoinDirective:CompilationLine->bool
    val isCompilationLineTypeNewItemJoinCombination:CompilationLine->bool
    val isCompilationLineTypeCompilerNamespaceDirectiveWithItem:CompilationLine->bool
    val isCompilationLineTypePoundTagWithItem:CompilationLine->bool
    val isCompilationLineTypeMentionTagWithItem:CompilationLine->bool
    val isCompilationLineTypeNameValueTagWithItem:CompilationLine->bool
    val isCompilationLineTypeCompilerSectionDirectiveWithItem:CompilationLine->bool
    val isCompilationLineTypeCompilerJoinTypeWithItem:CompilationLine->bool
    val isCompilationLineTypeCompilerNamespaceDirective:CompilationLine->bool
    val isCompilationLineTypeCompilerTagReset:CompilationLine->bool
    val isCompilationLineTypeCompilerSectionDirective:CompilationLine->bool
    val isCompilationLineTypeFreeFormText:CompilationLine->bool
    val isCompilationLineTypeEmptyLine:CompilationLine->bool

    
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
