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

### 3.2 Actor System
In the tutorial snippets we find remote actors is very useful. Akka remoting is designed for communication in a peer-to-peer fashion. Message sends to actors that are actually in the sending actor system do not get delivered via the remote actor ref provider. They’re delivered directly, by the local actor ref provider. Thus we decided to use remote actors for message communication in this project.

In Akka, used `system.ActorSelection(url)` to look up an actor and send message directly. In this project, we use three kind of remote actors:
- **Gossip Worker **: Remote actor that sending message and converged by Gossip algorithm, the message received is string of message.
- **Push-Sum Worker**: Remote actor that sending message and converged by Push-Sum algorithm, the message received is string of (s, w) pair.
- **Boss**: Remote actor that only listening to the workers to record the number of terminated worker, in order to determine the convergence of communication system. Each time when worker reach terminate condition, it will send message to boss and mark itself as terminated. When the number of message boss received is equal to number of nodes, the boss will mark the program as converged and stop the simulation.

### 3.3 Gossip Algorithm
For the gossip algorithm, each actor selects a random neighbor and send the message. It stops transmitting once it has heard the message 10 times.
Thus we will maintain one `count` for each actor and update count when received new message from neighbor.  The algorithm will execute as following:
- Increment `count` when receive new message
- Find list all possible neighbors
- Generate one random neighbor and send message
- When  `count` reached 10, send message to boss to indicate current actor is terminated

There will be some cases that could’t be converged based on this algorithm. The reason we analyzed is there may have some nodes whose neighbors are all terminated thus it cannot received enough message to reach 10 to terminate. Thus we improved the algorithm by maintaining a status array of termination and adding another termination condition:
- Find list all possible neighbors that are not terminated
- If list is empty, which means all neighbors are terminated, then current actor will also terminate, sent message to boss

This fixed most of the cases in the simulation. However I think there is still some tricky points here. The global termination status array will be modified by difference threads (actors), but the termination condition check will be asynchronous applied. There may be some condition that the neighbors of current node should marked to terminated by another condition, but the current node retrieve the status array before updates. Thus current node will still possibly send message to that nodes, but eventually current node will reach termination.

### 3.4 Push-Sum Algorithm
In Push-Sum algorithm, each actor receive one pair of `(s, w)` and update with the `(s, w)` of each. Then decrease by half and send half to another neighbor. It is a process to averaging `s/w`, when the difference between previous and current `s/w` is smaller than threshold consecutive 3 times, then we will terminate the nodes. This process is much easier and we don’t need to maintain a status array as Gossip algorithm. The tricky part is if we still use the boss to count number of terminated actors, then it will incorrectly converge before real convergence. Because if the difference can’t be lower than threshold consecutive 3 times, we will reset the count of actor to 0. Thus each time the count of actor reach 3, it will send message to boss, which will occur duplicate termination message to count.

Thus we define a boolean variable for actors to recursively parse, which is default set as `false`. Each time we update the current state based on last state. If the actor terminated, we just need to parse `true` to next recursion. Only at the condition that `count = 3 && not terminate`, we will send message to boss, which fixed this case.

## 4. Result
We have roughly test several input for the simulation and make plots as following:

#### Test Environment
- OS: Windows 10 Education, Version 2004
- Processor: AMD Ryzen Threadripper 2920X 12-Core 3.50 GHz
- RAM: 32.0 GB
- System Type: 64-bit Operating System, x64-based processor

### 4.1 Performance
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/performance.png)

#### Gossip
The result Gossip algorithm is similar as expected. The linear topology structure will take the longest time to communicating.  2D and imperfect 2D are similar. To our surprise, the fully connected network is not as good as we think. After 200 nodes, it even became worse than linear topology.

The reason we think is because there are so many neighbors for each node in the fully connected topology. If the number of nodes is not so large, the random picking will not have much negative impact. In the low number range, more neighbor means more opportunities to send messages to increase number that heard this rumor. It will speed up the communication process.

However, if the number of node is large, the random picking will became very low-efficiency. Just imagine if we can only choice to talk to 2 or 4 people, it will be surely easier to reach 10 times threshold to have choice to talk to 100 people. The fully-connected topology increase the degree of freedom, which statistically make the distribution more average, thus it will take longer for the average level to reach threshold.

#### Push-Sum
The Push-Sum increases the gap between linear structure and other topologies. The fully-connected topology works best in small range of node number. It satisfied our expectations. If we have high degree of freedom, we will be easier to generalize the value of `(s/w)` to be much easier to make difference smaller to reach threshold. It is not like the Gossip algorithm to increase count to converge. The whole averaging process will benefit from more connection. We can also see that imperfect 2D grid has one more freedom than 2D grid, which become faster than 2D grid. For large number of nodes, full network becomes low-performance due to large amount need to generalize for each node, thus it will exponentially increasing as the graph shown below.

### 4.2 Scalibility
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/scalibility.png)

Two algorithm both have good scalibility in 2D grid and impefect 2D grid. Full network and line spent much longer number time to converge after 200 nodes. The largest scale we tested is 40000 node for Gossip algorithm in 2D grid, which took 57 minutes to converge. Due to the limit of time and device, we didn't test so much for large scale.

### 4.3 Compare of Gossip and Push-Sum Algorithm
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/Compare.png)

As the scale increasing, Gossip algorithm shows better performance than Push-Sum algorithm. The performance dependency on these two algorithm shows worst performance for line, then full network, which are much worse than 2D systems. As the long convergence time, we didn't have further test for line and full network.