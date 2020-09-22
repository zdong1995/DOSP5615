#time "on"
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit

type InputParas = {
     StartNum: uint64
     Length: uint64 }

type Message = {
     RangeNum: uint64
     Length: uint64}     

let checkPerfectSqr (inputParas : InputParas) =
        let beginNum = inputParas.StartNum 
        let endNum = beginNum + inputParas.Length - uint64(1)
        let mutable sum = 0UL
        for i = (int)beginNum to (int)endNum do
            sum <- sum + ((uint64)i * (uint64)i)

        let root = uint64(sqrt((float)sum))
        // printfn "have been there and the sum is %i" sum
        let rootSqr = root * root
        if rootSqr = sum then printfn "%i, " inputParas.StartNum
        else () // break

// Actor part
let system = ActorSystem.Create("PerfectSqr")
     
type Processor(name) =
    inherit Actor()

    override x.OnReceive message =
        match box message with
        | :? InputParas as ip -> do checkPerfectSqr(ip)
        | _ ->  failwith "unknown message"

type Commandor(name) =
    inherit Actor()

    override x.OnReceive message =
        match box message with
        | :? Message as msg ->       
            let processors = 
                [1..10000]
                |> List.map(fun id ->  let properties = [| string(id) :> obj |]
                                       system.ActorOf(Props(typedefof<Processor>, properties)))

            let rand = Random(1234)

            for i = 1 to (int)msg.RangeNum do
                       
                let oneInput: InputParas = 
                    { StartNum = (uint64)i
                      Length = msg.Length }

                processors.Item(rand.Next() % 10000) <! oneInput   
               
        | _ ->  failwith "unknown message"

let args : string array = fsi.CommandLineArgs |> Array.tail
// let input = Console.ReadLine()
// let parameters = input.Split ' '
let thisRange = args.[0]|> uint64
let thisLength = args.[1]|> uint64
let thisMsg: Message = 
        { RangeNum = thisRange
          Length = thisLength}

let generateCommandor = system.ActorOf(Props(typedefof<Commandor>, [| string("1") :> obj |]))
generateCommandor <! thisMsg 
system.Terminate()  