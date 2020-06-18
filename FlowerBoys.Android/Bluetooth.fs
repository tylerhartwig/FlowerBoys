module FlowerBoys.Android.Bluetooth

open FlowerBoys.Bluetooth
open System.Diagnostics
open Android.Bluetooth
open Java.Util

type Listener() =
    inherit Java.Lang.Object()
    
//    let deviceName = "raspberrypi"
//    let mutable socket: BluetoothSocket = null
//    
//    interface IBluetoothProfileServiceListener with
//        member __.OnServiceConnected(profile, proxy) =
//            let devices = BluetoothAdapter.DefaultAdapter.BondedDevices
//            if devices = null then () else
//                match devices |> Seq.tryFind (fun device ->
//                    Debug.WriteLine <| sprintf "Device Name: %s" device.Name
//                    device.Name = deviceName) with
//                | Some device -> 
//                    socket <- device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"))
//                    socket.Connect()
//                | None -> ()
//            
//            
//        member __.OnServiceDisconnected(profile) =
//            ()
        
    interface IBluetooth with
        member __.CheckConnections() =
            let adapter = BluetoothAdapter.DefaultAdapter
            
            try 
                if adapter <> null then
                    Debug.WriteLine <| sprintf "Adapter Found"
                else
                    Debug.WriteLine <| sprintf "No Adapter Found"
                
                if adapter.IsEnabled then
                    Debug.WriteLine <| sprintf "Adapter is enabled"
                else
                    Debug.WriteLine <| sprintf "Adapter is not enabled"

                Debug.WriteLine <| sprintf "Discovering Devices"
                
                adapter.BondedDevices |> Seq.iter (fun d ->
                    Debug.WriteLine <| sprintf "Paired Device: %s" d.Name
                    if d.Name = "raspberrypi" then
                        let uuid = UUID.FromString ("00001101-0000-1000-8000-00805f9b34fb")
                        let socket = d.CreateInsecureRfcommSocketToServiceRecord(uuid)
                        
                        socket.Connect()
                        if socket.IsConnected then
                            Debug.WriteLine <| sprintf "Connected!"
                )
            with ex ->
                Debug.WriteLine <| sprintf "Failed to look at devices: %A" ex
                
            
            ()