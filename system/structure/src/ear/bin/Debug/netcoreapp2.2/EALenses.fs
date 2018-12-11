namespace EA
  module Lenses=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA
    open EA.Types
    open Logary

    // Tag-list for the logger is namespace, project name, file name
    let moduleLogger = logary.getLogger (PointName [| "EA"; "Lenses"; "EALenses" |])
    // For folks on anal mode, log the module being entered.  NounVerb Proper Case
    logEvent Verbose "Module enter...." moduleLogger
    Console.WriteLine "whoa"



    // Maybe there is a better way of doing this. Just not today. (Or should this be the start of the lenses...)
    // Discussion of the choice of type-methods, functions, or lenses based on project direction
    let isCompilationLineACommand (line:CompilationLine) =
      match line.Type with |CommandMatch ct->true |_->false
    let isCompilationLineALineType (line:CompilationLine) =
      match line.Type with |LineType lt->true |_->false
    let isCompilationLineAFileBegin(line:CompilationLine):bool=
      match line.Type with |LineType(EasyAMLineTypes.FileBegin)->true |_->false
    let isCompilationLineAFileEnd(line:CompilationLine):bool=
      match line.Type with |LineType(EasyAMLineTypes.FileEnd)->true |_->false
    let isCompilationLineAFileMarker (line:CompilationLine):bool= (isCompilationLineAFileBegin line || isCompilationLineAFileEnd line)
    let isCompilationLineFreeFormText(line:CompilationLine):bool=
      match line.Type with 
        |LineType(EasyAMLineTypes.FreeFormText)->true 
        |CommandMatch cm->cm.LineType=EasyAMCommandType.None // there's no command. It has to be freeform text
        |_->false
    let isCompilationLineTypeUnprocessed (line:CompilationLine) =
      match line.Type with |LineType(EasyAMLineTypes.Unprocessed)->true |_->false
    let isCompilationLineTypeFileBegin (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.FileBegin)->true |_->false
    let isCompilationLineTypeFileEnd (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.FileEnd)->true |_->false
    let isCompilationLineTypeNewSectionItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.NewSectionItem)->true |_->false
    let isCompilationLineTypeNewJoinedItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.NewJoinedItem)->true |_->false
    let isCompilationLineTypeCompilerJoinDirective (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerJoinDirective)->true |_->false
    let isCompilationLineTypeNewItemJoinCombination (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.NewItemJoinCombination)->true |_->false
    let isCompilationLineTypeCompilerNamespaceDirectiveWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerNamespaceDirectiveWithItem)->true |_->false
    let isCompilationLineTypePoundTagWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.PoundTagWithItem)->true |_->false
    let isCompilationLineTypeMentionTagWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.MentionTagWithItem)->true |_->false
    let isCompilationLineTypeNameValueTagWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.NameValueTagWithItem)->true |_->false
    let isCompilationLineTypeCompilerSectionDirectiveWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerSectionDirectiveWithItem)->true |_->false
    let isCompilationLineTypeCompilerJoinTypeWithItem (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerJoinTypeWithItem)->true |_->false
    let isCompilationLineTypeCompilerNamespaceDirective (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerNamespaceDirective)->true |_->false
    let isCompilationLineTypeCompilerTagReset (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerTagReset)->true |_->false
    let isCompilationLineTypeCompilerSectionDirective (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.CompilerSectionDirective)->true |_->false
    let isCompilationLineTypeFreeFormText (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.FreeFormText)->true |_->false
    let isCompilationLineTypeEmptyLine (line:CompilationLine) = 
      match line.Type with |LineType(EasyAMLineTypes.EmptyLine)->true |_->false

    
    // For folks on anal mode, log the module being exited.  NounVerb Proper Case
    logEvent Verbose "....Module exit" moduleLogger




// Do I need an FSI file? I don't think so. If so, here's what goes in it (so far)

    // helpers/lenses
    //val isCompilationLineACommand:CompilationLine->bool
    //val isCompilationLineALineType:CompilationLine->bool
    //val isCompilationLineAFileBegin:CompilationLine->bool
    //val isCompilationLineAFileEnd:CompilationLine->bool
    //val isCompilationLineAFileMarker:CompilationLine->bool 
    //val isCompilationLineFreeFormText:CompilationLine->bool

    //val isCompilationLineTypeUnprocessed:CompilationLine->bool
    //val isCompilationLineTypeFileBegin:CompilationLine->bool
    //val isCompilationLineTypeFileEnd:CompilationLine->bool
    //val isCompilationLineTypeNewSectionItem:CompilationLine->bool
    //val isCompilationLineTypeNewJoinedItem:CompilationLine->bool
    //val isCompilationLineTypeCompilerJoinDirective:CompilationLine->bool
    //val isCompilationLineTypeNewItemJoinCombination:CompilationLine->bool
    //val isCompilationLineTypeCompilerNamespaceDirectiveWithItem:CompilationLine->bool
    //val isCompilationLineTypePoundTagWithItem:CompilationLine->bool
    //val isCompilationLineTypeMentionTagWithItem:CompilationLine->bool
    //val isCompilationLineTypeNameValueTagWithItem:CompilationLine->bool
    //val isCompilationLineTypeCompilerSectionDirectiveWithItem:CompilationLine->bool
    //val isCompilationLineTypeCompilerJoinTypeWithItem:CompilationLine->bool
    //val isCompilationLineTypeCompilerNamespaceDirective:CompilationLine->bool
    //val isCompilationLineTypeCompilerTagReset:CompilationLine->bool
    //val isCompilationLineTypeCompilerSectionDirective:CompilationLine->bool
    //val isCompilationLineTypeFreeFormText:CompilationLine->bool
    //val isCompilationLineTypeEmptyLine:CompilationLine->bool
