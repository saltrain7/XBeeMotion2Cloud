

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;
using System.Threading;


// Description:  A basic program that reads XBee messages from the serial port

namespace ConsoleReadSerialPort
{
    public class PortChat
    {
        static bool ConsoleContinue;
        static SerialPort SPort;

        // This is the name of the file where each individual XBee message is stored.  Change to your desired file location. Can also change at console.
        static string MsgFileName = "F:\\SalTestFiles\\Zigbee03.txt";

        public static void Main()
        {
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // Allow the user to set the appropriate filename and path to store the XBee messages read from the serial port.
            MsgFileName = SetMsgFilename(MsgFileName);

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


        // Read serial port
        public static void Read()
        {
            byte MsgByte;
            string MsgByteString = null;

            byte[] MsgPacketBuffer = new byte[65536];  // Reserve an array to store the max number of bytes possible in a frame


            while (ConsoleContinue)  // Loop until user wants to stop the program.
            {
                try
                {
                    MsgByte = (byte)SPort.ReadByte(); // Read a byte                   

                    MsgByteString += MsgByte.ToString("X");


                    // Save the packet to a text file.                       
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@MsgFileName, true))
                    {
                        // Write the messsages to file, on hex or deciaml or both
                        // file.Write(SalDecPacketString); 
                        file.Write(MsgByteString);
                    }
                    Console.WriteLine(MsgByteString);  // Display the raw message to the console
                
                }  //try
                catch (TimeoutException) { }
              
            }
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

            return (MsgFileName);
        }
    }

}

