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