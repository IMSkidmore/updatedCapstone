using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DeviceManager
{
    public static class ComPortMods
    {
        [DllImport("DeviceManagerWin32.dll", CharSet = CharSet.Unicode)]
        public static extern void GetUsbBusDescription(string sVid, string sPid, string sComPort, StringBuilder sDesc);

        public static void ReconstructComBox(string sVid, string sPid, List<string> sTempComPorts, ToolStripComboBox cb, string sDefaultAppendedText)
        {
            try
            {
                List<string> sBusDescription = new List<string>(sTempComPorts.Count);

                int nTempSelection = cb.SelectedIndex;

                for (int i = 0; i < sTempComPorts.Count; i++)
                {
                    string sComPort = sTempComPorts[i].Substring(sTempComPorts[i].IndexOf("COM"));
                    sComPort = sComPort.Remove(sComPort.Length - 1);

                    sTempComPorts[i] = sComPort;
                    StringBuilder sDesc = new StringBuilder(128);
                    GetUsbBusDescription(sVid, sPid, sComPort, sDesc);
                    sBusDescription.Add(sDesc.ToString());

                    if (sBusDescription[i] == null)
                        sBusDescription[i] = sDefaultAppendedText;
                    else
                        sBusDescription[i] = string.Format("  ({0} detected)", (sBusDescription[i]));
                }

                for (int i = 0; i < sTempComPorts.Count; i++)
                {
                    string sComPort = sTempComPorts[i];
                    string sDescription = sBusDescription[i];

                    for (int j = 0; j < cb.Items.Count; j++)
                    {
                        string sComItemBx = cb.Items[j].ToString();
                        string sTrimmedComItemBx = "";

                        try
                        {
                            sTrimmedComItemBx = sComItemBx.Substring(0, sComItemBx.IndexOf(' '));
                        }
                        catch { sTrimmedComItemBx = sComItemBx; }

                        if (sTrimmedComItemBx.Equals(sComPort) && !sComItemBx.Contains(sDescription))
                        {
                            string sNewDeviceInfo = sComPort + sDescription;
                            cb.Items.RemoveAt(j);
                            cb.Items.Insert(j, sNewDeviceInfo);
                        }
                    }
                }

                cb.SelectedIndex = nTempSelection;
            }
            catch
            {
            }
        }
    }
}
