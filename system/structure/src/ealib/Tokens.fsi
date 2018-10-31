namespace EA.Core
  module Tokens=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open Logary // needed at bottom to give right "Level" lookup for logging
    //open System.Linq
    open System.Text.RegularExpressions
    open System.Collections.Concurrent


    type EasyAMLineTypes =
      |Unprocessed //14 - if parsing fails it defaults here
      |FileBegin
      |FileEnd
      |NewSectionItem
      |NewJoinedItem
      //|NewItem //2  // handles both newSectionItem and newJoinedItem, depending on context and indent
      |CompilerJoinDirective //11
      |NewItemJoinCombination //1
      |CompilerNamespaceDirectiveWithItem //4
      |CompilerTagDirectiveWithItem //5
      |CompilerSectionDirectiveWithItem //6
      |CompilerJoinTypeWithItem //7
      |CompilerNamespaceDirective //8
      |CompilerTagDirective //9
      |CompilerSectionDirective //10
      |FreeFormText //12
      |EmptyLine //13
        
    // Not the same as line types. Commands are what we search for. The line type depends on the context
    type EasyAMCommandType =
      |NewItem
      |Join
      |Namespace
      |Tag
      |SectionDirective
      |EmptyLine
      |None


    type RegexMatcherType = 
      {
        Regex:System.Text.RegularExpressions.Regex
        CaptureGroupsExpected:int
        PossibleLineTypes:EasyAMLineTypes list
      }
    type LineMatcherType = 
      {
        LineType:EasyAMCommandType;
        MatchTokens:RegexMatcherType []
      }
    val getRegExesForACommand:EasyAMCommandType->RegexMatcherType[]
    val findFirstLineTypeMatch:string->RegexMatcherType 
    val findFirstCommandTypeMatch:string->LineMatcherType 

