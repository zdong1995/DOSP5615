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
    | LogIn of String * String // （userId * passWord）
    | LogOut of String
    | Follow of String * String * String // (userId * password * toFollowId) 
    | Tweet of String * String * String // (userId * password * content)
    | ReTweet of String * String * String * String // (curUserId * password * tweetContent * oldAuthorId)
    | Query of int * String   // (Query type * [hasTag, mentionedId, curUserId]  --- for Query type : 0. only hashTag 1. only mentioned 2. hashTag and mentioned)
    | AutoQuery of string 

let server = system.ActorSelection(url + "APIsHandler")

let client (name : string) = 
    let mutable logInStatus = false
    spawn system name 
    <| fun mailbox ->
            let rec loop() =
                actor {
                    let! message = mailbox.Receive()
                    let sender = mailbox.Sender()
                    let mutable cmd = ""
                    let mutable res = ""
                    match box message :?> ClientMsg with

                    | LogIn(userId, password) ->
                        // printfn "%s receive the login message from console" userId
                        cmd <- "Login|" + userId + "|" + password + "||"
                        let mutable auth = false
                        auth <- Async.RunSynchronously(server <? cmd)
                        res <- auth |> string
                        if auth then
                            logInStatus <- true
                            let client = system.ActorSelection(url + userId)
                            client <? AutoQuery(userId) |> ignore

                    | LogOut(userId) ->
                        logInStatus <- false
                        // TODO
                    
                    | Register(userId, password) ->
                        // printfn "%s receive the reg message from console" userId
                        cmd <- "Register|" + userId + "|" + password + "||"
                        res <- Async.RunSynchronously(server <? cmd) |> string

                    | Follow(userId, password, toFollowId) ->
                        // printfn "%s receive the follow message from console to follow %s" userId toFollowId
                        cmd <- "Follow|" + userId + "|" + password + "|" + toFollowId + "|"
                        res <- Async.RunSynchronously(server <? cmd) |> string

                    | Tweet(authorId, password, content) ->
                        // printfn "%s receive the tweet message from console and the content is : %s" authorId content
                        cmd <- "Tweet|" + authorId + "|" + password + "|" + content + " |"
                        // printfn "%s" cmd
                        res <- Async.RunSynchronously(server <? cmd) |> string

                    | ReTweet(userId, password, oldContent, oldAuthorId) ->
                        printfn "%s receive the tweet message from console and the content is : %s" userId oldContent
                        let cmd = "ReTweet|" + userId + "|" + password + "|" + oldContent + " |" + oldAuthorId
                        res <- Async.RunSynchronously(server <? cmd) |> string

                    | Query(x, arg) ->
                        if x = 0 then 
                            cmd <- "Query|" + arg + "|||" 
                        
                        elif x = 1 then 
                            cmd <- "Tag|" + arg + "|||"

                        elif x = 2 then 
                            cmd <- "Mention|" + arg + "|||"

                        res <- Async.RunSynchronously(server <? cmd) |> string

                    | AutoQuery(userId) -> ()
                        // while logInStatus do
                        //     let cmd = "Query|"  + userId + "|||" 
                        //     server <? cmd |> ignore
                        //     Thread.Sleep 50000   

                    sender <? res |> ignore
                    // printf "%A" res
                    return! loop()
                }
            loop()  

let args : string array = fsi.CommandLineArgs |> Array.tail
let mutable numClients = args.[0] |> int

let mutable regTime = 0.0
let mutable logInTime = 0.0
let mutable fTime = 0.0

let mutable tTime = 0.0
let mutable rtTime = 0.0

let mutable uqTime = 0.0
let mutable tqTime = 0.0
let mutable mqTime = 0.0

