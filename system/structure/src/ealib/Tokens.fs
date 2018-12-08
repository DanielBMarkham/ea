namespace EA.Core
  module Tokens=
    open EA
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open Logary // needed at bottom to give right "Level" lookup for logging
    //open System.Linq
    open System.Text.RegularExpressions
    open System.Collections.Concurrent

    //open System.Web.Services.Description
    //open System.Windows.Forms

    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Core"; "Tokens"; "EALib"; "Tokens" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger
    //type CaseSensitive=Default|IgnoreCase|CaseSensitive
    //// Not the same as line types. Commands are what we search for. The line type depends on the context
    //type EasyAMCommandType =
    //  |NewItem
    //  |Join
    //  |Namespace
    //  |Tag
    //  |SectionDirective
    //  |EmptyLine
    //  |None
    //  static member ToList()=
    //    [NewItem;Join;Namespace;Tag;SectionDirective;EmptyLine;None]

    //let (|CaseSensitive|_|) (str:string) arg = 
    //  if String.Compare(str, arg, StringComparison.Ordinal) = 0
    //    then Some() else Option<_>.None
    //let (|CaseInsensitive|_|) (str:string) arg = 
    //  if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
    //    then Some() else Option<_>.None

    //// the command type sorts the string generally regardless of context
    //// once some kind of running context is established, we get the line type
    //type EasyAMLineTypes =
    //  |Unprocessed //14 - if parsing fails it defaults here
    //  |FileBegin
    //  |FileEnd
    //  |NewSectionItem
    //  |NewJoinedItem
    //  |CompilerJoinDirective //11
    //  |NewItemJoinCombination //1
    //  |CompilerNamespaceDirectiveWithItem //4
    //  |PoundTagWithItem //5
    //  |MentionTagWithItem //6
    //  |NameValueTagWithItem //5
    //  |CompilerSectionDirectiveWithItem //6
    //  |CompilerJoinTypeWithItem //7
    //  |CompilerNamespaceDirective //8
    //  |CompilerTagReset
    //  |CompilerSectionDirective //10
    //  |FreeFormText //12
    //  |EmptyLine //13
    //  static member ToList()=
    //    [
    //    NewSectionItem
    //    ;NewJoinedItem
    //    ;CompilerJoinDirective //11
    //    ;NewItemJoinCombination //1
    //    ;CompilerNamespaceDirectiveWithItem //4
    //    ;PoundTagWithItem
    //    ;MentionTagWithItem
    //    ;NameValueTagWithItem
    //    ;CompilerSectionDirectiveWithItem //6
    //    ;CompilerJoinTypeWithItem //7
    //    ;CompilerNamespaceDirective //8
    //    ;CompilerTagReset
    //    ;CompilerSectionDirective //10
    //    ;FreeFormText //12
    //    ;EmptyLine //13
    //    ;Unprocessed //14 - if parsing fails it defaults here
    //    ;FileBegin
    //    ;FileEnd
    //    ]
    //  static member TryParse (stringValue:string) (ignoreCase:CaseSensitive)= // I always do this for enum/DU types I plan to keep around, provide back-and-forth parsing
    //    let (|CaseMatch|_|) (str:string) arg =
    //      if ignoreCase=CaseSensitive || ignoreCase=Default
    //      then
    //        if String.Compare(str, arg, StringComparison.Ordinal) = 0
    //          then Some() else Option<_>.None
    //      else 
    //        if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
    //          then Some() else Option<_>.None

    //    match stringValue with 
    //      |CaseMatch "Unprocessed"->EasyAMLineTypes.Unprocessed
    //      |CaseMatch "FileBegin"->EasyAMLineTypes.FileBegin
    //      |CaseMatch "FileEnd"->EasyAMLineTypes.FileEnd
    //      |CaseMatch "NewSectionItem"->NewSectionItem
    //      |CaseMatch "NewJoinedItem"->NewJoinedItem
    //      |CaseMatch "NewItemJoinCombination"->EasyAMLineTypes.NewItemJoinCombination
    //      |CaseMatch "CompilerNamespaceDirectiveWithItem"->EasyAMLineTypes.CompilerNamespaceDirectiveWithItem
    //      |CaseMatch "PoundTagWithItem"->EasyAMLineTypes.PoundTagWithItem
    //      |CaseMatch "MentionTagWithItem"->EasyAMLineTypes.MentionTagWithItem
    //      |CaseMatch "NameValueTagWithItem"->EasyAMLineTypes.NameValueTagWithItem
    //      |CaseMatch "CompilerSectionDirectiveWithItem"->EasyAMLineTypes.CompilerSectionDirectiveWithItem
    //      |CaseMatch "CompilerJoinTypeWithItem"->EasyAMLineTypes.CompilerJoinTypeWithItem
    //      |CaseMatch "CompilerNamespaceDirective"->EasyAMLineTypes.CompilerNamespaceDirective
    //      |CaseMatch "CompilerTagReset"->EasyAMLineTypes.CompilerTagReset
    //      |CaseMatch "CompilerSectionDirective"->EasyAMLineTypes.CompilerSectionDirective
    //      |CaseMatch "CompilerJoinDirective"->EasyAMLineTypes.CompilerJoinDirective
    //      |CaseMatch "FreeFormText"->EasyAMLineTypes.FreeFormText
    //      |CaseMatch "EmptyLine"->EasyAMLineTypes.EmptyLine
    //      |CaseMatch "Unprocessed"->EasyAMLineTypes.Unprocessed
    //      |CaseMatch "FileBegin"->EasyAMLineTypes.FileBegin
    //      |CaseMatch "FileEnd"->EasyAMLineTypes.FileEnd
    //      |_->Unprocessed
    //  member self.MatchToString=
    //    match self with 
    //      |Unprocessed->"Unprocessed"
    //      |FileBegin->"FileBegin"
    //      |FileEnd->"FileEnd"
    //      |NewSectionItem->"NewSectionItem"
    //      |NewJoinedItem->"NewJoinedItem"
    //      |NewItemJoinCombination->"NewItemJoinCombination"
    //      |CompilerNamespaceDirectiveWithItem->"CompilerNamespaceDirectiveWithItem"
    //      |PoundTagWithItem->"PoundTagWithItem"
    //      |MentionTagWithItem->"MentionTagWithItem"
    //      |NameValueTagWithItem->"NameValueTagWithItem"
    //      |CompilerSectionDirectiveWithItem->"CompilerSectionDirectiveWithItem"
    //      |CompilerJoinTypeWithItem->"CompilerJoinTypeWithItem"
    //      |CompilerNamespaceDirective->"CompilerNamespaceDirective"
    //      |CompilerTagReset->"CompilerTagReset"
    //      |CompilerSectionDirective->"CompilerSectionDirective"
    //      |CompilerJoinDirective->"CompilerJoinDirective"
    //      |FreeFormText->"FreeFormText"
    //      |EmptyLine->"EmptyLine"

    //  override self.ToString() = self.MatchToString
    //  interface IFormattable with
    //    member self.ToString(format, formatProvider) = 
    //      self.MatchToString.ToString()
    //type RegexMatcherType = 
    //  {
    //    Regex:System.Text.RegularExpressions.Regex
    //    CaptureGroupsExpected:int
    //    PossibleLineTypes:EasyAMLineTypes list
    //  }
    //type LineMatcherType = 
    //  {
    //    LineType:EasyAMCommandType;
    //    MatchTokens:RegexMatcherType []
    //  }

    //[NewItem;Join;Namespace;Tag;SectionDirective;EmptyLine;None]

    // The regex has to have capture groups that correspond to the number of parms expected from line
    let GeneGeneTheLineMatchingMachine =
      [|
        { LineType=
            EasyAMCommandType.NewItem; // also handles NewJoinedItem dep
          MatchTokens=
            [|
              {Regex=new Regex("^\s*[-|*][ ]([\w][\w]?.*)$"); CaptureGroupsExpected=1; PossibleLineTypes=[NewSectionItem; NewJoinedItem] }             //1
              ;{Regex=new Regex("^\s*\[.\][ ]([\w][\w]?.*)$"); CaptureGroupsExpected=1; PossibleLineTypes=[NewSectionItem; NewJoinedItem] }            //2
              ;{Regex=new Regex("^\s*[0-9]+[\.|\)][ ]([\w][\w]?.*)$"); CaptureGroupsExpected=1; PossibleLineTypes=[NewSectionItem; NewJoinedItem] }    //3
              ;{Regex=new Regex("^\s*[A-Za-z]+[\.|\)][ ]([\w][\w]?.*)$"); CaptureGroupsExpected=1; PossibleLineTypes=[NewSectionItem; NewJoinedItem] } //4
              ;{Regex=new Regex("^\s*[M|m]{0,4}([C|c][M|m]|[C|c][D|d]|[D|d]?[C|c]{0,3})([X|x][C|c]|[X|x][L|l]|[L|l]?[X|x]{0,3})([I|i][X|x]|[I|i][V|v]|[V|v]?[I|i]{0,3})[\.|\)][ ]([\w][\w]?.*)$"); CaptureGroupsExpected=1; PossibleLineTypes=[NewSectionItem; NewJoinedItem] } //5
            |];
            };
        { LineType=
            Join;  // handles newJoinSection and item join item
          MatchTokens=
            [|
              {Regex=new Regex("^([^(-]*)(TO-DO(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //1
              ;{Regex=new Regex("^([^(-]*)(QUESTION(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //2
              ;{Regex=new Regex("^([^(-]*)(NOTE(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //2
              ;{Regex=new Regex("^([^(-]*)(PARENT(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //2
              ;{Regex=new Regex("^([^(-]*)(CHILDREN(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
              ;{Regex=new Regex("^([^(-]*)(CHILD(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //3
              ;{Regex=new Regex("^([^(-]*)(BECAUSE(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
              ;{Regex=new Regex("^([^(-]*)(REASON(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
              ;{Regex=new Regex("^([^(-]*)(WHENEVER(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
              ;{Regex=new Regex("^([^(-]*)(ITHASTOBETHAT(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
              ;{Regex=new Regex("^([^(-]*)(ASA(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
              ;{Regex=new Regex("^([^(-]*)(WHEN(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
              ;{Regex=new Regex("^([^(-]*)(INEEDTO(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
              ;{Regex=new Regex("^([^(-]*)(SOTHAT(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
              ;{Regex=new Regex("^([^(-]*)(OUTCOME(?:S?:?))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem; NewItemJoinCombination] }    //4
            |];
            }
        { LineType=
            Namespace; 
          MatchTokens=
            [| // namespace regex is a little different must have = or space after keyword
              {Regex=new Regex("^([^(-]*)(NAMESPACE(?:S?:?[=|\s]))(?-)(|[^-].*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerNamespaceDirective; CompilerNamespaceDirectiveWithItem] }    //1
            |];
            }
        { LineType=
            Tag; 
          MatchTokens=
            [|
              // reset ahead of everything else 
              {Regex=new Regex("\B#@\W"); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerTagReset] }    //2
              // # tags, both with quoted complex string and single words
              // quote versions always come first in matching order
              ;{Regex=new Regex("(?:(?:\B&\")(.+)(?:\"=\"))(.+)(?:\")"); CaptureGroupsExpected=2; PossibleLineTypes=[NameValueTagWithItem] }    //6
              ;{Regex=new Regex("(?:(?:\B#)(?:\")(.+)(?:\")\B)"); CaptureGroupsExpected=1; PossibleLineTypes=[PoundTagWithItem] }    //1
              ;{Regex=new Regex("(?:(?:\B@)(\w+))"); CaptureGroupsExpected=1; PossibleLineTypes=[MentionTagWithItem] }    //4
              ;{Regex=new Regex("(?:(?:\B#)(\w+))"); CaptureGroupsExpected=1; PossibleLineTypes=[PoundTagWithItem] }    //3
              ;{Regex=new Regex("(?:(?:@)\"([^\"]+)\"\s?)"); CaptureGroupsExpected=1; PossibleLineTypes=[MentionTagWithItem] }    //5
              ;{Regex=new Regex("(?:(?:\B&)(.+)=(.+))"); CaptureGroupsExpected=2; PossibleLineTypes=[NameValueTagWithItem] }    //7
            |];
            };
        { LineType=
            SectionDirective; 
          MatchTokens=
            [|
            // ARGHH. MUST TRIPLE QUOTE TO USE SAME REGEX ON WEB TESTING AS HERE
              {Regex=new Regex("""^(?:\s*)(USER STORIES)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //1
              ;{Regex=new Regex("""^(?:\s*)(SPRINT BACKLOG)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //2
              ;{Regex=new Regex("""^^(?:\s*)(PRODUCT BACKLOG)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //3
              ;{Regex=new Regex("""^(?:\s*)(PROJECT BACKLOG)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //4
              ;{Regex=new Regex("""^(?:\s*)(META SUPPLEMENTALS)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //5
              ;{Regex=new Regex("""^(?:\s*)(META BEHAVIOR)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //6
              ;{Regex=new Regex("""^(?:\s*)(BUSINESS BEHAVIOR)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //7
              ;{Regex=new Regex("""^(?:\s*)(BUSINESS SUPPLEMENTALS)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //8
              ;{Regex=new Regex("""^(?:\s*)(SYSTEM SUPPLEMENTALS)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //9
              ;{Regex=new Regex("""^(?:\s*)(SYSTEM BEHAVIOR)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //10
              ;{Regex=new Regex("""^(?:\s*)(SYSTEM ABSTRACT SUPPLEMENTALS)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //11
              ;{Regex=new Regex("""^(?:\s*)(BUSINESS ABSTRACT SUPPLEMENTALS)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //12
              ;{Regex=new Regex("""^(?:\s*)(BUSINESS ABSTRACT BEHAVIOR)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //13
              ;{Regex=new Regex("""^(?:\s*)(META ABSTRACT SUPPLEMENTALS)\b(?:.*)$"""); CaptureGroupsExpected=1; PossibleLineTypes=[CompilerSectionDirective] }    //13
            |];
            };
        { LineType=
            EasyAMCommandType.EmptyLine; 
          MatchTokens=
            [|
              {Regex=new Regex("^\s*$"); CaptureGroupsExpected=0; PossibleLineTypes=[EasyAMLineTypes.EmptyLine] }
            |];
            };
        { LineType=
            None; // There's something, but no command. Must be freeform text
          MatchTokens=
            [|
              {Regex=new Regex("(.?)"); CaptureGroupsExpected=1; PossibleLineTypes=[FreeFormText] }
            |];
            }
        // template to paste in later if needed
        //{ LineType=
        //    Namespace; 
        //  MatchTokens=
        //    [|
        //      {Regex=new Regex("^\s*NAMESPACE[S]?[:]=(.**)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem] }    //1
        //    |];
        //    }
      |]
    let getRegExesForACommand (commandType:EasyAMCommandType):RegexMatcherType[] =
      let firstMatchingItem=GeneGeneTheLineMatchingMachine |> Array.find(fun x->x.LineType=commandType)
      firstMatchingItem.MatchTokens
    let findFirstLineTypeMatch (line:string):RegexMatcherType =
      let parentItem = 
        GeneGeneTheLineMatchingMachine |> Array.find(fun x->
          let oneOfTheRegexesMatch = x.MatchTokens |> Array.exists(fun y->y.Regex.IsMatch(line))
          oneOfTheRegexesMatch
          )
      parentItem.MatchTokens |> Array.find(fun y->y.Regex.IsMatch(line))
    let findFirstCommandTypeMatch (line:string):LineMatcherType =
      GeneGeneTheLineMatchingMachine |> Array.find(fun x->
        let oneOfTheRegexesMatch = x.MatchTokens |> Array.exists(fun y->y.Regex.IsMatch(line))
        oneOfTheRegexesMatch
        )

    let matchLineWithRecommendedCommand (line:string):LineMatcherType =
      let ret = GeneGeneTheLineMatchingMachine |> Array.find(fun x->
        let oneOfTheRegexesMatch = x.MatchTokens |> Array.exists(fun y->y.Regex.IsMatch(line))
        oneOfTheRegexesMatch
        )
      let matchingRegexes = ret.MatchTokens |> Array.filter(fun x->x.Regex.IsMatch(line))
      {
        LineType=ret.LineType
        MatchTokens=matchingRegexes
      }
    //
    //
    // PLAEHOLDER FOR ADDING ROLE INFORMATION IF DESIRED LATER
    //
    //
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
    
    let ManageBucket      = {Name=Manage;       Category=ServicesOfferedAndValues;  Focus=ChangeMentalModelBasedOnInteractions;               WorkType=EmergentDiscoverRealityAndDecideStrategy;    BucketColor=HexColor("ffffff")}
    let UnderstandBucket  = {Name=Understand;   Category=Values;                    Focus=ChangeMentalModelBasedOnInteractions;               WorkType=DirectDecideAndChangeReality;                BucketColor=HexColor("ffffff")}
    let ExecuteBucket     = {Name=Execute;      Category=Values;                    Focus=AdjustHowMentalModelAndInteractionsWorkTogether;    WorkType=EmergentDiscoverRealityAndDecideStrategy;    BucketColor=HexColor("ffffff")}
    let InstantiateBucket = {Name=Instantiate;  Category=Values;                    Focus=ChangeInteractionsBasedOnMentalModel;               WorkType=DirectDecideAndChangeReality;                BucketColor=HexColor("ffffff")}
    let DeliverBucket     = {Name=Deliver;      Category=ServicesOfferedAndValues;  Focus=ChangeInteractionsBasedOnMentalModel;               WorkType=EmergentDiscoverRealityAndDecideStrategy;    BucketColor=HexColor("ffffff")}
    let OptimizeBucket    = {Name=Optimize;     Category=ServicesOffered;           Focus=ChangeInteractionsBasedOnMentalModel;               WorkType=DirectDecideAndChangeReality;                BucketColor=HexColor("ffffff")}
    let PlanBucket        = {Name=Plan;         Category=ServicesOffered;           Focus=AdjustHowMentalModelAndInteractionsWorkTogether;    WorkType=EmergentDiscoverRealityAndDecideStrategy;    BucketColor=HexColor("ffffff")}
    let GuessBucket       = {Name=Guess;        Category=ServicesOffered;           Focus=ChangeMentalModelBasedOnInteractions;               WorkType=DirectDecideAndChangeReality;                BucketColor=HexColor("ffffff")}
    let BucketList =
      [
        ManageBucket;
        UnderstandBucket;
        ExecuteBucket;
        InstantiateBucket;
        DeliverBucket;
        OptimizeBucket;
        PlanBucket;
        GuessBucket
      ]
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
    let defaultLocationPointer =
      {
        Genre=Business
        AbstractionLevel=Abstract
        Bucket=PlanBucket
        TemporaalIndicator=ToBe
        Tags=[||]
        Namespace=[||]
      }

    type EasyAMLineTypeToRegexMatchingType =
      {
      LineType:EasyAMLineTypes
      RegexMatchingArray:Regex[]
      }


    /// Takes an array of Regexes and passes the first index in the string of the first match,
    /// if any. None if nothing matched. Regexes are matched in order
    //let doesLineHaveRegexInIt (regexArr:Regex []) (line:string): int option =
    //  let tryToFindOne=regexArr|>Array.tryFind(fun x->x.IsMatch(line))
    //  if tryToFindOne.IsSome
    //    then Some (tryToFindOne.Value.Matches(line).[0].Index)
    //    else None

      

    //type Buckets =
    //    | Unknown
    //    | None
    //    | Behavior
    //    | Structure
    //    | Supplemental
    //     static member ToList() =
    //        [Unknown;None;Behavior;Structure;Supplemental]
    //     override self.ToString() =
    //      match self with
    //        | Unknown->"Unknown"
    //        | None->"None"
    //        | Behavior->"Behavior"
    //        | Structure->"Structure"
    //        | Supplemental->"Supplemental"
    //     static member TryParse(stringToParse:string) =
    //        match stringToParse.ToUpper() with
    //            |"BEHAVIOR"|"BEHAVIOUR"->(true,Buckets.Behavior)
    //            |"STRUCTURE"->(true,Buckets.Structure)
    //            |"SUPPLEMENTAL"->(true,Buckets.Supplemental)
    //            |_->(false, Buckets.Unknown)
    //     static member Parse(stringToParse:string) =
    //        match stringToParse.ToUpper() with
    //            |"BEHAVIOR"|"BEHAVIOUR"->Buckets.Behavior
    //            |"STRUCTURE"->Buckets.Structure
    //            |"SUPPLEMENTAL"->Buckets.Supplemental
    //            |_->raise(new System.ArgumentOutOfRangeException("Buckets","The string value provided for Buckets is not in the Buckets enum"))




    logEvent Verbose "....Module exit" moduleLogger



//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
