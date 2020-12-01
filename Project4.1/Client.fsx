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

type Message =
    |Register of String * String // (userId * passWord)
    |Tweet of String[] // (0. tweetContent 1. tweetId 2. authorId 3. hashTag 4. mention)
    |ReTweet of String * String // (tweetId * curUserId)
    |Subscribe of String * String // (curUserId * followUserId)    
    |Query of int * String[] // (Query type * [hasTag, mentionedId, curUserId]  --- for Query type : 0. only hashTag 1. only mentioned 2. hashTag and mentioned)


// Define some structures to contain information.
let mutable userMap : Map<String, String> = Map.empty
let mutable tweetInfo : Map<String, String[]> = Map.empty // ()
let mutable subscribeMap : Map<String, String[]> = Map.empty
let mutable followerMap : Map<String, String[]> = Map.empty

let system = ActorSystem.Create("RemoteFSharp", configuration)

let server = 
    spawn system "server" 
    <| fun mailbox ->
            let rec loop() =
                actor {
                    let! message = mailbox.Receive()
                    match box message :?> Message with
                    | Register(userId, passWord) ->
                        let mutable regResMsg = ""   
                        if  userMap.ContainsKey(userId) then  
                            regResMsg <- "Error! the registration failed for"
                            
                        else 
                            userMap <- userMap.Add(userId, passWord)
                            regResMsg <- "The registration succeeded for "                    

                        regResMsg <- regResMsg + userId
                        printfn "%s" regResMsg
                    
                    | Tweet(stringArr) ->
                        printfn "the server received the twitter from %s which content is： %s " stringArr.[2] stringArr.[0]
                        if tweetInfo.ContainsKey(stringArr.[1]) then 
                            printfn "Error! this tweet has already exist! "
                        else
                            tweetInfo <- tweetInfo.Add(stringArr.[1], stringArr)

                    | ReTweet(tweetId, curUserId) ->
                        

                    | Subscribe(followerId, subscribeId) ->    
                    
                    | Query(queryType, queryArr) ->

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
                        let userId = userId
                        let passWord = passWord
                        printfn "the userId is : %s and the passWord is : %s" userId passWord 
                        server <! Register(userId, passWord)
                    
                    | Tweet(stringArr) ->
                        // printf "the %s send a twitter and the content is : %s 
                        //        | the tweet id is %s | the hashTag is %s | mentioned %s " 
                        //        stringArr.[2] stringArr.[0] stringArr.[1] stringArr.[3] stringArr.[4]                        
                        server <! Tweet(stringArr)

                    | ReTweet(tweetId, curUserId) ->
                        printfn "client %s received a retweet msg and sent to server. " userId 
                        server <! ReTweet(tweetId, curUserId)
                    
                    | Subscribe(followerId, subscribeId) ->
                        printfn "client %s received a subscribe msg and sent to server. " userId
                        server <! Subscribe(followerId, subscribeId)

                    | Query(queryType, queryArr) ->
                        printfn "client %s received a retweet msg and sent to server. " userId
                        server <! Query(queryType, queryArr)

                    return! loop()
                }
            loop()  

let userId = "XiaobaiLi"
let passWord = "woaihaizei"

let curClient = client userId
curClient <! Register(userId, passWord)

let mutable oneTweet : string[] = Array.create 5 ""
oneTweet.[0] <- "this is the first tweet, hello! "
oneTweet.[1] <- userId + " tweet1"
oneTweet.[2] <- userId
oneTweet.[3] <- "#mydaily"
oneTweet.[4] <- "xiaoming"

curClient <! Tweet(oneTweet)
