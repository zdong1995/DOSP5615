#load "./Data.fsx"
#load "./Actor.fsx"
#load "./Server.fsx"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote"

open Data
open Actor
open Server

open Akka.FSharp


// Remote API handler Test
let main() =
    //initialize()
    let service = system.ActorSelection(url + "APIsHandler")
    // command should have length = 5
    // Register
    service <? "Register|user1|pw1||" |> ignore
    service <? "Register|user1|pw1||" |> ignore
    service <? "Register|user2|pw2||" |> ignore
    service <? "Register|user3|pw3||" |> ignore
    // Login
    service <? "Login|user1|pw1||" |> ignore
    service <? "Login|user1|pw2||" |> ignore
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
main()

// User Class API test
(*
let user1 = User("user1", "pw1")
let user2 = User("user2", "pw2")
let user3 = User("user3", "pw3")
user1.SubscribeTo(user2)
user1.SubscribeTo(user3)
printf "%A" [user1.GetSubscribingList()]
printf "%A" [user1.GetTweetList()]
*)

// Simulator API test
(* 
Simulator.Register("user1", "pw1")
Simulator.Register("user2", "pw2")
Simulator.Register("user3", "pw3")
Simulator.SendTweet("user1", "pw1", "#test This the first tweet")
Simulator.SendTweet("user2", "pw2", "#test This the second tweet")
Simulator.SendTweet("user3", "pw3", "#omg #wtf I created another tag")
Simulator.Follow("user1", "pw1", "user2")
Simulator.Follow("user1", "pw1", "user3")
Simulator.Follow("user2", "pw2", "user4")
*)