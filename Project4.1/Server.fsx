module Server

#load "./Data.fsx"
#load "./Actor.fsx"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote"

open Data
open Actor

open System
open System.Collections.Generic
open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit
open Akka.Remote

let mutable tweetTable = new Map<string, Tweet>([])
let mutable userTable = new Map<string, User>([])
let mutable tagTable = new Map<string, Tweet list>([])
let mutable mentionTable = new Map<string, Tweet list>([])
let mutable liveUser = new Set<String>([])

// extract all hashtags in the tweet content to return tags list
let ExtractTag(word: string, tag: char) =
    let mutable tags = List.empty
    let mutable index = 0
    while index < word.Length do
        if word.[index] = tag then
            // find end space of current tag
            let mutable endIdx = index
            while endIdx < word.Length && word.[endIdx] <> ' ' do
                endIdx <- endIdx + 1
            if endIdx <> word.Length
            then tags <- List.append tags [ word.[index..endIdx - 1] ]
            index <- endIdx + 1 // update index to search right
        else
            index <- index + 1
    tags
// serialize tweet list, return empty string if tweets list is empty
let SerializeList(tweets: Tweet list) =
    let mutable res = ""
    if tweets.Length <> 0 then
        for tweet in tweets do
            res <- res + tweet.Serializer() + "\n"
    res

type Simulator() =
    // methods to create new user
    member this.NewUser(user: User) =
        userTable <- userTable.Add(user.UserName, user)

    // getter of user
    member this.GetUser(username: string) =
        try
            userTable.[username]
        with :? KeyNotFoundException -> User("", "") // not exist

    // register user
    member this.Register(username: string, password: string) =
        let mutable response =
            "Username has already been used. Please choose a new one."

        if not (userTable.ContainsKey(username)) then
            this.NewUser(User(username, password))
            response <- username + " Registered successfully"

        response

    // authentication to return whether login success or fails (bool)
    member this.Login(username: string, password: string) =
        let mutable response = false
        if userTable.ContainsKey(username)
            && (userTable.[username].Password = password) then
            liveUser <- liveUser.Add username
            response <- true
        response

    member this.Logout(username: string) =
        let mutable response = false
        if liveUser.Contains(username) then
            liveUser <- liveUser.Remove username
            response <- true
        response

    // add one tweet to tagTable, if tag not exist, create one new record and add the tweet
    member this.AddToTagTable(hashTag: string, tweet: Tweet) =
        // initialize <key, value> pair with empty list
        if not (tagTable.ContainsKey(hashTag))
        then tagTable <- tagTable.Add(hashTag, List.empty)
        // add tweet to existed hashtag list in the map
        tagTable <- tagTable.Add(hashTag, List.append tagTable.[hashTag] [tweet])

    // add one tweet to mentionTable
    member this.AddToMentionTable(username: string, tweet: Tweet) =
        // initialize <key, value> pair with empty list
        if not (mentionTable.ContainsKey(username))
        then mentionTable <- mentionTable.Add(username, List.empty)
        // add tweet to existed hashtag list in the map
        mentionTable <- mentionTable.Add(username, List.append mentionTable.[username] [tweet])
    
    // wrapper of create new tweet for future re-use
    // auto parse hashtag to update table and mention table
    member this.NewTweet(username: string, password: string, content: string) =
        // use data time as unique key for tweetId
        let newTweet =
            Tweet(content, DateTime.Now.ToFileTime().ToString(), username)
        // create tweet in TweetTable and User's TweetList
        tweetTable <- tweetTable.Add(newTweet.TweetId, newTweet)
        userTable.[username].AddTweet(newTweet)
        // update TagTable
        let hashTags = ExtractTag(content, '#')
        for hashTag in hashTags do
            this.AddToTagTable(hashTag, newTweet)
        // update MentionTable
        let mentions = ExtractTag(content, '@')
        for mention in mentions do
            this.AddToMentionTable(mention, newTweet)
        // return the newly created tweet
        newTweet

    // sent tweet after authentication
    member this.SendTweet(username: string, password: string, content: string) =
        let mutable response = ""
        if not (this.Login(username, password)) then
            response <- "Error! Please check your login information!"
        else
            let curTweet = this.NewTweet(username, password, content)
            response <- "Tweet success!\n" + curTweet.Serializer()
        response
    
    // re-tweet after authentication, update reTweetFrom for retweets
    member this.ReTweet(username: string, password: string, content: string, reTweetFrom: string) =
        let mutable response = ""
        if not (this.Login(username, password)) then
            response <- "Error! Please check your login information!"
        else
            let curTweet = this.NewTweet(username, password, content)
            curTweet.ReTweet <- reTweetFrom
            response <- "ReTweet success!\n" + curTweet.Serializer()
        response

    // subscribe to another user
    member this.Follow(username: string, password: string, toFollow: string) =
        let mutable response = ""
        if not (this.Login(username, password)) then
            response <- "Error! Please check your login information!"
        else
            let user = this.GetUser(username)
            let userToFollow = this.GetUser(toFollow)
            if user.UserName <> "" && userToFollow.UserName <> "" then // null check
                user.AddSubscribingList(userToFollow)
                userToFollow.AddFollower(user)
                response <- username + " successfully followed " + toFollow
            else
                response <- "User not existed. Please check the user information"
        response

    // query of all tweets that user subscribed to after the use logined in
    member this.QuerySubscribed(username: string) =
        // assumtion: username will be valid and exist in userTable
        let mutable response = ""
        if not (liveUser.Contains(username)) then
            response <- "Error! Please login and retry!"
        else
            let user = this.GetUser(username)
            let followingList = user.GetSubscribingList()

            for i in followingList do
                response <- response + SerializeList(i.GetTweetList())
        response

    // query of all tweets that user was mentioned, not need login
    member this.QueryMentioned(username: string) =
        let mutable response = "Error! The user does not exist. Please verify the username."
        if mentionTable.ContainsKey("@" + username) then
            response <- SerializeList(mentionTable.["@" + username])
        response

    // query of tweets of specifc hashtag, not need login
    member this.QueryTag(hashtag: string) =
        let mutable response = ""
        if tagTable.ContainsKey(hashtag) then
            response <- SerializeList(tagTable.[hashtag])
        response


