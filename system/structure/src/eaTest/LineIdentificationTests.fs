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
    open EA.Core.Util
    open EA.Core.Compiler
    open Expecto
    open Util
    open Logary // needed at bottom to give right "Level" lookup for logging
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Test";  "EATest"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger


//    let translateIncomingIntoOneStream (filesIn:CompilationUnitType[]):CompilationStream =
    
    [<Tests>]
    let tests =
      testList "TranslateIncomingIntoOneStream" [
        testCase "Empty file gets bookends" <| fun _ ->
          let newFakeInfo=getFakeFileInfo()
          let newParm=[|{Info=FakeFile1; FileContents=[||]}|]
          let result=translateIncomingIntoOneStream(newParm)
          Expect.isTrue (result.Length=2) "Empty input producing two bookends"
          Expect.isTrue (result.[0].LineType=FileBegin) "Empty input produces FileBegin bookend"
          Expect.isTrue (result.[1].LineType=FileEnd) "Empty input produces FileEnd bookend"

        testCase "Two empty files each get bookends" <| fun _ ->
          let newFakeInfo=getFakeFileInfo()
          let newParm=[|{Info=FakeFile1; FileContents=[||]};{Info=FakeFile2; FileContents=[||]}|]
          let result=translateIncomingIntoOneStream(newParm)
          Expect.isTrue (result.Length=4) "Two empty files produce four bookends"
          Expect.isTrue (result.[0].LineType=FileBegin) "First empty file does not contain FileBegin bookend"
          Expect.isTrue (result.[1].LineType=FileEnd) "First empty file does not contain FileEnd bookend"
          Expect.isTrue (result.[2].LineType=FileBegin) "Second empty file does not contain FileBegin bookend"
          Expect.isTrue (result.[3].LineType=FileEnd) "Second empty file does not contain FileEnd bookend"

        testCase "Two static files are concatentated and each get appropriate bookends" <| fun _ ->
          let newFakeInfo=getFakeFileInfo()
          let newParm=[|StaticTenLineDummyFile;StaticTenLineDummyFile|]
          let result=translateIncomingIntoOneStream(newParm)
          Expect.isTrue (result.Length=24) "Two static files get concatenated"
          Expect.isTrue (result.[0].LineType=FileBegin) "First static file does not contain FileBegin bookend"
          Expect.isTrue (result.[11].LineType=FileEnd) "First static file does not contain FileEnd bookend"
          Expect.isTrue (result.[12].LineType=FileBegin) "Second static file does not contain FileBegin bookend"
          Expect.isTrue (result.[23].LineType=FileEnd) "Second static file does not contain FileEnd bookend"
          Expect.isTrue (result.[3].LineText="2") "First  static file not in right order"
          Expect.isTrue (result.[21].LineText="8") "Second  static file not in right order"

      ]


//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
