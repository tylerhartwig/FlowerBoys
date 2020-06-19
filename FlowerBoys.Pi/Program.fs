// Learn more about F# at http://fsharp.org

open System.IO.Ports
open FlowerBoys.Pi.Bluetooth
open NDesk.DBus
open System.Threading
open FlowerBoys.Pi



type SerialMsg = | SerialMsg of string
type BluetoothMsg =
    | DeviceAdded of Device

[<EntryPoint>]
let main argv =
    let busThread = Bluetooth.startBusMain()
    
    let startSerialActor (serialPort: SerialPort) =
        let mainLoop (inbox: MailboxProcessor<SerialMsg>) =
            let rec loop () =
                async {
                    match! inbox.Receive() with
                    | SerialMsg msg ->
                        printfn "Message Received: %s" msg
                        return! loop()
                }
            
            loop ()
        
        let mainActor = MailboxProcessor.Start(mainLoop)
        
        serialPort.DataReceived.Add(fun _ ->
            let next = serialPort.ReadExisting()
            mainActor.Post (SerialMsg next))
    
        mainActor

        
    let startBluetoothActor () =
        printfn "Starting bluetooth actor"
        let mainLoop (inbox: MailboxProcessor<BluetoothMsg>) =
            let rec loop () =
                async {
                    match! inbox.Receive() with
                    | DeviceAdded device ->
                        printfn "Bluetooth Actor processing device"
                        try
                            printfn "Device Added with name: %s" device.Name
                        with _ ->
                            printfn "Device added, could not get name"
                            
                        return! loop()
                }
            loop ()
        MailboxProcessor.Start (mainLoop)
    
    let bluetoothActor = startBluetoothActor()
    Bluetooth.bluetoothManager (fun d ->
        printfn "Bluetooth manager found device"
        bluetoothActor.Post (DeviceAdded d)) (fun _ -> ())
    
    printfn "Existing named devices: "
    getNamedDevices() |> Seq.iter (fun d -> printfn "Device: %s" d.Name)
    

    Thread.Sleep(Timeout.Infinite)
    0 // return an integer exit code
