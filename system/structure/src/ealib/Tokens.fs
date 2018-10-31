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

    //open System.Web.Services.Description
    //open System.Windows.Forms

    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Core"; "Tokens"; "EALib"; "Tokens" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger
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

    let (|CaseSensitive|_|) (str:string) arg = 
      if String.Compare(str, arg, StringComparison.Ordinal) = 0
        then Some() else Option<_>.None
    let (|CaseInsensitive|_|) (str:string) arg = 
      if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
        then Some() else Option<_>.None

    // the command type sorts the string generally regardless of context
    // once some kind of running context is established, we get the line type
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
      static member ToList()=
        [
        NewSectionItem
        ;NewJoinedItem
        //newItem
        ;CompilerJoinDirective //11
        ;NewItemJoinCombination //1
        ;CompilerNamespaceDirectiveWithItem //4
        ;CompilerTagDirectiveWithItem //5
        ;CompilerSectionDirectiveWithItem //6
        ;CompilerJoinTypeWithItem //7
        ;CompilerNamespaceDirective //8
        ;CompilerTagDirective //9
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
        //let stringChange:string->string = match ignoreCase with |Default|IgnoreCase->(fun x -> x.ToUpper()) | CaseSensitive->fun x -> x
        match stringValue with 
          |CaseMatch "Unprocessed"->EasyAMLineTypes.Unprocessed
          |CaseMatch "FileBegin"->EasyAMLineTypes.FileBegin
          |CaseMatch "FileEnd"->EasyAMLineTypes.FileEnd
          |CaseMatch "NewSectionItem"->NewSectionItem
          |CaseMatch "NewJoinedItem"->NewJoinedItem
          //|CaseMatch "NewItem"->EasyAMLineTypes.NewItem // new sectionItem or new JoinedItem
          |CaseMatch "NewItemJoinCombination"->EasyAMLineTypes.NewItemJoinCombination
          |CaseMatch "CompilerNamespaceDirectiveWithItem"->EasyAMLineTypes.CompilerNamespaceDirectiveWithItem
          |CaseMatch "CompilerTagDirectiveWithItem"->EasyAMLineTypes.CompilerTagDirectiveWithItem
          |CaseMatch "CompilerSectionDirectiveWithItem"->EasyAMLineTypes.CompilerSectionDirectiveWithItem
          |CaseMatch "CompilerJoinTypeWithItem"->EasyAMLineTypes.CompilerJoinTypeWithItem
          |CaseMatch "CompilerNamespaceDirective"->EasyAMLineTypes.CompilerNamespaceDirective
          |CaseMatch "CompilerTagDirective"->EasyAMLineTypes.CompilerTagDirective
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
          //|NewItem->"NewItem" // handles both newSectionItem and newJoinedItem, depending on context and indent
          |NewItemJoinCombination->"NewItemJoinCombination"
          |CompilerNamespaceDirectiveWithItem->"CompilerNamespaceDirectiveWithItem"
          |CompilerTagDirectiveWithItem->"CompilerTagDirectiveWithItem"
          |CompilerSectionDirectiveWithItem->"CompilerSectionDirectiveWithItem"
          |CompilerJoinTypeWithItem->"CompilerJoinTypeWithItem"
          |CompilerNamespaceDirective->"CompilerNamespaceDirective"
          |CompilerTagDirective->"CompilerTagDirective"
          |CompilerSectionDirective->"CompilerSectionDirective"
          |CompilerJoinDirective->"CompilerJoinDirective"
          |FreeFormText->"FreeFormText"
          |EmptyLine->"EmptyLine"
          |Unprocessed->"Unprocessed"
          |FileBegin->"FileBegin"
          |FileEnd->"FileEnd"
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
              {Regex=new Regex("^(.*|^(?=TO-DO)*)(TO-DOS?:?)(.*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem] }    //1
              ;{Regex=new Regex("^(.*|^(?=QUESTION)*)(QUESTIONS?:?)(.*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem] }    //2
              ;{Regex=new Regex("^(.*|^(?=PARENT)*)(PARENTS?:?)(.*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem] }    //2
              ;{Regex=new Regex("^(.*|^(?=CHILD)*)(CHILDS?:?)(.*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem] }    //3
              ;{Regex=new Regex("^(.*|^(?=CHILDREN)*)(CHILDREN?:?)(.*)$"); CaptureGroupsExpected=3; PossibleLineTypes=[CompilerJoinDirective; CompilerJoinTypeWithItem] }    //4
            |];
            }
        //{ LineType=
        //    SectionDirective; 
        //  MatchTokens=
        //    [|
        //      new Regex("^\s*USER STORIES[:]?[:space:]*$")
        //      ;new Regex("^\s*SPRINT BACKLOG[:]?[:space:]*$")
        //      ;new Regex("^\s*PRODUCT BACKLOG[:]?[:space:]*$")
        //      ;new Regex("^\s*PROJECT BACKLOG[:]?[:space:]*$")
        //    |];
        //    };
        //{ LineType=
        //    Namespace; 
        //  MatchTokens=
        //    [|
        //      new Regex("^\s*NAMESPACE[S]?[:]?[:space:]*$")
        //    |];
        //    };
        //{ LineType=
        //    Tag; 
        //  MatchTokens=
        //    [|
        //      new Regex("^\s*USER STORIES[:]?[:space:]*$")
        //    |];
        //    };
        { LineType=
            EasyAMCommandType.EmptyLine; 
          MatchTokens=
            [|
              {Regex=new Regex("^\s*$"); CaptureGroupsExpected=0; PossibleLineTypes=[EmptyLine] }
            |];
            };
        { LineType=
            None; // There's something, but no command. Must be freeform text
          MatchTokens=
            [|
              {Regex=new Regex("(.?)"); CaptureGroupsExpected=1; PossibleLineTypes=[FreeFormText] }
            |];
            };
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
      | THE_DORK
      | THE_AUTOMATRON
      | THE_WEASEL
      | THE_FACE
      | THE_ASSHOLE
      | THE_DREAMER
      | THE_CODEPENDENT
      | THE_SELF_HELP_GURU
      | THE_EXAMPLE
      | THE_TEACHER
      | THE_PROPHET
      | THE_EVANGELIST

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
