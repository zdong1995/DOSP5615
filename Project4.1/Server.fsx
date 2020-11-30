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

let mutable tweetTable = new Map<string, Tweet>([])
let mutable userTable = new Map<string, User>([])
let mutable tagTable = new Map<string, Tweet list>([])

type Server() =
    // methods to create new user
    member this.NewUser(user: User) =
        userTable <- userTable.Add(user.UserName, user)

    // authentication to return whether login success or fails (bool)
    member this.Auth(username: string, password: string) =
        let mutable response = false
        if (userTable.ContainsKey(username)) && (userTable.[username].Password = password) then
            response <- true
        response
    
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
        if (word.Contains("#")) then
            let mutable index = 0
            if index < word.Length then
                let startIdx = word.IndexOf("#", index)
                let endIdx = word.IndexOf(" ", startIdx)
                tags <- List.append tags [word.[startIdx..endIdx]]
                index <- endIdx + 1 // update index to search right
        tags

    // sent tweet after authentication, auto parse hashtag to update table
    member this.SendTweet(username: string, password: string, content: string) =
        let mutable response = ""
        if not (this.Auth(username, password)) then
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
            
    

// simple test
let user1 = User("user1", "pw1")
let user2 = User("user2", "pw2")
let user3 = User("user3", "pw3")
user1.SubsribeTo(user2)
user1.SubsribeTo(user3)
// printf "%A" [user1.GetSubsribingList()]
// printf "%A" [user1.GetTweetList()]

// server test
let Twitter = Server()
Twitter.NewUser(user1)
Twitter.NewUser(user2)
Twitter.NewUser(user3)
Twitter.SendTweet("user1", "pw1", "#test This the first tweet")
Twitter.SendTweet("user2", "pw2", "#test This the second tweet")
Twitter.SendTweet("user3", "pw3", "#omg I created another tag")