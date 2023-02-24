using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.IO.Pipes;

using CommandLine.Utility;
using StreamingLib;

namespace SerialPortConsole
{
    class Program
    {
        //private SerialPort m_serialPort;
        static string gs_szDataMOSIPipeName = "serial_dataMOSI";
        static string gs_szDataMISOPipeName = "serial_dataMISO";

        static void Main(string[] args)
        {
            setupParams(args);

            Console.WriteLine("[Client new] Started ...");

            SerialPort m_serialPort;
            m_serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            m_serialPort.Parity = Parity.None;
            m_serialPort.StopBits = StopBits.One;
            m_serialPort.Handshake = Handshake.None;
            //m_serialPort.RtsEnable = true;
            //m_serialPort.DtrEnable = true;

            int cLineEndChar;
            bool bWriteEndLineChar = true;

            // Set the read/write timeouts
            m_serialPort.ReadTimeout = 1000;
            m_serialPort.WriteTimeout = 500;

            NamedPipeClientStream dataMISOStream = new NamedPipeClientStream(gs_szDataMISOPipeName);

            try
            {
                using (NamedPipeClientStream dataMOSIStream = new NamedPipeClientStream(gs_szDataMOSIPipeName))
                {
                    byte[] b = new byte[1024];

                    // The connect function will indefinately wait for the pipe to become available
                    // If that is not acceptable specify a maximum waiting time (in ms)
                    dataMISOStream.Connect();
                    dataMISOStream.ReadMode = PipeTransmissionMode.Message;
                    Console.WriteLine("[Client] MISO pipe connection established on " + gs_szDataMISOPipeName);

                    dataMOSIStream.Connect();
                    dataMOSIStream.ReadMode = PipeTransmissionMode.Message;
                    Console.WriteLine("[Client] MOSI pipe connection established on " + gs_szDataMOSIPipeName);

                    do
                    {
                        Console.WriteLine("[Client] Waiting for message...");
                        SerialPipeMessage openMsg = SerialPipeMessage.WaitForMsg(dataMOSIStream);
                        if (openMsg.MsgType == SerialPipeMessage.Type.OpenPort)
                        {
                            char[] cSep = { ',' };
                            string[] sParams = openMsg.StringData.Split(cSep);

                            m_serialPort.PortName = sParams[0];
                            m_serialPort.BaudRate = Convert.ToInt32(sParams[1]);
                            m_serialPort.DataBits = Convert.ToInt32(sParams[3]);
                            if (sParams[2] == "PR")
                            {
                                m_serialPort.NewLine = "\r";
                                cLineEndChar = (int)'>';
                            }
                            else if (sParams[2] == "SL")
                            {
                                m_serialPort.NewLine = "\n";
                                cLineEndChar = (int)'/';
                                bWriteEndLineChar = false;
                            }
                            else if (sParams[2] == "LN")
                            {
                                m_serialPort.NewLine = "\n";
                                cLineEndChar = (int)'\n';
                            }
                            else
                            {
                                m_serialPort.NewLine = "\r";
                                cLineEndChar = (int)'\r';
                            }

                            try { m_serialPort.Open(); }
                            catch (Exception ex)
                            {
                                Console.WriteLine("[Client] Failed to open port {0} ... {1}", m_serialPort.PortName, ex.Message);
                                SerialPipeMessage.SendReply(dataMISOStream, SerialPipeMessage.Type.OpenError, ex.Message);
                                //continue;
                                Environment.Exit(0);
                            }
                            Console.WriteLine("[Client] Port " + openMsg.StringData + " open (baud = " + m_serialPort.BaudRate.ToString() + " bits = " + m_serialPort.DataBits.ToString() + ")...");
                            if (m_serialPort.NewLine == "\n")
                                Console.WriteLine("[Client] Using Line End...");
                            else
                                Console.WriteLine("[Client] Using Line Feed...");
                            SerialPipeMessage.SendReply(dataMISOStream, SerialPipeMessage.Type.OpenSuccess, "Ok");
                            break;
                        }
                        else
                        {
                            dataMOSIStream.Close();
                            dataMOSIStream.Dispose();
                            //throw new SystemException("[Client] Bad message, expected open ... " + openMsg.MsgType.ToString());
                        }
                    } while (true);

                    SerialPipeMessage msg = null;
                    do
                    {
                        Console.WriteLine("Waiting for Msg .. ");
                        msg = SerialPipeMessage.WaitForMsg(dataMOSIStream);
                        if (msg == null) break;
                        string sTemp = "";
                        int nReadTime = m_serialPort.ReadTimeout;
                        try
                        {
                            // Make it timeout real fast so if nothings in there we don't get hung up
                            m_serialPort.ReadTimeout = 5;

                            // Read the buffer first to clear out anyting that's in there, then send the new command
                            sTemp = m_serialPort.ReadExisting();
                        }
                        catch
                        {
                        }
                        // Restore the timeout to the previous value
                        if (msg.m_nTimeOut != 0)
                            m_serialPort.ReadTimeout = msg.m_nTimeOut;
                        else
                            m_serialPort.ReadTimeout = 1000;

                        Console.WriteLine("Got Message.." + msg.MsgType.ToString() + " Timeout " + msg.m_nTimeOut.ToString());
                        if ((msg.MsgType != SerialPipeMessage.Type.BinaryData) &&
                            (msg.MsgType != SerialPipeMessage.Type.FixedBinaryData) &&
                            (msg.MsgType != SerialPipeMessage.Type.GetStreamingBinaryData))
                        {
                            if ((sTemp != "") && (sTemp != null))
                                Console.WriteLine("Extra Data .. " + sTemp);

                            Console.WriteLine("Writing serial string .. " + msg.StringData);
#if NOTNOW
                            char[] cs = msg.StringData.ToCharArray();
                            byte[] bs = new byte[cs.Length + 2];
                            for (int j = 0; j < cs.Length; j++)
                            {
                                bs[j] = (byte)cs[j];
                            }
                            bs[cs.Length] = (byte)'\r';
                            m_serialPort.Write(bs, 0, cs.Length + 1);
#endif
                            if (bWriteEndLineChar)
                                m_serialPort.WriteLine(msg.StringData);
                            else
                                m_serialPort.Write(msg.StringData);

                            //m_serialPort.Write("mode cc");
                            //Environment.Exit(0);

                            bool bFastReply = (msg.MsgType == SerialPipeMessage.Type.PollingString);
                            string s = "";
                            do
                            {
                                try
                                {
                                    Console.WriteLine("Waiting serial read .. ");
                                    s = null;
                                    do
                                    {
                                        int c = m_serialPort.ReadChar();
                                        s += (char)c;
                                        if (c == cLineEndChar)
                                            break;
                                    } while (true);
#if ORIG
                                    s = m_serialPort.ReadLine();

                                    char[] cTrim = { '\r', '\n' };
                                    s = s.TrimStart(cTrim);
#endif
                                    Console.WriteLine("Pipe Send Reply.. " + s);
                                    SerialPipeMessage.SendReply(dataMISOStream, SerialPipeMessage.Type.ReplyString, s);
                                }
                                catch (TimeoutException ex)
                                {
                                    Console.WriteLine("Serial timeout.. with " + s);
                                    s = null;
                                }
                            } while ((!bFastReply) && (s != null) && (s != ""));

                            if ((s == null) || (s == ""))
                            {
                                Console.WriteLine("Pipe Send timeout.. ");
                                SerialPipeMessage.SendReply(dataMISOStream, SerialPipeMessage.Type.ReplyTimeout, "");
                            }
                        }
                        else
                        {
                            // binary data
                            Console.WriteLine("Write binary to serial.. sending " + msg.m_b.Length.ToString());

                            if (msg.MsgType != SerialPipeMessage.Type.GetStreamingBinaryData)
                                m_serialPort.Write(msg.m_b, 0, msg.m_b.Length);

                            byte[] inb = null;
                            int inCnt = 0;
                            try
                            {
                                inb = new byte[1024];
                                int inCh;
                                do
                                {
                                    inCh = m_serialPort.ReadByte();
                                    inb[inCnt] = (byte)inCh;
                                    inCnt++;

                                    if (((msg.MsgType == SerialPipeMessage.Type.FixedBinaryData) ||
                                         (msg.MsgType == SerialPipeMessage.Type.GetStreamingBinaryData)) &&
                                        (inCnt == msg.m_b.Length))
                                        break;

                                    if ((msg.MsgType == SerialPipeMessage.Type.BinaryData) &&
                                        (inCh == cLineEndChar))
                                        break;

                                } while (true);
                                Console.WriteLine("Binary reply.. got " + inCnt.ToString());
                            }
                            catch (TimeoutException ex)
                            {
                                Console.WriteLine("Binary timeout.. got " + inCnt.ToString());
                                inb = null;
                            }

                            if (inb == null)
                            {
                                Console.WriteLine("Pipe Send timeout.. ");
                                SerialPipeMessage.SendReply(dataMISOStream, SerialPipeMessage.Type.ReplyTimeout, "");
                            }
                            else
                            {
                                Console.WriteLine("Pipe Sending Binary " + inb[0].ToString() + inb[1].ToString());
                                SerialPipeMessage outmsg = new SerialPipeMessage(msg.MsgType, inb, inCnt, 1000);
                                byte[] outb = outmsg.Marshall();
                                dataMISOStream.Write(outb, 0, outb.Length);
                            }
                        }
                    } while (msg != null);
                    dataMOSIStream.Close();
                }
            }
            catch (SystemException ex)
            {
                                // doo nothing
                Console.WriteLine("[Client] fatal exception ... " + ex.Message);

                if (m_serialPort != null)
                {
                    try
                    {
                        if (m_serialPort.IsOpen)
                            m_serialPort.Close();
                        m_serialPort.Dispose();
                        m_serialPort = null;
                    }
                    catch { };
                }

                // last ditch effor to tell of our demise and cleanup.
                try 
                { 
                    SerialPipeMessage.SendReply(dataMISOStream, SerialPipeMessage.Type.ByeBye, ex.Message);
                    dataMISOStream.Flush();
                    dataMISOStream.WaitForPipeDrain();
                    dataMISOStream.Close();
                    dataMISOStream.Dispose();
                }
                catch { }
            }
            Environment.Exit(0);
        }

        static void setupParams(string[] args)
        {
            Arguments CommandLine = new Arguments(args);

            if (CommandLine["dataMOSI-pipe"] != null)
                gs_szDataMOSIPipeName = CommandLine["dataMOSI-pipe"];

            if (CommandLine["dataMISO-pipe"] != null)
                gs_szDataMISOPipeName = CommandLine["dataMISO-pipe"];
        }
    }
}
