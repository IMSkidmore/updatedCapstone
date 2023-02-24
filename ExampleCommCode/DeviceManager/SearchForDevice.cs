using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace DeviceManager
{
    public class SearchForDevice
    {
        [DllImport("DeviceManagerWin32.dll", CharSet=CharSet.Unicode)]
        public static extern void GetUsbBusDescription(string sVid, string sPid, string sComPort, StringBuilder sDesc);

        private ManagementPath m_Path = new ManagementPath();
        private ManagementClass m_Devices = null;

        public SearchForDevice()
        {
            m_Path.Server = ".";
            m_Path.NamespacePath = @"root\CIMV2";
            m_Path.RelativePath = @"Win32_PnPentity";

            m_Devices = new ManagementClass(new ManagementScope(m_Path), m_Path, new ObjectGetOptions(null, new TimeSpan(0, 0, 0, 2), true));
        }

        /// <summary>
        /// Gets the "Name" property for the first instance of the Device Id (VID and PID)
        /// Hex values are upper case
        /// </summary>
        /// <param name="searchPath">The path to Device Id to search for VID and PID</param>
        /// <returns></returns>
        public string GetNameByVidPid(string sVid, string sPid)
        {
            ManagementObjectCollection moc = m_Devices.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                bool bSearch = false;

                if ((sVid == null) || (sPid == null))
                    bSearch = true;
                else if (mo.Path.Path.Contains(sVid) && mo.Path.Path.Contains(sPid))
                    bSearch = true;

                if (bSearch)
                {
                    PropertyDataCollection devsProperties = mo.Properties;
                    foreach (PropertyData devProperty in devsProperties)
                    {
                        if (devProperty.Type == CimType.DateTime)
                            continue;
                        else if (devProperty.Name == "Name")
                        {
                            string sTemp = (string)devProperty.Value;
                            if (sTemp.Contains("COM"))
                                return sTemp;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the "Name" property for the all instances of the Device Id (VID and PID)
        /// Hex values are upper case
        /// </summary>
        /// <param name="searchPath">The path to Device Id to search for the VID and PID</param>
        /// <returns></returns>
        public List<string> GetAllNamesByVidPid(string sVid, string sPid)
        {
            List<string> sComPorts = new List<string>();

            ManagementObjectCollection moc = m_Devices.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                bool bSearch = false;

                if ((sVid == null) || (sPid == null))
                    bSearch = true;
                else if (mo.Path.Path.Contains(sVid) && mo.Path.Path.Contains(sPid))
                    bSearch = true;

                if (bSearch)
                {
                    PropertyDataCollection devsProperties = mo.Properties;
                    foreach (PropertyData devProperty in devsProperties)
                    {
                        if (devProperty.Type == CimType.DateTime)
                            continue;
                        else if (devProperty.Name == "Name")
                        {
                            sComPorts.Add((string)devProperty.Value);
                        }
                    }
                }
            }

            return sComPorts;
        }

        ////////////////////////////////////////////////////////////////////////////////////
        /////// Change Project to a Console Application and run it to see all info
        ////////////////////////////////////////////////////////////////////////////////////
        public static void Main()
        {
            ManagementPath path = new ManagementPath();
            ManagementClass devs = null;
            path.Server = ".";
            path.NamespacePath = @"root\CIMV2";
            path.RelativePath = @"Win32_PnPentity";

            using (devs = new ManagementClass(new ManagementScope(path), path, new ObjectGetOptions(null, new TimeSpan(0, 0, 0, 2), true)))
            {
                ManagementObjectCollection moc = devs.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    //plug in specific values here to search for something
                    if (mo.Path.Path.Contains("VID_03EB") && mo.Path.Path.Contains("PID_6119"))
                    //if (mo.Path.Path.Contains("PID_6119"))
                    {
                        PropertyDataCollection devsProperties = mo.Properties;
                        foreach (PropertyData devProperty in devsProperties)
                        {
                            if (devProperty.Type == CimType.DateTime)
                            {
                                if (devProperty.Value != null)
                                    Console.WriteLine("DateTime string");
                            }
                            else
                                Console.WriteLine("Property = {0}\t Value = {1}", devProperty.Name, devProperty.Value);
                        }

                        /*QualifierDataCollection qualProperties = mo.Qualifiers;
                        foreach (QualifierData qualProperty in qualProperties)
                        {
                            Console.WriteLine("Qualifier = {0}\t Value = {1}", qualProperty.Name, qualProperty.Value);
                        }

                        PropertyDataCollection devsSysProperties = mo.SystemProperties;
                        foreach (PropertyData devSysProperty in devsSysProperties)
                        {
                            if (devSysProperty.Type == CimType.DateTime)
                            {
                                if (devSysProperty.Value != null)
                                    Console.WriteLine("DateTime string");
                            }
                            else
                                Console.WriteLine("SysProp = {0}\t Value = {1}", devSysProperty.Name, devSysProperty.Value);
                        }*/

                        Console.WriteLine("----------------------");
                    }
                }
            }

            Console.WriteLine("-------------------------");

            StringBuilder sDesc = new StringBuilder(128);
            GetUsbBusDescription("VID_03EB", "PID_6119", "COM3", sDesc);

            if (sDesc.Length < 1)
                Console.WriteLine("Returned: Nothing (Maybe, untested new code here)");
            else
                Console.WriteLine("Returned: {0}", sDesc.ToString());

            Console.WriteLine("-------------------------");
        }
    }
}