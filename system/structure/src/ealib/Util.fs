namespace EA.Core
  module Util=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open Logary // needed at bottom to give right "Level" lookup for logging
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Core"; "EALib"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger


    type CompilationUnitType = {
      Info:System.IO.FileInfo
      FileContents:string[]
      }
    type CompilationResultType = {
      MasterModelText:string[]
      }    

    // sharing the above type is NOT working across the dll and exe. Being specific for now using primitives
    //let Compile = (fun (incomingCompileText:((System.IO.FileInfo*string[])[]))->
    //  let compilationUnitArray=incomingCompileText|>Array.map(fun x->{Info=fst x; FileContents=snd x})
    //  let squishedText:string[] = Array.concat (compilationUnitArray |> Array.map(fun x->x.FileContents))
    //  logEvent Logary.Debug ("Method doStuff squished text linecount = " + squishedText.Length.ToString()) moduleLogger
    //  let ret:string[]=squishedText
    //  ret
    //  )
    let Compile:CompilationUnitType[]->CompilationResultType = (fun (incomingCompilationUnits)->
      //let compilationUnitArray=incomingCompileText|>Array.map(fun x->{Info=fst x; FileContents=snd x})
      let squishedText:string[] = Array.concat (incomingCompilationUnits |> Array.map(fun x->x.FileContents))
      logEvent Logary.Debug ("Method doStuff squished text linecount = " + squishedText.Length.ToString()) moduleLogger
      let ret={MasterModelText=squishedText}
      ret
      )

    logEvent Verbose "....Module exit" moduleLogger



//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
