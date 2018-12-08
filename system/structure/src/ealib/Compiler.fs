namespace EA.Core
  module Compiler=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Core.Util
    open EA.Core.Tokens
    open Logary // needed at bottom to give right "Level" lookup for logging
    open System.Threading
    open System.Drawing
    open System.Drawing

    //open System.Web.Configuration

    //
    // THIS IS WHACK. WHAT TO DO WITH LOGGER STATIC VAR IN A SHARED DLL?
    //
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Core"; "Compiler"; "EALib"; "Compiler" |])
    let mutable clientLogBack:(LogLevel->string->unit) option=Option<LogLevel->string->unit>.None
    let logEventSig (lvl:LogLevel) (str:string) = logEvent lvl str moduleLogger
    // Decided on a mixed mode for now. Set one from client-side. Figure out what we have on this side
    let mutable logBack = logEventSig
    let setLogger (cl:LogLevel->string->unit):unit =
      logBack<-cl
      logBack Debug "Remote logger set"

    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logBack Verbose "Module enter...."

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

    // Maybe there is a better way of doing this. Just not today. (Or should this be the start of the lenses...)
    // Discussion of the choice of type-methods, functions, or lenses based on project direction
    let isCompilationLineACommand (line:CompilationLine) =
      match line.Type with |CommandMatch ct->true |_->false
    let isCompilationLineALineType (line:CompilationLine) =
      match line.Type with |LineType lt->true |_->false
    let isCompilationLineAFileBegin(line:CompilationLine):bool=
      match line.Type with |LineType(EasyAMLineTypes.FileBegin)->true |_->false
    let isCompilationLineAFileEnd(line:CompilationLine):bool=
      match line.Type with |LineType(EasyAMLineTypes.FileEnd)->true |_->false
    let isCompilationLineAFileMarker (line:CompilationLine):bool= (isCompilationLineAFileBegin line || isCompilationLineAFileEnd line)
    let isCompilationLineFreeFormText(line:CompilationLine):bool=
      match line.Type with 
        |LineType(EasyAMLineTypes.FreeFormText)->true 
        |CommandMatch cm->cm.LineType=EasyAMCommandType.None // there's no command. It has to be freeform text
        |_->false
    let isCompilationLineTypeUnprocessed (line:CompilationLine) =
      match line.Type with |LineType(EasyAMLineTypes.Unprocessed)->true |_->false
    let isCompilationLineTypeFileBegin (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.FileBegin)->true |_->false
    let isCompilationLineTypeFileEnd (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.FileEnd)->true |_->false
    let isCompilationLineTypeNewSectionItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.NewSectionItem)->true |_->false
    let isCompilationLineTypeNewJoinedItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.NewJoinedItem)->true |_->false
    let isCompilationLineTypeCompilerJoinDirective (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerJoinDirective)->true |_->false
    let isCompilationLineTypeNewItemJoinCombination (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.NewItemJoinCombination)->true |_->false
    let isCompilationLineTypeCompilerNamespaceDirectiveWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerNamespaceDirectiveWithItem)->true |_->false
    let isCompilationLineTypePoundTagWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.PoundTagWithItem)->true |_->false
    let isCompilationLineTypeMentionTagWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.MentionTagWithItem)->true |_->false
    let isCompilationLineTypeNameValueTagWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.NameValueTagWithItem)->true |_->false
    let isCompilationLineTypeCompilerSectionDirectiveWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerSectionDirectiveWithItem)->true |_->false
    let isCompilationLineTypeCompilerJoinTypeWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerJoinTypeWithItem)->true |_->false
    let isCompilationLineTypeCompilerNamespaceDirective (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerNamespaceDirective)->true |_->false
    let isCompilationLineTypeCompilerTagReset (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerTagReset)->true |_->false
    let isCompilationLineTypeCompilerSectionDirective (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerSectionDirective)->true |_->false
    let isCompilationLineTypeFreeFormText (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.FreeFormText)->true |_->false
    let isCompilationLineTypeEmptyLine (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.EmptyLine)->true |_->false


    type CompilationStream = CompilationLine []
    // This will evolve, probably as the compiler matures
    type CompilationResultType = {
      Results:CompilationStream
      }

    // spec
    type TranslateIncomingIntoOneStream=CompilationUnitType[]->CompilationStream
    type IdentifyCompileStreamByCommandType=CompilationStream->CompilationStream
    type TranslateCommandStreamIntoLineType=CompilationStream->CompilationStream

    // Little helper func to take the file markers back off 
    let removeFileMarkersFromStream (incomingStream:CompilationStream):CompilationStream =
      incomingStream |> Array.filter(fun x->
        match x.Type with 
          | LineType(FileBegin) | LineType(FileEnd) ->false
          |_->true
        )
    let removeFileMarkersFromResult (compileResult:CompilationResultType):CompilationResultType =
      let newResults = 
        compileResult.Results
        |> Array.filter(fun x->
          match x.Type with 
            | LineType(FileBegin) | LineType(FileEnd) ->false
            |_->true
          )
      {Results=newResults}

    let translateIncomingIntoOneStream:TranslateIncomingIntoOneStream = (fun filesIn->
        logBack Logary.Debug "Method translateIncomingIntoOneStream beginning..... "
        let firstMap = 
          filesIn
          |> Array.mapi(fun i x->
            let tempInfo=x.Info
            let newContents =
              x.FileContents
              |> Array.mapi(fun j y->
                  let leadingWhiteSpaceCount = y.IndexOfFirstNonWhitespace(0)  //y.Length - y.TrimStart(' ').Length
                  {
                  ShortFileName=tempInfo.Name
                  FullFileName=tempInfo.FullName
                  CompilationUnitNumber=i
                  LineNumber=j
                  // At first everything gets tagged LineEnd, which means nothing can come after it
                  Type=LineType(Unprocessed)
                  LineText=y
                  TextStartColumn=leadingWhiteSpaceCount
                  }
                )
            let fileBeginBookend=
              {
                ShortFileName=tempInfo.Name
                FullFileName=tempInfo.FullName
                CompilationUnitNumber=i
                LineNumber=(-1)
                Type=LineType(FileBegin)
                LineText=""
                TextStartColumn=0
              }
            let fileEndBookend=
              {
                ShortFileName=tempInfo.Name
                FullFileName=tempInfo.FullName
                CompilationUnitNumber=i
                LineNumber=x.FileContents.Length
                Type=LineType(FileEnd)
                LineText=""
                TextStartColumn=0
              }
            let contentsWithBookends =
                ([|fileEndBookend|] |> Array.append newContents) |> Array.append [|fileBeginBookend|]
            contentsWithBookends
            )
        |> Array.concat
        logBack Logary.Debug (".....Method translateIncomingIntoOneStream ending with a firstMap length of " + firstMap.Length.ToString())
        firstMap
      )

    let matchLineToCommandType:IdentifyCompileStreamByCommandType = (fun incomingStream->
      logBack Logary.Verbose "Method matchLineToCommandType beginning..... "
      let ret = 
        incomingStream |> Array.map(fun x->
          let commandMatch=matchLineWithRecommendedCommand x.LineText
          // For now, if it's a file marker line, just ignore whatever happens
          if isCompilationLineAFileBegin x || isCompilationLineAFileEnd x
            then x
            else {x with Type=CommandMatch(commandMatch)}
          )
      logBack Logary.Verbose "..... Method matchLineToCommandType ending"
      ret
      )
    type NextAddShouldBeType = Item|JoinToItem
    type NextAddExpected = 
      {
        AddType:NextAddShouldBeType
        IndentColumn:int
      }
    let matchLineWithCommandToLineType:TranslateCommandStreamIntoLineType = (fun incomingStream->
      logBack Logary.Debug "Method matchLineWithCommandToLineType beginning..... "
      if incomingStream.Length <2
        then incomingStream
        else
        
        let ret =
          incomingStream
          |> Array.mapFold(fun acc compilationLine->
            let newCompilationLine =
              match compilationLine.Type with 
                // Stuff comes in as a CommandMatch, goes out as a LineType
                |LineIdentification.CommandMatch cm ->
                  match cm.LineType with 
                    |EasyAMCommandType.Join->
                      let firstMatchToken:RegexMatcherType=cm.MatchTokens.[0]
                      let initialRegex:System.Text.RegularExpressions.Regex = firstMatchToken.Regex
                      let matches:System.Text.RegularExpressions.Match []=initialRegex.Matches(compilationLine.LineText).toArray
                      let newAcc={AddType=JoinToItem;IndentColumn=compilationLine.TextStartColumn}
                      let matchGroups=matches.[0].Groups.toArray |> Array.filter(fun x->x.ToString().Length>0)
                      //matchGroups |> Array.iteri(fun i (x:System.Text.RegularExpressions.Group)->Console.WriteLine(i.ToString() + ". " + x.ToString() + " - " + x.Captures.toArray.Length.ToString()))
                      //Remember: You want the first match, then you're interested in the Groups collection for that match
                      match matchGroups.Length-1 with 
                        |0->([|compilationLine|],newAcc) // error? First group is always total string
                        |1->([|{compilationLine with Type=LineType(EasyAMLineTypes.CompilerJoinDirective)}|], newAcc)
                        |2->([|{compilationLine with Type=LineType(EasyAMLineTypes.CompilerJoinDirective)}|], newAcc)
                        |3->([|{compilationLine with Type=LineType(EasyAMLineTypes.CompilerJoinTypeWithItem)}|], newAcc)
                        |4->([|{compilationLine with Type=LineType(EasyAMLineTypes.NewItemJoinCombination)}|], newAcc)
                        |_->([|compilationLine|],newAcc) //error?
                    |EasyAMCommandType.NewItem->
                      match acc.AddType with
                        |Item->([|{compilationLine with Type=LineType(EasyAMLineTypes.NewSectionItem)}|], acc)
                        |JoinToItem->
                          // if the indent is the same or greater, stay with join.
                          // otherwise switch it up to whatever the parent is
                          if compilationLine.TextStartColumn >= acc.IndentColumn
                            then 
                              //Console.WriteLine(compilationLine.TextStartColumn.ToString() + " / " + acc.IndentColumn.ToString() + "  " + compilationLine.LineText)
                              let newAcc={AddType=JoinToItem;IndentColumn=compilationLine.TextStartColumn}
                              ([|{compilationLine with Type=LineType(EasyAMLineTypes.NewJoinedItem)}|], newAcc)
                            else
                              //Console.WriteLine("*" + compilationLine.TextStartColumn.ToString() + " / " + acc.IndentColumn.ToString() + "  " + compilationLine.LineText)
                              let newAcc={AddType=Item;IndentColumn=compilationLine.TextStartColumn}
                              ([|{compilationLine with Type=LineType(EasyAMLineTypes.NewSectionItem)}|], newAcc)
                    |EasyAMCommandType.SectionDirective->
                      let newAcc={AddType=Item;IndentColumn=compilationLine.TextStartColumn}
                      ([|{compilationLine with Type=LineType(EasyAMLineTypes.CompilerSectionDirective)}|],newAcc)
                    |EasyAMCommandType.Namespace->
                      let firstMatchToken:RegexMatcherType=cm.MatchTokens.[0]
                      let initialRegex:System.Text.RegularExpressions.Regex = firstMatchToken.Regex
                      let matches:System.Text.RegularExpressions.Match []=initialRegex.Matches(compilationLine.LineText).toArray
                      let matchGroups=matches.[0].Groups.toArray |> Array.filter(fun x->x.ToString().Length>0)
                      //matchGroups |> Array.iteri(fun i (x:System.Text.RegularExpressions.Group)->Console.WriteLine(i.ToString() + ". " + x.ToString() + " - " + x.Captures.toArray.Length.ToString()))
                      //Remember: You want the first match, then you're interested in the Groups collection for that match
                      match matchGroups.Length-1 with 
                        |0->([|compilationLine|],acc) // error? First group is always total string
                        |1->([|{compilationLine with Type=LineType(EasyAMLineTypes.CompilerNamespaceDirective)}|], acc)
                        |2->([|{compilationLine with Type=LineType(EasyAMLineTypes.CompilerNamespaceDirectiveWithItem)}|], acc)
                        |3->([|{compilationLine with Type=LineType(EasyAMLineTypes.CompilerNamespaceDirectiveWithItem)}|], acc)
                        |_->([|compilationLine|],acc) // error?
                    |EasyAMCommandType.Tag->
                      // This one's funky. It's only time we can have multiple commands at once
                      let lineSplit=compilationLine.LineText.Split([|" "|], StringSplitOptions.None) |> Array.filter(fun x->x.Length>1) // to tag something, there has to be a tag and something else
                      let tagChunks=lineSplit|>Array.filter(fun x->x.Substring(0,1)="&" || x.Substring(0,1)="#" || x.Substring(0,1)="@" )
                      let newLines=tagChunks|>Array.map(fun x->
                        let newLineType=
                          match x.Substring(0,1), x.Substring(1,1)  with
                            |"&",_->LineType(EasyAMLineTypes.NameValueTagWithItem)
                            |"#","@" | "@","#"->LineType(EasyAMLineTypes.CompilerTagReset)
                            |"#",_->LineType(EasyAMLineTypes.PoundTagWithItem)
                            |"@",_->LineType(EasyAMLineTypes.MentionTagWithItem)
                            |_,_->compilationLine.Type
                        {compilationLine with Type=newLineType; LineText=x}
                        )
                      //let firstMatchToken:RegexMatcherType=cm.MatchTokens.[0]
                      //let initialRegex:System.Text.RegularExpressions.Regex = firstMatchToken.Regex
                      //let matches:System.Text.RegularExpressions.Match []=initialRegex.Matches(compilationLine.LineText).toArray
                      //let newAcc={AddType=JoinToItem;IndentColumn=compilationLine.TextStartColumn}
                      //let matchGroups=matches.[0].Groups.toArray |> Array.filter(fun x->x.ToString().Length>0)
                      //({compilationLine with Type=newLineType},acc)
                      (newLines, acc)
                    |EasyAMCommandType.None->
                      ([|{compilationLine with Type=LineType(EasyAMLineTypes.FreeFormText)}|],acc)
                    |EasyAMCommandType.EmptyLine->
                      ([|{compilationLine with Type=LineType(EasyAMLineTypes.EmptyLine)}|],acc)
                |LineIdentification.LineType lt->
                  match lt with 
                    |EasyAMLineTypes.EmptyLine->
                      ([|compilationLine|],acc)
                    |_->([|compilationLine|],acc)
                |LineIdentification.LineType lt->([|compilationLine|],acc)
            newCompilationLine
            ) {AddType=Item; IndentColumn=0}

        logBack Logary.Debug "..... Method matchLineWithCommandToLineType ending"
        let collapsedRet=
          ((fst ret |> Array.concat)
          ,snd ret)
        fst collapsedRet
      )

    //
    /// THE BIG KAHUNA. THE MAIN EVENT.
    let Compile:CompilationUnitType[]->CompilationResultType = (fun (incomingCompilationUnits)->
      logBack Logary.Debug "Method Compile beginning..... "
      let oneStream=translateIncomingIntoOneStream(incomingCompilationUnits)
      let lineTypeResult=matchLineToCommandType oneStream
      let compileResult=matchLineWithCommandToLineType lineTypeResult
      logBack Logary.Debug (".....Method Compile ending with compileResult.length of " + compileResult.Length.ToString())
      {Results=compileResult}
      )

    logBack Verbose "....Module exit"