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
    open EA.Core.Tokens
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
          let newParm=[|{Info=FakeFile1; FileContents=[||]}|]
          let result=translateIncomingIntoOneStream(newParm)
          Expect.isTrue (result.Length=2) "Empty input producing two bookends"
          Expect.isTrue (result.[0].LineType=FileBegin) "Empty input produces FileBegin bookend"
          Expect.isTrue (result.[1].LineType=FileEnd) "Empty input produces FileEnd bookend"

        testCase "Two empty files each get bookends" <| fun _ ->
          let newParm=[|{Info=FakeFile1; FileContents=[||]};{Info=FakeFile2; FileContents=[||]}|]
          let result=translateIncomingIntoOneStream(newParm)
          Expect.isTrue (result.Length=4) "Two empty files produce four bookends"
          Expect.isTrue (result.[0].LineType=FileBegin) "First empty file does not contain FileBegin bookend"
          Expect.isTrue (result.[1].LineType=FileEnd) "First empty file does not contain FileEnd bookend"
          Expect.isTrue (result.[2].LineType=FileBegin) "Second empty file does not contain FileBegin bookend"
          Expect.isTrue (result.[3].LineType=FileEnd) "Second empty file does not contain FileEnd bookend"

        testCase "Two static files are concatentated and each get appropriate bookends" <| fun _ ->
          let newParm=[|StaticTenLineDummyFile;StaticTenLineDummyFile|]
          let result=translateIncomingIntoOneStream(newParm)
          Expect.isTrue (result.Length=24) "Two static files get concatenated"
          Expect.isTrue (result.[0].LineType=FileBegin) "First static file does not contain FileBegin bookend"
          Expect.isTrue (result.[11].LineType=FileEnd) "First static file does not contain FileEnd bookend"
          Expect.isTrue (result.[12].LineType=FileBegin) "Second static file does not contain FileBegin bookend"
          Expect.isTrue (result.[23].LineType=FileEnd) "Second static file does not contain FileEnd bookend"
          Expect.isTrue (result.[3].LineText="2") "First  static file not in right order"
          Expect.isTrue (result.[21].LineText="8") "Second  static file not in right order"

        testCase "Simple one-line file is freeform text" <| fun _ ->
          let newParm=[|{Info=FakeFile1; FileContents=[|"I like ice cream"|]}|]
          let result=newParm |> translateIncomingIntoOneStream |> lineIdentification
          Expect.isTrue (result.Length=3) "Two static files get concatenated"
          Expect.isTrue (result.[1].LineType=FreeFormText) "Simple one-line file should be freeform text"
          Expect.isTrue (result.[0].LineType=FileBegin) "Simple one-line missing file begin"
          Expect.isTrue (result.[2].LineType=FileEnd) "Simple one-line missing file end"

      ]

    [<Tests>]
    let tests2 =
      testList "LineMatchingRegex" [
        testCase "Pass-through smoke test. Empty line falls through to last match" <| fun _ ->
          let newParm=""
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType=EmptyLine) "Line Matching for empty line is not returning empty line"
        testCase "Markdown: Basic Markdown list item matches 1" <| fun _ ->
          let newParm="- I love dogs"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType=NewItem) "Basic Markdown new item working 1"
        testCase "Markdown: Basic Markdown list item matches 1B" <| fun _ ->
          let newParm=" * I love cats"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType=NewItem) "Basic Markdown new item working 1B"
        testCase "Markdown: Negative markdown test bad newItem does not match 1" <| fun _ ->
          let newParm=" -I love dogs"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType<>NewItem) "Not a markdown item but thinking it is 1"
        testCase "Markdown: Negative markdown test bad newItem does not match 1B" <| fun _ ->
          let newParm="*I love cats"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType<>NewItem) "Not a markdown item but thinking it is 1B"
        testCase "Markdown: Extended Markdown list item matches 2" <| fun _ ->
          let newParm="[ ] Yes, I will have some pickles"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType=NewItem) "Extended Markdown new item working 2"
        testCase "Markdown: Negative Extended Markdown list item matches 2" <| fun _ ->
          let newParm="[ ]Yes, I will have some turkey"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType<>NewItem) "Negatve Extended Markdown 2 matching even though it shouldn't"
        testCase "Markdown: Extended Markdown list item matches 2B" <| fun _ ->
          let newParm="[x] Yes, I will have some noses"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType=NewItem) "Extended Markdown new item working 2B"
        testCase "Markdown: Extended Markdown list item matches 3" <| fun _ ->
          let newParm="12. Yes, I will have some moose"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType=NewItem) "Extended Markdown 3 not matching"
        testCase "Markdown: Negative Extended Markdown list item matches 3" <| fun _ ->
          let newParm="1.Yes, I will have some cow"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType<>NewItem) "Negatve Extended Markdown 3 matching even though it shouldn't"
        testCase "Markdown: Extended Markdown list item matches 3B" <| fun _ ->
          let newParm="12) Yes, I will have some moose"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType=NewItem) "Extended Markdown 3B not matching when it should"
        testCase "Markdown: Negative Extended Markdown list item matches 3B" <| fun _ ->
          let newParm="7)Yes, I will have some cow"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType<>NewItem) "Negatve Extended Markdown 3B matching even though it shouldn't"
        testCase "Markdown: Extended Markdown list item matches 4" <| fun _ ->
          let newParm="iZ. Yes, I not like the varmits"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType=NewItem) "Extended Markdown 4 not matching"
        testCase "Markdown: Negative Extended Markdown list item matches 4" <| fun _ ->
          let newParm="iA.No, there are no nulls here"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType<>NewItem) "Negatve Extended Markdown 4 matching even though it shouldn't"
        testCase "Markdown: Extended Markdown list item matches 5" <| fun _ ->
          let newParm="XIXCV. Yes, I not like the varmits"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType=NewItem) "Extended Markdown 5 not matching"
        testCase "Markdown: Negative Extended Markdown list item matches 5" <| fun _ ->
          let newParm="ii.No, there are no nulls here"
          let result=findFirstLineTypeMatch newParm
          Expect.isTrue (result.LineType<>NewItem) "Negatve Extended Markdown 5 matching even though it shouldn't"
      ]

      //findFirstLineTypeMatch (line:string):LineMatcherType

//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
