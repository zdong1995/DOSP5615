#load "./Data.fsx"
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote" 

open Data
open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

let user1 = User("user1", "pw1")
let user2 = User("user2", "pw2")
user1.SendTweet(Tweet("Hello World", "1", "user1"))
user1.SubsribeTo(user2)
printf "%A" [user1.GetSubsribingList()]
printf "%A" [user1.GetTweetList()]