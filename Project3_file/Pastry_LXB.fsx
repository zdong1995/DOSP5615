#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote" 

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit
open System.Collections.Generic

// Construct the basic structures.
type Request = 
    { KeyId: int
      Content: string}

type InputInfo = 
    { NumNodes: int
      NumRequests: int}      
    
// Define the basic methods.
// Transfer base 10 value to base 4 list.
let intToDigits baseN value : int list =
    let rec loop num digits =
        let q = num / baseN
        let r = num % baseN
        if q = 0 then 
            r :: digits 
        else 
            loop q (r :: digits)

    loop value []


// let list = intToDigits 4 254
// for item in [0..list.Length-1] do  
//     printfn "%i" list.[item]

// printfn "%i" list.[3]

// |> List.fold (fun acc x -> acc + x.ToString()) ""
// |> printfn "%s"

// Transfer base 4 list to base 10 number.

// Input the information.
let args : string array = fsi.CommandLineArgs |> Array.tail
let mutable numNodes = args.[0] |> int
let mutable numRequests = args.[1] |> int
let content = "This is a message that need to be delivered "

// printfn "the message received and the node number is %i and request number is %i" numNodes numRequests
// let p = 0xFFFFFFFFUL
// let k = 0xABCDEFABUL
// let mid = (p + k) / 0x2UL
// let mids = mid.ToString("X")

// let indToIdDict = new Dictionary<int, uint64>()
// indToIdDict.Add(3, 0x4567UL)
// let pair = indToIdDict.TryGetValue 3
// let value = snd pair
// printf "%d" value

// Define the Actor types
type GlobalInfo() =
    inherit Actor()
    
    let indToIdDict = new Dictionary<int, uint64>()
    let idToIndDict = new Dictionary<uint64, int>()
    
    override x.OnReceive(inputMsg) = 
        match inputMsg with
        | :? InputInfo as inputMsg -> 
                          printfn "the msg received and the node number is : %i " inputMsg.NumNodes
                    

        | _-> failwith "Wring Input!"

type Node(globalInfo: IActorRef, nodeId: uint64) = 
    inherit Actor()

    let mutable nodeId = nodeId
    
    override x.OnReceive(message) = 
        match message with 
        | :? string as msg -> 
                       let nodeIdStr = nodeId.ToString("X")        
                       printfn "Here the node ID is : %d and get the content %s" nodeId msg 
         
        | _-> failwith "Wrong message type!"


// Initialize the actors.
let system = ActorSystem.Create("Pastry")
let globalInfo = system.ActorOf(Props.Create(typeof<GlobalInfo>),"GlobalInfo")

let nodeArray = Array.zeroCreate (numNodes)
for i in [0..numNodes-1] do
       let dataSeg = 0xFFFFFFUL / uint64(numNodes)
       let nodeId = dataSeg * uint64(i) + dataSeg / uint64(2) 
       nodeArray.[i]<-system.ActorOf(Props.Create(typeof<Node>,globalInfo, nodeId),"demo"+string(i))

for i in [0..numNodes-1] do 
       nodeArray.[i] <! sprintf "node ind is %i" i     

let inputInfo: InputInfo = 
        { NumNodes = numNodes
          NumRequests = numRequests}
globalInfo <! inputInfo

system.Terminate()





