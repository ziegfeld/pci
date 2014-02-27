using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Enterprise
{

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public class EEBaseKeyChecker
    {

        public EEBaseKeyChecker()
        {
            m_strVersion = "";
            m_strProductHive = "";
            m_intProductCeasarCipher = -1;
            m_strCustomerNumber = "";
            m_strProductKey = "";
            m_objFocalPoints = new System.Collections.Generic.List<int>();
        }

        public EEBaseKeyChecker(string strVersion, string strProductHive, string intProductCeasarCipher, string strCustomerNumber)
        {
            m_strVersion = strVersion;
            m_strProductHive = strProductHive;
            m_intProductCeasarCipher = Convert.ToInt32(intProductCeasarCipher);
            m_strCustomerNumber = strCustomerNumber;
            m_objFocalPoints = new System.Collections.Generic.List<int>();
        }

        public EEBaseKeyChecker(string strVersion, string strProductHive, string intProductCeasarCipher, string strCustomerNumber, string strProductKey, ref Enterprise.EELog objLog)
        {
            m_strVersion = strVersion;
            m_strProductHive = strProductHive;
            m_intProductCeasarCipher = Convert.ToInt32(intProductCeasarCipher);
            m_strCustomerNumber = strCustomerNumber;
            m_strProductKey = strProductKey;
            m_objLog = objLog;
            m_objFocalPoints = new System.Collections.Generic.List<int>();
        }

        // ###################################################################################
        // ###################################################################################
        // Public Functions
        // ###################################################################################
        // ###################################################################################

        public bool CheckKey()
        {
            m_objLog.LogMessage("EEBaseKeyChecker: CheckKey()", 40);
            return CheckKeyStructure();
        }

        public bool CheckKey(string strProductKey)
        {
            m_objLog.LogMessage("EEBaseKeyChecker: CheckKey(): " + strProductKey, 40);
            m_strProductKey = strProductKey;
            return CheckKey();
        }

        public bool CheckKey(string strVersion, string strProductHive, string strProductCeasarCipher, string strCustomerNumber, string strProductKey)
        {
            int intProductCeasarCipher = Convert.ToInt32(strProductCeasarCipher);
            m_objLog.LogMessage("EEBaseKeyChecker: CheckKey(): " + strVersion + "-" + strProductHive + "-" + intProductCeasarCipher + "-" + strCustomerNumber + "-" + strProductKey, 40);
            m_strVersion = strVersion;
            m_strProductHive = strProductHive;
            m_intProductCeasarCipher = intProductCeasarCipher;
            m_strCustomerNumber = strCustomerNumber;
            m_strProductKey = strProductKey;
            return CheckKey();
        }

        protected virtual bool CheckEnd()
        {   // why not implemented? LfZ
            m_objLog.LogMessage("EEBaseKeyChecker: CheckEnd(): ", 40);
            return true;
        }

        // ###################################################################################
        // ###################################################################################
        // Private Functions
        // ###################################################################################
        // ###################################################################################

        private bool CheckKeyStructure()
        {
            bool blnReturn = false;
            m_objLog.LogMessage("EEBaseKeyChecker: CheckKeyStructure()", 40);
            blnReturn = BreakDownCustomerNumber();
            if (blnReturn)
                blnReturn = CheckFocalPoints();
            if (blnReturn)
                blnReturn = CheckHive();
            if (blnReturn)
                blnReturn = CheckEnd();
            m_objLog.LogMessage("EEBaseKeyChecker: CheckKeyStructure(): " + blnReturn, 40);
            return blnReturn;
        }

        private bool CheckHive()
        {
            bool blnReturn = false;

            m_objLog.LogMessage("EEBaseKeyChecker: CheckHive()", 40);

            // Only perform operation if m_strProductHive exists and is of the correct size and m_strProductKey is of the proper length.
            if ((m_strProductKey.Length != 32)  ||  (m_strProductHive.Length != 4))
                return blnReturn;

            int intMatchesFound = 0;
            string strHiveSection = m_strProductKey.Substring(24, 4);

            //check if only 2 of the 4 chars in strHiveSection matchs any of strProductHive(say, EEPM) LfZ
            for (int i = 0; i < strHiveSection.Length; i++)
                if (strHiveSection[i] == m_strProductHive[2] || strHiveSection[i] == m_strProductHive[3])
                    ++intMatchesFound;
            

            if (intMatchesFound == 2)
                blnReturn = true;

            m_objLog.LogMessage("EEBaseKeyChecker: CheckHive(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckFocalPoints()
        {
            bool blnReturn = false;

            m_objLog.LogMessage("EEBaseKeyChecker: CheckFocalPoints()", 40);

            // Only perform operation if m_objFocalPoints isn't empty, m_strProductKey is of the proper length and the Product version exists
            m_strVersion = m_strVersion.Replace(".", "");
            m_strProductKey = m_strProductKey.Replace("-", "");
            if ((m_objFocalPoints.Count <= 0) || (m_strProductKey.Length != 32) || (m_strVersion.Length < 3))
                return blnReturn;

            int intDivisor = 0;
            int intExpectedRemainder = 0;

            //TODO Add logic here to look down the number scheme to see if they have an older license issued.  
            //     Online check will confirm the older license is still valid.
            intDivisor = (m_strVersion[0] - '0') + 1;
            if ((intDivisor > 9))
                intDivisor = 7;
            intExpectedRemainder = m_strVersion[1] - '0';

            foreach (int i in m_objFocalPoints)
            {
                int intFocalPointValue = m_strProductKey[i + 4] - '0';
                if (intFocalPointValue % intDivisor != intExpectedRemainder)
                    return blnReturn;
            }
            blnReturn = true;

            m_objLog.LogMessage("EEBaseKeyChecker: CheckFocalPoints(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool BreakDownCustomerNumber()
        {
            bool blnReturn = false;
            m_objLog.LogMessage("EEBaseKeyChecker: BreakDownCustomerNumber(): " + m_strCustomerNumber, 40);
            // Only perform the operation if a customer number exists and a Cipher value has been entered
            if (string.IsNullOrEmpty(m_strCustomerNumber) || m_intProductCeasarCipher < 0)
                return blnReturn;
            // Clear the list of any old data
            m_objFocalPoints.Clear();

            //string strCustomerNumber = m_strCustomerNumber.Substring(0, 6);
            //strCustomerNumber = strCustomerNumber.Substring(1);
            // above 2 lines changed to the following. 0226 LfZ
            string strCustomerNumber = m_strCustomerNumber.Substring(1, 5);

            for (int intCounter = 0; intCounter < strCustomerNumber.Length; intCounter++)
            {
                int intCurrentChar = strCustomerNumber[intCounter] - '0';
                intCurrentChar += m_intProductCeasarCipher;
                // We only have numbers 0 - 19 available.  Move\Remove all others.  Anything 20 or over is moved to single digits.

                if (!(m_objFocalPoints.Contains(intCurrentChar)))
                {
                    // New value, increase by product specified amount and add.
                    if (intCurrentChar < 20)
                        m_objFocalPoints.Add(intCurrentChar);
                }
                else
                {
                    //   Value already exists.  Depending on it's value
                    if (intCurrentChar < 10)
                        intCurrentChar += m_intProductCeasarCipher;
                    else
                        intCurrentChar -= m_intProductCeasarCipher;
                    if (intCurrentChar >= 20)
                        intCounter %= 10;
                    if (!m_objFocalPoints.Contains(intCurrentChar))
                            m_objFocalPoints.Add(intCurrentChar);
                }
            }

            m_objFocalPoints.Sort();
            if ((m_objFocalPoints.Count > 0))
                blnReturn = true;

            m_objLog.LogMessage("EEBaseKeyChecker: BreakDownCustomerNumber(): " + blnReturn, 40);

            return blnReturn;
        }

        // ###################################################################################
        // ###################################################################################
        // Property Functions
        // ###################################################################################
        // ###################################################################################

        public string Product
        {
            get { return m_strProductHive; }
            set { m_strProductHive = value; }
        }

        public string ProductCipher
        {
            get { return Convert.ToString(m_intProductCeasarCipher); }
            set { m_intProductCeasarCipher = Convert.ToInt32(value); }
        }

        public string CustomerNumber
        {
            get { return m_strCustomerNumber; }
            set { m_strCustomerNumber = value; }
        }

        public string ProductKey
        {
            get { return m_strProductKey; }
            set { m_strProductKey = value; }
        }

        public string Version
        {
            get { return m_strVersion; }
            set { m_strVersion = value; }
        }

        // ###################################################################################
        // ###################################################################################
        // Member Variables
        // ###################################################################################
        // ###################################################################################
        protected string m_strVersion;
        protected string m_strProductHive;
        protected string m_strCustomerNumber;

        protected int m_intProductCeasarCipher;

        protected string m_strProductKey;

        protected Enterprise.EELog m_objLog;

        private System.Collections.Generic.List<int> m_objFocalPoints;
    }

    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public class EESpecificKeyClient
    {

        public EESpecificKeyClient()
        {
            Root = "";
            ProductHive = "";
            ProductVersion = "";
            Instance = "";
            m_objRequestKey = new List<char>();
            //Lf 022614 not used. m_objConfirmationKey = new List<char>();
        }

        public EESpecificKeyClient(string strProductRoot, string strProductVersion, string strProductHive, EELog objLog)
        {
            InitializeMembers(strProductRoot, strProductHive, strProductVersion, "", objLog);
        }

        public EESpecificKeyClient(string strProductRoot, string strProductVersion, string strProductHive, string strInstance, EELog objLog)
        {
            InitializeMembers(strProductRoot, strProductHive, strProductVersion, strInstance, objLog);
        }

        public EESpecificKeyClient(string strProductRoot, string strProductVersion, string strProductHive, string strInstance, EELog objLog, string strSpecificKey)
        {
            InitializeMembers(strProductRoot, strProductHive, strProductVersion, strInstance, objLog, strSpecificKey);
        }

        // ###################################################################################
        // ###################################################################################
        // Public Functions
        // ###################################################################################
        // ###################################################################################

        public bool CheckSpecificKey(int intBitCheck = -1, int intCaptureAmount = 0)
        {
            m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKey(): " + intBitCheck + " : " + intCaptureAmount, 40);

            string strSpecificString = m_objRegistry.ProductGetKeyValue("SpecificKey");
            if (strSpecificString.Length == 0)
            {
                m_objLog.LogMessage("SpecificKey is blank.  Request in progress.", 35);
                if ((!string.IsNullOrEmpty(BuildRequestKey())))
                {
                    m_objLog.LogMessage("BuildRequestKey produced: " + SpecificKeyRequest, 30);
                    EEPaymentManager.EEKMWeb.EEKMWeb objService = GetWebServiceReference();
                    strSpecificString = objService.BuildSpecificKey(SpecificKeyRequest);
                }
                else
                {
                    m_objLog.LogMessage("BuildRequestKey produced no request key.", 35);
                }
            }
            if (strSpecificString.Length == 0)
            {
                m_objLog.LogMessage("SpecificKey is blank and nothing was returned from request.", 35);
                return false;
            }
            if (strSpecificString.Substring(0, 6) == "ERROR:")
            {
                m_objLog.LogMessage("Error: " + strSpecificString.Substring(6), 1);
                return false;
            }
                SpecificKey = strSpecificString;
                SpecificKeyEEEMVersion = SpecificKey.Substring(SpecificKey.Length - 3, 3);
                m_objLog.LogMessage("CheckSpecificKey(intBitCheck As Integer): " + intBitCheck, 30);
                m_objLog.LogMessage("Specific Key to Check: " + SpecificKey, 30);

            if (!CheckSpecificKeyLength()) return false;
            // Check key length to 78
            if (!CheckIdentifyingKey()) return false;
            // Compare Base Key IdentifyingGuid.
            if (!CheckProductHive()) return false;;
            // Compare Product Hive to IdentifyingGuid
            if (!CheckProductVersion()) return false; ;
            // Compare Version Number to Variant
            if (!CheckCustomerNumber()) return false;
            // Compare Customer Number
            if (!CheckCustomerName()) return false;
            // Compare Customer Number
            if (!CheckCompUUIDParts()) return false;;
            // Compare Computer UUID to Key sections
            //If (blnReturn) Then blnReturn = CheckLicenseDates() ' Check License hasn't expired.

            // Check SpecificKeyDat info.
            
                m_objLog.LogMessage("Specific key checks out.  Checking Dat.", 35);
            if (!CheckSpecificKeyDat()) return false;;

            if (!FillSpecificKeyOptions()) return false;;
            //If (blnReturn) Then blnReturn = CheckSpecificKeyOption(intBitCheck)

            if (intCaptureAmount > 0)
                AddToCaptureTotal(intCaptureAmount);

            m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKey(): true", 40);

            return true;
        }

        // ###################################################################################
        // ###################################################################################
        // Private Functions
        // ###################################################################################
        // ###################################################################################
        private void InitializeMembers(string strRoot = "", string strProductHive = "", string strProductVersion = "", string strInstance = "", EELog objLog = null, string strSpecificKey = "")
        {
            m_objRequestKey = new List<char>();
            // LfZ 0226: not used.  m_objConfirmationKey = new List<char>();
            m_objSpecificKey = new List<char>();
            m_objSpecificKeySettings = new List<bool>();

            Root = strRoot;
            ProductHive = strProductHive;
            ProductVersion = strProductVersion;
            Instance = strInstance;

            if ((objLog != null))
                m_objLog = objLog;
            else
                m_objLog = new Enterprise.EELog(Root, ProductHive, ProductVersion, "");
            m_objLog.LogMessage("EESpecificKeyClient: InitializeMembers(): Entering : " + strRoot + " : " + strProductHive + " : " + strProductVersion + " : " + strInstance + " : ", 40);

            m_objCM = new Enterprise.EEComputerManagement();
            m_objRegistry = new Enterprise.EERegistry(Root, ProductHive, ProductVersion, Instance);

            m_objLog.LogMessage("EESpecificKeyClient: InitializeMembers(): Exiting.", 40);
        }

        private string BuildRequestKey()
        {
            string strReturn = "";
            m_objLog.LogMessage("EESpecificKeyClient: BuildRequestKey(): ", 40);
            if ((SetRequestKeyPoints()))
            {
                strReturn = SpecificKeyRequest;
            }
            m_objLog.LogMessage("EESpecificKeyClient: BuildRequestKey(): " + strReturn, 40);
            return strReturn;
        }

        private bool SetRequestKeyPoints() //!!!important LfZ 02242014
        {
            bool blnReturn = false;

            m_objLog.LogMessage("EESpecificKeyClient: SetRequestKeyPoints(): ", 40);

            int intCounter = 0;
            string strComputerName = "";
            string strComputerUUID = "";

            string strPseudoJulianDate = ((DateTime.Today.Year % 100) * 1000 + DateTime.Today.DayOfYear).ToString(); // DateTime.Today.DayOfYear.ToString("000");
            //string strPseudoJulianDate = Strings.Mid(DateAndTime.Year(DateAndTime.Now()).ToString(), 3) + DateAndTime.Now.DayOfYear.ToString().PadLeft(3, "0");
            string strEEEMVersion = "";
            string strCustomerNumber = "";
            string strCustomerBaseKey = "";

            strComputerName = m_objCM.ComputerName;
            strComputerUUID = m_objCM.ComputerUUID.Replace("-", "");

            strCustomerNumber = m_objRegistry.ProductGetKeyValue("Customer");
            if (strCustomerNumber.Length > 6)
                strCustomerNumber = strCustomerNumber.Substring(0, 6);
            strCustomerBaseKey = m_objRegistry.ProductGetKeyValue("BaseKey");

            m_objLog.LogMessage("EESpecificKeyClient: SetRequestKeyPoints(): " + Root + " : " + Instance + " : " + ProductHive, 40);

            strEEEMVersion = m_objRegistry.ProductGetLatestVersion(Root, Instance, ProductHive);

            m_objLog.LogMessage("EESpecificKeyClient: SetRequestKeyPoints(): " + strComputerName + " : " + strComputerUUID + " : " +
                strCustomerNumber + " : " + strCustomerBaseKey + " : " + strEEEMVersion, 40);

            // Endure that the needed BaseKey and EEEM information is installed
            if ((!string.IsNullOrEmpty(strCustomerBaseKey)) && (!string.IsNullOrEmpty(strCustomerNumber)) && (!string.IsNullOrEmpty(strEEEMVersion)))
            {
                // Place first 6 characters of UUID into spaces 0-5
                int intPlacement = 0;
                for (intCounter = 0; intCounter < 6; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, strComputerUUID[intCounter]);

                // Place the first 4 characters from the base key into spaces 6-9
                //intPlacement = 6
                for (intCounter = 0; intCounter < 4; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, strCustomerBaseKey[intCounter]);

                // Place characters 7-12 of UUID into spaces 10-15
                //intPlacement = 10
                for (intCounter = 6; intCounter < 12; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, strComputerUUID[intCounter]);

                // Place the Year Julian Date into spaces 16-20
                //intPlacement = 16
                for (intCounter = 0; intCounter < 5; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, strPseudoJulianDate[intCounter]);

                // Place the Product Hive into spaces 21-24
                //intPlacement = 21
                for (intCounter = 0; intCounter < 4; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, ProductHive[intCounter]);

                // Place the Product Version into spaces 25-27
                //intPlacement = 25
                for (intCounter = 0; intCounter < 3; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, ProductVersion[intCounter]);

                // Place characters 13-18 of UUID into spaces 28-33
                //intPlacement = 28
                for (intCounter = 12; intCounter < 18; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, strComputerUUID[intCounter]);

                // Place last 5 characters of Computer Name in spaces 34-38
                if (strComputerName.Length > 5)
                {
                    strComputerName = strComputerName.Substring(strComputerName.Length - 5, 5);
                }
                else
                {
                    strComputerName = strComputerName.PadLeft(5, '!');
                }
                //intPlacement = 34
                for (intCounter = 0; intCounter < 5; intCounter++)
                    m_objRequestKey.Insert(intPlacement++,strComputerName[intCounter]);

                // Place last 6 characters of UUID into spaces 39-44
                //intPlacement = 39
                for (intCounter = strComputerUUID.Length - 6; intCounter < strComputerUUID.Length; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, strComputerUUID[intCounter]);
                
                // Place the Customer number into spaces 45-50
                //intPlacement = 45
                for (intCounter = 0; intCounter < 6; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, strCustomerNumber[intCounter]);

                // Place the EEEM version into spaces 51-53
                //intPlacement = 51
                for (intCounter = 0; intCounter < 3; intCounter++)
                    m_objRequestKey.Insert(intPlacement++, strEEEMVersion[intCounter]);

                blnReturn = true;
            }
            else
            {
                m_objRequestKey.Clear();
            }

            m_objLog.LogMessage("EESpecificKeyClient: SetRequestKeyPoints(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckSpecificKeyLength()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyLength(): ", 40);

            if (SpecificKey.Length != 78)
            {
                blnReturn = false;
                m_objLog.LogMessage("SpecificKey reported length: " + SpecificKey.Length, 30);
            }

            m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyLength(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckProductHive()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckProductHive(): ", 40);

            string strCompare = SpecificKey.Substring(6, 4);
            if (ProductHive != strCompare)
            {
                blnReturn = false;
                m_objLog.LogMessage("Product Hives do not match: " + ProductHive + " != " + strCompare, 30);
            }

            m_objLog.LogMessage("EESpecificKeyClient: CheckProductHive(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckProductVersion()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckProductVersion(): ", 40);

            string strCompare = SpecificKey.Substring(29, 3);
            if (ProductVersion.Substring(0, 3) != strCompare)
            {
                blnReturn = false;
                m_objLog.LogMessage("Product Version do not match: " + ProductVersion + " != " + strCompare, 30);
            }

            m_objLog.LogMessage("EESpecificKeyClient: CheckProductVersion(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckCustomerNumber()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckCustomerNumber(): ", 40);

            string strCompare = SpecificKey.Substring(65, 6);
            if (m_objRegistry.ProductGetKeyValue("Customer").Substring(0, 6) != strCompare)
            {
                blnReturn = false;
                m_objLog.LogMessage("Customer Number do not match: " + m_objRegistry.ProductGetKeyValue("Customer").Substring(0, 6) + " != " + strCompare, 30);
            }

            m_objLog.LogMessage("EESpecificKeyClient: CheckCustomerNumber(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckCustomerName()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckCustomerName(): ", 40);

            string strCompare = SpecificKey.Substring(32, 5);
            string strCustomerName = m_objRegistry.ProductGetKeyValue("Customer");
            if (strCustomerName.IndexOf('-') >= 0)
            {
                strCustomerName = strCustomerName.Substring(strCustomerName.IndexOf('-') + 1, 5);//what if length of "-" to end <5?
                //Strings.Left(Strings.Mid(strCustomerName, Strings.InStr(strCustomerName, "-") + 1), 5);
                if (strCustomerName.Length <= 0)
                {
                    blnReturn = false;
                    m_objLog.LogMessage("Customer Name's length is <= 0.", 35);
                }
            }
            else
            {
                blnReturn = false;
                m_objLog.LogMessage("Customer Name isn't found in the registry.", 35);
            }
            if (blnReturn)
            {
                strCustomerName = strCustomerName.PadRight(5, 'X');
                if (strCustomerName != strCompare)
                {
                    blnReturn = false;
                    m_objLog.LogMessage("Customer Name does not match: " + strCustomerName + " != " + strCompare, 30);
                }
            }

            m_objLog.LogMessage("EESpecificKeyClient: CheckCustomerName(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckLicenseDates()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckLicenseDates(): ", 40);

            string strDateRequested = SpecificKey.Substring(20, 5);
            DateTime dteToday = DateTime.Now.Date;
            DateTime dteRequested = new DateTime(2000 + Convert.ToInt32(strDateRequested.Substring(0, 2)), 1, 1);
            dteRequested = dteRequested.AddDays(Convert.ToDouble(strDateRequested.Substring(2, 3)));
            if ((dteToday < dteRequested))
            {
                blnReturn = false;
                m_objLog.LogMessage("Request date issue.  Requested: " + dteRequested.ToString() + ".  Today: " + dteToday.ToString(), 30);
            }
            if ((blnReturn))
            {
                string strKeyLife = SpecificKey.Substring(55, 4);
                dteRequested = dteRequested.AddDays(Convert.ToInt32(strKeyLife));
                if ((dteToday > dteRequested))
                {
                    blnReturn = false;
                    m_objLog.LogMessage("Key Life Lapsed: " + strKeyLife, 30);
                }
            }

            m_objLog.LogMessage("EESpecificKeyClient: CheckLicenseDates(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckIdentifyingKey()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckIdentifyingKey(): ", 40);

            string strBaseIdentifyingKey = m_objRegistry.ProductGetKeyValue("BaseKey").Substring(0, 4);
            string strSpecificIdentifyingKey = SpecificKey.Substring(25, 4);
            if (strBaseIdentifyingKey != strSpecificIdentifyingKey)
            {
                blnReturn = false;
                m_objLog.LogMessage("Base Key Mismatch.  Requested: " + strBaseIdentifyingKey + ".  Today: " + strSpecificIdentifyingKey, 30);
            }

            m_objLog.LogMessage("EESpecificKeyClient: CheckIdentifyingKey(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckCompUUIDParts()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckCompUUIDParts(): ", 40);

            string strComputerUUID = m_objCM.ComputerUUID.Replace("-", "");
            // Check UUID section 1
            if (strComputerUUID.Substring(0, 6) != SpecificKey.Substring(59, 6))
            {
                blnReturn = false;
                m_objLog.LogMessage("UUID Mismatch.  Machine: " + strComputerUUID.Substring(0, 6) + ".  Key: " + SpecificKey.Substring(59, 6), 30);
            } 
            // Check UUID section 2
            else if (strComputerUUID.Substring(6, 6) != SpecificKey.Substring(0, 6))
            {
                blnReturn = false;
                m_objLog.LogMessage("UUID Mismatch.  Machine: " + strComputerUUID.Substring(6, 6) + ".  Key: " + SpecificKey.Substring(0, 6), 30);
            }
            // Check UUID section 3
            else if (strComputerUUID.Substring(12, 6) != SpecificKey.Substring(37, 6))
            {
                blnReturn = false;
                m_objLog.LogMessage("UUID Mismatch.  Machine: " + strComputerUUID.Substring(12, 6) + ".  Key: " + SpecificKey.Substring(37, 6), 30);
            }
            // Check UUID section 4
            else if (strComputerUUID.Substring(strComputerUUID.Length - 6, 6) != SpecificKey.Substring(14, 6))
            {
                blnReturn = false;
                m_objLog.LogMessage("UUID Mismatch.  Machine: " + strComputerUUID.Substring(strComputerUUID.Length - 6, 6) + ".  Key: " + SpecificKey.Substring(14, 6), 30);
            }

            m_objLog.LogMessage("EESpecificKeyClient: CheckCompUUIDParts(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckSpecificKeyDat()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): ", 40);

            bool blnForceCheck = false;

            // ##############################################################################################################
            // SpecificKeyDat format
            // (Last Check With Server)  (Last Call To Registry)     (Last Call Data)    (Last Call Data)    
            //     (FROM SERVER)                 (LOCAL)               (FROM SERVER)         (LOCAL)         
            //      (SEGMENT 1)                (SEGMENT 2)              (SEGEMNT 3)        (SEGMENT 4)
            //         XXXXX                      XXXXX                    XXXX             XXXX XXXX        
            //         
            // SEGMENT 1 (1-5)
            //   Is the first 5 characters of the response from the server, that doesn't equal "ERROR".
            //     It is the date of last communication with the server in YYDDD format, where the DDD is the day of the year.
            //
            // SEGMENT 2 (6-10)
            //   Is maintained locally.  It has the same format as SEGMENT 1, but is the last date this 
            //      SpecificKeyDat was processed.
            //
            // SEGMENT 3 (11-14)
            //   Is the Last Call Data returned by the server.
            //       Values from the server use the lower case alphabet ASCII values as the counter.
            //       So 97 (a) is 0. 122 (z) is 
            //       X   - Number of days failed communication with the server is allowed.
            //       X   - 
            //       X   - Max number of attempts per day to communicate with the server (and letter above 'a')
            //               This number should be 1-9 or 'b' - 'k'
            //       X   - Number of days between forced checks           
            //
            // SEGMENT 4 (15-18) (19-22)
            //   Is a collection of all locally stored data that can trigger a key check.  Of course, it can only
            //     trigger a key check if the Maximum number of checks (Set in SEGMENT 3) has not yet been reached.
            //     The Format:
            //       X   - A value between 0-9.  This is the number of random key matches.  At 10 this forces a 
            //               random key check
            //       X   -             
            //       X   - Number of tries between failed communications with the server. * 26
            //               This is the roll over from the next counter
            //               This value use the lower case alphabet ASCII values as the counter.
            //       X   - Number of tries between failed communications with the server.
            //               This value use the lower case alphabet ASCII values as the counter.
            //
            //       X   - Number of days of failed communication.
            //               This value use the lower case alphabet ASCII values as the counter.
            //       X   -
            //       XX  - The number of times the component has been instantiated.  Meaning, the number of times this
            //               this key has been checked.
            // ##############################################################################################################

            int intSKDLastSuccessfulDate = 0;
            int intSKDLastAttemptedDate = 0;
            int intSKDFailedCommAllowed = 0;
            int intSKDMaxDailyChecksAllowed = 0;
            int intSKDDaysBetweenChecks = 0;
            int intSKDDailyRandomMatches = 0;
            int intSKDAttemptedCommTries = 0;
            int intSKDDaysOfFailedComm = 0;
            int intSKDCountOfUsage = 0;

            string strSpecificKeyDat = m_objRegistry.ProductGetKeyValue("SpecificKeyDat");
            string strSpecificKeyDatMessage = m_objRegistry.ProductGetKeyValue("SpecificKeyDatMessage");
            int intPseudoJulianDate = (DateTime.Today.Year % 100) * 1000 + DateTime.Today.DayOfYear; 
            // Convert.ToInt32(Strings.Mid(DateAndTime.Year(DateAndTime.Now()).ToString(), 3) + DateAndTime.Now.DayOfYear.ToString().PadLeft(3, "0"));
            
            string strSpecificKeyDatResponse = null;

            if (strSpecificKeyDat.Length == 22)
            {
                // Process in the KeyDat for manipulation.                
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): strSpecificKeyDat: " + strSpecificKeyDat, 35);
                intSKDLastSuccessfulDate = Convert.ToInt32(strSpecificKeyDat.Substring(0, 5));
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDLastSuccessfulDate: " + intSKDLastSuccessfulDate, 35);
                intSKDLastAttemptedDate = Convert.ToInt32(strSpecificKeyDat.Substring(5, 5));
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDLastAttemptedDate: " + intSKDLastAttemptedDate, 35);
                intSKDFailedCommAllowed = strSpecificKeyDat[10] -  'a';
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDFailedCommAllowed: " + intSKDFailedCommAllowed, 35);
                intSKDMaxDailyChecksAllowed =strSpecificKeyDat[12] -  'a';
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDMaxDailyChecksAllowed: " + intSKDMaxDailyChecksAllowed, 35);
                intSKDDaysBetweenChecks = strSpecificKeyDat[13] -  'a';
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDDaysBetweenChecks: " + intSKDDaysBetweenChecks, 35);
                intSKDDailyRandomMatches = strSpecificKeyDat[14] - '0'; //right or wrong? CInt(Mid(strSpecificKeyDat, 15, 1)) LfZ 0227
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDDailyRandomMatches: " + intSKDDailyRandomMatches, 35);
                intSKDAttemptedCommTries = (strSpecificKeyDat[16] - 'a') * 26;
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDAttemptedCommTries: " + intSKDAttemptedCommTries, 35);
                intSKDAttemptedCommTries += strSpecificKeyDat[17] -  'a';
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDAttemptedCommTries: " + intSKDAttemptedCommTries, 35);
                intSKDDaysOfFailedComm = strSpecificKeyDat[18] -  'a';
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDDaysOfFailedComm: " + intSKDDaysOfFailedComm, 35);
                intSKDCountOfUsage = Convert.ToInt32(strSpecificKeyDat.Substring(20, 2));
                m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDCountOfUsage: " + intSKDCountOfUsage, 35);
            }
            else
            {
                // Setup the SpecificKeyDat as if it has never been called.  This way it will increment and save later as expected.
                intSKDLastSuccessfulDate = 0;
                intSKDLastAttemptedDate = intPseudoJulianDate;
                intSKDDailyRandomMatches = 0;
                intSKDAttemptedCommTries = 'a'; //?? *26? but actually "a" is zero
                intSKDAttemptedCommTries += 'a'; //? but a + a is 90ish! not zero!
                intSKDDaysOfFailedComm = 'a';
                intSKDCountOfUsage = 0;
            }

            m_objLog.LogMessage("Check Force. (strSpecificKeyDatMessage.Length > 0): " + (strSpecificKeyDatMessage.Length > 0) + " : " + strSpecificKeyDatMessage, 35);            
            blnForceCheck = (strSpecificKeyDatMessage.Length > 0);
            // Force if previously errored
            m_objLog.LogMessage("Check Force. ((intPseudoJulianDate - intSKDLastSuccessfulDate) > intSKDDaysBetweenChecks): "
                + ((intPseudoJulianDate - intSKDLastSuccessfulDate) > intSKDDaysBetweenChecks) + " : "
                + intPseudoJulianDate + " : " + intSKDLastSuccessfulDate + " : " + intSKDDaysBetweenChecks, 35);
            if (!(blnForceCheck))
                blnForceCheck = ((intPseudoJulianDate - intSKDLastSuccessfulDate) > intSKDDaysBetweenChecks);
            // Force if max days allowed reached
            m_objLog.LogMessage("Check Force. (intSKDAttemptedCommTries > 0): " + (intSKDAttemptedCommTries > 0) + " : " + intSKDAttemptedCommTries, 35);
            if (!(blnForceCheck))
                blnForceCheck = (intSKDAttemptedCommTries > 0);
            // Force if Comm tries have been failing.

            // Add force here for maximum amount nearing 20%

            bool blnRandomCheck = false;
            Random objRandom = new Random(System.DateTime.Now.Millisecond);
            char chrRandomChar = (char) objRandom.Next(65, 91);
            char chrRandomCharMatch = SpecificKey[9];
            // Use the last letter of the Product Hive as the match control
            if (!blnForceCheck)
                if ((chrRandomCharMatch == chrRandomChar))
                    blnRandomCheck = (intSKDDailyRandomMatches <= intSKDMaxDailyChecksAllowed);

            if (blnRandomCheck || blnForceCheck)
            {
                m_objLog.LogMessage("Checking Specific Key. " + chrRandomChar + " : " + chrRandomCharMatch + " - Force: " + blnForceCheck.ToString(), 30);

                try
                {
                    EEPaymentManager.EEKMWeb.EEKMWeb objService = GetWebServiceReference();
                    strSpecificKeyDatResponse = objService.CheckSpecificKey(SpecificKey);
                    intSKDAttemptedCommTries = 0;
                    intSKDDaysOfFailedComm = 0;
                }
                catch (Exception err)
                {
                    intSKDAttemptedCommTries += 1;
                    if ((intPseudoJulianDate > intSKDLastAttemptedDate))
                        intSKDDaysOfFailedComm += 1;

                    m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDAttemptedCommTries: " + intSKDAttemptedCommTries, 35);
                    m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): (intPseudoJulianDate > intSKDLastAttemptedDate) "
                        + (intPseudoJulianDate > intSKDLastAttemptedDate) + " - " + intPseudoJulianDate + " : " + intSKDLastAttemptedDate, 35);

                    strSpecificKeyDatMessage = "ERROR: Communication level error encountered: D" + intSKDDaysOfFailedComm + " : T" + intSKDAttemptedCommTries + " - " + err.Message;

                    m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): intSKDDaysOfFailedComm : intSKDFailedCommAllowed " + intSKDDaysOfFailedComm + " : " + intSKDFailedCommAllowed, 35);

                    if ((intSKDDaysOfFailedComm >= intSKDFailedCommAllowed) || (strSpecificKeyDat.Substring(0, 5) == "ERROR") || (strSpecificKeyDat.Length < 22))
                        strSpecificKeyDatResponse = strSpecificKeyDatMessage;
                    else
                        strSpecificKeyDatResponse = strSpecificKeyDat.Substring(0, 5) + strSpecificKeyDat.Substring(10, 4);
                }

                if (strSpecificKeyDatResponse.Substring(0, 5) == "ERROR")
                {
                    blnReturn = false;
                    m_objLog.LogMessage("Error checking specific key: " + strSpecificKeyDatResponse, 30);
                    strSpecificKeyDat = strSpecificKeyDatResponse;
                }
                else
                {
                    if ((intSKDAttemptedCommTries == 0))
                        SetCaptureTotal(0);
                    if ((intSKDAttemptedCommTries == 0))
                        m_objLog.LogMessage("SpecificKeyDat Success (With Comm Failure): " + strSpecificKeyDatResponse, 30);
                    else
                        m_objLog.LogMessage("SpecificKeyDat Success: " + strSpecificKeyDatResponse, 30);

                    // Temp measure during server upgrade.  If it is a classic basic response, then fake the funk to a more verbose response.
                    // This response allows for 5 days of failed communication, three attempted communications a day, 7 days between checks.
                    if (strSpecificKeyDatResponse.Length == 5)
                        strSpecificKeyDatResponse += "f" + Convert.ToString(chrRandomChar) + "dh";

                    strSpecificKeyDat = strSpecificKeyDatResponse.Substring(0, 5)
                     + Convert.ToString(intPseudoJulianDate)
                     + strSpecificKeyDatResponse.Substring(5, 4);
                }
                intSKDCountOfUsage = 0;
            }
            else
            {
                m_objLog.LogMessage("SpecificKeyDat Skipped: " + intPseudoJulianDate + " : " + strSpecificKeyDat.Substring(5, 5) + " # " + chrRandomChar + " : " + chrRandomCharMatch, 30);

                string strOldSpecificKeyDat = strSpecificKeyDat;
                strSpecificKeyDat = "";
                strSpecificKeyDat += strOldSpecificKeyDat.Substring(0, 5);
                strSpecificKeyDat += Convert.ToString(intPseudoJulianDate);
                strSpecificKeyDat += strOldSpecificKeyDat.Substring(10, 4);
            }

            if ((blnRandomCheck))
                strSpecificKeyDat += Convert.ToString(intSKDDailyRandomMatches + 1);
            else
                strSpecificKeyDat += Convert.ToString(intSKDDailyRandomMatches);
            strSpecificKeyDat += Convert.ToString(chrRandomChar)
                              + Convert.ToString((int)'a' + (intSKDAttemptedCommTries / 26))
                              + Convert.ToString((int)'a' + (intSKDAttemptedCommTries % 26))
                              + Convert.ToString((int)'a' + intSKDDaysOfFailedComm)
                              + Convert.ToString(chrRandomChar)
                              + Convert.ToString(intSKDCountOfUsage + 1).PadLeft(2, '0');

            m_objRegistry.ProductCreateValueEntry("SpecificKeyDat", strSpecificKeyDat);
            if ((!string.IsNullOrEmpty(strSpecificKeyDatMessage)))
                m_objRegistry.ProductCreateValueEntry("SpecificKeyDatMessage", strSpecificKeyDatMessage);

            m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyDat(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool FillSpecificKeyOptions()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: FillSpecificKeyOptions(): ", 40);

            byte bytCurrentByte = 0;
            bool StatusOfBit = false;

            
            int intSettingIndex = 0;

            for (int intSKIndex = 43; intSKIndex <= 54; intSKIndex += 1)
            {
                
                bytCurrentByte = (byte) m_objSpecificKey[intSKIndex];
                for (byte intBitIndex = 8; intBitIndex >= 1; intBitIndex-- )
                {
                    //bytCurrentByte = bytCurrentByte & 255; //
                    StatusOfBit = EEBitwise.ExamineBit(bytCurrentByte, intBitIndex);
                    m_objSpecificKeySettings.Insert(intSettingIndex, StatusOfBit);
                    ++intSettingIndex;
                }
            }

            m_objLog.LogMessage("EESpecificKeyClient: FillSpecificKeyOptions(): " + blnReturn, 40);

            return blnReturn;
        }

        private bool CheckSpecificKeyOption(int intBitCheck)
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyOption(): ", 40);

            blnReturn = m_objSpecificKeySettings[intBitCheck - 1];

            m_objLog.LogMessage("EESpecificKeyClient: CheckSpecificKeyOption(): " + blnReturn, 40);

            return blnReturn;
        }

        // Web Reference EEKMWeb (com.pcicharge..) 02212014 LfZ  
        private EEPaymentManager.EEKMWeb.EEKMWeb GetWebServiceReference(string strServiceURL = "")
        {

            m_objLog.LogMessage("EESpecificKeyClient: GetWebServiceReference(): " + strServiceURL, 40);

            // what should be the class? 02212014 LfZ
            EEPaymentManager.EEKMWeb.EEKMWeb objReturn = new EEPaymentManager.EEKMWeb.EEKMWeb();

            if ((string.IsNullOrEmpty(strServiceURL)))
                strServiceURL = m_objRegistry.ProductGetKeyValue("lclOverrideURL");
            else {
                objReturn.Url = strServiceURL + "/EEKMWeb.asmx";
                m_objLog.LogMessage("lclOverrideURL: " + strServiceURL + "/EEKMWeb.asmx", 30);
            }

            m_objLog.LogMessage("EESpecificKeyClient: GetWebServiceReference(): " + strServiceURL, 40);

            return objReturn;
        }


        private void AddToCaptureTotal(int intAmount)
        {
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("EESpecificKeyClient: AddToCaptureTotal(): " + intAmount, 125)
            // #End If

            UpgradeSpecificKey();

            int intTotal = GetCaptureTotal();
            intTotal += intAmount;
            SetCaptureTotal(intTotal);

            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("EESpecificKeyClient: AddToCaptureTotal(): " + intTotal, 125)
            // #End If

        }

        private int GetCaptureTotal()
        {
            // ##############################################################################################################
            // Capture Total format
            // Specific Key Layout
            // 
            // 5 places used.  1 spot is the roll over counter, so the other four are the current value.
            // This allows for values of up to 99,999 being stored in basic format, and 
            // 2.5 million being retained total.
            //
            // Value Markers
            // 00,000  X (Counter)
            // 12 345  6
            //
            // Note: In a future version this should read from an array.  This would allow for it to be changed based off
            //       The listed EEEM version
            //
            // XXXXXXXXXX 0000 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX 0000 XXX    
            //         (SEGMENT 1)                                                    (SEGMENT 2)
            //           (11-14)                                                        (72-75)
            //            XXXX                                                           XXXX
            //                     
            // SEGMENT 1
            //   X   -
            //   X   - Value from 3
            //   X   - Value From 1
            //   X   - Value from 6
            //           This is the roll over from the counter
            //           This value use the lower case alphabet ASCII values as the counter.
            //
            // SEGMENT 2
            //   X   - Value from 5
            //   X   -
            //   X   - Value from 4
            //   X   - Value from 2
            // 
            // ##############################################################################################################

            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("EESpecificKeyClient: GetCaptureTotal()", 125)
            // #End If

            int intReturn = 0;
            string strCurrentValue = "";
            string strSpecificString = SpecificKey;

            if ((strSpecificString.Length < 78))
                return intReturn;
            strCurrentValue += strSpecificString[12]
                            + strSpecificString[74]
                            + strSpecificString[11]
                            + strSpecificString[73]
                            + strSpecificString[71];
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("Mid(strSpecificString, 13, 1): " + Mid(strSpecificString, 13, 1), 120)
            // m_objLog.LogMessage(" Mid(strSpecificString, 75, 1): " + Mid(strSpecificString, 75, 1), 120)
            // m_objLog.LogMessage("Mid(strSpecificString, 12, 1): " + Mid(strSpecificString, 12, 1), 120)
            // m_objLog.LogMessage("Mid(strSpecificString, 74, 1): " + Mid(strSpecificString, 74, 1), 120)
            // m_objLog.LogMessage("Mid(strSpecificString, 72, 1): " + Mid(strSpecificString, 72, 1), 120)
            // #End If

            intReturn = Convert.ToInt32(strCurrentValue);
            intReturn += (int) (strSpecificString[13] -  'a') * 10000;

            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("EESpecificKeyClient: GetCaptureTotal(): " + intReturn, 125)
            // #End If

            return intReturn;
        }


        private void SetCaptureTotal(int intTotal)
        {
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("EESpecificKeyClient: SetCaptureTotal(): " + intTotal, 125)
            // #End If

            if ((intTotal > 2500000))
                intTotal = 2500000;

            char strMultiplier = (char) ('a' + intTotal / 100000);
            string strCurrentTotal = (Convert.ToString(intTotal % 100000)).PadLeft(5, '0');

            Random objRandom = new Random(System.DateTime.Now.Millisecond);
            char strRandomChar = (char) objRandom.Next(65, 91);

            SpecificKeyIndex(11, strRandomChar);
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("chrRandomChar: " + chrRandomChar, 120)
            // #End If
            SpecificKeyIndex(12, strCurrentTotal[2]);
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("Mid(strCurrentTotal, 3, 1): " + Mid(strCurrentTotal, 3, 1), 120)
            // #End If
            SpecificKeyIndex(13, strCurrentTotal[0]);
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("Mid(strCurrentTotal, 1, 1): " + Mid(strCurrentTotal, 1, 1), 120)
            // #End If
            SpecificKeyIndex(14, strMultiplier);
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("strMultiplier: " + strMultiplier, 120)
            // #End If
            SpecificKeyIndex(72, strCurrentTotal[4]);
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("Mid(strCurrentTotal, 5, 1): " + Mid(strCurrentTotal, 5, 1), 120)
            // #End If
            SpecificKeyIndex(73, strRandomChar);
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("chrRandomChar: " + chrRandomChar, 120)
            // #End If
            SpecificKeyIndex(74, strCurrentTotal[3]);
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("Mid(strCurrentTotal, 4, 1): " + Mid(strCurrentTotal, 4, 1), 120)
            // #End If
            SpecificKeyIndex(75, strCurrentTotal[1], true);
            // #If BLN_CLIENT_BUILD = 0 Then
            // m_objLog.LogMessage("Mid(strCurrentTotal, 2, 1): " + Mid(strCurrentTotal, 2, 1), 120)
            // m_objLog.LogMessage("EESpecificKeyClient: SetCaptureTotal(): " + SpecificKey(), 125)
            // #End If
        }


        private void UpgradeSpecificKey()
        {
            m_objLog.LogMessage("EESpecificKeyClient: UpgradeSpecificKey(): " + SpecificKey, 40);

            string strSpecificString = SpecificKey;
            if ((strSpecificString.Length < 78))
                return;
            if ( (strSpecificString.Substring(10,4) != "XXXX") && (strSpecificString.Substring(71,4)!= "XXXX") )
                return;
            SetCaptureTotal(0);

            m_objLog.LogMessage("EESpecificKeyClient: UpgradeSpecificKey(): " + SpecificKey, 40);

        }

        // ###################################################################################
        // ###################################################################################
        // Property Functions
        // ###################################################################################
        // ###################################################################################
        public string Root
        {
            get { return m_strProductRoot; }
            set { m_strProductRoot = value; }
        }

        public string ProductHive
        {
            get { return m_strProductHive; }
            set { m_strProductHive = value; }
        }

        public string ProductVersion
        {
            get { return m_strVersion; } 
                // was m_strVersion.Replace(".", "");  which is redundant since . is already replaced in Setter
            set { m_strVersion = value.Replace(".", ""); }
        }

        public string Instance
        {
            get { return m_strInstance; }
            set { m_strInstance = value; }
        }

        public string SpecificKeyRequest
        {
            get
            {                
                System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
                foreach (char chrCurrent in m_objRequestKey)
                    strBuilder.Append(chrCurrent);
                return strBuilder.ToString();
            }
        }

        //Public Property SpecificKeyConfirmation() As String
        //    Get
        //        Dim strReturn As String = ""
        //        Dim strElement As String
        //        For Each strElement In m_objConfirmationKey
        //            strReturn = strReturn + strElement
        //        Next
        //        Return strReturn
        //    End Get
        //    Set(ByVal value As String)
        //        Dim intCounter As Integer = 0
        //        For intCounter = 0 To value.Length - 1
        //            m_objConfirmationKey.Insert(intCounter, Mid(value, intCounter + 1, 1))
        //        Next
        //        'm_objRegistry
        //    End Set
        //End Property

        public string SpecificKey
        {
            get
            {
                System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
                foreach (char chrCurrent in m_objSpecificKey)
                    strBuilder.Append(chrCurrent);
                return strBuilder.ToString();
            }
            set
            {
                int intCounter = 0;
                for (intCounter = 0; intCounter <= value.Length - 1; intCounter++)
                {
                    m_objSpecificKey.Insert(intCounter, value[intCounter]);
                }
                m_objRegistry.ProductCreateValueEntry("SpecificKey", value);
            }
        }

        public string SpecificKeyIndex(int intIndex, char chr, bool blnSaveKey = false)
        {
            if ((chr == 0)) // '\0' or 0
                return "";
            m_objSpecificKey[intIndex - 1] = chr;
            string strSpecificKey = SpecificKey;
            if ((blnSaveKey))
                m_objRegistry.ProductCreateValueEntry("SpecificKey", strSpecificKey);
            return strSpecificKey;
        }

        public string SpecificKeyEEEMVersion
        {
            get { return m_strSpecificKeyEEEMVersion; }
            set { m_strSpecificKeyEEEMVersion = value; }
        }

        public bool SpecificKeySetting(int intSetting)
        {
            return m_objSpecificKeySettings[intSetting];
        }

        // ###################################################################################
        // ###################################################################################
        // Member Variables
        // ###################################################################################
        // ###################################################################################

        protected string m_strProductRoot;
        protected string m_strVersion;
        protected string m_strProductHive;

        protected string m_strInstance;
        // Length 54
        protected List<char> m_objRequestKey;
        // Length XX
        // LfZ 0226: not used. protected List<char> m_objConfirmationKey;

        // Length 78
        protected List<char> m_objSpecificKey;
        // Length 96 (84 Useable) (12 Characters, 8 bits a character.  8th bit is never used in any octet.)
        // Following positions have no meaning as the 8th bits: 7,15,23,31,39,47,55,63,71,79,87,95
        // If the 7th bit is used (6,14,22,30,38,46,54,62,70,78,86,94) then not every other bit can be set.
        //   This is because the maximum decimal number can be 126.  So any given byte's maximum bit set
        //   can look like this: 0111 1110.
        // If the 7th bit is not set, then the 6th bit (5,13,22,29,37,45,53,61,69,77,85,93) must be set.
        //   This is because the minimum decimal number can be 33.  So and given byte's minimum bit set
        //   can look like this: 0010 0001.
        protected List<bool> m_objSpecificKeySettings;

        protected string m_strSpecificKeyEEEMVersion;
        protected Enterprise.EELog m_objLog;
        protected Enterprise.EERegistry m_objRegistry;

        protected Enterprise.EEComputerManagement m_objCM;
    }

}
