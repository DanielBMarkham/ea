namespace EA.EAR
module Util=
open Logary
open SystemTypeExtensions
open SystemUtilities
open CommandLineHelper
open EA.Types
open EA.Lenses
open EA.Persist
open EA.Utilities


let inputStuff:LoadMasterFile = (fun opts->
     (opts, {MasterModelText=[||]})
)
let doStuff:RunTransformsAndFilters = (fun (opts, masterModel)->
    (opts, {MasterModelText=[||]})
)

let outputStuff:WriteOutModelReportType = (fun (opts, transformedModel)->
    0 //  it's always successful as far as the O/S is concerned
)

#nowarn "0067"
let newMain (argv:string[]):int  =
    try
        loadEARConfigFromCommandLine argv |> inputStuff  |> doStuff |> outputStuff
    with
        | :? UserNeedsHelp as hex ->
            defaultEARBaseOptions.PrintThis
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