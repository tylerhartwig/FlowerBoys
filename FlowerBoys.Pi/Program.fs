// Learn more about F# at http://fsharp.org

open System
open System.Threading
open FlowerBoys.Pi

type Msg = | DataReceived of string

[<EntryPoint>]
let main argv =
    let mainLoop (inbox: MailboxProcessor<Msg>) =
        let rec loop () =
            async {
                match! inbox.Receive() with
                | DataReceived msg ->
                    printfn "Message Received: %s" msg
                    return! loop()
            }
            
        loop ()
        
    
    let mainActor = MailboxProcessor.Start(mainLoop)
    
    let serialPort = Bluetooth.serialPort

    serialPort.DataReceived.Add(fun _ ->
        let next = serialPort.ReadExisting()
        mainActor.Post (DataReceived next))
    
    
    Thread.Sleep(Timeout.Infinite)
    0 // return an integer exit code
