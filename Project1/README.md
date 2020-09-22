# DOSP Project 1
### Group Member:
- Zhang Dong, UFID: 69633983
- Xiaobai Li, UFID: 31567109
## Project Requirement
Finding perfect squares  that are sums of consecutive squares.
- Input: N and k.
- Output: The first number in the sequence for each solution.

Find all k consecutive numbers starting at 1 and up to N, such that the sum of squares is itself a perfect square (square of an integer).

## Usage
In the F# scripts, new `#r "nuget:..."` command is used to corparate packages and resolve all dependencies. To run the script, the `--langversion:preview` flag after `dotnet fsi` command is required. This will not be required once F# 5 is released, which can be referenced to [Announcing F# 5 preview 1](https://devblogs.microsoft.com/dotnet/announcing-f-5-preview-1/)

To run the script, change your location to directory `Project1/`, using following command to test:
```shell
dotnet fsi --langversion:preview proj1.fsx 3 2
dotnet fsi --langversion:preview proj1.fsx 40 24
dotnet fsi --langversion:preview proj1.fsx 1000000 4
```

## Project Specifications
### 1. Result of input with N = 1000000, K = 4
There is no solution for this pair of input.

```shell
dotnet fsi --langversion:preview proj1.fsx 1000000 4

# Real: 00:00:00.000, CPU: 00:00:00.000, GC gen0: 0, gen1: 0, gen2: 0
# Real: 00:00:00.273, CPU: 00:00:00.342, GC gen0: 2, gen1: 0, gen2: 0
```
### 2. Ration of CPU time to Real

### 2. Size of the work unit with best performance
