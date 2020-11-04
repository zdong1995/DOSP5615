# DOSP Project 3 - Pastry
## 1. Project Requirement
We talked extensively in class about the overlay networks and how they can be used to provide services. The goal of this project is to implement in F# using the actor model the Pastry protocol and a simple object access service to prove its usefulness. The specification of the Pastry protocol can be found in the paper Pastry: Scalable, decentralized object location and routing for large-scale peer-to-peer systems.by A. Rowstron and P. Druschel. You can find the paper at http://rowstron.azurewebsites.net/PAST/pastry.pdf.

## 2. Usage
In the F# scripts, new `#r "nuget:..."` command is used to corporate packages and resolve all dependencies. To run the script, the `--langversion:preview` flag after `dotnet fsi` command is required. This will not be required once F# 5 is released, which can be referenced to [Announcing F# 5 preview 1] To run the script, change your location to directory `Project3_file/`, using following command to test: `dotnet fsi --langversion:preview project3.fsx <numNodes> <numRequests> ` Where `numNodes` is the number of nodes settled for pastry network, numRequests` is the number of requests each node need to send.

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
## 3. Implementation
For the Pastry simulation, Each node in the Pastry peer-to-peer overlay network is assigned a 128-bit node identifier (nodeId). The nodeId is used to indicate a node’s position in a circular nodeId space, which ranges from 0 to 2^128 - 1. The nodeId is assigned randomly when a node joins the system. It is assumed that nodeIds are generated such that the resulting set of nodeIds is uniformly distributed in the 128-bit nodeId space.

### 3.1 Node State
Each Pastry node maintains a routing table, a neighborhood set and a leaf set.We begin with a description of the routing table. A node’s routing table, R, is organized into log2^b N rows with 2^b - 1 entries each. The 2^b - 1 entries at row n of the routing table each refer to a node whose nodeId shares the present node’s nodeId in the first n digits, but whose (n + 1)th digit has one of the 2^b - 1 possible values other than the (n + 1)th digit in the present node’s id.

Each entry in the routing table contains the IP address of one of potentially many nodes whose nodeId have the appropriate prefix; in practice, a node is chosen that is
close to the present node, according to the proximity metric. If no node is known with a suitable nodeId, then the routing table entry is left empty. The uniform distribution of nodeIds ensures an even population of the nodeId space; thus, on average, only log2^b N rows are populated in the routing table.

The neighborhood set M contains the nodeIds and IP addresses of the |M| nodes that are closest (according the proximity metric) to the local node. The neighborhood set
is not normally used in routing messages; it is useful in maintaining locality properties. The leaf set L is the set of nodes with the |L|/2 numerically closest larger nodeIds, and the |L|/2 nodes with numerically closest smaller nodeIds, relative to the present node’s nodeId. The leaf set is used during the message routing, as described below. Typical values for |L| and |M| are 2^b or 2*2^b.

![](https://github.com/zdong1995/DOSP5615/blob/master/Project3_file/img/RoutingTable.png)

### 3.2 Routing
Given a message, the node first checks to see if the key falls within the range of nodeIds covered by its leaf set. If so, the message is forwarded directly to the destination node, namely the node in the leaf set whose nodeId is closest to the key (possibly the present node).

If the key is not covered by the leaf set, then the routing table is used and the message is forwarded to a node that shares a common prefix with the key by at least
one more digit. In certain cases, it is possible that the appropriate entry in the routing table is empty or the associated node is not reachable, in which case the message is forwarded to a node that shares a prefix with the key at least as long as the local node, and is numerically closer to the key than the present node’s id. Such a node must be in the leaf set unless the message has already arrived at the node with numerically closest nodeId. And, unless bjLj=2c adjacent nodes in the leaf set have
failed simultaneously, at least one of those nodes must be live.

This simple routing procedure always converges, because each step takes the message to a node that either (1) shares a longer prefix with the key than the local node, or
(2) shares as long a prefix with, but is numerically closer to the key than the local node. It can be shown that the expected number of routing steps is log2^b N steps.

![](https://github.com/zdong1995/DOSP5615/blob/master/Project3_file/img/PseudoCode.png)

## 4. Result
We have roughly test several input for the simulation and make plots as following:

#### Test Environment
- OS: Windows 10, Version Professional
- Processor: Intel(R) Core(TM) i5-4210U CPU @ 1.70GHz 2.39GHz
- RAM: 8.0 GB
- System Type: 64-bit Operating System, x64-based processor

### 4.1 Performance




