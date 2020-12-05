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

printfn "--------------Register Users----------------"

// register users
for i = 1 to 10 do
    let userId = "user" + string i
    client userId |> ignore
    let client = system.ActorSelection(url + userId)
    let password = userId + "_password"
    let res = Async.RunSynchronously(client <? Register(userId, password)) |> string
    printfn "%s" res // need cast the response to string to print
    Thread.Sleep 20

printfn "--------------Login Users----------------"

// login users
for i = 1 to 10 do
    let userId = "user" + string i
    let client = system.ActorSelection(url + userId)
    let password = userId + "_password"
    let res = Async.RunSynchronously(client <? LogIn(userId, password)) |> string
    printfn "%s" res // need cast the response to string to print
    Thread.Sleep 20
    

printfn "--------------Tweet----------------"

// login users
for i = 1 to 10 do
    let userId = "user" + string i
    let client = system.ActorSelection(url + userId)
    let password = userId + "_password"
    let res = Async.RunSynchronously(client <? Tweet(userId, password, "This is a tweet from" + userId )) |> string
    printfn "%s" res // need cast the response to string to print
    Thread.Sleep 20

printfn "--------------ReTweet----------------"

// login users
for i = 2 to 10 do
    let userId = "user" + string i
    let lastUser = "user" + string (i - 1)
    let client = system.ActorSelection(url + userId)
    let password = userId + "_password"
    let res = Async.RunSynchronously(client <? ReTweet(userId, password, "This is a tweet from " + lastUser, lastUser )) |> string
    printfn "%s" res // need cast the response to string to print
    Thread.Sleep 20

printfn "-------------Zipf Distribution------------"
let args : string array = fsi.CommandLineArgs |> Array.tail

let mutable numClients = args.[0] |> int

let test() = 
    let size = numClients
    let rand = System.Random()
    
    // // Calcuate the frequency of users base on zipf distribution : P(r) = C / (rank^gama), where C and gama are factors and r is rank of requency.
    let mutable zipfMap = new Map<int, int>([]) // Map<index, zipfNumber> zipfNumber is the number that this user index be subscribed and 
    let C = 5
    let gama = 1.5

    let mutable logInStaMap = new Map<String, Boolean>([])
    let mutable reTweetMap = new Map<String, String list>([])
    let mutable subscribeMap = new Map<String, String list>([])
    // // All account need to be registered fistly and then log in
    for i = 1 to size do
        let userId = "user" + string i
        client userId
        let client = system.ActorSelection(url + userId)
        let password = userId + "_password" + string i
        client <? Register(userId, password) |> ignore // change command
        Thread.Sleep 20

        client <? LogIn(userId, password) |> ignore
        logInStaMap <- logInStaMap.Add(userId, true)
        Thread.Sleep 20

        let rank = float i
        let zipfNum = int (float(size * C) / (rank ** gama))       
        zipfMap <- zipfMap.Add(i, zipfNum)

    Thread.Sleep 500

    // Arrange the followers for each account on condition of zipf distribution.
    for i = 1 to size do 
        let followers : int Set = Set.empty       
        let followerId = "user" + string i
        let password = followerId + "_password" + string i
        let follower = system.ActorSelection(url + followerId)
        let zipfNum = zipfMap.Item(i)
        
        for j = 1 to zipfNum do
            let mutable r = rand.Next() % size          
            while (r.Equals i || followers.Contains(r)) do   
                r <- rand.Next() % size           
            // printf "the one follower of user%i is : %i" i r
            let followUserId = "user" + string r
            follower <? Follow(followerId, password, followUserId) |> ignore
            Thread.Sleep 20
            followers.Add(r) |> ignore
            if not (subscribeMap.ContainsKey(followerId)) then
                subscribeMap <- subscribeMap.Add(followerId, List.empty)
            
            subscribeMap <- subscribeMap.Add(followerId, List.append subscribeMap.[followerId] [followUserId])      

    // Test the tweet function.
    for i = 1 to size do 
        let authorId = "user" + string i
        let password = authorId + "_password" + string i
        let client = system.ActorSelection(url + authorId)
        let zipfNum = zipfMap.Item(i)
        let tweetNum = int(1.2 * float(zipfNum))
        for j = 1 to tweetNum do
            let ind = string j
            let mentionedId = "user" + string ((rand.Next() % size) + 1)
            let content = "#Tag" + string i + " this is a twitter content written by " + authorId + " and the index is " + ind + " " + mentionedId
            // * printfn "%s  " content
            // printfn "the client is : %A" client
            client <? Tweet(authorId, password, content) |> ignore
            if not (reTweetMap.ContainsKey(authorId)) then 
                reTweetMap <- reTweetMap.Add(authorId, List.empty)

            reTweetMap <- reTweetMap.Add(authorId, List.append reTweetMap.[authorId] [content])
            Thread.Sleep 50
    
    // Test the retweet function.
    let mutable i = 1
    let mutable breakLoop = false
    while not breakLoop do 
        let authorId = "user" + string i
        let password = authorId + "_password" + string i
        let client = system.ActorSelection(url + authorId)
        let zipfNum = zipfMap.Item(i)
        let mutable rtNum = int(0.5 * float(zipfNum))
        printfn " the rtNum of %s is %i " authorId rtNum
    
        let tweetArr = reTweetMap |> Map.toSeq |> Seq.map snd |> Seq.toArray
        let mutable j = 1
        while j <= rtNum do           
            printfn "now for user %i the round number is %i" i j
            let mutable r = (rand.Next() % tweetArr.Length)
            while r < 1 || r > tweetArr.Length do
                r <- (rand.Next() % tweetArr.Length)

            let rtAuthorId = "user" + string r
            let oldContent = reTweetMap.Item(rtAuthorId).Head
            printfn "for %s, the old content is : %s and old author is %s !" authorId oldContent rtAuthorId
            client <? ReTweet(authorId, password, oldContent, rtAuthorId) |> ignore
            j <- j + 1
            Thread.Sleep 50

        i <- i + 1    
        if i > size || rtNum = 0 then 
            breakLoop <- true
            
    Test the query function
    for i = 1 to size do
        let userId = "user" + string i
        let password = userId + "_password" + string i
        let client = system.ActorSelection(url + userId)
        let tagContent = "#Tag" + string i

        client <? Query(0, userId) |> ignore
        Thread.Sleep 10
        client <? Query(1, tagContent) |> ignore
        Thread.Sleep 10
        client <? Query(2, userId) |> ignore
        Thread.Sleep 10

    for i = 0 to 1000 do   // A random number you could choose to test connecting and disconnecting.
        let r = rand.Next() % size
        let userId = "user" + string r
        let password = userId + "_password" + string r
        let client = system.ActorSelection(userId)
        if logInStaMap.Item(userId) then 
            client <? LogOut(userId) |> ignore

        else 
            client <? LogIn(userId, password) |> ignore
test()