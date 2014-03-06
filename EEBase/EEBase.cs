using System;

namespace Enterprise
{

    public class EEBase
    {

        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Protected\Private Functions
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        #region "Protected\PrivateFunctions"

        // ###################################################################################################################################################################
        //   Protected Functions
        // ###################################################################################################################################################################

        protected virtual void SetProductInfo(string strProductRoot, string strProductHive, string strProductCipherLength)
        {
            m_strProductRoot = strProductRoot;
            m_strProductHive = strProductHive;
            m_strProductCipherLength = strProductCipherLength;
            m_strProductVersion = ObjectVersion;
            m_strProductVersion = m_strProductVersion.Substring(0, m_strProductVersion.LastIndexOf("."));
                //Strings.Left(Strings.Replace(ObjectVersion(), ".", ""), 3);
                //objVersionInfo As Version = System.Reflection.Assembly.GetExecutingAssembly.GetName.Version
                //Return objVersionInfo.Major & "." & objVersionInfo.Minor & "." & objVersionInfo.Build & "." & objVersionInfo.Revision
            
        }

        protected virtual void SetupBase(string strProductRoot = "", string strProductHive = "", int intProductCipherLength = 0)
        {
            string strProductCipherLength = Convert.ToString(intProductCipherLength);
            SetProductInfo(strProductRoot, strProductHive, strProductCipherLength);
            m_objLog = new Enterprise.EELog(m_strProductRoot, m_strProductHive, m_strProductVersion, "");
            m_objComputerManagement = new Enterprise.EEComputerManagement();
            m_objLog.LogMessage("EEBase : SetupBase : Log Initialized", 35);
        }

        protected virtual void SetupBase(string strProductRoot, string strProductHive, string strProductCipherLength, string strBaseKey, string strSpecificKey, string strKeyDat, string strCustomerData, string strLogPath = "", bool blnLogEnabled = false, int intLogLevel = 0)
        {
            SetProductInfo(strProductRoot, strProductHive, strProductCipherLength);
            m_objLog = new Enterprise.EELog();
            m_objLog.OverrideRegistryInformation(strLogPath, blnLogEnabled, intLogLevel);
            m_objLog.LogMessage("EEBase : SetupBase : Log Initialized", 35);
            m_objComputerManagement = new Enterprise.EEComputerManagement();
        }

        protected bool CheckInternalLicenses(int intCheckLocation, int intCapturedAmount = 0, string strInstance = "")
        {
            bool blnReturn = false;

            m_objLog.LogMessage("EEBase: CheckInternalLicenses(): ", 40);

            m_objLog.LogMessage("EEBase: Creating Registry Object(): " + m_strProductRoot + " : " + m_strProductHive + " : " + m_strProductVersion + " : " + m_strProductInstance, 40);
            m_objRegistry = new Enterprise.EERegistry(m_strProductRoot, m_strProductHive, m_strProductVersion, m_strProductInstance);

            GetKeyInfo();
            blnReturn = CheckBaseKey();
            if (blnReturn)
                blnReturn = CheckSpecificKey(intCheckLocation, intCapturedAmount);

            m_objLog.LogMessage("EEBase: CheckInternalLicenses(): " + blnReturn, 40);

            return blnReturn;
        }

        // ###################################################################################################################################################################
        //   Private Functions
        // ###################################################################################################################################################################

        private void GetKeyInfo(string strBaseKey = "", string strSpecificKey = "", string strCustomerNumber = "")
        {
            m_objLog.LogMessage("EEBase : GetKeyInfo : Entering", 40);

            if ((string.IsNullOrEmpty(strBaseKey)))
                m_strBaseKey = m_objRegistry.ProductGetKeyValue("BaseKey");
            else
                m_strBaseKey = strBaseKey;
            if ((string.IsNullOrEmpty(strSpecificKey)))
                m_strSpecificKey = m_objRegistry.ProductGetKeyValue("SpecificKey");
            else
                m_strSpecificKey = strSpecificKey;
            if ((string.IsNullOrEmpty(strCustomerNumber)))
                m_strCustomerNumber = m_objRegistry.ProductGetKeyValue("Customer");
            else
                m_strCustomerNumber = strCustomerNumber;

            m_objLog.LogMessage("EEBase : GetKeyInfo : Exiting", 40);
        }

