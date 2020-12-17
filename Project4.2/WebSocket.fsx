#load "./Data.fsx"
#load "./Actor.fsx"
#load "./WebServer.fsx"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Remote"
#r "nuget: Suave"
#r "nuget: FSharp.Data"
#r "nuget: Newtonsoft.Json"
open Actor

open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit
open Akka.Remote

open Suave
open Suave.Http
open Suave.Operators
open Suave.Filters
open Suave.Successful
open Suave.Files
open Suave.RequestErrors
open Suave.Logging
open Suave.Utils

open System
open System.Net

open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

open Newtonsoft.Json
open FSharp.Data.JsonExtensions

type Content = {
    UserId: String
    PassWord: String
    ToFollowId: String
    TweetContent: String
    OriAuthor: String
    QueryTag: String
}
type frontMsg = {
    Operation: String
    Content: Content
}

type ResponseMsg = {
    ResType: String
    ResContent: String 
}

let backendServer = system.ActorSelection(url + "APIsHandler")
let ws (webSocket : WebSocket) (context: HttpContext) =
  socket {
    // if `loop` is set to false, the server will stop receiving messages
    let mutable loop = true

    while loop do
      // the server will wait for a message to be received without blocking the thread
      let mutable cmd = ""
      let mutable operation = ""
      let mutable userId = ""
      let mutable passWord = ""
      let mutable tweetContent = ""
      let mutable toFollowId = ""
      let mutable oriAuthorId = ""
      let mutable queryTag = ""

      let mutable resType = ""
      let mutable resContent = ""
      let! msg = webSocket.read()

      match msg with
      // the message has type (Opcode * byte [] * bool)
      //
      // Opcode type:
      //   type Opcode = Continuation | Text | Binary | Reserved | Close | Ping | Pong
      //
      // byte [] contains the actual message
      //
      // the last element is the FIN byte, explained later
      | (Text, data, true) ->
        // the message can be converted to a string
        let str = UTF8.toString data
        let info = JsonConvert.DeserializeObject<frontMsg> str
        printfn "get the info structure %A " info
        operation <- info.Operation
        printfn "the operation type is : %s" operation
        let content = info.Content
        printfn "the userId is : %s " content.UserId
        match operation with 
        | "Register" -> 
            userId <- content.UserId
            passWord <- content.PassWord
            cmd <- "Register|" + userId + "|" + passWord + "||"
            resType <- "RegRes"

        | "Login" ->
            userId <- content.UserId
            passWord <- content.PassWord
            cmd <- "Login|" + userId + "|" + passWord + "||"
            resType <- "LoginRes"

        | "Follow" ->
            userId <- content.UserId 
            passWord <- content.PassWord
            toFollowId <- content.ToFollowId
            cmd <- "Follow|" + userId + "|" + passWord + "|" + toFollowId + "|"
            resType <- "FollowRes"

        | "Tweet" ->
            userId <- content.UserId 
            passWord <- content.PassWord
            tweetContent <- content.TweetContent
            cmd <- "Tweet|" + userId + "|" + passWord + "|" + tweetContent + " |"
            resType <- "TweetRes"

        | "ReTweet" ->
            userId <- content.UserId 
            passWord <- content.PassWord
            tweetContent <- content.TweetContent
            oriAuthorId <- content.OriAuthor
            cmd <- "ReTweet|" + userId + "|" + passWord + "|" + tweetContent + " |" + oriAuthorId
            resType <- "ReTweetRes"

        | "Query" ->
            userId <- content.UserId 
            cmd <- "Query|" + userId + "|||"
            resType <- "QueryRes"

        | "Tag" ->
            queryTag <- content.QueryTag
            cmd <- "Tag|" + queryTag + "|||"
            resType <- "TagQueryRes"

        | "Mention" ->
            userId <- content.UserId 
            cmd <- "Mention|" + userId + "|||"
            resType <- "MentionRes"

        | _->()

        resContent <- Async.RunSynchronously(backendServer <? cmd) |> string
        let response: ResponseMsg = {
            ResType = resType
            ResContent = resContent
        }
        let jsonRes = JsonConvert.SerializeObject response
        printfn "the response structure sent is : %A" response
        // the response needs to be converted to a ByteSegment
        let byteResponse =
          jsonRes
          |> System.Text.Encoding.ASCII.GetBytes
          |> ByteSegment

        // the `send` function sends a message back to the client
        do! webSocket.send Text byteResponse true

      | (Close, _, _) ->
        let emptyResponse = [||] |> ByteSegment
        do! webSocket.send Close emptyResponse true

        // after sending a Close message, stop the loop
        loop <- false

      | _ -> ()
    }

/// An example of explictly fetching websocket errors and handling them in your codebase.
let wsWithErrorHandling (webSocket : WebSocket) (context: HttpContext) = 
   
   let exampleDisposableResource = { new IDisposable with member __.Dispose() = printfn "Resource needed by websocket connection disposed" }
   let websocketWorkflow = ws webSocket context
   
   async {
    let! successOrError = websocketWorkflow
    match successOrError with
    // Success case
    | Choice1Of2() -> ()
    // Error case
    | Choice2Of2(error) ->
        // Example error handling logic here
        printfn "Error: [%A]" error
        exampleDisposableResource.Dispose()
        
    return successOrError
   }

let app : WebPart = 
  choose [
    path "/websocket" >=> handShake ws
    path "/websocketWithSubprotocol" >=> handShakeWithSubprotocol (chooseSubprotocol "test") ws
    path "/websocketWithError" >=> handShake wsWithErrorHandling
    GET >=> choose [ path "/" >=> file "WebClient.html"; browseHome ]
    NOT_FOUND "Found no handlers." ]


let main _ =
  startWebServer { defaultConfig with logger = Targets.create Verbose [||] } app
  0

main()
//
// The FIN byte:
//
// A single message can be sent separated by fragments. The FIN byte indicates the final fragment. Fragments
//
// As an example, this is valid code, and will send only one message to the client:
//
// do! webSocket.send Text firstPart false
// do! webSocket.send Continuation secondPart false
// do! webSocket.send Continuation thirdPart true
//
// More information on the WebSocket protocol can be found at: https://tools.ietf.org/html/rfc6455#page-34
//