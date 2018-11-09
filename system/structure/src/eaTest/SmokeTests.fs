namespace EA.Test
  module SmokeTests=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist
    open EA.Core
    open EA.Core.Compiler
    open Expecto
    open Util
    open Logary // needed at bottom to give right "Level" lookup for logging
    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Test";  "EATest"; "Util" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    [<Tests>]
    let tests =
      testList "Smoke" [
        testCase "Empty file return empty" <| fun _ ->
          let newParm=[|{Info=FakeFile1; FileContents=[||]}|]
          let result=Compile(newParm) |> removeFileMarkersFromResult
          Expect.isTrue (result.Results.Length=0) "Empty input producing an output"

        testCase "Two empty files return empty" <| fun _ ->
          let newParm=[|{Info=FakeFile1; FileContents=[||]}; {Info=FakeFile2; FileContents=[||]}|]
          let result=Compile(newParm) |> removeFileMarkersFromResult
          Expect.isTrue (result.Results.Length=0) "Two empty inputs producing an output"

        testCase "Dummy static file remains the same" <| fun _ ->
          let newParm=[|StaticTenLineDummyFile|]
          let result=Compile(newParm) |> removeFileMarkersFromResult
          Expect.isTrue (result.Results.Length=10) "Static 10-line dummy not staying 10 lines"
      
        testCase "Two static dummy files concatenate" <| fun _ ->
          let newParm=[|StaticTenLineDummyFile|]
          let result=Compile(newParm) |> removeFileMarkersFromResult
          Expect.isTrue (result.Results.Length=10) "Two static 10-line dummy files not equalling 20 lines"
      
      ]

      // WHAT TO DO WITH FILE MARKERS HERE?

//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