        private bool CheckBaseKey(string strProductRoot = "", string strProductHive = "", int strProductCipherLength = 0)
        {
            bool blnReturn = false;

            m_objLog.LogMessage("EEBase: CheckBaseKey()", 40);

            if ((!string.IsNullOrEmpty(strProductRoot)) || (!string.IsNullOrEmpty(strProductHive)) || (strProductCipherLength != 0))
            {
                SetupBase(strProductRoot, strProductHive, strProductCipherLength);
                //if any of the 3 is not Null, then setup a new base? Lingfei1118
                GetKeyInfo();
            }
            if (!string.IsNullOrEmpty(m_strBaseKey))
            {
                Enterprise.EEBaseKeyChecker objKeyChecker = new Enterprise.EEBaseKeyChecker(m_strProductVersion, m_strProductHive, m_strProductCipherLength, m_strCustomerNumber, m_strBaseKey, ref m_objLog);
                blnReturn = objKeyChecker.CheckKey();
                if (!blnReturn)
                {
                    m_intResponseCode = 33;
                    m_strResponseDescription = "Base Key is invalid.  Please contact your partner to update your key.";
                }
            }
            else
            {
                m_intResponseCode = 32;
                m_strResponseDescription = "Base Key is Blank: " + m_strProductRoot + '\\' + m_strProductHive + '\\' + m_strProductVersion;
            }

            m_objLog.LogMessage("EEBase: CheckBaseKey(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckSpecificKey(int intCheckLocation, int intCapturedAmount = 0)
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EEBase: CheckSpecificKey(): Entering", 40);

            // MPF TODO Pass Instance Here
            Enterprise.EESpecificKeyClient objSpecificKeyChecker = new Enterprise.EESpecificKeyClient(m_strProductRoot, m_strProductVersion, m_strProductHive, m_strProductInstance, m_objLog);
            blnReturn = objSpecificKeyChecker.CheckSpecificKey(intCheckLocation, intCapturedAmount);
            if (!blnReturn)
            {
                m_intResponseCode = 34;
                m_strResponseDescription = "Specific Key is invalid.  Please contact your partner to update your key.";
            }

            m_objLog.LogMessage("EEBase: CheckSpecificKey(): " + blnReturn, 40);

            return blnReturn;
        }

        #endregion

        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Member Variables\Functions
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        #region "MemberVariables\Properties"

        // ###################################################################################################################################################################
        //   Property Functions
        // ###################################################################################################################################################################

        protected string ObjectVersion
        {
            get
            {
                Version objVersionInfo = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return objVersionInfo.Major + "." + objVersionInfo.Minor + "." + objVersionInfo.Build + "." + objVersionInfo.Revision;
            }
        }

        public virtual int ResponseCode
        {
            get { return m_intResponseCode; }
        }

        public virtual string ResponseDescription
        {
            get { return m_strResponseDescription; }
        }

        // ###################################################################################################################################################################
        //   Private\Protected Member Variables
        // ###################################################################################################################################################################

        protected string m_strProductRoot;
        protected string m_strProductHive;
        protected string m_strProductVersion;
        protected string m_strProductInstance;
        //  'inappropriate naming of integer type (starting with m_str) Lingfei Nov.18
        protected string m_strProductCipherLength;

        protected string m_strBaseKey;
        protected string m_strSpecificKey;

        protected string m_strCustomerNumber;
        protected Enterprise.EELog m_objLog;
        protected Enterprise.EERegistry m_objRegistry;

        protected Enterprise.EEComputerManagement m_objComputerManagement;
        // Internal response code to calling process.
        protected int m_intResponseCode;
        // Internal response message to calling process.
        protected string m_strResponseDescription;

        #endregion

    }

}
