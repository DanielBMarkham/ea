namespace EA.Core
  module Tokens=
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
            EasyAMCommandType.Join;  // handles newJoinSection and item join item
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

    type EasyAMLineTypeToRegexMatchingType =
      {
      LineType:EasyAMLineTypes
      RegexMatchingArray:Regex[]
      }




    logEvent Verbose "....Module exit" moduleLogger



//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
