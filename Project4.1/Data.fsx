module Data

// Tweet Class
type Tweet(content: string, tweetId: string, author: string) =
    // initialize as empty for orginal tweet
    let mutable reTweetFrom = ""

    member this.Content = content
    member this.TweetId = tweetId
    member this.Author = author
    member this.ReTweet with get() = reTweetFrom and set(fromUser : string) = reTweetFrom <- fromUser
    
    member this.Serializer() =
        let mutable res = sprintf "TweetID: %s\nContent: %s\n Author: %s\n" tweetId content author
        if this.ReTweet <> "" then
            res <- res + sprintf "Retweet from: %s\n" reTweetFrom
        res


type User(username: string, password: string) = 
    // let binding must come before interface and member declariation
    let mutable followers = List.empty : User list
    let mutable following = List.empty : User list
    let mutable tweets = List.empty : Tweet list
    
    // constructor
    member this.UserName = username
    member this.Password = password

    // getters
    member this.GetFollowers() =
        followers
    member this.GetSubscribingList() =
        following
    member this.GetTweetList() =
        tweets

    // setters
    // add another user to current user's followers list
    member this.AddFollower(user : User) =
        followers <- List.append followers [user]

    // add another user to current user's subscribing list
    member this.AddSubscribingList (user: User) =
        following <- List.append following [user]
    
    // add one tweet to current user's tweets list
    member this.AddTweet (tweet: Tweet) =
        tweets <- List.append tweets [tweet]

type Message =
    | MsgRegister of string * string // ("username", "password")
    | MsgTweet of string * string * string // ("username", "password", "content")
    | MsgFollow of string * string * string // ("username", "password", "toFollow")
    | MsgReTweet of string * string * string * string // ("username", "password", "content", "reTweetFrom")
    | MsgQuery of string * string // (Query type * [hasTag, mentionedId, curUserId]
    | MsgEmpty of string
    // TODO
    // Query type : 0. only hashTag 1. only mentioned 2. hashTag and mentioned