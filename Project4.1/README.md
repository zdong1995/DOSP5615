# Twitter Clone
Twitter clone using F#.

### Group Member:
- Zhang Dong, UFID: 69633983
- Xiaobai Li, UFID: 31567109

## Functionality

In this project, a Twitter Enginee Clone with support to following functionality was designed:
- Register account and Authenticatios
- Send tweet with support to auto-parse multiple hashtags and mentions from the content. e.g. `#dosp #uf Twitter clone is so cool! @twitter @gators`
- Subscribe to user's tweets
- Re-tweets with identification of re-tweets source
- Querying tweets subscribed to, tweets with specific hashtags, tweets in which the user is mentioned
- If the user is connected, deliver the above types of tweets live (without querying)

Also a tester/simulator to test the above was implemented with support to:
- Simulate as many users as you can
- Simulate periods of live connection and disconnection for users
- Simulate a Zipf distribution on the number of subscribers

## Implementation

The infrastructure of Twitter Enginee is as following:

![](https://github.com/zdong1995/DOSP5615/blob/twitter/Project4.1/img/Infra.jpg)

### Database Management

To simplify the simulator, we use 4 HashTable to store the data to simulate database and relations. It will similar to NoSQL database but easier to implement and rapid to retrieve relational data pair.

- tweetTable: `<tweetId, Tweet>`
- userTable: `<userName, User>`
- tagtTable: `<tag, Tweet List>`
- mentionTable: `<mentionedUser, Tweet List>`

### Server Infrastrcuture

For the server side, we use Actor as distributed microservice for different module and use a singleton `Simulator` to realize the business logic. There will be one actor `APIsHandler` as the gateway to communicate with clients, the received message will be distributed to each microservice actor `FunctionHandler` based on message type. Each `FunctionHandler` will invoke the APIs of `Simulator` and process the bussiness logic to modify the database table. Then the response will be parsed from API response to `FunctionHandler`, then `APIsHandler`, finally reached back to client.

There are four class elaborately designed in server side, which are `User`, `Tweet`, `Message` and `Simulator`. Object-Oriented Design with encapsulation has been fullfilled to design the class.

### Class

#### User
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

#### Tweet
Fields:
- Content: `string`
- TweetId: `string`
- Author: `string`
- ReTweet: `string`, will be empty for original tweet

#### Message
The `Message` is an assemble struct class with following type:

- MsgAccount of `string * string`: ("username", "password")
- MsgTweet of `string * string * string`: ("username", "password", "content")
- MsgFollow of `string * string * string`: ("username", "password", "toFollow")
- MsgReTweet of `string * string * string * string`: ("username", "password", "content", "reTweetFrom")
- MsgQuery of `string`: "hastag"/"mentionedUser"/"curUser"
- MsgEmpty of `string`

### Simulator
The `Simulator` is a class to support the business logic with following method:

- Update database talbe: `NewUser(User)`, `AddToTagTable(hashTag, Tweet)`, `AddToMentionTable(mentionedUser, Tweet)`
- Account management: `Register(username, password)`, `Login(username, password)`
- `NewTweet(username, password, content)`: Create new tweet and auto-parse hashtag and mentioned users from content, update `TagTable`, `TweetTable`, `MentionTable` and user's filed
- Tweet: `SendTweet(username, password, content)`, `ReTweet(username, password, content, reTweetFrom)`
- `Follow(username, password, toFollow)`: Subscribed to corresponding user after successful authentication
- Query: `QuerySubscribed(username)`, `QueryMentioned(username)`, `QueryTag(hashtag)`

### Handlers

Remote acotrs was used to implement microservice-like handlers:
- Gateway: `APIsHandler`
- `RegisterHandler`, `LoginHandler`, `FollowHandler`, `TweetHandler`, `RetweetHandler`, `QuerySubscribeHandler`, `QueryTagHandler`, `QueryMentionHandler`

The message from client to `APIsHandler` will be `string`, and the message between `Handler`s are `Message` type.

## Usage

The command message from client should be in the following format:
```
operation|username|password|arg1|arg2
```

The value of `arg1` and `arg2` will depend on the operations we want, which is showed in the following example:

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
service <? "ReTweet|user2|pw2|#uf Go Gators! @user3 |user1" |> ignore
service <? "ReTweet|user3|pw3|#uf Go Gators! @user3 |user1" |> ignore
// Query
service <? "Query|user1|||" |> Async.Ignore |> Async.RunSynchronously |> ignore
service <? "Tag|#dosp|||" |> Async.Ignore |> Async.RunSynchronously |> ignore
service <? "Mention|user3|||" |> Async.Ignore |> Async.RunSynchronously |> ignore
```

**!!** The hashtag and mention should always have one space after it.

### Test

The test workflow is showed as following:

![](https://github.com/zdong1995/DOSP5615/blob/twitter/Project4.1/img/Workflow.jpg)

There are two ways to run the test for this simulator:

- Run `Test.fsx` file to test the APIs and the correctness of the server.
- Run `Client.fsx` file to test the Cliend-Server communication.

### Service logic Test using `Test.fsx`

Run `Test.fsx` file and you should receive the following expected result.

### Service logic

```shell
Register response for "user1": "user1 Registered successfully" 
Register response for "user1": "Username has already been used. Please choose a new one." 
Register response for "user2": "user2 Registered successfully" 
Register response for "user3": "user3 Registered successfully" 
Follow response for "user1": "User not existed. Please check the user information" 
Follow response for "user1": "user1 successfully followed user2" 
Follow response for "user1": "user1 successfully followed user3" 
Follow response for "user2": "user2 successfully followed user3" 
Tweet response for "user1": "Success" 
Tweet response for "user2": "Success" 
Tweet response for "user1": "Success" 
Tweet response for "user3": "Success" 
ReTweet response for "user2": "Success" 
ReTweet response for "user3": "Success" 
```
#### Query feature
Query for hashtag:

```
Query hashtag response for "#dosp" : "TweetID: 132515239516962940
Content: #dosp Twitter Clone! @user3 
 Author: user2

TweetID: 132515239516965180
Content: #dosp #uf I think this is cool! @user3 
 Author: user1

TweetID: 132515239516967540
Content: #dosp #omg Have you guys completed the project? @user1 
 Author: user3

"
```

Query for mentioned:

```
Query mentioned response for "user3" :

"TweetID: 132515236333693580
Content: #uf Go Gators! @user3 
 Author: user1

TweetID: 132515236333708940
Content: #dosp Twitter Clone! @user3 
 Author: user2

TweetID: 132515236333710700
Content: #dosp #uf I think this is cool! @user3 
 Author: user1

TweetID: 132515236333723940
Content: #uf Go Gators! @user3 
 Author: user2
Retweet from: user1

TweetID: 132515236333728120
Content: #uf Go Gators! @user3 
 Author: user3
Retweet from: user1

"
```

Query for subscribing list:
```
Query subscribing response for "user1" :

"TweetID: 132515236333708940
Content: #dosp Twitter Clone! @user3 
 Author: user2

TweetID: 132515236333723940
Content: #uf Go Gators! @user3 
 Author: user2
Retweet from: user1

TweetID: 132515236333712730
Content: #dosp #omg Have you guys completed the project? @user1 
 Author: user3

TweetID: 132515236333728120
Content: #uf Go Gators! @user3 
 Author: user3
Retweet from: user1

"
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
    [("132515239516943870",
      FSI_0016.Data+Tweet {Author = "user1";
                           Content = "#uf Go Gators! @user3 ";
                           ReTweet = "";
                           TweetId = "132515239516943870";});
     ("132515239516962940",
      FSI_0016.Data+Tweet {Author = "user2";
                           Content = "#dosp Twitter Clone! @user3 ";
                           ReTweet = "";
                           TweetId = "132515239516962940";});
     ("132515239516965180",
      FSI_0016.Data+Tweet
        {Author = "user1";
         Content = "#dosp #uf I think this is cool! @user3 ";
         ReTweet = "";
         TweetId = "132515239516965180";});
     ("132515239516967540",
      FSI_0016.Data+Tweet
        {Author = "user3";
         Content = "#dosp #omg Have you guys completed the project? @user1 ";
         ReTweet = "";
         TweetId = "132515239516967540";});
     ("132515239516982040",
      FSI_0016.Data+Tweet {Author = "user2";
                           Content = "#uf Go Gators! @user3 ";
                           ReTweet = "user1";
                           TweetId = "132515239516982040";});
     ("132515239516988390",
      FSI_0016.Data+Tweet {Author = "user3";
                           Content = "#uf Go Gators! @user3 ";
                           ReTweet = "user1";
                           TweetId = "132515239516988390";})]
```

The `tagTable` will be:
```F#
> tagTable;;

val it : Map<string,Tweet list> =
  map
    [("#dosp",
      [FSI_0016.Data+Tweet {Author = "user2";
                            Content = "#dosp Twitter Clone! @user3 ";
                            ReTweet = "";
                            TweetId = "132515239516962940";};
       FSI_0016.Data+Tweet
         {Author = "user1";
          Content = "#dosp #uf I think this is cool! @user3 ";
          ReTweet = "";
          TweetId = "132515239516965180";};
       FSI_0016.Data+Tweet
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project? @user1 ";
          ReTweet = "";
          TweetId = "132515239516967540";}]);
     ("#omg",
      [FSI_0016.Data+Tweet
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project? @user1 ";
          ReTweet = "";
          TweetId = "132515239516967540";}]);
     ("#uf",
      [FSI_0016.Data+Tweet {Author = "user1";
                            Content = "#uf Go Gators! @user3 ";
                            ReTweet = "";
                            TweetId = "132515239516943870";};
       FSI_0016.Data+Tweet
         {Author = "user1";
          Content = "#dosp #uf I think this is cool! @user3 ";
          ReTweet = "";
          TweetId = "132515239516965180";};
       FSI_0016.Data+Tweet {Author = "user2";
                            Content = "#uf Go Gators! @user3 ";
                            ReTweet = "user1";
                            TweetId = "132515239516982040";};
       FSI_0016.Data+Tweet {Author = "user3";
                            Content = "#uf Go Gators! @user3 ";
                            ReTweet = "user1";
                            TweetId = "132515239516988390";}])]
```

The `mentionTable` will be:
```F#
> mentionTable;;

val it : Map<string,Tweet list> =
  map
    [("@user1",
      [FSI_0016.Data+Tweet
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project? @user1 ";
          ReTweet = "";
          TweetId = "132515239516967540";}]);
     ("@user3",
      [FSI_0016.Data+Tweet {Author = "user1";
                            Content = "#uf Go Gators! @user3 ";
                            ReTweet = "";
                            TweetId = "132515239516943870";};
       FSI_0016.Data+Tweet {Author = "user2";
                            Content = "#dosp Twitter Clone! @user3 ";
                            ReTweet = "";
                            TweetId = "132515239516962940";};
       FSI_0016.Data+Tweet
         {Author = "user1";
          Content = "#dosp #uf I think this is cool! @user3 ";
          ReTweet = "";
          TweetId = "132515239516965180";};
       FSI_0016.Data+Tweet {Author = "user2";
                            Content = "#uf Go Gators! @user3 ";
                            ReTweet = "user1";
                            TweetId = "132515239516982040";};
       FSI_0016.Data+Tweet {Author = "user3";
                            Content = "#uf Go Gators! @user3 ";
                            ReTweet = "user1";
                            TweetId = "132515239516988390";}])]
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