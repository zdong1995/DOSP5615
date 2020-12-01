#load "./Data.fsx"
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote" 

open Data
open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

let configuration = 
    ConfigurationFactory.ParseString(
        @"akka {
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

let url = "akka.tcp://RemoteFSharp@localhost:8777/user/"
// let serverUrl = "akka.tcp://RemoteFSharp@localhost:8777/user/server"

type Message =
    |Register of String * String // (userId * passWord)
    |Tweet of String[] // (0. tweetContent 1. tweetId 2. authorId 3. hashTag 4. mention)
    |Subscribe of String * String // (curUserId * followUserId)
    |ReTweet of String * String // (tweetId * curUserId)
    |Query of int * String[] // (Query type * [hasTag, mentionedId, curUserId]  --- for Query type : 0. only hashTag 1. only mentioned 2. hashTag and mentioned)
    |RegResponse of String

// Define some structures to contain information.
let mutable userMap : Map<String, String> = Map.empty
let mutable tweetInfo : Map<String, String[]> = Map.empty // ()

let system = ActorSystem.Create("RemoteFSharp", configuration)

let server = 
    spawn system "server" 
    <| fun mailbox ->
            let rec loop() =
                actor {
                    let! message = mailbox.Receive()
                    match box message :?> Message with
                    | Register(userId, passWord) ->
                        printfn "the server receive the reg msg from client!"
                        let mutable regResMsg = ""   
                        if  userMap.ContainsKey(userId) then  
                            regResMsg <- "the registration failed for"
                            
                        else 
                            userMap <- userMap.Add(userId, passWord)
                            regResMsg <- "the registration succeeded for "                    

                        regResMsg <- regResMsg + userId
                        let curClient = system.ActorSelection(url + userId)
                        curClient <! RegResponse(regResMsg)
                    
                    | _-> ()
                    
                    return! loop()
                }
            loop()  

let client (name : string) = 
    let mutable userId = ""
    let mutable passWord = ""
    spawn system name 
    <| fun mailbox ->
            let rec loop() =
                actor {
                    let server = system.ActorSelection(url + "server")
                    let! message = mailbox.Receive()
                    match box message :?> Message with
                    | Register(userId, passWord) -> 
                        printfn "client receive reg msg request!"  
                        let userId = userId
                        let passWord = passWord
                        printfn "the userId is : %s and the passWord is : %s" userId passWord 
                        server <! Register(userId, passWord)
                    
                    | RegResponse(msg) ->
                        printfn "%s" msg
                 
                    return! loop()
                }
            loop()  

let userName = "XiaobaiLi"
let passWord = "woaihaizei"

// let engine = system.ActorSelection(url + "server")
// engine <! Register(userName, passWord)
let curClient = client userName
curClient <! Register(userName, passWord)
