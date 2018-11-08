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
    //open System.Web.Configuration

    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Core"; "Compiler"; "EALib"; "Compiler" |])
    let mutable clientLogBack:(LogLevel->string->unit) option=Option<LogLevel->string->unit>.None
    let l2 (lvl:LogLevel) (str:string) = logEvent lvl str moduleLogger
    let mutable logBack = l2
    let setLogger (cl:LogLevel->string->unit):unit =
      logBack<-cl
      //clientLogBack<-Some cl
      logBack Debug "Remote logger set"

    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logBack Verbose "Module enter...."
    //matchLineWithRecommendedCommand:string->LineMatcherType

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
    // There is a better way of doing this. Just not today. (Or should this be the start of lenses...)
    let isCompilationLineAFileBegin(line:CompilationLine):bool=
      match line.Type with |LineType(EasyAMLineTypes.FileBegin)->true |_->false
    let isCompilationLineAFileEnd(line:CompilationLine):bool=
      match line.Type with |LineType(EasyAMLineTypes.FileEnd)->true |_->false
    let isCompilationLineAFileMarker (line:CompilationLine):bool= (isCompilationLineAFileBegin line || isCompilationLineAFileEnd line)
    let isCompilationLineFreeFormText(line:CompilationLine):bool=
      match line.Type with |LineType(EasyAMLineTypes.FreeFormText)->true |_->false

    type CompilationStream = CompilationLine []

    type CompilationResultType = {
      Results:CompilationStream
      }

    type TranslateIncomingIntoOneStream=CompilationUnitType[]->CompilationStream
    type IdentifyCompileStreamByCommandType=CompilationStream->CompilationStream
    type TranslateCommandStreamIntoLineType=CompilationStream->CompilationStream

    let stripBookEnds (incomingStream:CompilationStream):CompilationStream =
      incomingStream |> Array.filter(fun x->
        match x.Type with 
          | LineType(FileBegin) | LineType(FileEnd) ->false
          |_->true
        )

    let matchLineToCommandType:IdentifyCompileStreamByCommandType = (fun incomingStream->
      logBack Logary.Verbose "Method matchLineToCommandType beginning..... "
      let ret = 
        incomingStream |> Array.map(fun x->
          let commandMatch=matchLineWithRecommendedCommand x.LineText
          {x with Type=CommandMatch(commandMatch)}
          )
      logBack Logary.Verbose "..... Method matchLineToCommandType ending"  
      ret
      )
    let matchLineWithCommandToLineType:TranslateCommandStreamIntoLineType = (fun incomingStream->
      logBack Logary.Debug "Method matchLineWithCommandToLineType beginning..... "
      logBack Error "Method matchLineWithCommandToLineType NOTHING HERE "
      failwith "NOT IMPLEMENTED YET"
      incomingStream
      )

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

    let Compile:CompilationUnitType[]->CompilationResultType = (fun (incomingCompilationUnits)->
      logBack Logary.Debug "Method Compile beginning..... "
      Console.Error.WriteLine("ASDFSADF A ASDF SAD ")
      logBack Logary.Debug " MOOSE MOOSE!"
      //let compilationUnitArray=incomingCompileText|>Array.map(fun x->{Info=fst x; FileContents=snd x})
      let oneStream=translateIncomingIntoOneStream(incomingCompilationUnits)
      let compileResult=matchLineToCommandType oneStream
      //let squishedText =
      //  compileResult |> stripBookEnds |> Array.map(fun x->x.LineText)
      //let squishedText:string[] = Array.concat (incomingCompilationUnits |> Array.map(fun x->x.FileContents))
      logBack Logary.Debug ("Method doStuff squished text linecount = " + compileResult.Length.ToString())
//      let ret={Results=squishedText}
//      ret
      logBack Logary.Debug (".....Method Compile ending with Results.length of " + compileResult.Length.ToString())
      {Results=compileResult}
      )

    logBack Verbose "....Module exit"