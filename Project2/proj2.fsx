#time "on"
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

let args : string array = fsi.CommandLineArgs |> Array.tail
let numNodes = args.[0] |> int
let topology = args.[1] |> string
let algorithm = args.[2] |> string

let mutable arr = Array2D.init

let buildTopo topology =
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
    
    let getRandom(r: Random) list =
        // get random element from list
        list |> Seq.sortBy(fun _ -> r.Next())
    
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
            let mutable needToConnect = numNodes

            for i = 0 to numNodes - 1 do
                if connected.[i] = false then
                    let mutable candidates = []
                    // find possible neighbors
                    for j in [0 .. i - 1] @ [i + 1 .. numNodes - 1] do // @: join list
                        if connected.[j] = false && arr.[i, j] = 0 then
                            candidates <- List.append candidates [j]
                    // generate random node index
                    let randomNode = candidates |> getRandom (Random()) |> Seq.head
                    // link the random node as neighbor
                    arr.[i, randomNode] <- 1
                    arr.[randomNode, i] <- 1
                    connected.[i] <- true
                    connected.[randomNode] <- true