let Simulator = Simulator()

let RegisterHandler =
    spawn system "RegisterHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgAccount(username, password) ->
                    let response = Simulator.Register(username, password)
                    // printfn "Register response for %A: %A " username response
                    sender <? response |> ignore
                | _ -> failwith "Exception"
                return! loop ()
            }
        loop ()

let LoginHandler =
    spawn system "LoginHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgAccount(username, password) ->
                    let response = Simulator.Login(username, password)
                    // printfn "Login response for %A: %A " username response |> string
                    sender <? response |> ignore
                | _ -> failwith "Exception"
                return! loop ()
            }
        loop ()

let LogOutHandler =
    spawn system "LogOutHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgAccount(username, "") ->
                    let response = Simulator.Logout(username)
                    // printfn "LogOut response for %A: %A " username response |> string
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
                    // printfn "Follow response for %A: %A " username response
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
                    // printfn "Tweet response for %A: %A " username response
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
                    // printfn "ReTweet response for %A: %A " username response
                    sender <? response |> ignore
                | _ -> failwith "Exception"
                return! loop ()
            }
        loop ()

let QuerySubscribeHandler =
    spawn system "QuerySubscribeHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgQuery(username) ->
                    let response = Simulator.QuerySubscribed(username)
                    // printfn "Query subscribing response for %A : %A" username response
                    sender <? response |> ignore
                | _ -> failwith "Exception"
                return! loop ()
            }
        loop ()

let QueryTagHandler =
    spawn system "QueryTagHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgQuery(hashtag) ->
                    let response = Simulator.QueryTag(hashtag)
                    // printfn "Query hashtag response for %A : %A" hashtag response
                    sender <? response |> ignore
                | _ -> failwith "Exception"
                return! loop ()
            }
        loop ()

let QueryMentionHandler =
    spawn system "QueryMentionHandler"
    <| fun mailbox ->
        let rec loop () =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()

                match box message :?> Message with
                | MsgQuery(mentioned) ->
                    let response = Simulator.QueryMentioned(mentioned)
                    // printfn "Query mentioned response for %A : %A" mentioned response
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
                        msg <- MsgAccount(username, password)
                    | "Login" ->
                        handler <- system.ActorSelection(url + "LoginHandler")
                        msg <- MsgAccount(username, password)
                    | "Logout" ->
                        handler <- system.ActorSelection(url + "LogOutHandler")
                        msg <- MsgAccount(username, "")
                    | "Follow" ->
                        handler <- system.ActorSelection(url + "FollowHandler")
                        msg <- MsgFollow(username, password, arg1)
                    | "Tweet" ->
                        handler <- system.ActorSelection(url + "TweetHandler")
                        msg <- MsgTweet(username, password, arg1)
                    | "ReTweet" ->
                        handler <- system.ActorSelection(url + "ReTweetHandler")
                        msg <- MsgReTweet(username, password, arg1, arg2)
                    | "Query" ->
                        handler <- system.ActorSelection(url + "QuerySubscribeHandler")
                        msg <- MsgQuery(username)
                    | "Tag" ->
                        handler <- system.ActorSelection(url + "QueryTagHandler")
                        msg <- MsgQuery(commands.[1]) // "hashtag"
                    | "Mention" ->
                        handler <- system.ActorSelection(url + "QueryMentionHandler")
                        msg <- MsgQuery(username) // "mentioned_user"

                    let res = Async.RunSynchronously(handler <? msg, 100)
                    sender <? res |> ignore
                    // printf "%A" res
                return! loop ()
            }
        loop ()