module FlowerBoys.Pi.Bluetooth

open System.Collections.Generic
open System.Threading
open NDesk.DBus
open System.IO.Ports
open System.Text


let bus = Bus.System
let startBusMain () =
    printfn "Starting bus main"
    let thread = Thread(fun () ->
          while true do
              try
                  printfn "Iterating"
                  bus.Iterate()
              with ex ->
                  printfn "Iteration failed! %A" ex
              )
    thread.IsBackground <- true
    thread.Start()
    thread

[<Literal>]
let BluezServiceName = "org.bluez"

[<Literal>]
let DeviceInterfaceName = BluezServiceName + ".Device1"

[<Interface(DeviceInterfaceName)>]
type Device =
    abstract Connected: bool with get
    abstract Name: string with get


let getManagedObjects () =
    let o = bus.GetObject<ObjectManager>(BluezServiceName, ObjectPath.Root)
    o.GetManagedObjects()
    
let (|KeyValue|) (kvp: KeyValuePair<'a, 'b>) = kvp.Key, kvp.Value

let findInterfacePaths i =
    let objs = getManagedObjects()
    objs |> Seq.choose (fun (KeyValue (path, ifaces)) ->
        if ifaces.Keys.Contains(i) then Some path else None)
    
    
let getDevice path =
    bus.GetObject<Device>(DeviceInterfaceName, path)

let getAllDevices () =
    findInterfacePaths DeviceInterfaceName |> Seq.map getDevice
    
let getNamedDevices () =
    getAllDevices () |> Seq.choose (fun d ->
        try
            let _ = d.Name
            Some d
        with _ -> None)

let getConnectedDevices () =
    getNamedDevices () |> Seq.filter (fun d -> d.Connected)
    
let bluetoothManager onDeviceAdded onDeviceRemoved =
    printfn "Setting up bluetooth manager"
    try
        let o = bus.GetObject<ObjectManager>(BluezServiceName, ObjectPath("/"))
        
        let objects = o.GetManagedObjects()
        printfn "Listening for interfaces added"
        o.add_InterfacesAdded(fun path interfaces ->
            printfn "Interface added at path: %A" path
            if interfaces.ContainsKey DeviceInterfaceName then
                let device = getDevice path
                onDeviceAdded device
            )
        
        printfn "Listening for interfaces removed"
        o.add_IntefacesRemoved(fun path interfaces ->
            if interfaces |> Array.contains DeviceInterfaceName then
                onDeviceRemoved path
            )
        
        printfn "Finished setting up bluetooth manager"
    with ex ->
        printfn "Failed to setup bluetooth manager with exception: %A" ex
    
//
//let serialPort =
//    let port = new SerialPort(
//                                 portName = "FlowerPort",
//                                 ReadBufferSize = 10,
//                                 dataBits = 8,
//                                 baudRate = 115200,
//                                 Encoding = Encoding.UTF8,
//                                 parity = Parity.Space,
//                                 stopBits = StopBits.One,
//                                 Handshake = Handshake.None
//                             )
//    
//    port.Open()
//    
//    
//    port

