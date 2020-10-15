#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote" 

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

let mutable arr = Array2D.zeroCreate 0 0
let args : string array = fsi.CommandLineArgs |> Array.tail
let mutable numNodes = args.[0] |> int
let mutable topology = args.[1] |> string
// let algorithm = args.[2] |> string

let configuration = 
    ConfigurationFactory.ParseString(
        @"akka {
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

let url = "akka.tcp://RemoteFSharp@localhost:8777/user/"

let system = ActorSystem.Create("RemoteFSharp", configuration)

// let stopTime = 50

let mutable converged = false
// terminated represented whether the node at index is terminated: 0 -> not terminated, 1 -> terminated
let mutable terminated = Array.zeroCreate numNodes

let getRandom next list =
    // get random element from list
    list |> Seq.sortBy (fun _ -> next())
    
let r = System.Random()

let gossipWorker (name : string) = 
    spawn system name
    <| fun mailbox ->
        let rec loop count message =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                match box message with
                | :? string -> 
                        let newcount = count + 1
                        let mutable validNeighbors = []
                        let curIdx = int name
                        // find possible neighbors which are not terminated
                        for i = 0 to numNodes - 1 do
                            // check connected node, skip current index and terminated index
                            if arr.[curIdx, i] = 1 && i <> curIdx && terminated.[i] = 0 then
                                validNeighbors <- List.append validNeighbors [i]
                        
                        // all neighbors are terminated
                        if validNeighbors.Length = 0 then
                            terminated.[curIdx] <- 1
                            let boss = system.ActorSelection(url + "boss")
                            // notice boss current actor terminated
                            boss <? name |> ignore
                        else
                            let nextName = validNeighbors |> getRandom (fun _ -> r.Next()) |> Seq.head |> string
                            let nextNode = system.ActorSelection(url + nextName)
                            nextNode <? message |> ignore

                        if count = 10 then
                            let boss = system.ActorSelection(url + "boss")
                            // notice boss current actor terminated
                            boss <? name |> ignore

                        return! loop newcount message
                | _ ->  failwith "unknown message"
            } 
        loop 0 ""

let boss =
    spawn system "boss"
    <| fun mailbox ->
        let rec loop count =
            actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                match box message with
                | :? string -> 
                        printfn "%s finished" message
                        let idx = message |> int
                        
                        let newCount = count + 1
                        if newCount = numNodes then
                            printfn "Converged! All actors finished"
                            converged <- true
                        return! loop newCount
                | _ ->  failwith "unknown message"
            } 
        loop 0

let buildTopo topology numNodes =
    let build2DGrid numNodes =
        // use ajacent matrix to represent the relation between nodes
        arr <- Array2D.zeroCreate numNodes numNodes
        let gridSize = int(sqrt(float numNodes))
     
        for i = 0 to numNodes - 1 do
            // coordinate of current node in 2D grid
            let row = i / gridSize
            let col = i % gridSize
     
            // fill horizontal adjacency
            if col = 0 then
                arr.[i, i + 1] <- 1 // only have neighbor rightside
            elif col = gridSize - 1 then
                arr.[i, i - 1] <- 1 // only have neighbor leftside
            else
                arr.[i, i + 1] <- 1
                arr.[i, i - 1] <- 1
            
            // fill vertical adjacency
            if row = 0 then
                arr.[i, i + gridSize] <- 1 // only have neighbor downside
            elif row = gridSize - 1 then
                arr.[i, i - gridSize] <- 1 // only have neighbor upside
            else
                arr.[i, i + gridSize] <- 1
                arr.[i, i - gridSize] <- 1
    
    match topology with
        | "full" ->
            arr <- Array2D.create numNodes numNodes 1
        | "2D" ->
            build2DGrid numNodes
        | "line" ->
            arr <- Array2D.zeroCreate numNodes numNodes
            for i = 0 to numNodes - 1 do
                if i = 0 then
                    arr.[i, i + 1] <- 1
                elif i = numNodes - 1 then
                    arr.[i, i - 1] <- 1
                else
                    arr.[i, i - 1] <- 1
                    arr.[i, i + 1] <- 1
        | "imp2D" ->
            build2DGrid numNodes

            // connect one random other node to each node
            let mutable connected = Array.create numNodes false

            for i = 0 to numNodes - 1 do
                if connected.[i] = false then
                    let mutable candidates = []
                    // find possible neighbors
                    for j = 0 to numNodes - 1 do // @: join list
                        if i <> j && connected.[j] = false && arr.[i, j] = 0 then
                            candidates <- List.append candidates [j]
                    // generate random node index
                    if candidates.Length <> 0 then
                        let randomNode = candidates |> getRandom (fun _ -> r.Next()) |> Seq.head
                        // link the random node as neighbor
                        arr.[i, randomNode] <- 1
                        arr.[randomNode, i] <- 1
                        connected.[i] <- true
                        connected.[randomNode] <- true

let main() =
    // build topology structure
    buildTopo topology numNodes
    printfn "topology constructed"

    // create actors
    for i = 0 to numNodes - 1 do
        let name = string i
        gossipWorker name
    printfn "actors generated"

    let timer = System.Diagnostics.Stopwatch.StartNew()
    // start sending message
    // Gossip
    let startActor = system.ActorSelection(url + "0")
    startActor <? "Test message." |> ignore

    while not converged do
        0 |> ignore

    // printfn "%A" terminated
    timer.Stop()
    printfn "%f ms" timer.Elapsed.TotalMilliseconds
    printfn "Main program finish!"
    (* TODO
    match algorithm with
        | "gossip" -> 
            
        | "push-sum" ->
            //
    *)

main()