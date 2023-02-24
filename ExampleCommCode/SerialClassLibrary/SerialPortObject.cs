using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO.Ports;
using System.IO.Pipes;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using StreamingLib;

namespace SerialClassLibrary
{
    #region Public declarations
    public delegate void RxDataUpdateCallback(string s, byte[] b, int nUserData, int nUserData2);
    public delegate void RxErrorCallback(int nUserData);

    #endregion

    #region Public structs

    public enum LineEndChar
    {
        LineEnd,
        LineFeed,
        Slash,
        Prompt
    };

    public struct PollerObject
    {
        public string device;
        public string argument;
        public string value;
        public int pollType;
        public bool active;
        public bool dataDumpFormat;
        public string[] dataDumpLabels;
        public bool bUpdated;
        public string[] dataDumpValues;
        public int[] dataDumpScaleFactor;
        public bool bNoPollCommandParse;
    }

    public struct SerialMessageEntry
    {
        public string s;
        public int nTimeoutMsecs;
        public Form form;
        public RxDataUpdateCallback cbFunc;
        public byte[] b;
        public bool bIsPollCommand;
        public PollerObject pPollCmd;
        public int nUserData;
        public int nUserData2;
        public bool bStreaming;
        public bool bFixedBinary;
        public bool bStreamingBinary;
    }

    #endregion

    public class SerialPortObject
    {
        #region Private Members

        private RxErrorCallback cbErrorFunc;
        private Form cbErrorForm;
        private int cbErrorUserData;

        private ArrayList m_aRawCommandData;
        private Object lockObject;
        private SerialMessageEntry m_ActiveEntry;

        private bool m_bInError;
        private string m_sErrorString;
        private System.Timers.Timer m_ClosePortTimer;

        private NamedPipeServerStream m_dataMOSIStream;
        private NamedPipeServerStream m_dataMISOStream;
        private ManualResetEvent m_syncEvent;
        private Process m_serialProcess;
        private Semaphore m_lock;
        private bool m_bGoingDown;
        private bool m_bStarted;
        private string m_sPortName;
        private LineEndChar m_LineEndChar;
        private int m_nBaudRate;
        private int m_nBitCount;
        private int m_nMaxQueuedMessages = 0;
        private bool m_bHideProcess;
        
        private string m_sDataMOSIPipeName = "serial_dataMOSI";
        private string m_sDataMISOPipeName = "serial_dataMISO";
        private static ManualResetEvent m_appSyncEvent = new ManualResetEvent(true);

        #endregion

        #region Properties

        public int RemainingMessageCount
        {
            get { return m_aRawCommandData.Count; }
        }

        public int NumQueuedMessages
        {
            get
            {
                return m_aRawCommandData.Count;
            }
        }

        public int MaxQueuedMessages
        {
            get
            {
                return m_nMaxQueuedMessages;
            }
            set
            {
                m_nMaxQueuedMessages = value;
            }
        }

        public string ErrorString
        {
            get
            {
                return m_sErrorString;
            }
        }

        public bool HideProcess
        {
            get
            {
                return m_bHideProcess;
            }
            set
            {
                m_bHideProcess = value;
            }
        }

        public bool IsOpen
        {
            get
            {
                return (m_serialProcess != null && !m_serialProcess.HasExited);
            }
        }

        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        #endregion

        #region Constructor
        public SerialPortObject()
        {
            m_aRawCommandData = new ArrayList();
            lockObject = new Object();
            m_ActiveEntry = new SerialMessageEntry();
            m_bInError = false;
            m_bHideProcess = true;  // hide by default

            m_syncEvent = null;
            m_dataMOSIStream = null;
            m_dataMISOStream = null;
            m_serialProcess = null;
            m_bGoingDown = false;
            m_bStarted = false;
            m_LineEndChar = LineEndChar.LineFeed;

            m_ClosePortTimer = null;
        }


        public LineEndChar MessageEnd
        {
            set
            {
                m_LineEndChar = value;
            }
        }

        #endregion

        #region Port Open/Closing

