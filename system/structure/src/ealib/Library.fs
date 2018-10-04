namespace ealib

open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
open EATypeExtensions
open EALenses
open EAPersist
open EAAppUtilities
open Util

module Say =
    let hello name =
        printfn "Hello %s" name
