namespace EA.Core
  module Util=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Core.Tokens

    // THE HEART OF THE CODE. COMPILER RULES
    //type EasyAMLineTypes =
    //    |Unprocessed
    //    |FileBegin
    //    |FileEnd
    //    |EmptyLine
    //    |FreeFormText
    //    |CompilerSectionDirective
    //    |CompilerNamespaceDirective
    //    |CompilerTagDirective
    //    |CompilerSectionDirectiveWithItem
    //    |CompilerNamespaceDirectiveWithItem
    //    |CompilerTagDirectiveWithItem
    //    |CompilerJoinTypeWithItem
    //    |NewSectionItem
    //    |NewJoinedItem
    //    |NewItemJoinCombination
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

    type CompilationUnitType = {
      Info:System.IO.FileInfo
      FileContents:string[]
      }
    type CompilationResultType = {
      MasterModelText:string[]
      }
