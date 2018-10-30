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
    
    type CompilationLine =
      {
      ShortFileName:string
      FullFileName:string
      CompilationUnitNumber:int
      LineNumber:int 
      LineType:EasyAMLineTypes
      LineText:string
      TextStartColumn:int
      }
    type CompilationStream = CompilationLine []


    val Compile:CompilationUnitType[]->CompilationResultType
    val translateIncomingIntoOneStream:CompilationUnitType[]->CompilationStream
    val lineIdentification:CompilationStream->CompilationStream

