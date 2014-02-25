using System;
using System.Collections.Generic;
using System.Linq;  
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports; 
using System.Threading;  

using SalSerialPort01.ServiceReference1; // Needed for Service1Client _svc in Azure;


// Description:  A basic program that reads XBee messages from the serial port and sends information to an Azure worker role.
// The message is only sent to Azure if the Xbee message is of a particular type and contains a specific data point.  The data point indicates that motion was detected by a motion 
//sensor attached to the XBee controller.
// Code has minimal error catching code.

namespace XBeeMotionSensor2Azure
{
    public class PortChat
    {
        static bool ConsoleContinue;
        static SerialPort SPort;
        static Service1Client MyAzureService;

        // This is the name of the file where each individual XBee message is stored.  Change to your desired file location. Can also change at console.
        static string MsgFileName = "F:\\SalTestFiles\\Zigbee03.txt"; 
       
        public static void Main()
        {             
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // Allow the user to set the appropriate filename and path to store the XBee messages read from the serial port.
            MsgFileName =  SetMsgFilename(MsgFileName);

            // Create a new SerialPort object with default settings.
            SPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            SPort.PortName = SetPortName(SPort.PortName);
            SPort.BaudRate = SetPortBaudRate(SPort.BaudRate);
            SPort.Parity = SetPortParity(SPort.Parity);
            SPort.DataBits = SetPortDataBits(SPort.DataBits);
            SPort.StopBits = SetPortStopBits(SPort.StopBits);
            SPort.Handshake = SetPortHandshake(SPort.Handshake);

            // Set the read/write timeouts
            SPort.ReadTimeout = 500;
            SPort.WriteTimeout = 500;

            SPort.Open();
            ConsoleContinue = true;
            readThread.Start();

            // Allow user to gracefully exit console by typing 'quit'
            Console.WriteLine("Type QUIT to exit");

            while (ConsoleContinue)
            {
                message = Console.ReadLine();
                if (stringComparer.Equals("quit", message))
                {
                    ConsoleContinue = false;
                }
            }

            readThread.Join();
            SPort.Close();
        }


