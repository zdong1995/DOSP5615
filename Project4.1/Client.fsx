#load "./Data.fsx"
#load "./Message.fsx"
#load "./Server.fsx"
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote" 

open Data
open Message
open Server

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

let server = system.ActorSelection("APIsHandler")

let client (name : string) = 
    spawn system name 
    <| fun mailbox ->
            let rec loop() =
                actor {
                    let server = system.ActorSelection(url + "server")
                    let! message = mailbox.Receive()
                    match box message with
                    | :? string ->
                        let cmd = "Register, " + name + ", " + "password" + name
                        server <? cmd |> Async.RunSynchronously
                    return! loop()
                }
            loop()  

let test() = 
    for i = 0 to 10 do
        let id = "user" + string i
        let client = system.ActorSelection(id)
        client <? "" |> ignore // change command
