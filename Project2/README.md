# DOSP Project 2 - Gossip Simulator
### Group Member:
- Zhang Dong, UFID: 69633983
- Xiaobai Li, UFID: 31567109
## Project Requirement
Implement gossip and push sum algorithms for different types of topologies. 
### Input:
The input provided (as command line to your project2) will be of the form:
`project2 numNodes topology algorithm`
Where numNodes is the number of actors involved (for 2D based topologies you can round up until you get a square), `topology` is one of `full`, `2D`, `line`, `imp2D`, `algorithm` is one of `gossip`, `push-sum`.

### Output:
Print the amount of time it took to achieve convergence of the algo- rithm. Please measure the time using
```
... build topology
val b = System.currentTimeMillis; ..... start protocol println(b-System.currentTimeMillis)
```

## Implementation
### Topology
Use Adjacency Matrix to build topology structure.

### How to test?
Uncomment `let numNodes = 9`, then select all codes below and evaluate using F# Interactive(FSI).
```
val mutable arr : int [,] = []
val numNodes : int = 9
val buildTopo : topology:string -> unit

> buildTopo "2D";;
val it : unit = ()

> arr;;
val it : int [,] = [[0; 1; 0; 1; 0; 0; 0; 0; 0]
                    [1; 0; 1; 0; 1; 0; 0; 0; 0]
                    [0; 1; 0; 0; 0; 1; 0; 0; 0]
                    [1; 0; 0; 0; 1; 0; 1; 0; 0]
                    [0; 1; 0; 1; 0; 1; 0; 1; 0]
                    [0; 0; 1; 0; 1; 0; 0; 0; 1]
                    [0; 0; 0; 1; 0; 0; 0; 1; 0]
                    [0; 0; 0; 0; 1; 0; 1; 0; 1]
                    [0; 0; 0; 0; 0; 1; 0; 1; 0]]

> buildTopo "imp2D";;
val it : unit = ()

> arr;;
val it : int [,] = [[0; 1; 0; 1; 0; 1; 0; 0; 0]
                    [1; 0; 1; 1; 1; 0; 0; 0; 0]
                    [0; 1; 0; 0; 0; 1; 0; 0; 1]
                    [1; 1; 0; 0; 1; 0; 1; 0; 0]
                    [0; 1; 0; 1; 0; 1; 1; 1; 0]
                    [1; 0; 1; 0; 1; 0; 0; 0; 1]
                    [0; 0; 0; 1; 1; 0; 0; 1; 0]
                    [0; 0; 0; 0; 1; 0; 1; 0; 1]
                    [0; 0; 1; 0; 0; 1; 0; 1; 0]]

> 

```

The following shows the node structure corresponding to Adjacency Matrix:

#### Full
Adjacency matrix all filled by 1, with size = number of nodes.

#### Line
![](https://raw.githubusercontent.com/zdong1995/PicGo/master/img/%E6%88%AA%E5%B1%8F2020-10-14%2023.34.08.png)

#### 2D Grid
![](https://raw.githubusercontent.com/zdong1995/PicGo/master/img/%E6%88%AA%E5%B1%8F2020-10-15%2000.38.47.png)

#### imperfect 2D Grid
Generate 2D grid and random generate extra edge to connect nodes.
If the node number is odd number, there will be one node doesn't have extra edge.

Example 1:
![](https://raw.githubusercontent.com/zdong1995/PicGo/master/img/%E6%88%AA%E5%B1%8F2020-10-15%2000.32.41.png)

Example 2:
![](https://raw.githubusercontent.com/zdong1995/PicGo/master/img/%E6%88%AA%E5%B1%8F2020-10-15%2001.02.12.png)


## Usage
To run the script, change your location to directory `Project2/`, using following command to test:

Currently only add support to Gossip algorithm. Use command `dotnet fsi --langversion:preview project2.fsx <nodeNumber> <topoType>` to run the program. Example:
```shell
> dotnet fsi --langversion:preview project2.fsx 9 2D

topology constructed
actors generated
4 finished
2 finished
1 finished
5 finished
7 finished
6 finished
3 finished
8 finished
0 finished
Converged! All actors finished
14.934400 ms
Main program finish!
```