        public static bool OpenSerialPort(ref SerialPortObject m_SerialPort, ref string m_sSelComString, Form pThis, SerialClassLibrary.RxErrorCallback SerialErrorCallback, bool bQuiet)
        {
            return OpenSerialPort(ref m_SerialPort, ref m_sSelComString, pThis, SerialErrorCallback, bQuiet, 115200);
        }

        public static bool OpenSerialPort(ref SerialPortObject m_SerialPort, ref string m_sSelComString, Form pThis, SerialClassLibrary.RxErrorCallback SerialErrorCallback, bool bQuiet, int nBaudrate)
        {
            try
            {
                if ((m_SerialPort == null) && (m_sSelComString != null))
                {
                    m_SerialPort = new SerialPortObject();
                    m_SerialPort.Open(m_sSelComString, pThis,
                        new SerialClassLibrary.RxErrorCallback(SerialErrorCallback), 0, nBaudrate);

                    if (!m_SerialPort.IsOpen) // New process method can fail a bit softer on opening...
                        m_SerialPort = null;
                }
            }
            catch
            {
                if (m_SerialPort != null)
                {
                    m_SerialPort.Close();
                    m_SerialPort = null;
                }
            }

            if (bQuiet) return m_SerialPort != null;

            if (m_SerialPort == null)
            {
                MessageBox.Show("Serial port not available.  Does another program have it open?");
                return false;
            }

            return true;
        }

        [Conditional("DEBUG")]
        private void ShowSerialConsole()
        {
            m_bHideProcess = false;
        }

        public bool Open(string sName, Form cbErrForm, RxErrorCallback cbErrFunc, int nUserData, int nBaudRate)
        {
            return Open(sName, cbErrForm, cbErrFunc, nUserData, nBaudRate, 8);
        }

        public bool Open(string sName, Form cbErrForm, RxErrorCallback cbErrFunc, int nUserData, int nBaudRate, int nBits)
        {
            cbErrorForm = cbErrForm;
            cbErrorFunc = cbErrFunc;
            cbErrorUserData = nUserData;

            if ((!TestAvailable(sName)))
                return false;

            try
            {
                m_syncEvent = new ManualResetEvent(false);
                m_lock = new Semaphore(1, 1);
                m_sPortName = sName;
                m_nBaudRate = nBaudRate;
                m_nBitCount = nBits;

                locateValidPipeName(0);

                Thread ServerThread = new Thread(this.ThreadStartServer);
                ServerThread.Start();

                m_syncEvent.WaitOne();
                m_syncEvent.Reset();

                m_serialProcess = new Process();
                int index = Application.ExecutablePath.LastIndexOf('\\');
                m_serialProcess.StartInfo.WorkingDirectory = Application.ExecutablePath.Substring(0, index);
                m_serialProcess.StartInfo.FileName = "SerialPortConsole";
                m_serialProcess.StartInfo.Arguments = "-dataMOSI-pipe=" + m_sDataMOSIPipeName;
                m_serialProcess.StartInfo.Arguments += " -dataMISO-pipe=" + m_sDataMISOPipeName;
                m_serialProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

                // Always show the console in debug mode
                ShowSerialConsole();

                if (m_bHideProcess)
                {
                    m_serialProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    m_serialProcess.StartInfo.UseShellExecute = false;
                    m_serialProcess.StartInfo.CreateNoWindow = true;
                }

                m_serialProcess.Start();
                m_bStarted = true;
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Error starting serial port: " + ex.ToString(), "Fatal Error", MessageBoxButtons.OK);
                return false;
            }

            return true;
        }

        public void Open(string sName, Form cbErrForm, RxErrorCallback cbErrFunc, int nUserData)
        {
            Open(sName, cbErrForm, cbErrFunc, nUserData, 9600);
        }

