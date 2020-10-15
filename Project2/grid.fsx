// 2d 
let arr = Array2D.zeroCreate 9 9
let gridSize = int(sqrt(float 9))
let getRandom next list =
            // get random element from list
            list |> Seq.sortBy(fun _ -> next())

for i = 0 to 8 do
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

// connect one random other node to each node
let mutable connected = Array.create 9 false

for i = 0 to 8 do
    if connected.[i] = false then
        let mutable candidates = []
        // find possible neighbors
        for j = 0 to 8 do // @: join list
            if i <> j && connected.[j] = false && arr.[i, j] = 0 then
                candidates <- List.append candidates [j]
        // generate random node index
        
        
        let r = System.Random()

        if candidates.Length <> 0 then
            let randomNode = candidates |> getRandom (fun _ -> r.Next()) |> Seq.head
            // link the random node as neighbor
            arr.[i, randomNode] <- 1
            arr.[randomNode, i] <- 1
            connected.[i] <- true
            connected.[randomNode] <- true

// line
let arr1D = Array2D.zeroCreate 3 3
for i = 0 to 2 do
    if i = 0 then
        arr1D.[i, i + 1] <- 1
    elif i = 2 then
        arr1D.[i, i - 1] <- 1
    else
        arr1D.[i, i - 1] <- 1
        arr1D.[i, i + 1] <- 1
