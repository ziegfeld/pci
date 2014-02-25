using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.Management;
using System.Runtime.InteropServices;

namespace Enterprise
{

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public class EEComputerManagement
    {

        // ###################################################################################
        // ###################################################################################
        // Public Constructors
        // ###################################################################################
        // ###################################################################################
        public EEComputerManagement()
        {
            InitializeMembers();
        }


        // ###################################################################################
        // ###################################################################################
        // Public Functions
        // ###################################################################################
        // ###################################################################################
        public void Refresh()
        {
            InitializeMembers();
        }


        // ###################################################################################
        // ###################################################################################
        // Private Functions
        // ###################################################################################
        // ###################################################################################
        private void InitializeMembers()
		{
			ManagementObject objMgmt = default(ManagementObject);
			ManagementObjectSearcher objMgmtSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

			foreach ( objMgmt in objMgmtSearcher.Get()) {
				if ((Strings.InStr(objMgmt("name").ToString(), "|") > 0)) {
					m_strWindowsOS = Strings.Mid(objMgmt("name").ToString(), 1, Strings.InStr(objMgmt("name").ToString(), "|") - 2);
					m_strWindowsLocation = Strings.Mid(objMgmt("name").ToString(), Strings.InStr(objMgmt("name").ToString(), "|") + 1);
				} else {
					m_strWindowsOS = objMgmt("name").ToString();
				}
				m_strComputerName = objMgmt("csname").ToString();
				m_strWindowsRoot = objMgmt("windowsdirectory").ToString();
			}

			objMgmtSearcher = null;
			objMgmtSearcher = new ManagementObjectSearcher("Select UUID From Win32_ComputerSystemProduct");
			foreach ( objMgmt in objMgmtSearcher.Get()) {
				m_strComputerUUID = objMgmt("UUID").ToString();
			}

			objMgmtSearcher = null;
			objMgmtSearcher = new ManagementObjectSearcher("Select * From Win32_NetworkAdapterConfiguration");
			foreach ( objMgmt in objMgmtSearcher.Get()) {
				if ((objMgmt("IPEnabled").ToString() == "True")) {
					if ((!string.IsNullOrEmpty(objMgmt("DefaultIPGateway")(0).ToString()))) {
						m_strNetworkIPAddress = objMgmt("IPAddress")(0).ToString();
						m_strNetworkMACAddress = objMgmt("MacAddress").ToString();
						break; // TODO: might not be correct. Was : Exit For
					}
				}
			}
		}


        // ###################################################################################
        // ###################################################################################
        // Property Functions
        // ###################################################################################
        // ###################################################################################
        public string ComputerName
        {
            get { return m_strComputerName; }
        }
        public string ComputerUUID
        {
            get { return m_strComputerUUID; }
        }

        public string WindowsOS
        {
            get { return m_strWindowsOS; }
        }
        public string WindowsRoot
        {
            get { return m_strWindowsRoot; }
        }

        public string IPAddress
        {
            get { return m_strNetworkIPAddress; }
        }
        public string MACAddress
        {
            get { return m_strNetworkMACAddress; }
        }


        // ###################################################################################
        // ###################################################################################
        // Member Variables
        // ###################################################################################
        // ###################################################################################
        private string m_strComputerName;

        private string m_strComputerUUID;
        private string m_strWindowsOS;
        private string m_strWindowsRoot;

        private string m_strWindowsLocation;
        private string m_strNetworkIPAddress;

        private string m_strNetworkMACAddress;
    }
}
