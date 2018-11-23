  namespace EA

    type EasyAMLineTypes =
      |Unprocessed
      |FileBegin
      |FileEnd
      |NewSectionItem
      |NewJoinedItem
      |CompilerJoinDirective
      |NewItemJoinCombination
      |CompilerNamespaceDirectiveWithItem
      |PoundTagWithItem
      |MentionTagWithItem
      |NameValueTagWithItem
      |CompilerSectionDirectiveWithItem
      |CompilerJoinTypeWithItem
      |CompilerNamespaceDirective
      |CompilerTagReset
      |CompilerSectionDirective
      |FreeFormText
      |EmptyLine
      interface System.IFormattable

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