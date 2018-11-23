  namespace EA 
    open System
    type CaseSensitive=Default|IgnoreCase|CaseSensitive
    // Not the same as line types. Commands are what we search for. The line type depends on the context
    type EasyAMCommandType =
      |NewItem
      |Join
      |Namespace
      |Tag
      |SectionDirective
      |EmptyLine
      |None
      static member ToList()=
        [NewItem;Join;Namespace;Tag;SectionDirective;EmptyLine;None]

    //let (|CaseSensitive|_|) (str:string) arg = 
    //  if String.Compare(str, arg, StringComparison.Ordinal) = 0
    //    then Some() else Option<_>.None
    //let (|CaseInsensitive|_|) (str:string) arg = 
    //  if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
    //    then Some() else Option<_>.None

    // the command type sorts the string generally regardless of context
    // once some kind of running context is established, we get the line type
    type EasyAMLineTypes =
      |Unprocessed //14 - if parsing fails it defaults here
      |FileBegin
      |FileEnd
      |NewSectionItem
      |NewJoinedItem
      |CompilerJoinDirective //11
      |NewItemJoinCombination //1
      |CompilerNamespaceDirectiveWithItem //4
      |PoundTagWithItem //5
      |MentionTagWithItem //6
      |NameValueTagWithItem //5
      |CompilerSectionDirectiveWithItem //6
      |CompilerJoinTypeWithItem //7
      |CompilerNamespaceDirective //8
      |CompilerTagReset
      |CompilerSectionDirective //10
      |FreeFormText //12
      |EmptyLine //13
      static member ToList()=
        [
        NewSectionItem
        ;NewJoinedItem
        ;CompilerJoinDirective //11
        ;NewItemJoinCombination //1
        ;CompilerNamespaceDirectiveWithItem //4
        ;PoundTagWithItem
        ;MentionTagWithItem
        ;NameValueTagWithItem
        ;CompilerSectionDirectiveWithItem //6
        ;CompilerJoinTypeWithItem //7
        ;CompilerNamespaceDirective //8
        ;CompilerTagReset
        ;CompilerSectionDirective //10
        ;FreeFormText //12
        ;EmptyLine //13
        ;Unprocessed //14 - if parsing fails it defaults here
        ;FileBegin
        ;FileEnd
        ]
      static member TryParse (stringValue:string) (ignoreCase:CaseSensitive)= // I always do this for enum/DU types I plan to keep around, provide back-and-forth parsing
        let (|CaseMatch|_|) (str:string) arg =
          if ignoreCase=CaseSensitive || ignoreCase=Default
          then
            if String.Compare(str, arg, StringComparison.Ordinal) = 0
              then Some() else Option<_>.None
          else 
            if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
              then Some() else Option<_>.None

        match stringValue with 
          |CaseMatch "Unprocessed"->EasyAMLineTypes.Unprocessed
          |CaseMatch "FileBegin"->EasyAMLineTypes.FileBegin
          |CaseMatch "FileEnd"->EasyAMLineTypes.FileEnd
          |CaseMatch "NewSectionItem"->NewSectionItem
          |CaseMatch "NewJoinedItem"->NewJoinedItem
          |CaseMatch "NewItemJoinCombination"->EasyAMLineTypes.NewItemJoinCombination
          |CaseMatch "CompilerNamespaceDirectiveWithItem"->EasyAMLineTypes.CompilerNamespaceDirectiveWithItem
          |CaseMatch "PoundTagWithItem"->EasyAMLineTypes.PoundTagWithItem
          |CaseMatch "MentionTagWithItem"->EasyAMLineTypes.MentionTagWithItem
          |CaseMatch "NameValueTagWithItem"->EasyAMLineTypes.NameValueTagWithItem
          |CaseMatch "CompilerSectionDirectiveWithItem"->EasyAMLineTypes.CompilerSectionDirectiveWithItem
          |CaseMatch "CompilerJoinTypeWithItem"->EasyAMLineTypes.CompilerJoinTypeWithItem
          |CaseMatch "CompilerNamespaceDirective"->EasyAMLineTypes.CompilerNamespaceDirective
          |CaseMatch "CompilerTagReset"->EasyAMLineTypes.CompilerTagReset
          |CaseMatch "CompilerSectionDirective"->EasyAMLineTypes.CompilerSectionDirective
          |CaseMatch "CompilerJoinDirective"->EasyAMLineTypes.CompilerJoinDirective
          |CaseMatch "FreeFormText"->EasyAMLineTypes.FreeFormText
          |CaseMatch "EmptyLine"->EasyAMLineTypes.EmptyLine
          |CaseMatch "Unprocessed"->EasyAMLineTypes.Unprocessed
          |CaseMatch "FileBegin"->EasyAMLineTypes.FileBegin
          |CaseMatch "FileEnd"->EasyAMLineTypes.FileEnd
          |_->Unprocessed
      member self.MatchToString=
        match self with 
          |Unprocessed->"Unprocessed"
          |FileBegin->"FileBegin"
          |FileEnd->"FileEnd"
          |NewSectionItem->"NewSectionItem"
          |NewJoinedItem->"NewJoinedItem"
          |NewItemJoinCombination->"NewItemJoinCombination"
          |CompilerNamespaceDirectiveWithItem->"CompilerNamespaceDirectiveWithItem"
          |PoundTagWithItem->"PoundTagWithItem"
          |MentionTagWithItem->"MentionTagWithItem"
          |NameValueTagWithItem->"NameValueTagWithItem"
          |CompilerSectionDirectiveWithItem->"CompilerSectionDirectiveWithItem"
          |CompilerJoinTypeWithItem->"CompilerJoinTypeWithItem"
          |CompilerNamespaceDirective->"CompilerNamespaceDirective"
          |CompilerTagReset->"CompilerTagReset"
          |CompilerSectionDirective->"CompilerSectionDirective"
          |CompilerJoinDirective->"CompilerJoinDirective"
          |FreeFormText->"FreeFormText"
          |EmptyLine->"EmptyLine"

      override self.ToString() = self.MatchToString
      interface IFormattable with
        member self.ToString(format, formatProvider) = 
          self.MatchToString.ToString()
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
