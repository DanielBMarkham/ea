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
    open Logary // needed at bottom to give right "Level" lookup for logging
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Core"; "Compiler"; "EALib"; "Compiler" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    type CompilationLine =
      {
      ShortFileName:string
      FullFileName:string
      CompilationUnitNumber:int
      LineNumber:int 
      LineType:EasyAMLineTypes
      LineText:string
      TextStartColumn:int
      }
    type CompilationStream = CompilationLine []

    //let myLine = {LineType=FileBegin ;LineText=""}

    let stripBookEnds (incomingStream:CompilationStream):CompilationStream =
      incomingStream |> Array.filter(fun x->
        x.LineType<>FileBegin && x.LineType<>FileEnd
        )

    let lineIdentification (incomingStream:CompilationStream):CompilationStream =
      incomingStream

    let translateIncomingIntoOneStream (filesIn:CompilationUnitType[]):CompilationStream =
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
                LineType=Unprocessed
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
              LineType=FileBegin
              LineText=""
              TextStartColumn=0
            }
          let fileEndBookend=
            {
              ShortFileName=tempInfo.Name
              FullFileName=tempInfo.FullName
              CompilationUnitNumber=i
              LineNumber=x.FileContents.Length
              LineType=FileEnd
              LineText=""
              TextStartColumn=0
            }
          let contentsWithBookends =
              ([|fileEndBookend|] |> Array.append newContents) |> Array.append [|fileBeginBookend|]
          contentsWithBookends
          )
      |> Array.concat
      firstMap


    let Compile:CompilationUnitType[]->CompilationResultType = (fun (incomingCompilationUnits)->
      //let compilationUnitArray=incomingCompileText|>Array.map(fun x->{Info=fst x; FileContents=snd x})
      let compileResult=translateIncomingIntoOneStream(incomingCompilationUnits)
      let squishedText =
        compileResult |> stripBookEnds |> Array.map(fun x->x.LineText)
      //let squishedText:string[] = Array.concat (incomingCompilationUnits |> Array.map(fun x->x.FileContents))
      logEvent Logary.Debug ("Method doStuff squished text linecount = " + squishedText.Length.ToString()) moduleLogger
      let ret={MasterModelText=squishedText}
      ret
      )

    logEvent Verbose "....Module exit" moduleLogger