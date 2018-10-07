module EaTest
open Expecto
open Expecto.Logging
open Expecto.Logging.Message

let logger = Log.create "MyTests"
[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
