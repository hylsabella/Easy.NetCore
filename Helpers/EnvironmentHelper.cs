using System;
using System.Collections.Generic;
using System.Management;

namespace Easy.Common.NetCore.Helpers
{
    public static class EnvironmentHelper
    {
        public static string GetMacAddress()
        {
            string mac = string.Empty;

            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }

                moc.Dispose();
                mc.Dispose();

                return mac;
            }
            catch
            {
                mac = "unknow";
            }

            return mac;
        }

        public static List<string> GetAllMacAddress()
        {
            List<string> macList = new List<string>();

            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        string mac = mo["MacAddress"].ToString();
                        macList.Add(mac);
                    }
                }

                moc.Dispose();
                mc.Dispose();
            }
            catch (Exception)
            {

            }

            return macList;
        }
    }
}