        static public void FillComboWithPorts(ToolStripComboBox cb, string sSel, bool bForceUpdate)
        {
            int nSelIdx = 0;
            string[] sNames = SerialPort.GetPortNames();

            if (bForceUpdate)
            {                
                cb.Items.Clear();
                cb.MaxDropDownItems = 24;
                
                for (int i = 0; i < sNames.Length; i++)
                {
                    cb.Items.Add(sNames[i]);
                    if (sNames[i] == sSel)
                        nSelIdx = i;
                }

                if (sNames.Length != 0)
                    cb.SelectedIndex = nSelIdx;
            }
            else
            {
                for (int i = 0; i < cb.Items.Count;)
                {
                    bool bFound = false;
                    string sTempItem = cb.Items[i].ToString();

                    if (sTempItem.IndexOf(' ') != -1)
                        sTempItem = sTempItem.Remove(sTempItem.IndexOf(' '));

                    foreach (string sName in sNames)
                    {
                        if (sTempItem.Equals(sName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            bFound = true;
                            break;
                        }
                    }

                    if (!bFound)
                        cb.Items.RemoveAt(i);
                    else
                        i++;
                }

                for (int i = 0; i < sNames.Length; i++)
                {
                    bool bFound = false;

                    for (int j = 0; j < cb.Items.Count; j++)
                    {
                        string sTempItem = cb.Items[j].ToString();

                        if (sTempItem.IndexOf(' ') != -1)
                            sTempItem = sTempItem.Remove(sTempItem.IndexOf(' '));

                        if (sNames[i].Equals(sTempItem, StringComparison.CurrentCultureIgnoreCase))
                        {
                            bFound = true;
                            break;
                        }
                    }

                    if (!bFound)
                        cb.Items.Add(sNames[i]);
                }
            }
        }

        static public void FillComboWithPorts(ToolStripComboBox cb, string sSel)
        {
            FillComboWithPorts(cb, sSel, true);
        }

        static public string[] GetPortList()
        {
            return SerialPort.GetPortNames();
        }

        static public bool TestAvailable(string sName)
        {
            try
            {
                SerialPort serialPort = new SerialPort();

                serialPort.PortName = sName;
                serialPort.BaudRate = 9600;
                serialPort.Parity = Parity.None;
                serialPort.DataBits = 8;
                serialPort.StopBits = StopBits.One;
                serialPort.Handshake = Handshake.None;
                serialPort.NewLine = "\r";

                // Set the read/write timeouts
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;

                serialPort.Open();
                serialPort.Close();

                return true;
            }
            catch
            {
            }
            return false;
        }

        public void Close()
        {
            if (m_bStarted && !m_bGoingDown)
            {
                m_lock.WaitOne();
                m_bGoingDown = true;
                m_syncEvent.Set();
                m_lock.Release();
            }

            // do something
            m_dataMISOStream.Close();
            m_dataMOSIStream.Close();
            m_syncEvent.Close();
            m_lock.Close();

            try
            {
                m_serialProcess.Kill();
                m_serialProcess.WaitForExit();
            }
            catch
            {
                // ignore if already down
            }
        }

        public void CloseAfterSend()
        {
            m_ClosePortTimer = new System.Timers.Timer();
            m_ClosePortTimer.AutoReset = false;
            m_ClosePortTimer.Interval = 5;
            m_ClosePortTimer.Elapsed += new ElapsedEventHandler(ClosePortTimer_Elapsed);

            m_ClosePortTimer.Start();
        }

        private void ClosePortTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer tTimer = (System.Timers.Timer)sender;
            tTimer.Stop();

            Close();
            tTimer.Dispose();
            tTimer = null;
        }

        #endregion

        #region Message Sending
        public bool SendMessage(string sMsg, int nTimeout, Form form, RxDataUpdateCallback cbFunc, int nUserData, int nUserData2)
        {
            return SendMessage(sMsg, nTimeout, form, cbFunc, nUserData, nUserData2, false);
        }

        public bool SendMessage(string sMsg, int nTimeout, Form form, RxDataUpdateCallback cbFunc, int nUserData, int nUserData2, bool bPollingMsg)
        {
            SerialMessageEntry ce = new SerialMessageEntry();
            ce.nTimeoutMsecs = nTimeout;
            ce.s = sMsg;
            ce.cbFunc = cbFunc;
            ce.form = form;
            ce.b = null;
            ce.bIsPollCommand = bPollingMsg;
            ce.nUserData = nUserData;
            ce.nUserData2 = nUserData2;
            ce.bStreaming = false;
            ce.bFixedBinary = false;
            ce.bStreamingBinary = false;

            return !ProcessSendMessage(ce);
        }

