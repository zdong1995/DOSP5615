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
- `ExtractTag(String, Tag)`: Auto parse hashtag and mention from tweet. **The hashtag and mention need has one space after the tag itself**. For example: "#dosp @Twitter clone is so cool! @uf "
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
```F#
let service = system.ActorSelection(url + "APIsHandler")
// command should have length = 5
// Register
service <? "Register|user1|pw1||" |> ignore
service <? "Register|user1|pw1||" |> ignore
service <? "Register|user2|pw2||" |> ignore
service <? "Register|user3|pw3||" |> ignore
// Follow
service <? "Follow|user1|pw1|user0|" |> ignore
service <? "Follow|user1|pw1|user2|" |> ignore
service <? "Follow|user1|pw1|user3|" |> ignore
service <? "Follow|user2|pw2|user3|" |> Async.Ignore |> Async.RunSynchronously |> ignore
// Tweet
service <? "Tweet|user1|pw1|#uf Go Gators! @user3 |" |> ignore
service <? "Tweet|user2|pw2|#dosp Twitter Clone! @user3 |" |> ignore
service <? "Tweet|user1|pw1|#dosp #uf I think this is cool! @user3 |" |> ignore
service <? "Tweet|user3|pw3|#dosp #omg Have you guys completed the project? @user1 |" |> ignore
// ReTweet
service <? "ReTweet|user2|pw2|#uf Go Gators!|user1" |> ignore
```
### Service logic

```shell
Register response for "user1": "user1Registered successfully" 
Register response for "user1": "Username has already been used. Please choose a new one." 
Register response for "user2": "user2Registered successfully" 
Register response for "user3": "user3Registered successfully" 
Follow response for "user1": "User not existed. Please check the user information" 
Follow response for "user1": "user1 successfully followed user2" 
Follow response for "user1": "user1 successfully followed user3" 
Follow response for "user2": "user2 successfully followed user3" 
Tweet response for "user1": "Success" 
Tweet response for "user2": "Success" 
Tweet response for "user1": "Success" 
Tweet response for "user3": "Success" 
```

### Data Consistency

After these operations, the `userTable` will be:
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

The `tweetTable` will be:

```F#
> tweetTable;;

val it : Map<string,Tweet> =
  map
    [("132515208335392600",
      FSI_0021.Data+Tweet {Author = "user1";
                           Content = "#uf Go Gators! @user3 ";
                           ReTweet = "";
                           TweetId = "132515208335392600";});
     ("132515208335414070",
      FSI_0021.Data+Tweet {Author = "user2";
                           Content = "#dosp Twitter Clone! @user3 ";
                           ReTweet = "";
                           TweetId = "132515208335414070";});
     ("132515208335416700",
      FSI_0021.Data+Tweet
        {Author = "user1";
         Content = "#dosp #uf I think this is cool! @user3 ";
         ReTweet = "";
         TweetId = "132515208335416700";});
     ("132515208335418770",
      FSI_0021.Data+Tweet
        {Author = "user3";
         Content = "#dosp #omg Have you guys completed the project? @user1 ";
         ReTweet = "";
         TweetId = "132515208335418770";});
     ("132515208335430160",
      FSI_0021.Data+Tweet {Author = "user2";
                           Content = "#uf Go Gators!";
                           ReTweet = "user1";
                           TweetId = "132515208335430160";})]
```

The `tagTable` will be:
```F#
> tagTable;;

val it : Map<string,Tweet list> =
  map
    [("#dosp",
      [FSI_0021.Data+Tweet {Author = "user2";
                            Content = "#dosp Twitter Clone! @user3 ";
                            ReTweet = "";
                            TweetId = "132515208335414070";};
       FSI_0021.Data+Tweet
         {Author = "user1";
          Content = "#dosp #uf I think this is cool! @user3 ";
          ReTweet = "";
          TweetId = "132515208335416700";};
       FSI_0021.Data+Tweet
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project? @user1 ";
          ReTweet = "";
          TweetId = "132515208335418770";}]);
     ("#omg",
      [FSI_0021.Data+Tweet
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project? @user1 ";
          ReTweet = "";
          TweetId = "132515208335418770";}]);
     ("#uf",
      [FSI_0021.Data+Tweet {Author = "user1";
                            Content = "#uf Go Gators! @user3 ";
                            ReTweet = "";
                            TweetId = "132515208335392600";};
       FSI_0021.Data+Tweet
         {Author = "user1";
          Content = "#dosp #uf I think this is cool! @user3 ";
          ReTweet = "";
          TweetId = "132515208335416700";};
       FSI_0021.Data+Tweet {Author = "user2";
                            Content = "#uf Go Gators!";
                            ReTweet = "user1";
                            TweetId = "132515208335430160";}])]
```

The `mentionTable` will be:
```F#
> mentionTable;;

val it : Map<string,Tweet list> =
  map
    [("@user1",
      [FSI_0021.Data+Tweet
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project? @user1 ";
          ReTweet = "";
          TweetId = "132515208335418770";}]);
     ("@user3",
      [FSI_0021.Data+Tweet {Author = "user1";
                            Content = "#uf Go Gators! @user3 ";
                            ReTweet = "";
                            TweetId = "132515208335392600";};
       FSI_0021.Data+Tweet {Author = "user2";
                            Content = "#dosp Twitter Clone! @user3 ";
                            ReTweet = "";
                            TweetId = "132515208335414070";};
       FSI_0021.Data+Tweet
         {Author = "user1";
          Content = "#dosp #uf I think this is cool! @user3 ";
          ReTweet = "";
          TweetId = "132515208335416700";}])]
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

#### Query feature
Query for hashtag:
```F#
> Simulator.QueryTag("#dosp");;

val it : string =
  "TweetID: 132515208335414070
Content: #dosp Twitter Clone! @user3 
 Author: user2

TweetID: 132515208335416700
Content: #dosp #uf I think this is cool! @user3 
 Author: user1

TweetID: 132515208335418770
Content: #dosp #omg Have you guys completed the project? @user1 
 Author: user3

"
```

Query for mentioned:
```F#
> Simulator.QueryMentioned("user3");;

val it : string =
  "TweetID: 132515208335392600
Content: #uf Go Gators! @user3 
 Author: user1

TweetID: 132515208335414070
Content: #dosp Twitter Clone! @user3 
 Author: user2

TweetID: 132515208335416700
Content: #dosp #uf I think this is cool! @user3 
 Author: user1

"
```

Query for subscribing list:
```F#
> Simulator.QuerySubscribedTo("user1", "pw1");;

val it : string =
  "TweetID: 132515216112257240
Content: #dosp Twitter Clone! @user3 
 Author: user2

TweetID: 132515216112275190
Content: #uf Go Gators!
 Author: user2
Retweet from: user1

TweetID: 132515216112261430
Content: #dosp #omg Have you guys completed the project? @user1 
 Author: user3

"
```