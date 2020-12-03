module Data

// Tweet Class
type Tweet(content: string, tweetId: string, author: string) =
    member this.Content = content
    member this.TweetId = tweetId
    member this.Author = author
    // rewrite the to String method to help debug
    override this.ToString() =
        this.TweetId + " " + this.Content

type User(username: string, password: string) = 
    // mutable member for subscribers and tweets
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
    // method to subsribeTo another user
    member this.SubscribeTo (user: User) =
        following <- List.append following [user]
    // mehtod to send tweet
    member this.SendTweet (tweet: Tweet) =
        tweets <- List.append tweets [tweet]
    // rewrite the to String method to help debug
    override this.ToString() =
        this.UserName