        public bool SendMessage(byte[] bMsg, int nTimeout, Form form, RxDataUpdateCallback cbFunc, int nUserData, int nUserData2)
        {
            SerialMessageEntry ce = new SerialMessageEntry();
            ce.nTimeoutMsecs = nTimeout;
            ce.s = null;
            ce.cbFunc = cbFunc;
            ce.form = form;
            ce.b = bMsg;
            ce.bIsPollCommand = false;
            ce.nUserData = nUserData;
            ce.nUserData2 = nUserData2;
            ce.bStreaming = false;
            ce.bFixedBinary = false;
            ce.bStreamingBinary = false;

            return !ProcessSendMessage(ce);
        }

        public bool SendFixedMessage(byte[] bMsg, int nTimeout, Form form, RxDataUpdateCallback cbFunc, int nUserData, int nUserData2)
        {
            SerialMessageEntry ce = new SerialMessageEntry();
            ce.nTimeoutMsecs = nTimeout;
            ce.s = null;
            ce.cbFunc = cbFunc;
            ce.form = form;
            ce.b = bMsg;
            ce.bIsPollCommand = false;
            ce.nUserData = nUserData;
            ce.nUserData2 = nUserData2;
            ce.bStreaming = false;
            ce.bFixedBinary = true;
            ce.bStreamingBinary = false;

            return !ProcessSendMessage(ce);
        }

        public bool SendExtractMessage(int nNumToExtract, int nTimeout, Form form, RxDataUpdateCallback cbFunc, int nUserData, int nUserData2)
        {
            SerialMessageEntry ce = new SerialMessageEntry();
            ce.nTimeoutMsecs = nTimeout;
            ce.s = null;
            ce.cbFunc = cbFunc;
            ce.form = form;
            ce.b = new byte[nNumToExtract];
            ce.bIsPollCommand = false;
            ce.nUserData = nUserData;
            ce.nUserData2 = nUserData2;
            ce.bStreaming = false;
            ce.bFixedBinary = false;
            ce.bStreamingBinary = true;

            return !ProcessSendMessage(ce);
        }

        public bool SendStreamingMessage(string sMsg, int nTimeout, Form form, RxDataUpdateCallback cbFunc, int nUserData, int nUserData2)
        {
            SerialMessageEntry ce = new SerialMessageEntry();
            ce.nTimeoutMsecs = nTimeout;
            ce.s = sMsg;
            ce.cbFunc = cbFunc;
            ce.form = form;
            ce.b = null;
            ce.bIsPollCommand = false;
            ce.nUserData = nUserData;
            ce.nUserData2 = nUserData2;
            ce.bStreaming = true;
            ce.bFixedBinary = false;
            ce.bStreamingBinary = false;

            return !ProcessSendMessage(ce);
        }

        public bool SendStreamingMessage(byte[] bMsg, int nTimeout, Form form, RxDataUpdateCallback cbFunc, int nUserData, int nUserData2)
        {
            SerialMessageEntry ce = new SerialMessageEntry();
            ce.nTimeoutMsecs = nTimeout;
            ce.s = null;
            ce.cbFunc = cbFunc;
            ce.form = form;
            ce.b = bMsg;
            ce.bIsPollCommand = false;
            ce.nUserData = nUserData;
            ce.nUserData2 = nUserData2;
            ce.bStreaming = true;
            ce.bFixedBinary = false;
            ce.bStreamingBinary = false;

            return !ProcessSendMessage(ce);
        }