        // Read serial port and decode XBee messages
        public static void Read()
        {
            byte MsgByte;

            byte[] MsgPacketBuffer = new byte[65536];  // Reserve an array to store the max number of bytes possible in a frame
            byte MsgPacketBufferOffset = 0;

            byte MsgFrameLengthMSB;
            byte MsgFrameLengthLSB;
            int MsgFrameLength=0;

            string MsgDecPacketString  = null;
            string MsgHexPacketString = null;

            long BitResult;         
            string ReadingTimeStamp;  // Timestamp when sensor reading was recorded

            string SourceDeviceAddress = null; // Address of Xbee that sent the message
            byte Motion;  


            while (ConsoleContinue)  // Loop until user wants to stop the program.
            {
                try
                {
                    MsgByte = (byte) SPort.ReadByte(); // Read a byte
                
                    // Look for the start of the frame                   
                    
                    //--- BEGIN reading and decoding packet Frame Type 0x7E - 
                    //          Sample Rx Indicator (XBee ref manual page 114).  
                    //          Packet Type 0x7E contains the digital samples. In our case the motion sensor reading.
                    if (MsgByte == 126)  // 0x7E    This is the begining of a packet
                    {
                        // Found frame type 0x7E, keep reading serial port until the full frame is decoded    
                        MsgPacketBufferOffset = 0; // Reset buffer offset
                        MsgPacketBuffer[MsgPacketBufferOffset++] = 126;  // Save frame type to buffer
                                                
                        // Read the frame (packet) length
                        MsgFrameLengthMSB = (byte)SPort.ReadByte(); // Read frame length MSB                        
                        MsgFrameLengthLSB= (byte)SPort.ReadByte(); // Read frame length LSB 
                        
                        MsgPacketBuffer[MsgPacketBufferOffset++] = MsgFrameLengthMSB; // Store frame length MSB                         
                        MsgPacketBuffer[MsgPacketBufferOffset++] = MsgFrameLengthLSB; // Store frame length LSB 

                        // Calculate frame length.  Don't forget 1 last byte for checksum not included in the frame length.
                        MsgFrameLength = (int) ((MsgFrameLengthMSB << 16) + MsgFrameLengthLSB);

                        // Read and store the entire frame based on the length read previously
                         for (int i = 0; i < MsgFrameLength; i++)
                         {
                             // Read each byte in the frame and store in the buffer
                             MsgByte = (byte) SPort.ReadByte(); // Read a byte
                             MsgPacketBuffer[MsgPacketBufferOffset++] = MsgByte;
                         }

                        // Read the last byte in the frame, the checksum
                        MsgByte = (byte)SPort.ReadByte(); // Read checksum from port 
                        MsgPacketBuffer[MsgPacketBufferOffset++] = MsgByte; // Save the checksum in the buffer
                        
                        //--- END reading and decoding packet Frame Type 0x7E

                        // --- BEGIN storing data

                        // ----- TO DO ------
                            // Save the entire RAW frame stored in the buffer to a more permanent location (includes the frame type (1 byte), length (2 bytes) and checksum (1 byte))
                            // for (int j = 0; j < MsgFrameLength ; j++)
                        

                        // Convert the data packet to a string and store the string in a text file
                        // Store in both Decimal and Hexadecimal notation
                        MsgDecPacketString = null; // Clear the string first.
                        MsgHexPacketString = null; // Clear the string first.

                        // For loop includes saving the last byte corresponding to the checksum
                        for (int j = 0; j < MsgPacketBufferOffset; j++) 
                         {
                             MsgDecPacketString += " " + MsgPacketBuffer[j];
                             MsgHexPacketString += " " + MsgPacketBuffer[j].ToString("X");
                         }

                         MsgDecPacketString += "\n";  // Newline to make it easier to read
                         MsgHexPacketString += "\n";  // Newline to make it easier to read

                         
                        // Save the packet to a text file.                       
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@MsgFileName, true))
                         {
                            // Write the messsages to file, on hex or deciaml or both
                            // file.Write(SalDecPacketString); 
                             file.Write(MsgHexPacketString);
                         }
                         Console.WriteLine(MsgDecPacketString);  // Display the raw message to the console

                        // --- END storing data

                        // --- BEGIN Notify Cloud of any detected motion

                        // ---- For the motion sensor particular application, we want to notify our Cloud only when motion is detected by the sensor. No
                        // point in sending data if there was no motion.  The XBee transmitters are sending data on periodic basis, but we don't want to waste 
                        // resources sending data to Cloud when no motion is detected.
                        // Send notification to Cloud only if the packet contained a 1 for the digital sample. 1 indicates a high signal on the digital XBee pin.
                        // The Digital Sample is at offset 20.  The Digital Channel Mask is at offset 17.
                         
                        BitResult = MsgPacketBuffer[20] & MsgPacketBuffer[17];
                        if (BitResult != 0) 
                        {  // Packet contains motion
                           // Send notification to the cloud

                            Console.WriteLine("Something is moving!!  {0} AND {1} Result: {2}", MsgPacketBuffer[20], MsgPacketBuffer[17], BitResult);

                            // Source device.  
                            // 64-bit source address is stored at offsets 4 MSB - 11 LSB
                            SourceDeviceAddress = null;
                            for (int i = 1; i < 8; i++)
                                SourceDeviceAddress += " " + MsgPacketBuffer[i].ToString("X");

                            // Timestamp - Record thed time that the reading was taken.
                            ReadingTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                   
                            Motion = 1;  // Indicate that motion was detected

                            // Notify the Cloud of the motion reading
                            NotifyCloud(MsgHexPacketString, ReadingTimeStamp, SourceDeviceAddress, Motion);
                         }
                        // --- END Notify Cloud of any detected motion
                    } // Frame type 0x7E
                }  //try
                catch (TimeoutException) { }
            }
        }


        public static void NotifyCloud(string PacketHex, string  TimeS, string SourceAddress, int Motion)
        {
            // Notify the Cloud that there was motion detected by one of the sensors
            // Data will be stored in the Azure SQL database

            // Connect to the Azure worker role to save the data in the SQL table
            MyAzureService = new Service1Client();
            MyAzureService.AddMotionReading(TimeS, TimeS, SourceAddress, Motion);
            MyAzureService.Close();

        }

        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("COM port({0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "")
            {
                portName = defaultPortName;
            }
            return portName;
        }

        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate({0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Parity({0}):", defaultPortParity.ToString());
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity);
        }

        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Data Bits({0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits);
        }

        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available Stop Bits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Stop Bits({0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits);
        }

        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Handshake({0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake);
        }


        public static string SetMsgFilename(string DefaultFileName)
        {
            string MsgFileName = null;

            Console.Write("File name to store message data ({0} ): ", DefaultFileName);
            MsgFileName = Console.ReadLine();
            if (MsgFileName == "")
              MsgFileName = DefaultFileName;

            return(MsgFileName);           
        }
    }

}

