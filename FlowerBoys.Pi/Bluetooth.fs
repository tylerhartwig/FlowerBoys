module FlowerBoys.Pi.Bluetooth

open System.IO.Ports
open System.Text

let serialPort =
    let port = new SerialPort(
                                 portName = "FlowerPort",
                                 ReadBufferSize = 10,
                                 dataBits = 8,
                                 baudRate = 115200,
                                 Encoding = Encoding.UTF8,
                                 parity = Parity.Space,
                                 stopBits = StopBits.One,
                                 Handshake = Handshake.None
                             )
    
    port.Open()
    
    
    port