        public bool SendPollerString(string msg, int nTimeout, Form form, RxDataUpdateCallback cbFunc, int nUserData, int nUserData2)
        {
            {
                SerialMessageEntry ce = new SerialMessageEntry();
                ce.nTimeoutMsecs = nTimeout;
                ce.s = msg;
                ce.cbFunc = cbFunc;
                ce.form = form;
                ce.b = null;
                ce.bIsPollCommand = true;
                ce.pPollCmd.argument = null;
                ce.nUserData = nUserData;
                ce.nUserData2 = nUserData2;
                ce.bStreaming = false;
                ce.bFixedBinary = false;
                ce.bStreamingBinary = false;

                return !ProcessSendMessage(ce);
            }
        }

        private bool ProcessSendMessage(SerialMessageEntry ce)
        {
            m_lock.WaitOne();
            if ((m_nMaxQueuedMessages == 0) || (NumQueuedMessages < MaxQueuedMessages))
            {
                m_aRawCommandData.Add(ce);
                m_syncEvent.Set();
            }
            m_lock.Release();

            return m_bInError;
        }

        #endregion

        #region Read the Port (New process model)

        public void ThreadSerialDataHandler()
        {
            try
            {
                string sComParams = m_sPortName + "," + m_nBaudRate.ToString() + ",";
                switch (m_LineEndChar)
                {
                    case LineEndChar.LineEnd:
                        sComParams += "LN,";
                        break;
                    case LineEndChar.Slash:
                        sComParams += "SL,";
                        break;
                    case LineEndChar.Prompt:
                        sComParams += "PR,";
                        break;
                    default:
                        sComParams += "LF,";
                        break;
                }
                sComParams += m_nBitCount.ToString();

                SerialPipeMessage openmsg = new SerialPipeMessage(SerialPipeMessage.Type.OpenPort, sComParams);
                byte[] openb = openmsg.Marshall();
                m_dataMOSIStream.Write(openb, 0, openb.Length);

                SerialPipeMessage replyopenmsg = SerialPipeMessage.WaitForMsg(m_dataMISOStream);
                if (replyopenmsg.MsgType != SerialPipeMessage.Type.OpenError)
                {
                    do
                    {
                        int nCount = 0;
                        while (nCount == 0)
                        {
                            m_lock.WaitOne();
                            nCount = m_aRawCommandData.Count;
                            if (nCount == 0)
                                m_syncEvent.Reset();
                            m_lock.Release();

                            if (m_bGoingDown)
                                return;

                            if (nCount == 0)
                                m_syncEvent.WaitOne();
                        }

                        m_lock.WaitOne();
                        Array arryTemp = m_aRawCommandData.ToArray();

                        // Send the first item in the array
                        SerialMessageEntry ce = (SerialMessageEntry)arryTemp.GetValue(0);
                        m_aRawCommandData.RemoveAt(0);
                        m_lock.Release();

                        m_ActiveEntry = ce;

                        SerialPipeMessage msg = null;
                        if (ce.b != null)
                        {
                            if (ce.bStreamingBinary)
                            {
                                msg = new SerialPipeMessage(SerialPipeMessage.Type.GetStreamingBinaryData, ce.b, ce.b.Length, ce.nTimeoutMsecs);
                            }
                            else if (ce.bFixedBinary)
                            {
                                msg = new SerialPipeMessage(SerialPipeMessage.Type.FixedBinaryData, ce.b, ce.b.Length, ce.nTimeoutMsecs);
                            }
                            else
                                msg = new SerialPipeMessage(ce.b, ce.b.Length, ce.nTimeoutMsecs);
                        }
                        else
                        {
                            msg = new SerialPipeMessage(ce.s, m_ActiveEntry.bIsPollCommand, ce.nTimeoutMsecs);
                        }
                        byte[] b = msg.Marshall();
                        m_dataMOSIStream.Write(b, 0, b.Length);
                        m_dataMOSIStream.Flush();

                        string s = null;
                        byte[] replyb = null;

                        SerialPipeMessage pipemsg;

                        do
                        {
                            s = null;
                            pipemsg = SerialPipeMessage.WaitForMsg(m_dataMISOStream);
                            if (pipemsg.MsgType == SerialPipeMessage.Type.ReplyString)
                            {
                                s = pipemsg.StringData;
                                char[] cTrim = { '\r', '\n' };
                                s = s.TrimStart(cTrim);

                                if ((m_ActiveEntry.bIsPollCommand) && 
                                    (m_ActiveEntry.pPollCmd.argument != null) &&
                                    !(m_ActiveEntry.pPollCmd.bNoPollCommandParse))
                                {
                                    string[] sTemp = s.Split(new char[] { ':' });
                                    if (sTemp.Length > 1)
                                    {
                                        string[] sTemp2 = sTemp[0].Split(new char[] { ' ' });

                                        if ((sTemp2.Length > 0) && (sTemp2[sTemp2.Length - 1] == m_ActiveEntry.pPollCmd.argument))
                                        {
                                            s = sTemp[1];
                                            s = s.TrimEnd(cTrim);
                                        }
                                        else
                                            s = "";
                                    }
                                    else
                                        s = "";
                                }
                            }
                            else if ((pipemsg.MsgType == SerialPipeMessage.Type.BinaryData) ||
                                     (pipemsg.MsgType == SerialPipeMessage.Type.FixedBinaryData) ||
                                     (pipemsg.MsgType == SerialPipeMessage.Type.GetStreamingBinaryData))
                            {
                                replyb = pipemsg.m_b;
                            }
                            else if (pipemsg.MsgType == SerialPipeMessage.Type.ReplyTimeout)
                            {
                                s = null;
                            }
                            else
                            {
                                break;
                            }

                            try
                            {
                                if ((m_ActiveEntry.cbFunc != null))
                                    m_ActiveEntry.form.Invoke(m_ActiveEntry.cbFunc, new object[] { s, replyb, m_ActiveEntry.nUserData, m_ActiveEntry.nUserData2 });
                            }
                            catch// (SystemException ex)
                            {
                                // ignore so thread keeps on truckin
                            }
                        } while ((m_ActiveEntry.bStreaming) && (pipemsg.MsgType == SerialPipeMessage.Type.ReplyString));
                    } while (true);
                }
            }
            catch
            {

            }
            m_dataMISOStream.Close();
        }

