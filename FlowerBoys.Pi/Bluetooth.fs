module FlowerBoys.Pi.Bluetooth

open System.Collections.Generic
open System.Threading
open NDesk.DBus
open System.IO.Ports
open System.Text


let bus = Bus.System
let startBusMain () =
    let thread = Thread(fun () -> bus.Iterate())
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
    let o = bus.GetObject<ObjectManager>(BluezServiceName, ObjectPath("/"))
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
    let o = bus.GetObject<ObjectManager>(BluezServiceName, ObjectPath("/"))
    
    o.add_InterfacesAdded(fun path interfaces ->
        if interfaces.ContainsKey DeviceInterfaceName then
            let device = getDevice path
            onDeviceAdded device
        )
    
    o.add_IntefacesRemoved(fun path interfaces ->
        if interfaces |> Array.contains DeviceInterfaceName then
            onDeviceRemoved path
        )
    
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

