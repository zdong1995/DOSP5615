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

tagTable
```F#
Map<string,Tweet list> =
  map
    [("#dosp",
      [132514955636686980 #dosp Twitter Clone!
         {Author = "user2";
          Content = "#dosp Twitter Clone!";
          TweetId = "132514955636686980";};
       132514955636690060 #dosp #uf I think this is cool!
         {Author = "user1";
          Content = "#dosp #uf I think this is cool!";
          TweetId = "132514955636690060";};
       132514955636754520 #dosp #omg Have you guys completed the project?
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project?";
          TweetId = "132514955636754520";}]);
     ("#omg",
      [132514955636754520 #dosp #omg Have you guys completed the project?
         {Author = "user3";
          Content = "#dosp #omg Have you guys completed the project?";
          TweetId = "132514955636754520";}]);
     ("#uf",
      [132514955636667090 #uf Go Gators! {Author = "user1";
                                          Content = "#uf Go Gators!";
                                          TweetId = "132514955636667090";};
       132514955636690060 #dosp #uf I think this is cool!
         {Author = "user1";
          Content = "#dosp #uf I think this is cool!";
          TweetId = "132514955636690060";}])]
```

tweetTable
```F#
 Map<string,Tweet> = 
  map
    [("132514955636667090",
          132514955636667090 #uf Go Gators! {Author = "user1";
                                            Content = "#uf Go Gators!";
                                            TweetId = "132514955636667090";});
        ("132514955636686980",
          132514955636686980 #dosp Twitter Clone!
            {Author = "user2";
            Content = "#dosp Twitter Clone!";
            TweetId = "132514955636686980";});
        ("132514955636690060",
          132514955636690060 #dosp #uf I think this is cool!
            {Author = "user1";
            Content = "#dosp #uf I think this is cool!";
            TweetId = "132514955636690060";});
        ("132514955636754520",
          132514955636754520 #dosp #omg Have you guys completed the project?
            {Author = "user3";
            Content = "#dosp #omg Have you guys completed the project?";
            TweetId = "132514955636754520";})]
```

userTable
```F#
Map<string,User> =
  map
    [("user1", user1 {Password = "pw1";
                      UserName = "user1";});
     ("user2", user2 {Password = "pw2";
                      UserName = "user2";});
     ("user3", user3 {Password = "pw3";
                      UserName = "user3";})]
```