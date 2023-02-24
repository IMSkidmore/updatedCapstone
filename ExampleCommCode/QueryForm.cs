using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using SerialClassLibrary;

/*
 * Notes:
 * Because Device Manager Win32 is a Win32 project, the entire C# app must be build as an x86 project.
 * Building it as an x64 or Any CPU will cause things to not function properly
 * 
 * This Project was built using .Net 3.5 Framework.  Earlier frameworks may work but have not been tested.
*/

namespace ExampleCommCode
{
    public partial class QueryForm : Form
    {
        #region Public Members
        readonly string m_sPtxDeviceDetected = " (Protonex Device Detected)";
        #endregion

        #region Private Members
        private SerialPortObject m_SerialPort;
        private string m_sSelComString;
        private List<string> m_PollList;
        #endregion

        #region Constructors
        public QueryForm()
        {
            InitializeComponent();
            btnStop.Enabled = false;

            SerialPortObject.FillComboWithPorts(tscmbxCommPorts, m_sSelComString);
            FillAvailableComPorts();

            dataGridView1.Rows.Add(5);

            // This is the list of items to poll
            // Modify the method RxPollData as necessary to handle the polling list
            m_PollList = new List<string>();
            m_PollList.Add("get Port1.name");
            m_PollList.Add("get Port1.v1");
            m_PollList.Add("get Port1.i1");
            m_PollList.Add("get Port2.name");
            m_PollList.Add("get Port2.v1");
            m_PollList.Add("get Port2.i1");
            m_PollList.Add("get Port3.name");
            m_PollList.Add("get Port3.v1");
            m_PollList.Add("get Port3.i1");
            m_PollList.Add("get Port4.name");
            m_PollList.Add("get Port4.v1");
            m_PollList.Add("get Port4.i1");
            m_PollList.Add("get Port5.name");
            m_PollList.Add("get Port5.v1");
            m_PollList.Add("get Port5.i1");
            m_PollList.Add("get Port6.name");
            m_PollList.Add("get Port6.v1");
            m_PollList.Add("get Port6.i1");
            m_PollList.Add("get sys.uptime");
        }
        #endregion

        #region Helpers
        private void FillAvailableComPorts()
        {
            SerialPortObject.FillComboWithPorts(tscmbxCommPorts, m_sSelComString, false);

            // Update our comm box
            if (tscmbxCommPorts.SelectedItem != null)
                UpdateCommBox();
            else
            {
                tscmbxCommPorts.SelectedItem = null;
                tscmbxCommPorts.Text = "";
                m_sSelComString = null;
            }

            /////////////////////////////
            // See if we can locate any Protonex devices
            // In Vista/Windows 7 this results in (for example) SPM61x SN: 520  appearing
            // on Windows XP only m_sPtxDeviceDetected will be shown due to the way XP handles USB enumeration
            // By default, m_sPtxDeviceDetected is: " (Protonex Device Detected)"
            DeviceManager.SearchForDevice devmgr = new DeviceManager.SearchForDevice();
            List<string> sTempComPorts = devmgr.GetAllNamesByVidPid("VID_03EB", "PID_6119");

            if (sTempComPorts.Count > 0)
                DeviceManager.ComPortMods.ReconstructComBox("VID_03EB", "PID_6119", sTempComPorts, tscmbxCommPorts, m_sPtxDeviceDetected);
        }

        private void UpdateCommBox()
        {
            int nSpace = tscmbxCommPorts.SelectedItem.ToString().IndexOf(' ');

            if (nSpace > 0)
                m_sSelComString = tscmbxCommPorts.SelectedItem.ToString().Remove(nSpace);
            else
                m_sSelComString = tscmbxCommPorts.SelectedItem.ToString();
        }

        private void closeHelper()
        {
            if (m_SerialPort != null)
            {
                m_SerialPort.Close();
                m_SerialPort = null;
            }

            btnStop.Enabled = false;
            btnStart.Enabled = true;
        }

        #endregion

