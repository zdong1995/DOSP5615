#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote" 

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open FSharp.Core
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

let url = "akka.tcp://Pastry@localhost:8777/user/"

// Construct the basic structures. 
type Message =
    |InitializeNode of String * int
    |RouteMsg of String * String * int
    |JoinNode of String*int
    |UpdateRoutingTable of int * String[]
    |Print
    |PassValue of String * int  
    |ShowResult 
                 
// Input the information.
let args : string array = fsi.CommandLineArgs |> Array.tail
let mutable numNodes = args.[0] |> int
let mutable numRequests = args.[1] |> int

// Define the data structures to keep global information.
let mutable indToIdMap : Map<int, String> = Map.empty
let mutable idToIndMap : Map<String, int> = Map.empty
let mutable nodeMap : Map<String, IActorRef> = Map.empty 

// Define the methods here.
let rand = Random()
let copyOneRow ind (arr: 'T[,]) = arr.[ind..ind, *] |> Seq.cast<'T> |> Seq.toArray

let standardFrom oriNodeId : string = 
    let mutable newNodeId = oriNodeId
    while String.length newNodeId < 8 do
          newNodeId <- ("0" + newNodeId)
    newNodeId

// Create the actor system and define the actors.
let system = ActorSystem.Create("Pastry", configuration)

let boss (name : string) = 
    let mutable reqHopMap: Map<String, int> = Map.empty
    spawn system name 
    <| fun mailbox ->
        let rec loop() = 
            actor {
                let! message = mailbox.Receive()
                match box message :?> Message with
                | PassValue(sourceId, hops) ->
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

                    let mutable sum = 0.0
                    printfn "the length of the array is : %d " averageHop.Length
                    for i in [0..numNodes-1] do
                        let mutable hopNum = averageHop.[i] 
                        sum <- sum + double(hopNum)

                    let totalReqNum = double(numNodes * numRequests)
                    let res =  sum / totalReqNum
                    printfn "the ave of this algo is %f " res
                
                | _-> return! loop()
                return! loop() 
            }        
        loop()

let node (name : string) = 
    let mutable nodeId =""
    let mutable rowNum = 0
    let mutable colNum = 16 
    let mutable leafSet : Set<String> = Set.empty
    let mutable routingTable: string[,] = Array2D.zeroCreate 0 0
    let mutable commonPrefixLength = 0    
    spawn system name
    <| fun mailbox ->
        let rec loop()  =
            actor { 
                let! message = mailbox.Receive()
                match box message :?> Message with
                   | InitializeNode(inputNodeId, digitNum)->
                        nodeId <- inputNodeId
                        rowNum <- digitNum
                        routingTable <- Array2D.zeroCreate rowNum colNum
                              
                        let curNodeInd = idToIndMap.Item(nodeId)
                        let mutable leftInd = curNodeInd - 1
                        let mutable rightInd = curNodeInd + 1
                       
                        let mutable i = 0
                        while i < 8 do 
                            if leftInd < 0 then 
                               leftInd <- numNodes - 1
                              
                            let oneLeafId = indToIdMap.Item(leftInd)   
                            leafSet <- leafSet.Add(oneLeafId)                               
                            leftInd <- leftInd - 1
                            i <- i + 1

                        while i < 16 do
                            if rightInd > numNodes - 1 then
                                rightInd <- 0
                            let oneLeafId = indToIdMap.Item(rightInd)     
                            leafSet <- leafSet.Add(oneLeafId)
                            rightInd <- rightInd + 1
                            i <- i + 1   
                                                                      
                   | JoinNode(joinNodeId, startRowInd) ->
                        let mutable i = 0
                        let mutable curRowInd =  startRowInd
                        while joinNodeId.[i] = nodeId.[i] do
                            i <- i + 1
                        commonPrefixLength <- i
                                                        
                        let mutable oneRow : string[] = Array.zeroCreate 0                                                      
                        while curRowInd <= commonPrefixLength do 
                            oneRow <- copyOneRow curRowInd routingTable                                                                                                                                       
                            oneRow.[Int32.Parse(nodeId.[commonPrefixLength].ToString(), Globalization.NumberStyles.HexNumber)] <- nodeId                   
                            
                            let findJoinNode = nodeMap.TryFind joinNodeId
                            match findJoinNode with
                             | Some joinNode->
                                joinNode <! UpdateRoutingTable(curRowInd, oneRow)
                                ()
                             | None -> printfn "Error! The join node could nof be found in node map"

                            curRowInd <- curRowInd + 1

                        let rowInd = commonPrefixLength
                        let colInd = Int32.Parse(joinNodeId.[commonPrefixLength].ToString(), Globalization.NumberStyles.HexNumber)
                        
                        if isNull routingTable.[rowInd, colInd] then
                            routingTable.[rowInd, colInd] <- joinNodeId
                        else
                            let newStartNode = routingTable.[rowInd, colInd]
                            let findNewStarter = nodeMap.TryFind newStartNode

                            match findNewStarter with
                            | Some newStarter ->
                                newStarter <! JoinNode(joinNodeId, commonPrefixLength+1)
                            | None -> printfn "Error! This node does not exist in the map! "

                    | UpdateRoutingTable(rowInd, row)->
                        routingTable.[rowInd, *] <- row
                                                                
                    | RouteMsg(keyId, sourceId, hops) ->
                        if nodeId = keyId then
                            // printfn "this case happen and will pass (%s , %i)" sourceId hops 
                            let boss = system.ActorSelection(url + "boss")
                            boss <! PassValue(sourceId, hops)

                        elif leafSet.Contains(keyId) then
                            let nextNode = nodeMap.Item(keyId)
                            nextNode <! RouteMsg(keyId, sourceId, hops+1)
                              
                        else
                            let mutable i = 0
                            while keyId.[i] = nodeId.[i] do
                                i<- i+1
                            commonPrefixLength <- i
                            let mutable rtrow = commonPrefixLength
                            let mutable rtcol = Int32.Parse(keyId.[commonPrefixLength].ToString(), Globalization.NumberStyles.HexNumber)
                            if isNull routingTable.[rtrow, rtcol] then
                                rtcol <- 0

                            nodeMap.Item(routingTable.[rtrow, rtcol]) <! RouteMsg(keyId, sourceId, hops+1)
                            
                    
                    | Print ->
                        let ind = idToIndMap.Item(nodeId)
                        printfn "the routing table of (%i , %s) is %A" ind nodeId routingTable
                        Thread.Sleep 100    
                        
                    | _-> return! loop()   
                return! loop()               
            }
        loop()
            

// let numDigits = Math.Log(numNodes |> float, 16.0) |> ceil |> int
// let multiply text times = String.replicate times text
printfn "Network construction initiated"
let mutable curNodeId = ""
let mutable hexNum = ""
let mutable len = 0

let oneSeg = uint32(0xFFFFFFFF) / uint32(numNodes)
for ind in [0..numNodes-1] do
    curNodeId <- (uint32(ind) * oneSeg).ToString("X") 
    curNodeId <- standardFrom curNodeId
    indToIdMap <- indToIdMap.Add(ind, curNodeId)
    idToIndMap <- idToIndMap.Add(curNodeId, ind)    
    // printfn "the node ( %i, %s ) generated" ind curNodeId

let startNode = node "0"
let startNodeId = indToIdMap.Item(0)
startNode <! InitializeNode(startNodeId, 8)
nodeMap <- nodeMap.Add(startNodeId, startNode)

for ind in [1.. numNodes-1] do
    if ind = numNodes / 2 then
        printfn "The network is 50 percent done"
   
    curNodeId <- indToIdMap.Item(ind)
    let curNode = node curNodeId
    nodeMap <- nodeMap.Add(curNodeId, curNode) 
    curNode <! InitializeNode(curNodeId, 8)
    startNode <! JoinNode(curNodeId, 0)       
    Thread.Sleep 5
    
Thread.Sleep 1000
printfn "Network is now built"
      
printfn "Now start to processing the requests."
let calculator = boss "boss"
let mutable roundNum = 1
while roundNum <= numRequests do
    for sourceInd in [0..numNodes-1] do
        let sourceId = indToIdMap.Item(sourceInd)
        let sourceNode = nodeMap.Item(sourceId)
        let mutable desInd = rand.Next() % numNodes        
        while desInd = sourceInd do
            desInd <- rand.Next() % numNodes        
        let mutable desId = indToIdMap.Item(desInd)    
        sourceNode <! RouteMsg(desId, sourceId, 0)
    printfn "Now all the node send %i requests" roundNum  
    roundNum <- roundNum + 1
    Thread.Sleep 10

Thread.Sleep 1000
printfn "The requests are processed"
    
calculator <! ShowResult













