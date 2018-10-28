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


    type CompilationUnitType = 
      {
      Info:System.IO.FileInfo
      FileContents:string[]
      }
    type CompilationResultType = 
      {
      MasterModelText:string[]
      }    
    type CompilationLine =
      {
      ShortFileName:string
      FullFileName:string
      CompilationUnitNumber:int
      LineNumber:int 
      LineType:EasyAMLineTypes
      LineText:string
      }
    type CompilationStream = CompilationLine []
    //let myLine = {LineType=FileBegin ;LineText=""}

    let LineIdentification (filesIn:CompilationUnitType[]):CompilationStream =
      let firstMap = 
        filesIn
        |> Array.mapi(fun i x->
          let tempInfo=x.Info
          let newContents =
            x.FileContents
            |> Array.mapi(fun j y->
                {
                ShortFileName=tempInfo.Name
                FullFileName=tempInfo.FullName
                CompilationUnitNumber=i
                LineNumber=j
                // At first everything gets tagged LineEnd, which means nothing can come after it
                LineType=FileEnd
                LineText=y
                }
              )
          newContents
          )
      |> Array.concat
      firstMap


    let Compile:CompilationUnitType[]->CompilationResultType = (fun (incomingCompilationUnits)->
      //let compilationUnitArray=incomingCompileText|>Array.map(fun x->{Info=fst x; FileContents=snd x})
      let squishedText:string[] = Array.concat (incomingCompilationUnits |> Array.map(fun x->x.FileContents))
      logEvent Logary.Debug ("Method doStuff squished text linecount = " + squishedText.Length.ToString()) moduleLogger
      let ret={MasterModelText=squishedText}
      ret
      )

    logEvent Verbose "....Module exit" moduleLogger