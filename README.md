# DOSP5615
Repository of projects using F# and Akka for 2020 Fall Distributed Operation System Principle.

#### Group Member
- Zhang Dong
- Xiaobai Li

## [Project 1: Perfect Square](https://github.com/zdong1995/DOSP5615/tree/master/Project1)

This project aim to find perfect squares that are sums of consecutive squares.

- Input: `N` and `k`.
- Output: The first number in the sequence for each solution.

Find all k consecutive numbers starting at 1 and up to N, such that the sum of squares is itself a perfect square (square of an integer).

## [Project 2: Gossip Simulator](https://github.com/zdong1995/DOSP5615/tree/master/Project2)

Gossip type algorithms can be used both for group communication and for aggregate computation. This project is to determine the convergence of Gossip algorithms and Push-Sum Algorithm through a simulator based on actors written in F#.

- Input: `nodeNumber`, `topoType` and `algorithm`, Where `numNodes` is the number of actors involved, `topology` is one of `full`, `2D`, `line`, `imp2D`,  and `algorithm` is one of `gossip`, `push-sum`.
- Output: Convergence time