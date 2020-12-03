# Twitter Clone
Twitter clone using F#.

## Simple Version Twitter Server
Use three HashTable to store the data to simulate database.
- tweetTable: `<tweetId, Tweet>`
- userTable: `<userName, User>`
- tagtTable: `<tag, Tweet List>`

Currently provide following features:
- `NewUser(User)`
- `Auth(username, password)`: Return boolean for whether successful authenticated.
- `NewTweet(Tweet)`
- `AddToTagTable(hashTag, Tweet)`: Add one tweet to tagTable, if tag not exist, create one new record and add the tweet
- `ExtractTag(String)`: subsribeTo another user
- `SendTweet(username, password, content)`: Sent tweet after authentication, auto-parse hashtag from content and update TagTable

## Data types

### User
Fields:
- username: `string`
- password: `string`
- followers: `User List`
- following: `User List`
- tweets: `Tweet List`

Methods:
- `GetFollowers()`
- `GetSubsribingList()`
- `GetTweetList()`
- `SubsribeTo(User)`: subsribeTo another user
- `SendTweet(Tweet)`: update tweet to its tweetlist

### Tweeter
Fields:
- Content: `string`
- tweetId: `string`
- author: `string`

## Test
Run `Test.fsx` file and you should receive the following expected result:

```shell
Register response for "user1": "user1Registered successfully" 
Register response for "user1": "Username has already been used. Please choose a new one." 
Register response for "user2": "user2Registered successfully" 
Register response for "user3": "user3Registered successfully" 
Follow response for "user1": "User not existed. Please check the user information" 
Follow response for "user1": "user1 successfully followed user2" 
Follow response for "user1": "user1 successfully followed user3" 
Follow response for "user2": "user2 successfully followed user3"
Tweet response for "user1": "Success " 
Tweet response for "user2": "Success " 
Tweet response for "user1": "Success " 
Tweet response for "user3": "Success " 
```

The `tagTable` will be:
```F#
> tagTable;;

val it : Map<string,Tweet list> =
  map
    [("#dosp",
      [132514969730821810 #dosp Twitter Clone!
         {Author = "user2";
          Content = "#dosp Twitter Clone!";
          TweetId = "132514969730821810";};
       132514969730823820 #dosp #uf I think this is cool!
         {Author = "user1";
          Content = "#dosp #uf I think this is cool!";
          TweetId = "132514969730823820";};
       132514969730825860 #dosp #omg Have you guys completed the project?
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project?";
          TweetId = "132514969730825860";}]);
     ("#omg",
      [132514969730825860 #dosp #omg Have you guys completed the project?
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project?";
          TweetId = "132514969730825860";}]);
     ("#uf",
      [132514969730804680 #uf Go Gators! {Author = "user1";
                                          Content = "#uf Go Gators!";
                                          TweetId = "132514969730804680";};
       132514969730823820 #dosp #uf I think this is cool!
         {Author = "user1";
          Content = "#dosp #uf I think this is cool!";
          TweetId = "132514969730823820";}])]
```

The `tweetTable` will be:

```F#
> tweetTable;;
val it : Map<string,Tweet> =
  map
    [("132515008790235520",
      FSI_0006.Data+Tweet {Author = "user1";
                           Content = "#uf Go Gators!";
                           ReTweet = "";
                           TweetId = "132515008790235520";});
     ("132515008790278040",
      FSI_0006.Data+Tweet {Author = "user2";
                           Content = "#dosp Twitter Clone!";
                           ReTweet = "";
                           TweetId = "132515008790278040";});
     ("132515008790310310",
      FSI_0006.Data+Tweet {Author = "user1";
                           Content = "#dosp #uf I think this is cool!";
                           ReTweet = "";
                           TweetId = "132515008790310310";});
     ("132515008790336620",
      FSI_0006.Data+Tweet
        {Author = "user3";
         Content = "#dosp #omg Have you guys completed the project?";
         ReTweet = "";
         TweetId = "132515008790336620";});
     ("132515008790357460",
      FSI_0006.Data+Tweet {Author = "user2";
                           Content = "#uf Go Gators!";
                           ReTweet = "user1";
                           TweetId = "132515008790357460";})]
```

The `userTable` will be:
```F#
> userTable;;

val it : Map<string,User> =
  map
    [("user1", FSI_0006.Data+User {Password = "pw1";
                                   UserName = "user1";});
     ("user2", FSI_0006.Data+User {Password = "pw2";
                                   UserName = "user2";});
     ("user3", FSI_0006.Data+User {Password = "pw3";
                                   UserName = "user3";})]
```

Users fields
```F#
> userTable.["user3"].GetFollowers();;

val it : User list =
  [FSI_0006.Data+User {Password = "pw1";
                       UserName = "user1";};
   FSI_0006.Data+User {Password = "pw2";
                       UserName = "user2";}]

> userTable.["user1"].GetSubscribingList();;

val it : User list =
  [FSI_0006.Data+User {Password = "pw2";
                       UserName = "user2";};
   FSI_0006.Data+User {Password = "pw3";
                       UserName = "user3";}]
```