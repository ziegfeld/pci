
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
			 //objMgmt = default(ManagementObject);
			ManagementObjectSearcher objMgmtSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

			foreach (ManagementObject objMgmt in objMgmtSearcher.Get()) {
				if (objMgmt["name"].ToString().IndexOf('|') >= 0) {
					// = Strings.Mid(objMgmt["name"].ToString(), 1, Strings.InStr(objMgmt["name"].ToString(), "|") - 2);
					m_strWindowsLocation = objMgmt["name"].ToString();
                    m_strWindowsOS = m_strWindowsLocation.Substring(0, m_strWindowsLocation.IndexOf('|'));// or - 1);?
                    m_strWindowsLocation = m_strWindowsLocation.Substring(m_strWindowsLocation.IndexOf('|') + 1);
                       
				} else {
					m_strWindowsOS = objMgmt["name"].ToString();
				}
				m_strComputerName = objMgmt["csname"].ToString();
				m_strWindowsRoot = objMgmt["windowsdirectory"].ToString();
			}

			objMgmtSearcher = null;
			objMgmtSearcher = new ManagementObjectSearcher("Select UUID From Win32_ComputerSystemProduct");
            foreach (ManagementObject objMgmt in objMgmtSearcher.Get())
            {
				m_strComputerUUID = objMgmt["UUID"].ToString();
			}

			objMgmtSearcher = null;
			objMgmtSearcher = new ManagementObjectSearcher("Select * From Win32_NetworkAdapterConfiguration");
			foreach (ManagementObject objMgmt in objMgmtSearcher.Get()) {
				if ((objMgmt["IPEnabled"].ToString() == "True")) {
					if ((!string.IsNullOrEmpty(objMgmt["DefaultIPGateway"].ToString()))) {
						m_strNetworkIPAddress = objMgmt["IPAddress"].ToString();
						m_strNetworkMACAddress = objMgmt["MacAddress"].ToString();
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
