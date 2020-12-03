module Handler

#load "./Data.fsx"
#load "./Server.fsx"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote"

open Data
open Server

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit
open Akka.Remote

let configuration =
    ConfigurationFactory.ParseString
        (@"akka {
            log-config-on-start : on
            stdout-loglevel : DEBUG
            loglevel : ERROR
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                }
            }
            remote {
                helios.tcp {
                    port = 8777
                    hostname = localhost
                }
            }
        }")

let url =
    "akka.tcp://RemoteFSharp@localhost:8777/user/"

let system =
    ActorSystem.Create("RemoteFSharp", configuration)

let Simulator = Simulator()

let RegisterHandler =
    spawn system "RegisterHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgRegister(username, password) ->
                    let response = Simulator.Register(username, password)
                    printfn "Register response for %A: %A " username response
                    sender <? response |> ignore
                | _ -> failwith "Exception"
                return! loop ()
            }
        loop ()

let FollowHandler =
    spawn system "FollowHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgFollow(username, password, toFollow) ->
                    let response = Simulator.Follow(username, password, toFollow)
                    printfn "Follow response for %A: %A " username response
                    sender <? response |> ignore
                | _ -> failwith "Exception"
                return! loop ()
            }
        loop ()

let TweetHandler =
    spawn system "TweetHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgTweet(username, password, content) ->
                    let response = Simulator.SendTweet(username, password, content)
                    printfn "Tweet response for %A: %A " username response
                    sender <? response |> ignore
                | _ -> failwith "Exception"
                return! loop ()
            }
        loop ()

let ReTweetHandler =
    spawn system "ReTweetHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgReTweet(username, password, content, reTweetFrom) ->
                    let response = Simulator.ReTweet(username, password, content, reTweetFrom)
                    printfn "ReTweet response for %A: %A " username response
                    sender <? response |> ignore
                | _ -> failwith "Exception"
                return! loop ()
            }
        loop ()

let APIsHandler =
    spawn system "APIsHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                // printf "%A" message
                let sender = mailbox.Sender()

                match box message with
                | :? string ->
                    if message = "" then
                        return! loop()
                    
                    let mutable handler = system.ActorSelection(url + "")
                    let mutable msg = MsgEmpty("")
                    // Register, username, password, content/toFollow
                    let commands = message.Split('|')
                    let operation = commands.[0]
                    let username = commands.[1]
                    let password = commands.[2]
                    let arg1 = commands.[3]
                    let arg2 = commands.[4]
                    // printfn "%A" commands

                    match operation with
                    | "Register" ->
                        handler <- system.ActorSelection(url + "RegisterHandler")
                        msg <- MsgRegister(username, password)
                    | "Follow" ->
                        handler <- system.ActorSelection(url + "FollowHandler")
                        msg <- MsgFollow(username, password, arg1)
                    | "Tweet" ->
                        handler <- system.ActorSelection(url + "TweetHandler")
                        msg <- MsgTweet(username, password, arg1)
                    | "ReTweet" ->
                        handler <- system.ActorSelection(url + "ReTweetHandler")
                        msg <- MsgReTweet(username, password, arg1, arg2)

                    let res = Async.RunSynchronously(handler <? msg, 1000)
                    sender <? res |> ignore
                return! loop ()
            }
        loop ()