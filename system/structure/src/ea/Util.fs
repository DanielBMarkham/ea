module Util
open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
open EATypeExtensions
open EALenses
open EAPersist
open EAAppUtilities

//let inputStuff:GetCompileDataType =
// let doStuff:RunCompilationType=
//     0
// let outputStuff:WriteOutCompiledModelType->
//     0

#nowarn "0067"
let newMain (argv:string[]):int  =
    try
        let opts = loadEAConfigFromCommandLine argv
        //commandLinePrintWhileEnter opts.ConfigBase (opts.PrintThis)
        //opts |> inputStuff |> doStuff |> outputStuff
        0
    with
        | :? UserNeedsHelp as hex ->
            defaultEABaseOptions.PrintThis
            0
        | ex ->
            System.Console.WriteLine ("Program terminated abnormally " + ex.Message)
            System.Console.WriteLine (ex.StackTrace)
            if ex.InnerException = null
                then
                    1
                else
                    System.Console.WriteLine("---   Inner Exception   ---")
                    System.Console.WriteLine (ex.InnerException.Message)
                    System.Console.WriteLine (ex.InnerException.StackTrace)
                    1