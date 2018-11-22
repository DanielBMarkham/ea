namespace EA.Test
  module LineID2Tests=
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
  Recognize and sort lines by line type
    PARENT
	  Organize files by line type
	  Be easy-to-use
  Print out project To-Do items
    PARENT
	  Organize files by line type
	  Be easy-to-use



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
  Must be able to consume anything it produces without the model changing
  Must allow markdown language anywhere text is used
  Must be useable from the command line just like GCC
  Must use latest version of dotnet
  Philosophies to use: microservices, unix philosohphy, pure FP, DDD

META BEHAVIOR
  Record item titles in any of the three buckets
  Create sprint stories by matching titles
  Change story details (title, ac, etc)
  Create sprint story cards
  Type in the code that creates the compiler and it compiles

BUSINESS BEHAVIOR
 Squash conversation notes

BUSINESS SUPPLEMENTALS
  Be easy-to-use
    Work like linux
    Run fast
    Run anywhere
    Log stuff if you need to
    Run everywhere

SYSTEM SUPPLEMENTALS
  Support a command-line verbosity switch
SYSTEM BEHAVIOR
  Concatenate files

USER STORIES
  Process files without doing anything
    PARENT
	  Concatenate files
	  Be easy-to-use
  Process files and concatenate by filename
    PARENT
	  Concatenate files
	  Be easy-to-use

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

namespace EasyAM2
  BUSINESS ABSTRACT BEHAVIOR
    * Organize a set of notes I took today
      ASA Person trying to learn about this thing we want to build
      WHEN I'm talking to the other guys and the customer
      INEEDTO Organize all of my notes according to Structured Analysis
      SOTHAT
        * I can find stuff easily later
        * I can organize and prioritize important questions for the next time we start chatting

  USER STORIES
      @episode=2 @status=done @testscript=test_episode2.sh
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
      @episode=2 @status=done @testscript=test_episode2.sh
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
      //let resultLines = snd compilerResults
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
      testCase "FirstLine processing" <| fun _ ->
        if (isCompilationLineTypeEmptyLine episode3Results.Results.[1] = false) then Tests.failtest ("First line should be empty line type but is not " + episode3Results.Results.[1].Type.ToString()) else()
      ]