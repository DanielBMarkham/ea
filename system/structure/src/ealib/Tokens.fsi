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
      |NewItem //2  // handles both newSectionItem and newJoinedItem, depending on context and indent
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
        

    type LineMatcherType = 
      {
        LineType:EasyAMLineTypes;
        RegexesThatMatch:System.Text.RegularExpressions.Regex []
      }
    val findFirstLineTypeMatch:string->LineMatcherType 

