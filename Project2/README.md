# DOSP Project 2 - Gossip Simulator
### Group Member:
- Zhang Dong, UFID: 69633983
- Xiaobai Li, UFID: 31567109
## Project Requirement
Finding perfect squares  that are sums of consecutive squares.
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

## Usage
To run the script, change your location to directory `Project2/`, using following command to test:
```shell
```