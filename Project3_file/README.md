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
Each Pastry node maintains a routing table, a neighborhood set and a leaf set.We begin with a description of the routing table. A node’s routing table, R, is organized into dlog2bNe rows with 2^b - 1 entries each. The 2^b - 1 entries at row n of the routing table each refer to a node whose nodeId shares the present node’s nodeId in the first n digits, but whose (n + 1)th digit has one of the 2^b - 1 possible values other than the (n + 1)th digit in the present node’s id.

