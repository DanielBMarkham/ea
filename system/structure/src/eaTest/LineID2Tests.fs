namespace EA.Test
  module LineID2Tests=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA
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
    open System.CodeDom.Compiler

    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Test";  "EATest"; "LineID2Tests" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger

    // I took 3 of the amin files I had been using to organize the project by episode 3
    // and made them into dummy files to drive out the last few bits of the engine framework

    let file1="""
# These are the stories for Episode 3

They should work fine by themselves

TO-DO
  * Create a DU type to handle all the line types
  * Look into using FParsec to sort between line types and return the DU and text
  * Decide what I mean when I say "print out"

USER STORIES
  - Recognize and sort lines by line type
      PARENT
      - Organize files by line type
      - Be easy-to-use
  - Print out project To-Do items
      PARENT
      - Organize files by line type
      - Be easy-to-use



*This is both the sprint backlog and the test data*

NOTES: 
  * BASH tests failing last time led to huge adventure with threads and streams (time overrun)
  * Be sure to talk about using the compiler as a solo dev versus working in a team!
  * Do we have tests to show? What happened to printing out to-do items?""".Split([|"\r\n";"\n";"\r"|], StringSplitOptions.None)

    let file2="""
NOTES
  * the link directive should be able to coordinate the reference with any web-based tooling system that allows deeplinks. It doesn't do data movement or translation, of course, only location. Also works with GitHub code/version stuff. Would make for a nice demo
  * I should strive to have this file compiling ASAP. It'd make the blog/video series more realistic
META SUPPLEMENTALS
  * Must be able to consume anything it produces without the model changing
  * Must allow markdown language anywhere text is used
  * Must be useable from the command line just like GCC
  * Must use latest version of dotnet
  * Philosophies to use: microservices, unix philosohphy, pure FP, DDD

META BEHAVIOR
  * Record item titles in any of the three buckets
  * Create sprint stories by matching titles
  * Change story details (title, ac, etc)
  * Create sprint story cards
  * Type in the code that creates the compiler and it compiles

BUSINESS BEHAVIOR
 * Squash conversation notes

BUSINESS SUPPLEMENTALS
  * Be easy-to-use
    CHILDREN
    - Work like linux
    - Run fast
    - Run anywhere
    - Log stuff if you need to
    - Run everywhere

SYSTEM SUPPLEMENTALS
  * Support a command-line verbosity switch
SYSTEM BEHAVIOR
  * Concatenate files

USER STORIES
  * Process files without doing anything
    PARENT
    - Concatenate files
    - Be easy-to-use
  * Process files and concatenate by filename
    PARENT
    - Concatenate files
    - Be easy-to-use

* Where are we?
* Stories are props for Meta development
* Should first tests be Bash or Expecto? Yes!
"""

    let file3="""
BUSINESS ABSTRACT SUPPLEMENTALS
  * Work like other Unix commands
    BECAUSE
      * We value people's time
      * Anybody in the technology industry will have some exposure to Unix commands
    WHENEVER
      * Users are interacting with the program from the command line
    ITHASTOBETHAT
      * EasyAM looks and acts as much as possible like all of the other built-in Unix commands users know
  * Never be a bore
    BECAUSE
      * People are always in a hurry
      * They need some results right now, dammit!
    WHENEVER
      * They're at the command prompt interacting with our program
    ITHASTOBETHAT
      * We never take longer than 3 seconds to provide them results
      * This does not inlude OS issues because we can't help those things

NAMESPACE EasyAM2
  BUSINESS ABSTRACT BEHAVIOR
    * Organize a set of notes I took today
      ASA Person trying to learn about this thing we want to build
      WHEN I'm talking to the other guys and the customer
      INEEDTO Organize all of my notes according to Structured Analysis
      SOTHAT
        * I can find stuff easily later
        * I can organize and prioritize important questions for the next time we start chatting

  USER STORIES
      &episode=2 &status=done &testscript=test_episode2.sh
    * I mistyped the name of my notes file - episode 2
      PARENT
        * Organize a set of notes I took today
        * Work like other Unix commands
      OUTCOME
        * There is no output or an empty file is generated
      @episode=2 @status=done @testscript=test_episode2.sh
    * I took a bunch of notes without any tagged information in a single file
      PARENT
        * Organize a set of notes I took today
        * Work like other Unix commands
      OUTCOME
        * Whatever notes I took are copied over to the output without any changes to them
      #episode=2 #status=done #testscript=test_episode2.sh
    * I took a bunch of notes without any tagged information in two separate files
      PARENT
        * Organize a set of notes I took today
        * Work like other Unix commands
      OUTCOME
        * Whatever notes I took are copied over to the output without any changes to them

"""
    //let newParm=(defaultEAConfig,[|{Info=FakeFile1; FileContents=[||]}|])
    //let result=Compile(snd newParm)

    let compileCheck (compilerResults:CompilationResultType) =
      let resultLines = compilerResults.Results
      let linesThatHaveNotBeenProcessed = resultLines |> Array.filter(fun x->isCompilationLineALineType x)
      //let hasBookends =
      //  if resultLines.Length > 2 then resultLines.[0]
      ()

    let episode3Input=(defaultEAConfig,[|{Info=FakeFile1; FileContents=file1}|])
    let episode3Results=Compile(snd episode3Input)      

    [<Tests>]
    let tests =
      testList "Episode3LineTypeTests" [
      testCase "Truth" <| fun _ ->
        Expect.isTrue true "I am truth"
      testCase "Results array" <| fun _ ->
        if (episode3Results.Results.Length<>30) then Tests.failtest  ("Length is not 30. It is: " + episode3Results.Results.Length.ToString()) else ()
      testCase "First Line processing" <| fun _ ->
        if (isCompilationLineTypeEmptyLine episode3Results.Results.[1] = false) then Tests.failtest ("First line should be empty line type but is not. It is " + episode3Results.Results.[1].Type.ToString()) else()
      testCase "Second line is markup text" <| fun _ ->
        if (isCompilationLineTypeFreeFormText episode3Results.Results.[2] = false) then Tests.failtest ("Second line should be freeform text but is not. It is " + episode3Results.Results.[2].Type.ToString()) else()
      testCase "Third Line processing" <| fun _ ->
        if (isCompilationLineTypeEmptyLine episode3Results.Results.[3] = false) then Tests.failtest ("Third line should be empty line type but is not. It is " + episode3Results.Results.[3].Type.ToString()) else()
      testCase "Fourth line is markup text" <| fun _ ->
        if (isCompilationLineTypeFreeFormText episode3Results.Results.[4] = false) then Tests.failtest ("Fourth line should be freeform text but is not. It is " + episode3Results.Results.[4].Type.ToString()) else()
      testCase "Fifth Line processing" <| fun _ ->
        if (isCompilationLineTypeEmptyLine episode3Results.Results.[5] = false) then Tests.failtest ("Fifth line should be empty line type but is not. It is " + episode3Results.Results.[5].Type.ToString()) else()
      testCase "Sixth Line processing" <| fun _ ->
        if (isCompilationLineTypeCompilerJoinDirective episode3Results.Results.[6] = false) then Tests.failtest ("Sixth line should be stand alone join line type but is not. It is " + episode3Results.Results.[6].Type.ToString()) else()
      testCase "Seventh Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[7] = false) then Tests.failtest ("Seventh line should be new join itemline type but is not. It is " + episode3Results.Results.[7].Type.ToString()) else()
      testCase "Eight Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[8] = false) then Tests.failtest ("Eighth line should be new join itemline type but is not. It is " + episode3Results.Results.[8].Type.ToString()) else()
      testCase "Ninth Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[9] = false) then Tests.failtest ("Ninth line should be new join itemline type but is not. It is " + episode3Results.Results.[9].Type.ToString()) else()
      testCase "Tenth Line processing" <| fun _ ->
        if (isCompilationLineTypeEmptyLine episode3Results.Results.[10] = false) then Tests.failtest ("Tenth line should be empty line but is not. It is " + episode3Results.Results.[10].Type.ToString()) else()
      testCase "Eleventh Line processing" <| fun _ ->
        if (isCompilationLineTypeCompilerSectionDirective episode3Results.Results.[11] = false) then Tests.failtest ("Eleventh line should be new section directive but is not. It is " + episode3Results.Results.[11].Type.ToString()) else()
      testCase "Twelveth Line processing" <| fun _ ->
        if (isCompilationLineTypeNewSectionItem episode3Results.Results.[12] = false) then Tests.failtest ("Twelveth line should be new section item but is not. It is " + episode3Results.Results.[12].Type.ToString() + "\n" + episode3Results.Results.[12].LineText ) else()
      testCase "Thirteenth Line processing" <| fun _ ->
        if (isCompilationLineTypeCompilerJoinDirective episode3Results.Results.[13] = false) then Tests.failtest ("Thirteenth line should be stand alone join line type but is not. It is " + episode3Results.Results.[13].Type.ToString() + "\n" + episode3Results.Results.[13].LineText ) else()
      testCase "Fourteenth Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[14] = false) then Tests.failtest ("Fourteenth line should be new join itemline type but is not. It is " + episode3Results.Results.[14].Type.ToString() + "\n" + episode3Results.Results.[14].LineText ) else()
      testCase "Fifteenth Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[15] = false) then Tests.failtest ("Fifteenth line should be new join itemline type but is not. It is " + episode3Results.Results.[15].Type.ToString() + "\n" + episode3Results.Results.[15].LineText ) else()
      testCase "Sixteenth Line processing" <| fun _ ->
        if (isCompilationLineTypeNewSectionItem episode3Results.Results.[16] = false) then Tests.failtest ("Twelveth line should be new section item but is not. It is " + episode3Results.Results.[16].Type.ToString() + "\n" + episode3Results.Results.[16].LineText ) else()
      testCase "Seventeenth Line processing" <| fun _ ->
        if (isCompilationLineTypeCompilerJoinDirective episode3Results.Results.[17] = false) then Tests.failtest ("Thirteenth line should be stand alone join line type but is not. It is " + episode3Results.Results.[17].Type.ToString() + "\n" + episode3Results.Results.[17].LineText ) else()
      testCase "Eighteenth Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[18] = false) then Tests.failtest ("Fourteenth line should be new join itemline type but is not. It is " + episode3Results.Results.[18].Type.ToString() + "\n" + episode3Results.Results.[18].LineText ) else()
      testCase "Nineteenth Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[19] = false) then Tests.failtest ("Fifteenth line should be new join itemline type but is not. It is " + episode3Results.Results.[19].Type.ToString() + "\n" + episode3Results.Results.[19].LineText ) else()
      testCase "20th Line processing" <| fun _ ->
        if (isCompilationLineTypeEmptyLine episode3Results.Results.[20] = false) then Tests.failtest ("Twentieth line should be empty line type but is not. It is " + episode3Results.Results.[20].Type.ToString()) else()
      testCase "21st Line processing" <| fun _ ->
        if (isCompilationLineTypeEmptyLine episode3Results.Results.[21] = false) then Tests.failtest ("Twenty-First line should be empty line type but is not. It is " + episode3Results.Results.[21].Type.ToString()) else()
      testCase "22nd Line processing" <| fun _ ->
        if (isCompilationLineTypeEmptyLine episode3Results.Results.[22] = false) then Tests.failtest ("Twenty-Second line should be empty line type but is not. It is " + episode3Results.Results.[22].Type.ToString()) else()
      testCase "23rd line is markup text" <| fun _ ->
        if (isCompilationLineTypeFreeFormText episode3Results.Results.[23] = false) then Tests.failtest ("Twenty-third line should be freeform text but is not. It is " + episode3Results.Results.[23].Type.ToString()) else()
      testCase "24th Line processing" <| fun _ ->
        if (isCompilationLineTypeEmptyLine episode3Results.Results.[24] = false) then Tests.failtest ("Twenty-Fourth line should be empty line type but is not. It is " + episode3Results.Results.[24].Type.ToString()) else()
      testCase "25th Line processing" <| fun _ ->
        if (isCompilationLineTypeCompilerJoinDirective episode3Results.Results.[25] = false) then Tests.failtest ("Twenty-fifth line should be stand alone join line type but is not. It is " + episode3Results.Results.[25].Type.ToString() + "\n" + episode3Results.Results.[25].LineText ) else()
      testCase "26th Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[26] = false) then Tests.failtest ("Twenty-sixth line should be new join itemline type but is not. It is " + episode3Results.Results.[26].Type.ToString() + "\n" + episode3Results.Results.[26].LineText ) else()
      testCase "27th Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[27] = false) then Tests.failtest ("Twenty-seventh line should be new join itemline type but is not. It is " + episode3Results.Results.[27].Type.ToString() + "\n" + episode3Results.Results.[27].LineText ) else()
      testCase "28th Line processing" <| fun _ ->
        if (isCompilationLineTypeNewJoinedItem episode3Results.Results.[28] = false) then Tests.failtest ("Twenty-eigth line should be new join itemline type but is not. It is " + episode3Results.Results.[28].Type.ToString() + "\n" + episode3Results.Results.[28].LineText ) else()
      ]
    
    let episode4Input=(defaultEAConfig,[|{Info=FakeFile2; FileContents=file2.Split([|"\r\n";"\n";"\r"|], StringSplitOptions.None)}|])
    let episode4Results=Compile(snd episode4Input)      
    let resultLineChecker ((
                            resultLine:EA.Core.Compiler.CompilationLine)
                            , (funChecker:EA.Core.Compiler.CompilationLine->bool)) =
      let result=funChecker resultLine
      if result=true
        then ()
        else
          let commandMatch = matchLineWithRecommendedCommand resultLine.LineText
          let tabMsg = if resultLine.LineText.IndexOf('\t')>=0 then " --TAB DETECTED--  " else ""
          let prefix = "  COMMANDS TO LINETYPE " + tabMsg + "TRANSFORMATION FAILURE: INCOMINGLINE \n  " + resultLine.ToFileLocation + " '" + resultLine.LineText + "'\n"
          let suffix = "\n  Command Match Found: " + commandMatch.ToString()
          Tests.failtest (prefix + "  Actual LineType was: " + resultLine.Type.ToString() + suffix)
      
    // Ok, we did some manually. Now to generalize a bit
    [<Tests>]
    let testsEpisode3 =
      testList "Episode3LineTypeTests" [
      testCase "Episode3 Line Type Checks" <| fun _ ->
        resultLineChecker(episode4Results.Results.[0], isCompilationLineAFileBegin)
        resultLineChecker(episode4Results.Results.[1], isCompilationLineTypeEmptyLine)
        resultLineChecker(episode4Results.Results.[2], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(episode4Results.Results.[3], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[4], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[5], isCompilationLineTypeCompilerSectionDirective)
        resultLineChecker(episode4Results.Results.[6], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[7], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[8], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[9], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[10], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[11], isCompilationLineTypeEmptyLine)
        resultLineChecker(episode4Results.Results.[12], isCompilationLineTypeCompilerSectionDirective)
        resultLineChecker(episode4Results.Results.[13], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[14], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[15], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[16], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[17], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[18], isCompilationLineTypeEmptyLine)
        resultLineChecker(episode4Results.Results.[19], isCompilationLineTypeCompilerSectionDirective)
        resultLineChecker(episode4Results.Results.[20], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[21], isCompilationLineTypeEmptyLine)
        resultLineChecker(episode4Results.Results.[22], isCompilationLineTypeCompilerSectionDirective)
        resultLineChecker(episode4Results.Results.[23], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[24], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(episode4Results.Results.[25], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[26], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[27], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[28], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[29], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[30], isCompilationLineTypeEmptyLine)
        resultLineChecker(episode4Results.Results.[31], isCompilationLineTypeCompilerSectionDirective)
        resultLineChecker(episode4Results.Results.[32], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[33], isCompilationLineTypeCompilerSectionDirective)
        resultLineChecker(episode4Results.Results.[34], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[35], isCompilationLineTypeEmptyLine)
        resultLineChecker(episode4Results.Results.[36], isCompilationLineTypeCompilerSectionDirective)
        resultLineChecker(episode4Results.Results.[37], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[38], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(episode4Results.Results.[39], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[40], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[41], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[42], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(episode4Results.Results.[43], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[44], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(episode4Results.Results.[45], isCompilationLineTypeEmptyLine)
        resultLineChecker(episode4Results.Results.[46], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[47], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[48], isCompilationLineTypeNewSectionItem)
        resultLineChecker(episode4Results.Results.[49], isCompilationLineTypeEmptyLine)
        resultLineChecker(episode4Results.Results.[50], isCompilationLineAFileEnd)
        ]

    let eaAtEpisode4Input=(defaultEAConfig,[|{Info=FakeFile3; FileContents=file3.Split([|"\r\n";"\n";"\r"|], StringSplitOptions.None)}|])
    let eaAtEpisoderResults=Compile(snd eaAtEpisode4Input)      
    // Ok, we did some manually. Now to generalize a bit
    [<Tests>]
    let testsEAMasterStartOfEpisode4 =
      testList "EAMasterStartOfEpisode4 LineTypeTests" [
      testCase "EAMasterStartOfEpisode4 Line Type Checks" <| fun _ ->
        resultLineChecker(eaAtEpisoderResults.Results.[0], isCompilationLineAFileBegin)
        resultLineChecker(eaAtEpisoderResults.Results.[1], isCompilationLineTypeEmptyLine)
        resultLineChecker(eaAtEpisoderResults.Results.[2], isCompilationLineTypeCompilerSectionDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[3], isCompilationLineTypeNewSectionItem)
        resultLineChecker(eaAtEpisoderResults.Results.[4], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[5], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[6], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[7], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[8], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[9], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[10], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[11], isCompilationLineTypeNewSectionItem)
        resultLineChecker(eaAtEpisoderResults.Results.[12], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[13], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[14], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[15], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[16], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[17], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[18], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[19], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[20], isCompilationLineTypeEmptyLine)
        resultLineChecker(eaAtEpisoderResults.Results.[21], isCompilationLineTypeCompilerNamespaceDirectiveWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[22], isCompilationLineTypeCompilerSectionDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[23], isCompilationLineTypeNewSectionItem)
        resultLineChecker(eaAtEpisoderResults.Results.[24], isCompilationLineTypeCompilerJoinTypeWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[25], isCompilationLineTypeCompilerJoinTypeWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[26], isCompilationLineTypeCompilerJoinTypeWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[27], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[28], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[29], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[30], isCompilationLineTypeEmptyLine)
        resultLineChecker(eaAtEpisoderResults.Results.[31], isCompilationLineTypeCompilerSectionDirective)

        // with multi-tags on one line, the line multiplies. This means array index no longer useful for lookup
        resultLineChecker(eaAtEpisoderResults.Results.[32], isCompilationLineTypeNameValueTagWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[33], isCompilationLineTypeNameValueTagWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[34], isCompilationLineTypeNameValueTagWithItem)

        resultLineChecker(eaAtEpisoderResults.Results.[35], isCompilationLineTypeNewSectionItem)
        resultLineChecker(eaAtEpisoderResults.Results.[36], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[37], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[38], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[39], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[40], isCompilationLineTypeNewJoinedItem)

        // another split/multiply
        resultLineChecker(eaAtEpisoderResults.Results.[41], isCompilationLineTypeMentionTagWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[42], isCompilationLineTypeMentionTagWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[43], isCompilationLineTypeMentionTagWithItem)


        resultLineChecker(eaAtEpisoderResults.Results.[44], isCompilationLineTypeNewSectionItem)
        resultLineChecker(eaAtEpisoderResults.Results.[45], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[46], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[47], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[48], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[49], isCompilationLineTypeNewJoinedItem)


        resultLineChecker(eaAtEpisoderResults.Results.[50], isCompilationLineTypePoundTagWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[51], isCompilationLineTypePoundTagWithItem)
        resultLineChecker(eaAtEpisoderResults.Results.[52], isCompilationLineTypePoundTagWithItem)

        resultLineChecker(eaAtEpisoderResults.Results.[53], isCompilationLineTypeNewSectionItem)
        resultLineChecker(eaAtEpisoderResults.Results.[54], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[55], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[56], isCompilationLineTypeNewJoinedItem)
        resultLineChecker(eaAtEpisoderResults.Results.[57], isCompilationLineTypeCompilerJoinDirective)
        resultLineChecker(eaAtEpisoderResults.Results.[58], isCompilationLineTypeNewJoinedItem) //MADE IT HERE
        resultLineChecker(eaAtEpisoderResults.Results.[59], isCompilationLineTypeEmptyLine)
        resultLineChecker(eaAtEpisoderResults.Results.[60], isCompilationLineTypeEmptyLine)
        resultLineChecker(eaAtEpisoderResults.Results.[61], isCompilationLineAFileEnd)
      ]