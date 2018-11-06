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
    
    type LineIdentification =
      |CommandMatch of LineMatcherType
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
    type CompilationStream = CompilationLine []

    val Compile:CompilationUnitType[]->CompilationResultType
    type TranslateIncomingIntoOneStream=CompilationUnitType[]->CompilationStream
    type IdentifyCompileStreamByCommandType=CompilationStream->CompilationStream
    type TranslateCommandStreamIntoLineType=CompilationStream->CompilationStream

    val translateIncomingIntoOneStream:TranslateIncomingIntoOneStream
    val lineIdentification:IdentifyCompileStreamByCommandType

    // new ones
    val matchLineToCommandType:IdentifyCompileStreamByCommandType
    val matchLineWithCommandToLineType:TranslateCommandStreamIntoLineType
