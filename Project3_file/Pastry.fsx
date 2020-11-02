#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote" 

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open System.Threading

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

// Construct the basic structures. 
type Message =
    |InitailizeNode of String * int
    |Route of String * String * int
    |Join of String*int
    |UpdateRoutingTable of String[]
    |Print
    |PassValue of String * int  
    |ShowResult 
       
          
// Input the information.
let args : string array = fsi.CommandLineArgs |> Array.tail
let mutable numNodes = args.[0] |> int
let mutable numRequests = args.[1] |> int

let mutable nodeMap : Map<String, IActorRef> = Map.empty 
let rand = Random()
let clone i (arr:'T[,]) = arr.[i..i, *]|> Seq.cast<'T> |> Seq.toArray

let system = ActorSystem.Create("RemoteFSharp", configuration)

let boss (name : string) = 
    let mutable reqHopMap: Map<String, int> = Map.empty
    spawn system name 
    <| fun mailbox ->
        let rec loop() = 
            actor {
                let! message = mailbox.Receive()
                match box message :?> Message with
                | PassValue(sourceId, hops) ->
                    // printfn "get a res (%s, %i) !" sourceId hops
                    if reqHopMap.ContainsKey sourceId then 
                        let hopNum = reqHopMap.TryFind sourceId
                        match hopNum with 
                        | Some x ->
                            let newHopNum = (x + hops)
                            reqHopMap <- reqHopMap.Add(sourceId, newHopNum)
                        | None -> printfn "Error! Could not get the source node id"
                    
                    else
                        reqHopMap <- reqHopMap.Add(sourceId, hops)    

                | ShowResult -> 
                    let averageHop = reqHopMap |> Map.toSeq |> Seq.map snd |> Seq.toArray

                    // LXB test
                    let mutable sum = 0.0
                    printfn "the length of the array is : %d " averageHop.Length
                    for i in [0..numNodes-1] do
                        let mutable hopNum = averageHop.[i] 
                        sum <- sum + double(hopNum)

                    let totalReqNum = double(numNodes * numRequests)
                    let res =  sum / totalReqNum
                    printfn "the ave of this algo is %f ! " res
                
                | _-> return! loop()
                return! loop() 
            }        
        loop()

let node (name : string) = 
    let mutable nodeId =""
    let mutable rowNum = 0
    let mutable colNum = 16 
    let mutable routingTable: string[,] = Array2D.zeroCreate 0 0
    let mutable commonPrefixLength = 0
    let mutable curRow = 0
    let mutable leafSet : Set<String> = Set.empty
    spawn system name
    <| fun mailbox ->
        let rec loop()  =
            actor { 
                let! message = mailbox.Receive()
                match box message :?> Message with
                   | InitailizeNode(i,d)->
                        nodeId <- i
                        rowNum <- d
                        routingTable <- Array2D.zeroCreate rowNum colNum
               
                        let mutable itr=0
                        let number = Int32.Parse(nodeId, Globalization.NumberStyles.HexNumber)

                        let mutable left = number
                        let mutable right = number
                
                        while itr < 8 do 
                           if left = 0 then
                              left <- nodeMap.Count-1 //check
                           leafSet <- leafSet.Add(left.ToString())
                           itr <- itr + 1
                           left <- left - 1
                  
                        while itr < 16 do
                           if right = nodeMap.Count-1 then
                              right <- 0
                           leafSet <- leafSet.Add(right.ToString())
                           itr <- itr + 1
                           right <- right + 1

                        // printfn "the node %s is initialized" nodeId
                                              
                   | Join(key, currentIndex) ->
                        let mutable i = 0
                        let mutable j = 0
                        let mutable k = currentIndex

                        while key.[i] = nodeId.[i] do
                            i <- i + 1
                        commonPrefixLength <- i
                        let mutable routingRow: string[] = Array.zeroCreate 0

                        while k <= commonPrefixLength do
                             routingRow <- clone k routingTable
                             routingRow.[Int32.Parse(nodeId.[commonPrefixLength].ToString(), Globalization.NumberStyles.HexNumber)] <- nodeId                        
                             // LXB test
                             // printfn "after clone the routing table now is %A " routingTable
                             // printfn "take out a row and want to send from %s is %A " nodeId routingRow
                             // LXB
                             let foundKey = nodeMap.TryFind key
                             match foundKey with
                             | Some x->
                                x<! UpdateRoutingTable(routingRow)
                                ()
                             | None -> printfn "Key does not exist in the map!"

                             k <- k+1

                        // printfn "the node %s has joined!" nodeId

                        let rtrow = commonPrefixLength
                        let rtcol = Int32.Parse(key.[commonPrefixLength].ToString(), Globalization.NumberStyles.HexNumber)
                        if isNull routingTable.[rtrow, rtcol] then
                            routingTable.[rtrow, rtcol] <- key
                        else
                            let temp = routingTable.[rtrow, rtcol]
                            let final = nodeMap.TryFind temp

                            match final with
                            | Some x ->
                                x<!Join(key, k)
                            | None ->printfn "Key does not exist in the map "    

                    | UpdateRoutingTable(row: String[])->
                        routingTable.[curRow, *] <- row
                        // LXB test
                        // printfn "for node %s now the routing table is %A" nodeId routingTable
                        // LXB
                        curRow <- curRow + 1         
                    
                    | Route(key, source, hops) ->
                        if nodeId = key then
                            let boss = system.ActorSelection(url + "boss")
                            boss <! PassValue(source, hops)

                        elif leafSet.Contains(key) then
                            let nextNode = nodeMap.Item(key)
                            nextNode <! Route(key, source, hops+1)
                              
                        else
                            let mutable i = 0
                            let mutable j = 0
                            while key.[i] = nodeId.[i] do
                                i<- i+1
                            commonPrefixLength <- i
                            let mutable rtrow = commonPrefixLength
                            let mutable rtcol = Int32.Parse(key.[commonPrefixLength].ToString(), Globalization.NumberStyles.HexNumber)
                            if isNull routingTable.[rtrow, rtcol] then
                                rtcol <- 0

                            nodeMap.Item(routingTable.[rtrow, rtcol]) <! Route(key, source, hops+1)
                            
                    
                        | Print ->
                          printfn "Routing table of node %s is \n%A" nodeId routingTable      
                        
                        | _-> return! loop()   
                return! loop()               
            }
        loop()
            


let numDigits = Math.Log(numNodes |> float, 16.0) |> ceil |> int
let multiply text times = String.replicate times text
printfn "Network construction initiated"
let mutable nodeId = ""
let mutable hexNum = ""
let mutable len = 0
nodeId <- multiply "0" numDigits

let mutable startNode = node nodeId
startNode <! InitailizeNode(nodeId, numDigits)
nodeMap <- nodeMap.Add(nodeId, startNode)

for i in [1.. numNodes-1] do
    if i = numNodes / 4 then
        printfn "The network is 25 percent done"
    elif i = numNodes / 2 then
        printfn "The network is 50 percent done"
    elif i = numNodes*(3/4) then
        printfn "The network is 75 percent done"

    hexNum <- i.ToString("X")
    len <- hexNum.Length
    nodeId <-  multiply "0" (numDigits-len) + hexNum
    let newNode = node nodeId
    newNode <! InitailizeNode(nodeId, numDigits)
    nodeMap <- nodeMap.Add(nodeId, newNode)
    // nodeMap |> Map.toSeq |> Seq.length |> printfn "the size of nodemap is %i"
    let temp = multiply "0" numDigits
    let final = nodeMap.Item temp
    final <! Join(nodeId, 0)
    Thread.Sleep 5
    
Thread.Sleep 1000
printfn "Network is now built"
      
let actorsArray = nodeMap |> Map.toSeq |> Seq.map fst |> Seq.toArray

// LXB test the actors 
// for i in actorsArray do
//     printfn "the actor array has %s" i
//     let actor = nodeMap.Item i
//     printfn "and the correspond actor is %A" actor
//     actor <! Print

printfn "Processing requests" 

let calculator = boss "boss" 
let mutable k = 1
let mutable destinationId = ""
let mutable ctr = 0
while k <= numRequests do
    for sourceId in actorsArray do
        ctr <- ctr + 1
        destinationId <- sourceId
        while destinationId = sourceId do
            destinationId <-  actorsArray.[rand.Next actorsArray.Length]
        let temp = nodeMap.Item sourceId
        temp <! Route(destinationId, sourceId, 0)
        Thread.Sleep 5

    printfn "Each peer performed %i requests" k
    k <- k + 1
    
Thread.Sleep 1000

calculator <! ShowResult