        public void ThreadStartServer()
        {
            // Create a name pipe
            m_dataMISOStream = new NamedPipeServerStream(m_sDataMISOPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.None);
            m_dataMOSIStream = new NamedPipeServerStream(m_sDataMOSIPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.None);
            m_syncEvent.Set();

            // Wait for a connection
            m_dataMISOStream.WaitForConnection();
            m_dataMOSIStream.WaitForConnection();
            
            //m_appSyncEvent.Set();

            // start data handling thread
            Thread DataThread = new Thread(this.ThreadSerialDataHandler);
            DataThread.Start();

            //m_pipeStream.ReadTimeout = 1000;

            try
            {
                do
                {
                    m_serialProcess.WaitForExit();

                    m_lock.WaitOne();
                    m_bGoingDown = true;
                    m_syncEvent.Set();
                    m_lock.Release();

                    m_sErrorString = "Client is disconnecting  .. ";
                    cbErrorForm.Invoke(cbErrorFunc, new object[] { cbErrorUserData });

                } while (false);
            }

            catch
            {
            }
        }

        private void locateValidPipeName(int nValue)
        {
            string sCtrlPipe = m_sDataMISOPipeName + nValue.ToString();
            string sDataOutPipe = m_sDataMOSIPipeName + nValue.ToString();

            try
            {
                m_dataMISOStream = new NamedPipeServerStream(sCtrlPipe, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.None);
                m_dataMOSIStream = new NamedPipeServerStream(sDataOutPipe, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.None);
            }
            catch
            {
                if (m_dataMISOStream != null)
                    m_dataMISOStream.Close();
                if (m_dataMOSIStream != null)
                    m_dataMOSIStream.Close();

                locateValidPipeName(nValue + 1);
                return;
            }

            m_dataMISOStream.Close();
            m_dataMOSIStream.Close();

            m_sDataMISOPipeName = sCtrlPipe;
            m_sDataMOSIPipeName = sDataOutPipe;
        }

        #endregion

    }
}
