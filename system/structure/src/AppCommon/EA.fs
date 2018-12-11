  namespace EA 
    open System


    // the command type sorts the string generally regardless of context
    // once some kind of running context is established, we get the line type
    type CaseSensitive=Default|IgnoreCase|CaseSensitive
    
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
    // Couldn't decide for separate types or all Compilation Line
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
      with
      // This linetype/command/matcher crap is getting mixed up too easily. name better!
      member self.LineTypeDescription =
        match self.Type with
        |CommandMatch cm->cm.LineType.ToString()
        |LineType lt->lt.ToString()
      member self.ToFileLocation =
        self.CompilationUnitNumber.ToString().PadLeft(3,'0') + ":"
        + self.LineNumber.ToString().PadLeft(4,'0')
      member self.ToFullLocation =
        self.ShortFileName.PadLeft(System.Math.Min(32,self.ShortFileName.Length)) + ":" 
        + self.ToFileLocation
    
    type CompilationStream = CompilationLine []

    type CompilationUnitType = {
      Info:System.IO.FileInfo
      FileContents:string[]
      }







    type BucketWorkType =
      | DirectDecideAndChangeReality
      | EmergentDiscoverRealityAndDecideStrategy
    type BucketName =
      | Manage
      | Understand 
      | Execute 
      | Instantiate 
      | Deliver 
      | Optimize 
      | Plan 
      | Guess
    type BucketCategoryType =
      | ServicesOffered // What's our service/product DO that people want?
      | Values // What are our values and how do they relate to our work?
      | ServicesOfferedAndValues // The critical areas where these two come into conflict
    type BucketFocusType =
      | ChangeMentalModelBasedOnInteractions
      | ChangeInteractionsBasedOnMentalModel
      | AdjustHowMentalModelAndInteractionsWorkTogether
    // System.Drawing not available yet in dotnet. Let's fake it out.
    //type BucketColorType = System.Drawing.Color
    type BucketColorType =
      | RGB of (int8*int8*int8)
      | HexColor of string
    type BucketType =
      {
        Name:BucketName
        Category:BucketCategoryType
        Focus: BucketFocusType
        WorkType:BucketWorkType
        BucketColor:BucketColorType
      }
    

    type BucketMetaTypeName =
      | Aim
      | Supplementals
      | Target
      | Service
    type BucketMetaTypeDrivers =
      {
        MakeItHappenEasy:BucketType
        MakeItHappen:BucketType
        MakeItHappenRight:BucketType
      }
    type BucketMetaType =
      {
        Name: BucketMetaTypeName
        Drivers: BucketMetaTypeDrivers
      }
    // May need to add roles in at some point
    type RoleSillyNameType =
      | TheDork
      | TheAutomatron
      | TheWeasel
      | TheFace
      | TheAsshole
      | TheDreamer
      | TheCodependent
      | TheSelfHelpGuru
      | TheExample
      | TheTeacher
      | TheProphet
      | TheEvangelist

    type RoleMetaType =
      | WorkForTheUsers
      | WorkForTheTeams
      | WorkForEverybody

    type TemporalIndicatorType =
      | Was
      | AsIs
      | ToBe 
    type GenreType =
      | Meta
      | Business
      | System
    type AbstractionLevelType =
      | Abstract
      | Realized

    type LocationPointerType =
      {
        Genre:GenreType
        AbstractionLevel:AbstractionLevelType
        Bucket: BucketType
        TemporaalIndicator:TemporalIndicatorType
        Tags:string[]
        Namespace:string[]
      }








    type ModelItemType =
      | Item 
      | Join of (int*int)
    type ModelItem =
      {
      Id:int 
      Type:ModelItemType
      Parent:int
      Location:LocationPointerType 
      Description:string 
      References:string 
      }


