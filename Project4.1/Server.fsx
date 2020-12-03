module Server

#load "./Data.fsx"

open Data

open System
open System.Collections.Generic

let mutable tweetTable = new Map<string, Tweet>([])
let mutable userTable = new Map<string, User>([])
let mutable tagTable = new Map<string, Tweet list>([])
let mutable mentionTable = new Map<string, Tweet list>([])

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
            response <- username + "Registered successfully"

        response

    // authentication to return whether login success or fails (bool)
    member this.Login(username: string, password: string) =
        try
            userTable.ContainsKey(username)
            && (userTable.[username].Password = password)
        with :? KeyNotFoundException -> false

    // add one tweet to tagTable, if tag not exist, create one new record and add the tweet
    member this.AddToTagTable(hashTag: string, tweet: Tweet) =
        // initialize <key, value> pair with empty list
        if not (tagTable.ContainsKey(hashTag))
        then tagTable <- tagTable.Add(hashTag, List.empty)
        // add tweet to existed hashtag list in the map
        tagTable <- tagTable.Add(hashTag, List.append tagTable.[hashTag] [ tweet ])

    
    // wrapper of create new tweet for future re-use
    // auto parse hashtag to update table and mention table
    member this.NewTweet(username: string, password: string, content: string) =
        // use data time as unique key for tweetId
        let tweet =
            Tweet(content, DateTime.Now.ToFileTime().ToString(), username)
        // create tweet in TweetTable and User's TweetList
        tweetTable <- tweetTable.Add(tweet.TweetId, tweet)
        userTable.[username].AddTweet(tweet)
        // update TagTable
        let hashTags = ExtractTag(content, '#')
        for hashTag in hashTags do
            this.AddToTagTable(hashTag, tweet)
        // update MentionTable
        let mentions = ExtractTag(content, '@')
        for mention in mentions do
            this.AddToTagTable(mention, tweet)
        
        tweet

    // sent tweet after authentication
    member this.SendTweet(username: string, password: string, content: string) =
        let mutable response = ""
        if not (this.Login(username, password)) then
            response <- "Error! Please check your login information!"
        else
            this.NewTweet(username, password, content) |> ignore
            response <- "Success"
        response
    
    // re-tweet after authentication, update reTweetFrom for retweets
    member this.ReTweet(username: string, password: string, content: string, reTweetFrom: string) =
        let mutable response = ""
        if not (this.Login(username, password)) then
            response <- "Error! Please check your login information!"
        else
            let curTweet = this.NewTweet(username, password, content)
            curTweet.ReTweet <- reTweetFrom
            response <- "Success"
        response

    member this.QueryTweetsOfSubscribes(username: string, password: string) =
        let mutable response = ""
        if not (this.Login(username, password)) then
            response <- "Error! Please check your login information!"
        else
            let user = this.GetUser(username)
            let followingList = user.GetSubscribingList()
            let mutable res = List.empty : Tweet list

            for x in followingList do
                res <- x.GetTweetList()
                // "Query, username, password" 500 ms/次

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
