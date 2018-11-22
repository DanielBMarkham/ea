namespace EA.Core
  module Compiler=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Core.Util
    open EA.Core.Tokens
    open Logary // needed at bottom to give right "Level" lookup for logging
    open System.Threading
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
                  let leadingWhiteSpaceCount = y.Length - y.TrimStart(' ').Length
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
    let matchLineWithCommandToLineType:TranslateCommandStreamIntoLineType = (fun incomingStream->
      logBack Logary.Debug "Method matchLineWithCommandToLineType beginning..... "
      if incomingStream.Length <2
        then incomingStream
        else
        
        let ret =
          incomingStream
          |> Array.mapFold(fun acc x->
            (x,acc)
            ) 0
        //logBack Error "Method matchLineWithCommandToLineType NOTHING HERE "
        incomingStream
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