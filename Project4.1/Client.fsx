#load "./Data.fsx"
#load "./Actor.fsx"
#load "./Server.fsx"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote"

open Data
open Actor
open Server

open System
open System.Threading
open System.Collections.Generic
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit
open Akka.Remote

type ClientMsg =
    | Register of String * String // (userId * passWord)   
    | LogIn of String * String 
    | LogOut of String
    | Follow of String * String * String // (userId * password * followUserId) 
    | Tweet of String * String * String // (userId * password * content)
    | ReTweet of String * String * String // (curUserId * password * tweetId)
    | Query of String * String   // (Query type * [hasTag, mentionedId, curUserId]  --- for Query type : 0. only hashTag 1. only mentioned 2. hashTag and mentioned)
    | AutoQuery of string 
    

let server = system.ActorSelection(url + "APIsHandler")

let client (name : string) = 
    let mutable logInStatus = false
    spawn system name 
    <| fun mailbox ->
            let rec loop() =
                actor {
                    // let server = system.ActorSelection(url + "server")
                    let! message = mailbox.Receive()
                    match box message :?> ClientMsg with
                    | Register(userId, password) ->
                        let cmd = "Register|" + userId + "|" + password + "|" + "arg1" + "|"
                        server <? cmd |> Async.RunSynchronously
                    
                    | LogIn(userId, password) ->
                        logInStatus <- true
                        let client = system.ActorSelection(url + userId)
                        client <? AutoQuery(userId) 

                    | LogOut(userId) ->
                        logInStatus <- false

                    | Follow(userId, password, followUserId) ->
                        let cmd = "Follow|" + userId + "|" + password + "|" + "followUserId" + "|"
                        server <? cmd |> Async.RunSynchronously

                    | Tweet(authorId, password, content) ->
                        let cmd = "Tweet|" + authorId + "|" + password + "|" + content 
                        server <? cmd |> Async.RunSynchronously

                    | ReTweet(userId, password, oldTweetId) ->
                        let cmd = "ReTweet|" + userId + "|" + password + "|" + oldTweetId
                        server <? cmd |> Async.RunSynchronously

                    | Query(arg0, arg1) ->
                        let cmd = arg0 + "|" + arg1 + "|||" // arg0 = "Tag"/"Mention"
                        server <? cmd |> Async.RunSynchronously

                    | AutoQuery(userId) ->
                        while logInStatus do
                            let cmd = "Query|"  + userId + "|||" 
                            server <? cmd |> ignore
                            Thread.Sleep 10000   

                    return! loop()
                }
            loop()  

let test() = 
    let size = 10
    let mid = size / 2
    let rand = System.Random()
    
    // Calcuate the frequency of users base on zipf distribution : P(r) = C / (rank^gama), where C and gama are factors and r is rank of requency.
    let mutable zipfMap = new Map<int, int>([]) // Map<index, zipfNumber> zipfNumber is the number that this user index be subscribed and 
    let C = 5
    let gama = 1.5

    let mutable logInStaMap = new Map<String, Boolean>([])
    // All account need to be registered fistly and then log in
    for i = 0 to size do
        let id = "user" + string i
        let client = system.ActorSelection(id)
        let password = id + "_password" + string i
        client <? Register(id, password) |> ignore // change command
        Thread.Sleep 20

        client <? LogIn(id, password) |> ignore
        logInStaMap <- logInStaMap.Add(id, true)

        let rank = float i
        let zipfNum = int (float(size * C) / (rank ** gama))       
        zipfMap <- zipfMap.Add(i, zipfNum)

    // Arrange the followers for each account on condition of zipf distribution.
    for i = 0 to size do 
        let followers : HashSet<int> = new HashSet<int>();
        let zipfNum = zipfMap.Item(i)
        let mutable j = 0
        while j < zipfNum do
            let mutable r = rand.Next() % size
            while (r.Equals i || followers.Contains(r)) do   
                r <- rand.Next() % size
            let followerId = "user" + string i
            let password = followerId + "_password" + string i
            let followUserId = "user" + string r
            let follower = system.ActorSelection(followerId)
            follower <? Follow(followerId, password, followUserId) |> ignore
            followers.Add(r) |> ignore

        Thread.Sleep 10

    let modNum = (size % 10) + 1
    for i = 0 to size do 
        let authorId = "user" + string i
        let password = authorId + "_password" + string i
        let zipfNum = zipfMap.Item(i)
        let tweetNum = int(0.7 * float(zipfNum))
        let mutable j = 0
        while j < tweetNum do
            let client = system.ActorSelection(authorId)
            let x = j % modNum
            if x = 0 then 
                let oldContent = "this is a retweet! "    // map.get the subscribers and then get the tweet list then extract the content.
                client <? ReTweet(authorId, password, oldContent)
            else 
                let content = "This is a tweet content from " + authorId + " and index is : " + "j"
                client <? Tweet(content, authorId) |> ignore
    

    for i = 0 to 1000 do   // A random number you could choose to test connecting and disconnecting.
        let r = rand.Next() % size
        let userId = "user" + string r
        let password = userId + "_password" + string r
        let client = system.ActorSelection(userId)
        if logInStaMap.Item(userId) then 
            client <? LogOut(userId) |> ignore

        else 
            client <? LogIn(userId, password) |> ignore

    










        
