# DOSP Project 3 - Pastry
## 1. Project Requirement
We talked extensively in class about the overlay networks and how they can be used to provide services. The goal of this project is to implement in F# using the actor model the Pastry protocol and a simple object access service to prove its usefulness. The specification of the Pastry protocol can be found in the paper Pastry: Scalable, decentralized object location and routing for large-scale peer-to-peer systems.by A. Rowstron and P. Druschel. You can find the paper at http://rowstron.azurewebsites.net/PAST/pastry.pdf.

## 2. Usage
In the F# scripts, new `#r "nuget:..."` command is used to corporate packages and resolve all dependencies. To run the script, the `--langversion:preview` flag after `dotnet fsi` command is required. This will not be required once F# 5 is released, which can be referenced to [Announcing F# 5 preview 1]
To run the script, change your location to directory `Project3_file/`, using following command to test:
`dotnet fsi --langversion:preview project3.fsx <numNodes> <numRequests> `
Where `numNodes` is the number of nodes settled for pastry network, numRequests` is the number of requests each node need to send.
