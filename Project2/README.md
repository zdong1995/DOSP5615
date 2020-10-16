# DOSP Project 2 - Gossip Simulator
## 1. Project Requirement
As described in class Gossip type algorithms can be used both for group communication and for aggregate computation. The goal of this project is to determine the convergence of Gossip algorithms and Push-Sum Algorithm through a simulator based on actors written in F#.

## 2. Usage
In the F# scripts, new `#r "nuget:..."` command is used to corporate packages and resolve all dependencies. To run the script, the `--langversion:preview` flag after `dotnet fsi` command is required. This will not be required once F# 5 is released, which can be referenced to [Announcing F# 5 preview 1]
To run the script, change your location to directory `Project2/`, using following command to test:
`dotnet fsi --langversion:preview project2.fsx <nodeNumber> <topoType> <algorithm> `
Where `numNodes` is the number of actors involved (only square number allowed), `topology` is one of `full`, `2D`, `line`, `imp2D`,  and `algorithm` is one of `gossip`, `push-sum`.

Example:
```shell
> dotnet fsi --langversion:preview project2.fsx 9 2D gossip

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

## 3. Implementation
For this group communication simulation, the model is amounts of actors that connected in one kind of topology structure. We start the simulation by sent rumor message to one actor, then the actors in the whole system will starting hearing the message. Finally each actor  has heard the message and reach convergence conditions.

Thus the implementation consists by three parts, topology,  messaging system, and communication algorithm. The main program will start by building topology, generating actor nodes, starting timer, sending message to one actor and reporting simulation time after reached convergence.

### 3.1 Topology Structure
There will be four kind of topology we will test in this simulation experiment, including `full`, `2D`, `line` and `imp2D` . We will use adjacency matrix to represent the graph topology structure.

#### Full Network
Every actor is a neighbor of all other actors. That is, every actor can talk directly to any other actor. Thus each node will have connections will the other nodes. The whole adjacency matrix will be filled by 1, with size = number of nodes.

#### 2D Grid
Actors form a 2D grid and the actors can only talk to the grid neighbors. We will first find the size of grid by square root of number of nodes. Then for each node find the row number and column number to find each neighbor to fill adjacency matrix. One example of 2D grid, adjacency matrix and code export is shown below.

![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/topo_2d.png)

#### Line
Actors are arranged in a line. Each actor has only 2 neighbors (one left and one right, unless you are the first or last actor). Each node will have connections with predecessor and successor. One example of line, adjacency matrix and code export is shown below.
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/topo_line.png)

#### imperfect 2D Grid
Grid arrangement but one random other neighbor is selected from the list of all actors (4+1 neighbors). The building structure is as following:
- Generate a 2D grid
- traverse the whole matrix, for each node find list of possible neighbors and randomly generate one node from list
- connect the two node by adding extra edge, then update the possible neighbor list
- repeat until no possible neighbor exists

If the node number is odd number, there will always be one node that can’t have extra edge. Two examples of imperfect 2D grid, adjacency matrix and code export is shown below.

Example 1:
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/topo_imp2D_1.png)

Example 2:
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/topo_imp2d_2.png)


## Result
### Performance
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/performance.png)

### Scalibility
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/scalibility.png)

### Compare of Gossip and Push-Sum Algorithm
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/Compare.png)