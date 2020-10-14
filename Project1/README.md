# DOSP Project 1 - Perfect squares
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
# Real: 00:00:00.190, CPU: 00:00:00.406, GC gen0: 1, gen1: 0, gen2: 0
```
### 2. Ratio of CPU time to Real
CPU Time / Real Time = 0.406/0.190 = 2.13684

### 3. Size of the work unit with best performance
For task with input N = 10000000, K = 4, the following table shows test result for different worker numbers.

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

Based on the table, the best size of work unit is 12.

### 4. Largest Problem size
The largest problem size the script managed to solve is 10^7.

## Environment
- OS: Windows 10 Education, Version 2004
- Processor: AMD Ryzen Threadripper 2920X 12-Core 3.50 GHz
- RAM: 32.0 GB
- System Type: 64-bit Operating System, x64-based processor
