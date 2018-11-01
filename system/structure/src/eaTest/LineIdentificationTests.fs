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
    //val findFirstCommandTypeMatch:string->EasyAMCommandType 
    let MatchesLineType (lineType:EasyAMLineTypes) (str:string) (result:RegexMatcherType) = Expect.contains result.PossibleLineTypes lineType str
    let MatchesCommandType (commandType:EasyAMCommandType) (str:string) (result:LineMatcherType) = Expect.isTrue (result.LineType=commandType) str
    let lineMatchesCommandInfersLineType (commandType:EasyAMCommandType) (lineType:EasyAMLineTypes) (str:string) (line:string) =
      let result1=findFirstLineTypeMatch line 
      let result2=findFirstCommandTypeMatch line 
      let prefix = "FAILURE: Incoming Line '" + line + "'\n"
      if (result2.LineType<>commandType)
        then
          let regexesThatWereSupposedToMatch:RegexMatcherType[]=getRegExesForACommand commandType 
          let listOfRegexesThatWereSupposedToMatch = (regexesThatWereSupposedToMatch |> Array.map(fun x->"'" + x.Regex.ToString() + "'\n"))
          let suffix="\nRegexs that were supposed to match but did not:\n---------------------------------------------\n" + String.Join("", listOfRegexesThatWereSupposedToMatch) + "\nNone of these matched the above line to create a new " + commandType.ToString() + "."
          Tests.failtest (prefix + str + " results in Command Match Failure. CommandType '" + commandType.ToString() + "' expected but not matched. Instead got '" + result2.LineType.ToString() + "'" + suffix)
        else ()
      let suffix = 
        if (result2.LineType=commandType)
        then " CommandType '" + commandType.ToString() + "' is correct and expected."
        else " CommandType '" + commandType.ToString() + "' is not  correct. Instead we got '" + result2.LineType.ToString() + "'"
      if (result1.PossibleLineTypes |> List.exists(fun x->x=lineType))
        then ()
        else Tests.failtest (prefix + str + " results in wrong Line Type. Expected '" + lineType.ToString() + "' but got '[" + (String.Join("', '", result1.PossibleLineTypes)) + "]'" + suffix)
    /// NEGATIVE TEST. SEND IN WHAT THE CORRECT RESULT IS
    let OPPOSITElineMatchesCommandInfersLineType (commandType:EasyAMCommandType) (lineType:EasyAMLineTypes) (str:string) (line:string) =
      let result1=findFirstLineTypeMatch line 
      let result2=findFirstCommandTypeMatch line 
      let prefix = "FAILURE: Incoming Line '" + line + "'\n"
      if (result2.LineType<>commandType)
        then
          let regexesThatWereSupposedToMatch:RegexMatcherType[]=getRegExesForACommand commandType 
          let listOfRegexesThatWereNOTSupposedToMatchBUTDID = (regexesThatWereSupposedToMatch |>Array.filter(fun x->x.Regex.IsMatch(line)) |> Array.map(fun x->"'" + x.Regex.ToString() + "'\n"))
          let suffix="\nRegexs that were NOT supposed to match but did:\n---------------------------------------------\n" + String.Join("", listOfRegexesThatWereNOTSupposedToMatchBUTDID) + "\nOne of these matched the above line to create a new " + commandType.ToString() + "."
          Tests.failtest (prefix + str + " results in Command Match Failure. CommandType '" + commandType.ToString() + "' expected but not matched. Instead got '" + result2.LineType.ToString() + "'" + suffix)
        else ()
      let suffix = 
        if (result2.LineType<>commandType)
        then " CommandType '" + commandType.ToString() + "' is NOT correct and NOT expected."
        else " CommandType '" + commandType.ToString() + "' is what we wanted. Instead we got '" + result2.LineType.ToString() + "'"
      if (result1.PossibleLineTypes |> List.exists(fun x->x=lineType))
        then ()
        else Tests.failtest (prefix + str + " results in WRONG Line Type. Expected '" + lineType.ToString() + "' but got '[" + (String.Join("', '", result1.PossibleLineTypes)) + "]'" + suffix)

      //|NewItem
      //|Join
      //|Namespace
      //|Tag
      //|SectionDirective
      //|EmptyLine
      //|None

    [<Tests>]
    let tests2 = // Might want to match my test lists with my user stories, or not. Haven't decided
      testList "Line Matching" [
        testCase "Smoke test. Empty line falls through to last match" <| fun _ ->
          ""
            |> lineMatchesCommandInfersLineType EmptyLine EasyAMLineTypes.EmptyLine "Empty Line 1: Matching for empty line"
          "     "
            |> lineMatchesCommandInfersLineType EmptyLine EasyAMLineTypes.EmptyLine "Empty Line 2: spaces on line matching for empty line"
          "  \t \n\t  \t  \r"
            |> lineMatchesCommandInfersLineType EmptyLine EasyAMLineTypes.EmptyLine "Empty Line 3: mixture of spaces and control codes on line matching for empty line"
          "'"
            |> OPPOSITElineMatchesCommandInfersLineType None EasyAMLineTypes.FreeFormText "Empty Line NEG 4: single quote by itself should not be an empty line"

        // group test cases by command line token. Overall name is function. Description is reminder, 
        // then # for correct regex order, then variation, then NEG (if appl) then long(er) text
        testCase "Markdown (New Item Command)" <| fun _ ->
          "- I love dogs" 
            |> lineMatchesCommandInfersLineType NewItem NewSectionItem "Markdown: 1-1. List item matches"
          " - I love inside dogs" 
            |> lineMatchesCommandInfersLineType NewItem NewSectionItem "Markdown: 1-2. List item with indent matches"
          "-I also love big dogs"
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText  "Markdown NEG: 1-3. List indicator with no space between should not match"
          " -I also love big dogs"
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText  "Markdown NEG: 1-4. List indicator indented with no space between should not match"
          " * I love cats"
            |> lineMatchesCommandInfersLineType NewItem NewSectionItem "Markdown 1-5: Asterisk list item indicator works"
          " *I love cats"
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Markdown 1-6 NEG: Asterisk squished with item should not be a new item" 
          "[ ] Yes, I will have some pickles"
            |> lineMatchesCommandInfersLineType NewItem NewSectionItem "Markdown 2-7: Alternate checkbox new item working"  
          "[ ]Yes, I will have some turkey"
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Markdown 2-8 NEG: Alternate textbox squished with text shouldn't work as a new item"
          "[x] Yes, I will have some noses"
            |> lineMatchesCommandInfersLineType NewItem NewSectionItem "Markdown 2-9: Alernate checkbox format with x in the middle works"
          "12. Yes, I will have some moose" 
          |> lineMatchesCommandInfersLineType  NewItem NewSectionItem "Markdown 3-10: List item using numbers working"
          "1.Yes, I will have some cow"
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Markdown 3-10 NEG: List item with number squished not working"
          "12) Yes, I will have some moose"
            |> lineMatchesCommandInfersLineType NewItem NewSectionItem "Markdown 3-11: List item with number and parens working"
          "7)Yes, I will have some cow"
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Markdown 3-12: List item with parens squished shouldn't match"        
          "iA.No, there are no nulls here"
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Markdown 4-13 NEG: alpha squished with text shouldn't be a new item"
          "XIXCV. Yes, I not like the varmits"
            |> lineMatchesCommandInfersLineType NewItem NewSectionItem "Markdown: 5-14: Roman numerals working"
          "ii.No, there are no nulls here"
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Markdown: 5-15 NEG: Romans shouldn't work without the space"


        // Gets complicated is the 3-part combo deal with two other items, handled above
        testCase "Setup For Joins (Join Command)" <| fun _ ->
          "TO-DO: Wash the dishes"
            |> lineMatchesCommandInfersLineType Join CompilerJoinTypeWithItem "Joins 1-1: Parses at the beginning of the line"
          "Weasel hate TO-DO My weasel"
            |> lineMatchesCommandInfersLineType Join NewItemJoinCombination "Joins 1-2: Parses in the middle of the line"
          "Unicorns are all around us TO-DO" // THIS IS AN ERROR BUT I DONT THINK ITS HANDLED HERE
            |> lineMatchesCommandInfersLineType Join CompilerJoinDirective "Joins 1-3: Parses at end of line"
          "TO-DOTO-DO" // should only be one
            |> lineMatchesCommandInfersLineType Join CompilerJoinDirective "Joins 1-4: Parses when doubled-up"
          "*TO-DOS:" // 
            |> lineMatchesCommandInfersLineType Join CompilerJoinDirective "Joins 1-5: Parses with wacky wrong newline token and with optional endings"
          "-----TO-DO------" // 
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Joins 1-6 NEG: Does not parse in middle of hyphens"
          "TO-DO:" // 
            |> lineMatchesCommandInfersLineType Join CompilerJoinDirective "Joins 1-7: Parses in canonical format"
          "-----TO-DO" // 
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Joins 1-8 NEG: Does not parse with no space before"
          "TO-DO-----" // 
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Joins 1-9 NEG: Does not parse with no space after"

        // Namespace can only exist on a line by itself
        testCase "New/Modified Namespace (Namespace Command)" <| fun _ ->
          "NAMESPACE=Raccoons"
            |> lineMatchesCommandInfersLineType Namespace CompilerNamespaceDirective "Namespace 1-1: Namespace with an equals"
          "NAMESPACE Possums"
            |> lineMatchesCommandInfersLineType Namespace CompilerNamespaceDirective "Namespace 1-2: Namespace with a space"
          "NAMESPACEMonkeyButt"
            |> OPPOSITElineMatchesCommandInfersLineType None FreeFormText "Namespace 1-3 NEG: Namespace with no space afterwards shouldn't work"

        // Tag is a special case because it can only exist on a line by itself - and there can be multiples of different types
        testCase "New/Modified Tag (Tag Command)" <| fun _ ->
          "## Donkeys rule the world! ##"
            |> lineMatchesCommandInfersLineType None FreeFormText "Tags 1-1: "

        // Section Directive is tough because of all of the shortcuts. Also the two-parter with a directive and a new/referenced item
        testCase "New/Modified Section (Section Directive Command)" <| fun _ ->
          "## Donkeys rule the world! ##"
            |> lineMatchesCommandInfersLineType None FreeFormText "Sections 1-1: "

        // Are we looking for just spaces to consider a line empty? Control characters? How about unprintable Unicode?
        testCase "No Nothing (Empty Line Command)" <| fun _ ->
          "## Donkeys rule the world! ##"
            |> lineMatchesCommandInfersLineType None FreeFormText "FreeForm 1-1: Basic H2 mardown text gets recognized as FreeForm"

        // None command is the default for any markdown text. It's the fall-through, and should match-up to anything we think should be markdown
        testCase "FreeForm Markdown (None Command)" <| fun _ ->
          "## Donkeys rule the world! ##"
            |> lineMatchesCommandInfersLineType None FreeFormText "FreeForm 1-1: Basic H2 mardown text gets recognized as FreeForm"

        //testCase "New/Modified Namespace (Namespace Command)" <| fun _ ->
        //  "## Donkeys rule the world! ##"
        //    |> lineMatchesCommandInfersLineType None FreeFormText "Namespace 1-1:"
      ]

      //findFirstLineTypeMatch (line:string):LineMatcherType

//logEvent Verbose "Method XXXXX beginning....." moduleLogger
//logEvent Verbose "..... Method XXXXX ending. Normal Path." moduleLogger
