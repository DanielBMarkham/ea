namespace EA.Core

open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
open EA.Types
open EA.Lenses
open EA.Persist
open EA.Utilities
open EA.Core.Util

module Say =
    let hello name =
        printfn "Hello %s" name
