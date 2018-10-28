namespace EA.Test
  module LineIdentificationTests=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Core
    open Expecto
    open Util
    open Logary // needed at bottom to give right "Level" lookup for logging
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Test";  "EATest"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    [<Tests>]
    let tests =
      testList "file" [
        //testCase "Empty files return empty" <| fun _ ->
        //  let newFakeInfo=getFakeFileInfo()
        //  let newFakeFileContents=[||]
        //  let newParm=[|{Info=newFakeInfo; FileContents=newFakeFileContents}|]
        //  let result=compile(newParm)
        //  Expect.isTrue (result.MasterModelText.Length=0) "Empty input producing an output"
      ]


//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
