module EATypeExtensions
open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
open System

type CompilationUnit = {
    Info:System.IO.FileInfo
    FileContents:string[]
}
