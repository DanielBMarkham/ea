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

    type EACompilationUnitType = {
      Info:System.IO.FileInfo
      FileContents:string[]
      }
    type EACompilationResultType = {
      MasterModelText:string[]
      }

    // THE HEART OF THE CODE. COMPILER RULES
    type EasyAMLineTypes =
        |FileBegin
        |FileEnd
        |EmptyLine
        |FreeFormText
        |CompilerSectionDirective
        |CompilerNamespaceDirective
        |CompilerTagDirective
        |CompilerSectionDirectiveWithItem
        |CompilerNamespaceDirectiveWithItem
        |CompilerTagDirectiveWithItem
        |CompilerJoinTypeWithItem
        |NewSectionItem
        |NewJoinedItem
        |NewItemJoinCombination
    type CompilerRuleType =
        |FileBeginType of AllowedNextLinesType
        |FileEndType of AllowedNextLinesType
        |EmptyLineType of AllowedNextLinesType
        |FreeFormTextType of AllowedNextLinesType
        |CompilerSectionDirectiveType of AllowedNextLinesType
        |CompilerNamespaceDirectiveType of AllowedNextLinesType
        |CompilerTagDirectiveType of AllowedNextLinesType
        |CompilerSectionDirectiveWithItemType of AllowedNextLinesType
        |CompilerNamespaceDirectiveWithItemType of AllowedNextLinesType
        |CompilerTagDirectiveWithItemType of AllowedNextLinesType
        |CompilerJoinTypeWithItemType of AllowedNextLinesType
        |NewSectionItemType of AllowedNextLinesType
        |NewJoinedItemType of AllowedNextLinesType
        |NewItemJoinCombinationType of AllowedNextLinesType
    and AllowedNextLinesType = AllowedNextLines of (EasyAMLineTypes list)

    
    val CompilerRules:CompilerRuleType list
    val isThisLineAllowedNext: EasyAMLineTypes->lineTypeToTest:EasyAMLineTypes->bool

