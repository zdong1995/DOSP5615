# DOSP5615
Repository of projects using F# and Akka for 2020 Fall Distributed Operation System Principle.

#### Group Member
- Zhang Dong
- Xiaobai Li

## [Project 1: Perfect Square](https://github.com/zdong1995/DOSP5615/tree/master/Project1)

This project aim to find perfect squares that are sums of consecutive squares, using Akka actor model in F#. Find all k consecutive numbers starting at 1 and up to N, such that the sum of squares is itself a perfect square (square of an integer).

- Input: `N` and `k`.
- Output: The first number in the sequence for each solution.

Example:
| #worker | CPU Time | Real Time | CPU / Real |
|----|----|----|----|
|1000|0.265|0.187|1.41711|
|500|0.328|0.186|1.76344|
|250|0.343|0.186|1.84409|
|100|0.375|0.192|1.95313|
|50|0.390|0.197|1.9797|
|15|0.406|0.187|2.17112|
|12|0.437|0.185|2.3379|
|10|0.406|0.190|2.1369|
|5|0.375|0.189|1.98413|
|1|0.281|0.187|1.50267|

## [Project 2: Gossip Simulator](https://github.com/zdong1995/DOSP5615/tree/master/Project2)

Gossip type algorithms can be used both for group communication and for aggregate computation. This project is to determine the convergence of Gossip algorithms and Push-Sum Algorithm through a simulator based on actors written in F#.

- Input: `nodeNumber`, `topoType` and `algorithm`, Where `numNodes` is the number of actors involved, `topology` is one of `full`, `2D`, `line`, `imp2D`,  and `algorithm` is one of `gossip`, `push-sum`.
- Output: Convergence time

### Topology Structure
- 2D Grid
- Line
- imperfect 2D Grid

### Performance
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/performance.png)

### Scalibility
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/scalibility.png)

### Compare of Gossip and Push-Sum Algorithm
![](https://github.com/zdong1995/DOSP5615/blob/master/Project2/img/Compare.png)

## [Project 3: Pastry Protocal](https://github.com/zdong1995/DOSP5615/tree/twitter/Project3)

Implementation the Pastry protocol in F# using the actor model and a simple object access service to prove its usefulness. The specification of the Pastry protocol can be found in the paper Pastry: [Scalable, decentralized object location and routing for large-scale peer-to-peer systems](http://rowstron.azurewebsites.net/PAST/pastry.pdf).


- Input: `nodeNumber` and `umRequests`, Where `numNodes` is the number of nodes settled for pastry network, `numRequests` is the number of requests each node need to send.
- Output: Average number of hops (node connections) that have to be traversed to deliver a message.

Example:
```shell
> dotnet fsi --langversion:preview project3.fsx 8 3

Network has been built
Now start to processing the requests.
Now all the node send 1 requests
Now all the node send 2 requests
Now all the node send 3 requests
The requests are all processed
the ave of this algo is 1.000000
```

## [Project 4.1: Twitter Enginee Clone](https://github.com/zdong1995/DOSP5615/tree/twitter/Project4.1)

Twitter Enginee Clone using F# and Akka with support to following functionality was designed:
- Register account and Authenticatios
- Send tweet with support to auto-parse multiple hashtags and mentions from the content. e.g. `#dosp #uf Twitter clone is so cool! @twitter @gators`
- Subscribe to user's tweets
- Re-tweets with identification of re-tweets source
- Querying tweets subscribed to, tweets with specific hashtags, tweets in which the user is mentioned
- If the user is connected, deliver the above types of tweets live (without querying)

Also a tester/simulator to test the above was implemented with support to:
- Simulate as many users as you can
- Simulate periods of live connection and disconnection for users
- Simulate a Zipf distribution on the number of subscribers

### Infrastructure
![](https://github.com/zdong1995/DOSP5615/blob/twitter/Project4.1/img/Infra.jpg)

### Workflow
![](https://github.com/zdong1995/DOSP5615/blob/master/Project4.1/img/Workflow.jpg)

## [Project 4.2: Twitter Enginee Clone WebSocket](https://github.com/zdong1995/DOSP5615/tree/twitter/Project4.2)

Credit to Xiaobai Li @leesunlion

[Demo Video](https://www.youtube.com/watch?v=_NDVHhxO5DQ&feature=youtu.be)

## Reference
1. "[Actor-based Concurrency with F# and Akka.NET](https://gist.github.com/akimboyko/e58e4bfbba3e9a551f05)" sample code [@Akim Boyko](https://gist.github.com/akimboyko)

2. [F# documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/)

3. [Akka.NET documentation](https://getakka.net/articles/intro/what-is-akka.html)

4. [Akka.NET with F#](https://russcam.github.io/fsharp-akka-talk/#/intro) slides [@Russ Cam](https://twitter.com/forloop)

5. [Functional Actor Patterns with Akka.NET and F#](https://mikhail.io/2016/03/functional-actor-patterns-with-akkadotnet-and-fsharp/)