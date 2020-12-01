#load "./Data.fsx"
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote" 

open Data
open System
open System.Collections.Generic
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

let mutable tweetTable = new Map<string, Tweet>([])
let mutable userTable = new Map<string, User>([])
let mutable tagTable = new Map<string, Tweet list>([])

type Simulator() =
    // methods to create new user
    member this.NewUser(user: User) =
        userTable <- userTable.Add(user.UserName, user)
    
    // getter of user
    member this.GetUser(username: string) =
        try
            userTable.[username]
        with
            | :? KeyNotFoundException -> User("","") // not exist

    // register user
    member this.Register(username: string, password: string) =
        let mutable response = "Username has already been used. Please choose a new one."
        if not (userTable.ContainsKey(username)) then
            this.NewUser(User(username, password))
            response <- "Registered successfully"
            
        response

    // authentication to return whether login success or fails (bool)
    member this.Login(username: string, password: string) =
        try
            userTable.ContainsKey(username) && (userTable.[username].Password = password)
        with
            | :? KeyNotFoundException -> false
    
    // add new tweet to tweetTable
    member this.NewTweet(tweet: Tweet) =
        tweetTable <- tweetTable.Add(tweet.TweetId, tweet)

    // add one tweet to tagTable, if tag not exist, create one new record and add the tweet
    member this.AddToTagTable(hashTag: string, tweet: Tweet) =
        // initialize <key, value> pair with empty list
        if not (tagTable.ContainsKey(hashTag)) then
            tagTable <- tagTable.Add(hashTag, List.empty)
        // add tweet to existed hashtag list in the map
        tagTable <- tagTable.Add(hashTag, List.append tagTable.[hashTag] [tweet])
    
    // extract all hashtags in the tweet content to return tags list
    member this.ExtractTag(word: string) =
        let mutable tags = List.empty
        let mutable index = 0
        while index < word.Length do
            if word.[index] = '#' then
                // find end space of current tag
                let mutable endIdx = index
                while endIdx < word.Length && word.[endIdx] <> ' ' do
                    endIdx <- endIdx + 1
                if endIdx <> word.Length then
                    tags <- List.append tags [word.[index..endIdx - 1]]
                index <- endIdx + 1 // update index to search right
            else
                index <- index + 1
        tags

    // sent tweet after authentication, auto parse hashtag to update table
    member this.SendTweet(username: string, password: string, content: string) =
        let mutable response = ""
        if not (this.Login(username, password)) then
            response <-  "Error! Please check your login information!"
        else
            // use data time as unique key for tweetId
            let tweet = Tweet(content, DateTime.Now.ToFileTime().ToString(), username)
            // create tweet in TweetTable and User's TweetList
            this.NewTweet(tweet)
            userTable.[username].SendTweet(tweet)
            // update TagTable
            let tags = this.ExtractTag(content)
            for tag in tags do
                this.AddToTagTable(tag, tweet)
            response <- "Success "

        response
    
    // retweet
    // member this.ReTweet(username: string, password: string, content: string) =

    // subscribe to another user
    member this.Follow(username: string, password: string, toFollow: string) =
        let mutable response = ""
        if not (this.Login(username, password)) then
            response <-  "Error! Please check your login information!"
        else
            let user = this.GetUser(username)
            let userToFollow = this.GetUser(toFollow)
            if user.UserName <> "" && userToFollow.UserName <> "" then // null check
                user.SubsribeTo(userToFollow)
                response <- username + " successfully followed " + toFollow
            else
                response <- "User not existed. Please check the user information"
        response
            

// simple test
// let user1 = User("user1", "pw1")
// let user2 = User("user2", "pw2")
// let user3 = User("user3", "pw3")
// user1.SubsribeTo(user2)
// user1.SubsribeTo(user3)
// printf "%A" [user1.GetSubsribingList()]
// printf "%A" [user1.GetTweetList()]

// server test
let Simulator = Simulator()
Simulator.Register("user1", "pw1")
Simulator.Register("user2", "pw2")
Simulator.Register("user2", "pw2")
Simulator.SendTweet("user1", "pw1", "#test This the first tweet")
Simulator.SendTweet("user2", "pw2", "#test This the second tweet")
Simulator.SendTweet("user3", "pw3", "#omg #wtf I created another tag")
Simulator.Follow("user1", "pw1", "user2")
Simulator.Follow("user1", "pw1", "user3")
Simulator.Follow("user2", "pw2", "user4")