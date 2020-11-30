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

```F#
val mutable tweetTable : Map<string,Data.Tweet> =
  map
    [("132512530690696810", 132512530690696810 #test This the first tweet);
     ("132512530690702420", 132512530690702420 #test This the second tweet);
     ("132512530690702770", 132512530690702770 #omg I created another tag)]
val mutable userTable : Map<string,Data.User> =
  map [("user1", user1); ("user2", user2); ("user3", user3)]
val mutable tagTable : Map<string,Data.Tweet list> =
  map
    [("#omg ", [132512530690702770 #omg I created another tag]);
     ("#test ",
      [132512530690696810 #test This the first tweet;
       132512530690702420 #test This the second tweet])]
```