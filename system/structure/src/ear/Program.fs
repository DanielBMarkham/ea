namespace EA.EAR
module Main=
open System

open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
open EA.Types
open EA.Lenses
open EA.Persist
open EA.Utilities

open Util


[<EntryPoint>]
let main argv = Util.newMain argv
