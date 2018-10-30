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
    open Logary // needed at bottom to give right "Level" lookup for logging
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Core"; "Util"; "EALib"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger


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
    
    let makeRule (ruleName) (nextLinesAllowed:EasyAMLineTypes list) = (ruleName)(AllowedNextLines(nextLinesAllowed))
    let fileBeginRule = makeRule FileBeginType [FileEnd; EmptyLine; FreeFormText; CompilerSectionDirective; NewItem]
    let fileEndRule = makeRule FileEndType []
    let emptyLineRule= makeRule EmptyLineType [FileEnd; EmptyLine;FreeFormText;CompilerSectionDirective;NewItem]
    let freeformTextRule= makeRule FreeFormTextType [FileEnd; EmptyLine; FreeFormText; CompilerSectionDirective; NewItem]
    let compilerSectionDirectiveRule = makeRule CompilerSectionDirectiveType [FileEnd; EmptyLine; FreeFormText; CompilerSectionDirective; NewItem]
    let newSectionItemRule = makeRule NewSectionItemType [FileEnd; EmptyLine; FreeFormText; CompilerSectionDirective; NewItem]

    let CompilerRules =
        [
            fileBeginRule
            ;fileEndRule
            ;emptyLineRule
            ;freeformTextRule
            ;compilerSectionDirectiveRule
            ;newSectionItemRule
        ]
    let isThisLineAllowedNext (previousLineType:EasyAMLineTypes) (lineTypeToTest:EasyAMLineTypes)=
      let rulesThatApply=
        CompilerRules|>List.filter(fun x->
          match x with 
            | previousLineType->true 
            |_-> false
          ) 

      rulesThatApply |> List.exists (fun x->
        match x with
          | currentLineType->true
          |_->false
        )

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

    type CompilationUnitType = {
      Info:System.IO.FileInfo
      FileContents:string[]
      }
    type CompilationResultType = {
      MasterModelText:string[]
      }

    logEvent Verbose "....Module exit" moduleLogger



//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
