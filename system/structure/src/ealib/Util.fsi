namespace EA.Core
  module Util=
    open System
    open SystemTypeExtensions
    open SystemUtilities
    open CommandLineHelper
    open EA.Types
    open EA.Lenses
    open EA.Persist


    type CompilationUnitType = {
        Info:System.IO.FileInfo
        FileContents:string[]
    }
    type CompilationResultType = {
        MasterModelText:string[]
        }

    val Compile:CompilationUnitType[]->CompilationResultType