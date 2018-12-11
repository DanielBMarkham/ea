  namespace EA
    type CaseSensitive=Default|IgnoreCase|CaseSensitive
    type EasyAMLineTypes =
      |Unprocessed
      |FileBegin
      |FileEnd
      |NewSectionItem
      |NewJoinedItem
      |CompilerJoinDirective
      |NewItemJoinCombination
      |CompilerNamespaceDirectiveWithItem
      |PoundTagWithItem
      |MentionTagWithItem
      |NameValueTagWithItem
      |CompilerSectionDirectiveWithItem
      |CompilerJoinTypeWithItem
      |CompilerNamespaceDirective
      |CompilerTagReset
      |CompilerSectionDirective
      |FreeFormText
      |EmptyLine
      interface System.IFormattable

    // Not the same as line types. Commands are what we search for. The line type depends on the context
    type EasyAMCommandType =
      |NewItem
      |Join
      |Namespace
      |Tag
      |SectionDirective
      |EmptyLine
      |None


    type RegexMatcherType = 
      {
        Regex:System.Text.RegularExpressions.Regex
        CaptureGroupsExpected:int
        PossibleLineTypes:EasyAMLineTypes list
      }
    type LineMatcherType = 
      {
        LineType:EasyAMCommandType;
        MatchTokens:RegexMatcherType []
      }
    // Couldn't decide for separate types or all Compilation Line
    type LineIdentification =
      /// We've identified the command on the line but we don't know what the line is
      |CommandMatch of LineMatcherType
      /// We've taken the command we identified and the context and figured out what kind of line it is
      |LineType of EasyAMLineTypes
    type CompilationLine =
      {
      ShortFileName:string
      FullFileName:string
      CompilationUnitNumber:int
      LineNumber:int 
      Type:LineIdentification
      LineText:string
      TextStartColumn:int
      }
      member LineTypeDescription:string
      member ToFileLocation:string 
      member ToFullLocation:string
    type CompilationUnitType = {
      Info:System.IO.FileInfo
      FileContents:string[]
      }

    type BucketWorkType =
      | DirectDecideAndChangeReality
      | EmergentDiscoverRealityAndDecideStrategy
    type BucketName =
      | Manage
      | Understand 
      | Execute 
      | Instantiate 
      | Deliver 
      | Optimize 
      | Plan 
      | Guess
    type BucketCategoryType =
      | ServicesOffered // What's our service/product DO that people want?
      | Values // What are our values and how do they relate to our work?
      | ServicesOfferedAndValues // The critical areas where these two come into conflict
    type BucketFocusType =
      | ChangeMentalModelBasedOnInteractions
      | ChangeInteractionsBasedOnMentalModel
      | AdjustHowMentalModelAndInteractionsWorkTogether
    type BucketColorType =
      | RGB of (int8*int8*int8)
      | HexColor of string
    type BucketType =
      {
        Name:BucketName
        Category:BucketCategoryType
        Focus: BucketFocusType
        WorkType:BucketWorkType
        BucketColor:BucketColorType
      }


    type BucketMetaTypeName =
      | Aim
      | Supplementals
      | Target
      | Service
    type BucketMetaTypeDrivers =
      {
        MakeItHappenEasy:BucketType
        MakeItHappen:BucketType
        MakeItHappenRight:BucketType
      }
    type BucketMetaType =
      {
        Name: BucketMetaTypeName
        Drivers: BucketMetaTypeDrivers
      }
    // May need to add roles in at some point
    type RoleSillyNameType =
      | TheDork
      | TheAutomatron
      | TheWeasel
      | TheFace
      | TheAsshole
      | TheDreamer
      | TheCodependent
      | TheSelfHelpGuru
      | TheExample
      | TheTeacher
      | TheProphet
      | TheEvangelist

    type RoleMetaType =
      | WorkForTheUsers
      | WorkForTheTeams
      | WorkForEverybody

    type TemporalIndicatorType =
      | Was
      | AsIs
      | ToBe 
    type GenreType =
      | Meta
      | Business
      | System
    type AbstractionLevelType =
      | Abstract
      | Realized

    type LocationPointerType =
      {
        Genre:GenreType
        AbstractionLevel:AbstractionLevelType
        Bucket: BucketType
        TemporaalIndicator:TemporalIndicatorType
        Tags:string[]
        Namespace:string[]
      }


