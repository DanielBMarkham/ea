namespace EA.Core
  open System.Numerics
  module ModelTypes=
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

    type CaseSensitive=Default|IgnoreCase|CaseSensitive
    // typical DU type contains..
    type MyDUType =
      |Choice1
      |Choice2
      |Choice3
      static member ToList()=
        [
        Choice1
        ;Choice2
        ;Choice3
        ]
      static member TryParse (stringValue:string) (ignoreCase:CaseSensitive)= 
        let (|CaseMatch|_|) (str:string) arg =
          if ignoreCase=CaseSensitive || ignoreCase=Default
          then
            if String.Compare(str, arg, StringComparison.Ordinal) = 0
              then Some() else Option<_>.None
          else 
            if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
              then Some() else Option<_>.None
        match stringValue with 
          |CaseMatch "Choice1"->Choice1
          |CaseMatch "Choice2"->Choice2
          |CaseMatch "Choice3"->Choice3
          |_->Choice1
      member self.MatchToString=
        match self with 
          |Choice1->"Choice1"
          |Choice2->"Choice2"
          |Choice3->"Choice3"
      override self.ToString() = self.MatchToString
      interface IFormattable with
        member self.ToString(format, formatProvider) = 
          self.MatchToString.ToString()
    type Genres =
      |Business
      |System
      |Meta
      static member ToList()=
        [Business;System;Meta]
      static member TryParse (stringValue:string) (ignoreCase:CaseSensitive)=
        let (|CaseMatch|_|) (str:string) arg =
          if ignoreCase=CaseSensitive || ignoreCase=Default
          then
            if String.Compare(str, arg, StringComparison.Ordinal) = 0
              then Some() else Option<_>.None
          else
            if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
              then Some() else Option<_>.None
        match stringValue with
          |CaseMatch "Business"->Business
          |CaseMatch "System"->System
          |CaseMatch "Meta"->Meta
          |_->Business
      member self.MatchToString=
        match self with 
          |Business->"Business"
          |System->"System"
          |Meta->"Meta"
      override self.ToString() = self.MatchToString
      interface IFormattable with
        member self.ToString(format, formatProvider) = 
          self.MatchToString.ToString()

    type TemporalIndicators =
      |Was
      |AsIs
      |ToBe
      static member ToList()=
        [Was; AsIs; ToBe]
      static member TryParse (stringValue:string) (ignoreCase:CaseSensitive)=
        let (|CaseMatch|_|) (str:string) arg =
          if ignoreCase=CaseSensitive || ignoreCase=Default
          then
            if String.Compare(str, arg, StringComparison.Ordinal) = 0
              then Some() else Option<_>.None
          else
            if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
              then Some() else Option<_>.None
        match stringValue with
          |CaseMatch "Was"->Was
          |CaseMatch "As-Is"|"AsIs"->AsIs
          |CaseMatch "To-Be"|"ToBe"->ToBe
          |_->ToBe
      member self.MatchToString=
        match self with 
          |Was->"Was"
          |AsIs->"As-Is"
          |ToBe->"To-Be"
      override self.ToString() = self.MatchToString
      interface IFormattable with
        member self.ToString(format, formatProvider) = 
          self.MatchToString.ToString()

    type AbstractionLevels =
      |Abstract
      |Realized
      static member ToList()=
        [Abstract; Realized]
      static member TryParse (stringValue:string) (ignoreCase:CaseSensitive)=
        let (|CaseMatch|_|) (str:string) arg =
          if ignoreCase=CaseSensitive || ignoreCase=Default
          then
            if String.Compare(str, arg, StringComparison.Ordinal) = 0
              then Some() else Option<_>.None
          else
            if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
              then Some() else Option<_>.None
        match stringValue with
          |CaseMatch "Abstract"->Abstract
          |CaseMatch "Realized"->Realized
          |_->Abstract
      member self.MatchToString=
        match self with 
          |Abstract->"Abstract"
          |Realized->"Realized"
      override self.ToString() = self.MatchToString
      interface IFormattable with
        member self.ToString(format, formatProvider) = 
          self.MatchToString.ToString()

    type Buckets =
      |Manage
      |Understand
      |Execute
      |Instantiate 
      |Deliver 
      |Optimize
      |Plan 
      |Guess 

      static member ToList()=
        [Manage;Understand;Execute;Instantiate;Deliver;Optimize;Plan;Guess]
      static member TryParse (stringValue:string) (ignoreCase:CaseSensitive)=
        let (|CaseMatch|_|) (str:string) arg =
          if ignoreCase=CaseSensitive || ignoreCase=Default
          then
            if String.Compare(str, arg, StringComparison.Ordinal) = 0
              then Some() else Option<_>.None
          else
            if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
              then Some() else Option<_>.None
        match stringValue with
          |CaseMatch "Manage"->Manage
          |CaseMatch "Understand"|"Marketing"->Understand
          |CaseMatch "Execute"|"Values"|"Supplementals"->Execute
          |CaseMatch "Instantiate"|"Development"->Instantiate
          |CaseMatch "Deliver"|"Service"|"Product"->Deliver
          |CaseMatch "Optimize"|"Operations"->Optimize
          |CaseMatch "Plan"|"Backlog"|"Behavior"->Plan
          |CaseMatch "Guess"|"Abduction"|"DataSciences"->Guess
          |_->Plan
      member self.MatchToString=
        match self with 
          |Manage->"Manage"
          |Understand->"Understand"
          |Execute->"Execute"
          |Instantiate->"Instantiate"
          |Deliver->"Deliver"
          |Optimize->"Optimize"
          |Plan->"Plan"
          |Guess->"Guess"
      member self.ToFriendlyString()=
        match self with
          |Manage->"Manage"
          |Understand->"Marketing"
          |Execute->"Supplementals"
          |Instantiate->"Development"
          |Deliver->"Service"
          |Optimize->"Operations"
          |Plan->"Behavior"
          |Guess->"Abductions"

      override self.ToString() = self.MatchToString
      interface IFormattable with
        member self.ToString(format, formatProvider) = 
          self.MatchToString.ToString()

    type ModelLocation =
      {
        Bucket:Buckets
        Genre:Genres
        TemporalIndicator:TemporalIndicators
        AbstractionLevel:AbstractionLevels
        Title:string
      }
      override self.ToString() =
        let ti=if self.TemporalIndicator=ToBe then "" else self.TemporalIndicator.ToString()
        self.Genre.ToString() + " " + self.AbstractionLevel.ToString() + " " + self.Bucket.ToFriendlyString() + " " + ti + self.Title


    type TagTypes =
      |PoundTag of string
      |MentionTag of string
      |NameValueTag of string*string
      static member ToList():TagTypes list=
        let pt:TagTypes=PoundTag("")
        let mt=MentionTag("")
        let nvt=NameValueTag("","")
        [pt; mt; nvt]
      static member TryParse (stringValue:string) (ignoreCase:CaseSensitive)=
        let (|CaseMatch|_|) (str:string) arg =
          if ignoreCase=CaseSensitive || ignoreCase=Default
          then
            if String.Compare(str, arg, StringComparison.Ordinal) = 0
              then Some() else Option<_>.None
          else
            if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
              then Some() else Option<_>.None
        match stringValue with
          |CaseMatch "PoundTag"->PoundTag("")
          |CaseMatch "MentionTag"->MentionTag("")
          |CaseMatch "NameValueTag"->NameValueTag("","")
          |_->PoundTag("")
      member self.MatchToString=
        match self with
          |PoundTag x->"#\"" + (x) + "\""
          |MentionTag x->"@\"" + (x) + "\""
          |NameValueTag (x,y)->"&\"" + x + "\"=\"" + y + "\""
      override self.ToString() = self.MatchToString
      interface IFormattable with
        member self.ToString(format, formatProvider) = 
          self.MatchToString.ToString()

    // NOT IMPLEMENTED YET. IT'S A DAG LIKE EVERYTHING ELSE HERE
    type TagHierarchy = 
      {
        SourceTag:TagTypes 
        TargetTagTypes:TagTypes []
      }
    type CompilerLocation =
      {
        ModelLoc:ModelLocation
        Namespace:string list
        Tags:TagTypes list
      }
      override self.ToString()=
        let namespaceDesc = "NAMESPACE=" + String.Join("/", self.Namespace) + "\r\n"
        let tagDesc = "#@ " + String.Join(" ", self.Tags) + "\r\n"
        namespaceDesc + tagDesc + self.ModelLoc.ToString()

    type JoinTypes =
      | ParentChild
      | Trigger
      | Actor
      | Goal
      | BusinessContext
      | Scenario
      | Outcome
      | Contains
      | Because
      | Whenever
      | ItHasToBeThat
      | Note
      | Question
      | ToDo
      | Work
      | Diagram
      | Code
      | Defect
      | ReferenceLink
      static member ToList()=
        [ParentChild;Trigger;Actor;Goal;BusinessContext;Scenario;Outcome;Contains;Because;Whenever
        ;ItHasToBeThat; Note; Question; ToDo; Work; Diagram; Code; Defect; ReferenceLink]
      static member TryParse (stringValue:string) (ignoreCase:CaseSensitive)=
        let (|CaseMatch|_|) (str:string) arg =
          if ignoreCase=CaseSensitive || ignoreCase=Default
          then
            if String.Compare(str, arg, StringComparison.Ordinal) = 0
              then Some() else Option<_>.None
          else
            if String.Compare(str, arg, StringComparison.OrdinalIgnoreCase) = 0
              then Some() else Option<_>.None
        match stringValue with
          |CaseMatch "ParentChild"->ParentChild
          |CaseMatch "Trigger"->Trigger
          |CaseMatch "Actor"->Actor
          |CaseMatch "Goal"->Goal
          |CaseMatch "BusinessContext"->BusinessContext
          |CaseMatch "Scenario"->Scenario
          |CaseMatch "Outcome"->Outcome
          |CaseMatch "Contains"->Contains
          |CaseMatch "Because"->Because
          |CaseMatch "Whenever"->Whenever
          |CaseMatch "ItHasToBeThat"->ItHasToBeThat
          |CaseMatch "Note"->Note
          |CaseMatch "Question"->Question
          |CaseMatch "ToDo"->ToDo
          |CaseMatch "Work"->Work
          |CaseMatch "Diagram"->Diagram
          |CaseMatch "Code"->Code
          |CaseMatch "Defect"->Defect
          |CaseMatch "ReferenceLink"->ReferenceLink
          |_->ParentChild
      member self.MatchToString=
        match self with
        | ParentChild->"ParentChild"
        | Trigger->"Trigger"
        | Actor->"Actor"
        | Goal->"Goal"
        | BusinessContext->"BusinessContext"
        | Scenario->"Scenario"
        | Outcome->"Outcome"
        | Contains->"Contains"
        | Because->"Because"
        | Whenever->"Whenever"
        | ItHasToBeThat->"ItHasToBeThat"
        | Note->"Note"
        | Question->"Question"
        | ToDo->"ToDo"
        | Work->"Work"
        | Diagram->"Diagram"
        | Code->"Code"
        | Defect->"Defect"
        | ReferenceLink->"ReferenceLink"

      override self.ToString() = self.MatchToString
      interface IFormattable with
        member self.ToString(format, formatProvider) = 
          self.MatchToString.ToString()

    type ModelJoin =
      {
        JoinType: JoinTypes 
        FreeText: string 
      }
    type ModelRelationship =
      {
        SourceModelLoc:ModelLocation
        TargetModelLoc:ModelLocation option
        Joins:ModelJoin []
      }
