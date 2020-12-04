module Actor

#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote"

open Akka.Actor
open Akka.Configuration


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