        #region Serial Port
        private bool OpenSerialPort(bool bQuiet)
        {
            if (m_SerialPort == null)
                return SerialPortObject.OpenSerialPort(ref m_SerialPort, ref m_sSelComString, this, SerialErrorCallback, bQuiet);

            return true;
        }

        public void SerialErrorCallback(int nUserData)
        {
            SerialError(m_SerialPort.ErrorString.TrimEnd());
        }

        public void SerialError(string sErr)
        {
            txtbxHyperTermOutput.AppendText(string.Format("{0}: has the following error and will be closed: {1}\r\n", m_sSelComString, sErr));
        }
        #endregion

        #region Messaging

        private void SendPolls()
        {        
            for (int i = 0; i < m_PollList.Count; i++)
            {
                if (!m_SerialPort.SendPollerString(m_PollList[i], 1000, this, new SerialClassLibrary.RxDataUpdateCallback(RxPollData), i, 0))
                {
                    // serial port error
                    string sErr = m_SerialPort.ErrorString.TrimEnd();
                    txtbxHyperTermOutput.AppendText(m_sSelComString + ": " + " has the following error and will be closed.\r\n'" + sErr + "'" + "\r\n");

                    closeHelper();
                    break;
                }
            }
        }

        public void RxPollData(string sMsg, byte[] b, int nUserData, int nUserData2)
        {
            // This is the method you want to modify based upon the poll list you are doing

            // Note:  If you are polling a number of items at blazing speed, things can get misaligned if this receive function
            // processes too quickly (seems counter-intuitive).  Reason for this has yet to be determined.
            // Adding in a two millisecond delay is sufficient.  Extremely hacky, but works.
            System.Threading.Thread.Sleep(2);

            if (sMsg != null)
            {
                sMsg = sMsg.Substring(sMsg.IndexOf(':') +1);
                sMsg = sMsg.Trim();

                if (nUserData == m_PollList.Count - 1)
                    txtbxUptime.Text = sMsg;
                else
                {
                    DataGridViewRowCollection rows = dataGridView1.Rows;
                    DataGridViewRow row = rows[nUserData / 3];

                    DataGridViewCellCollection cells = row.Cells;
                    int nCell = (nUserData % 3);

                    cells[nCell].Value = sMsg;

                    // Compute the Power column
                    if ((nUserData % 3) == 2)
                        cells[3].Value = ((Convert.ToInt32(cells[1].Value) * Convert.ToInt32(cells[2].Value)) / 1000);
                }
            }
            else
            {
                // some sort of error or timeout likely occured
            }

            // Kick off the next round of polling
            if (nUserData == m_PollList.Count - 1)
                SendPolls();
        }

        private void SendHyperTermMessage()
        {
            if (OpenSerialPort(false))
            {
                m_SerialPort.SendStreamingMessage(txtbxHyperTerm.Text, 1000, this, new SerialClassLibrary.RxDataUpdateCallback(RxHyperTermData), 0, 0);
            }   
        }

        public void RxHyperTermData(string s, byte[] b, int nUserData, int nUserData2)
        {
            if ((s != null) && (s != ""))
                txtbxHyperTermOutput.AppendText(m_sSelComString + ": " + s + "\r\n");
        }

        #endregion

        #region User Interface
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!OpenSerialPort(false))
                return;

            btnStop.Enabled = true;
            btnStart.Enabled = false;

            SendPolls();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            closeHelper();
        }

        private void tsbtnRefresh_Click(object sender, EventArgs e)
        {
            FillAvailableComPorts();
        }

        private void tscmbxCommPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tscmbxCommPorts.SelectedItem != null)
                UpdateCommBox();
        }

        private void QueryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_SerialPort != null)
            {
                m_SerialPort.Close();
                m_SerialPort = null;
            }
        }

        private void btnSendHyperTerm_Click(object sender, EventArgs e)
        {
            SendHyperTermMessage();
        }

        private void txtbxHyperTerm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                SendHyperTermMessage();
        }
        #endregion

    }
}
