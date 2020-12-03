module Message

#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote"

type Message =
    | MsgRegister of string * string // ("username", "password")
    | MsgTweet of string * string * string // ("username", "password", "content")
    | MsgFollow of string * string * string // ("username", "password", "toFollow")
    | MsgReTweet of string * string * string // ("username", "password", "content")
    | MsgQuery of int * string [] // (Query type * [hasTag, mentionedId, curUserId]
    | MsgEmpty of string
    // TODO
    // Query type : 0. only hashTag 1. only mentioned 2. hashTag and mentioned

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

let configuration =
    ConfigurationFactory.ParseString
        (@"akka {
            log-config-on-start : on
            stdout-loglevel : DEBUG
            loglevel : ERROR
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                debug : {
                    receive : on
                    autoreceive : on
                    lifecycle : on
                    event-stream : on
                    unhandled : on
                }
            }
            remote {
                helios.tcp {
                    port = 8777
                    hostname = localhost
                }
            }
        }")

let url =
    "akka.tcp://RemoteFSharp@localhost:8777/user/"

let system =
    ActorSystem.Create("RemoteFSharp", configuration)