let test() = 
    let rand = System.Random()
    let mutable logInStaMap = new Map<String, Boolean>([])
    let mutable reTweetMap = new Map<String, String list>([])
    let mutable subscribeMap = new Map<String, String list>([])    
        
    printfn "-------------calculate the zipf distributions-----------"
    let mutable zipfMap = new Map<int, int>([]) // Map<index, zipfNumber> zipfNumber is the number that this user index be subscribed and 
    let C = 5
    let gama = 1.5

    for i = 1 to numClients do 
        let rank = float i
        let zipfNum = int (float(numClients * C) / (rank ** gama))       
        zipfMap <- zipfMap.Add(i, zipfNum)

        let zip = zipfMap.Item(i)
        printfn "the zip num for %i is : %i" i zip

    printfn "--------------Register Users----------------"
    let watch = System.Diagnostics.Stopwatch.StartNew()
    for i = 1 to numClients do
        let userId = "user" + string i
        client userId |> ignore
        let client = system.ActorSelection(url + userId)
        let password = userId + "_password"
        let res = Async.RunSynchronously(client <? Register(userId, password)) |> string
        printfn "%s" res // need cast the response to string to print
        Thread.Sleep 5

    watch.Stop()
    regTime <- watch.Elapsed.TotalMilliseconds

    printfn "--------------Login Users----------------"

    let watch = System.Diagnostics.Stopwatch.StartNew()
    for i = 1 to numClients do
        let userId = "user" + string i
        let client = system.ActorSelection(url + userId)
        let password = userId + "_password"
        let res = Async.RunSynchronously(client <? LogIn(userId, password)) |> string
        printfn "%s" res // need cast the response to string to print
        Thread.Sleep 5
        
    watch.Stop()
    logInTime <- watch.Elapsed.TotalMilliseconds

    printfn "-------------Follow Users----------------"

    let watch = System.Diagnostics.Stopwatch.StartNew()
    for i = 1 to numClients do 
       let followers : int Set = Set.empty       
       let followerId = "user" + string i
       let password = followerId + "_password"
       let client = system.ActorSelection(url + followerId)
       let zipfNum = zipfMap.Item(i)
        
       for j = 1 to zipfNum do
           let mutable r = rand.Next() % numClients        
           while (r.Equals i || followers.Contains(r)) do   
               r <- rand.Next() % numClients          
           // printf "the one follower of user%i is : %i" i r
           let toFollowId = "user" + string r
           let res = Async.RunSynchronously(client <? Follow(followerId, password, toFollowId)) |> string
           printf "%s" res
           Thread.Sleep 5
           followers.Add(r) |> ignore
           if not (subscribeMap.ContainsKey(followerId)) then
               subscribeMap <- subscribeMap.Add(followerId, List.empty)
            
           subscribeMap <- subscribeMap.Add(followerId, List.append subscribeMap.[followerId] [toFollowId])      

    watch.Stop()
    fTime <- watch.Elapsed.TotalMilliseconds

    printfn "--------------Tweet----------------"

    let watch = System.Diagnostics.Stopwatch.StartNew()
    for i = 1 to numClients do
        let userId = "user" + string i
        let client = system.ActorSelection(url + userId)
        let password = userId + "_password"
        let tweetNum = zipfMap.Item(i)
        for j = 1 to tweetNum do
            let index = string j
            let tagContent = "#Tag" + string (i % 5)
            let mutable mentionInd = (i+3) % numClients
            if mentionInd = 0 then 
                mentionInd <- mentionInd + 1
            let mentionId = "user" + string mentionInd
            let content = tagContent + " This is a tweet and index is " + index + " from "  + userId + " @" + mentionId
            printfn "%s" content
            let res = Async.RunSynchronously(client <? Tweet(userId, password, content)) |> string
            printfn "%s" res // need cast the response to string to print
            Thread.Sleep 5

    watch.Stop()
    tTime <- watch.Elapsed.TotalMilliseconds

    printfn "--------------ReTweet----------------"

    let watch = System.Diagnostics.Stopwatch.StartNew()
    for i = 2 to numClients do
        let userId = "user" + string i
        let lastUser = "user" + string (i - 1)
        let client = system.ActorSelection(url + userId)
        let password = userId + "_password"
        let res = Async.RunSynchronously(client <? ReTweet(userId, password, "This is a tweet from " + lastUser, lastUser )) |> string
        printfn "%s" res // need cast the response to string to print
        Thread.Sleep 5
    
    watch.Stop()
    rtTime <- watch.Elapsed.TotalMilliseconds

    printfn "--------------UserId Query----------------"
    let uqWatch = System.Diagnostics.Stopwatch.StartNew()
    for i = 1 to numClients do
        let userId = "user" + string i
        let client = system.ActorSelection(url + userId)
        let tagContent = "#Tag" + string (i % 5)

        let res1 = Async.RunSynchronously(client <? Query(0, userId)) |> string
        printfn "%s" res1
        Thread.Sleep 5

    uqWatch.Stop()
    uqTime <- uqWatch.Elapsed.TotalMilliseconds


    printfn "--------------Tag Query----------------"
    let tqWatch = System.Diagnostics.Stopwatch.StartNew()
    for i = 1 to numClients do
        let userId = "user" + string i
        let client = system.ActorSelection(url + userId)
        let tagContent = "#Tag" + string (i % 5)

        let res2 = Async.RunSynchronously(client <? Query(1, tagContent)) |> string
        printfn "%s" res2
        Thread.Sleep 5
    tqWatch.Stop()
    tqTime <- tqWatch.Elapsed.TotalMilliseconds

    printfn "--------------Mentioned Query----------------"
    let mqWatch = System.Diagnostics.Stopwatch.StartNew()
    for i = 1 to numClients do
        let userId = "user" + string i
        let client = system.ActorSelection(url + userId)

        let res3 = Async.RunSynchronously(client <? Query(2, userId)) |> string
        Thread.Sleep 5
        printfn "%s" res3

    mqWatch.Stop()
    mqTime <- mqWatch.Elapsed.TotalMilliseconds


test()
printfn "----------------RunningTime Measuring----------------"
printfn "the Registration time is : %f ms" regTime
printfn "the Log In time is : %f ms" logInTime
printfn "the Follow time is : %f ms" fTime
printfn "the Tweet time is : %f ms" tTime
printfn "the ReTweet time is : %f ms" rtTime
printfn "the UserId Query runing time is : %f ms" uqTime
printfn "the Tag Query runing time is : %f ms" tqTime
printfn "the Mentioned Query runing time is : %f ms" mqTime
