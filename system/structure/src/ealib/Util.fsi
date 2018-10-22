namespace EA.Core
  module Util=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist

    // * I only needed two types and a function sig
    // * Stick with MA! She'll look after you
    // * YAGNI
    type CompilationUnitType = {
      Info:System.IO.FileInfo
      FileContents:string[]
      }
    type CompilationResultType = {
      MasterModelText:string[]
      }

    val Compile:CompilationUnitType[]->CompilationResultType