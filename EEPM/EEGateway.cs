using System;

using System.Runtime.InteropServices;

namespace EEPM
{

    [ComVisible(true), ClassInterface(ClassInterfaceType.None), Guid("E21B4036-2B47-437A-BA7C-4FB73000F847")] //Guid("D3503563-C96D-4bf5-8C6F-AF267A67DD9E")] 
    public partial class EEGateway : Enterprise.EEBase, IEEGateway
    {
        
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Constructors\Destructors
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // "Constructors\Destructors"

        public EEGateway()
        {
            m_intGatewayID = -1;
            m_strGatewayURL = "";
            m_strMerchantLogin = "";
            m_strMerchantPassword = "";
            m_intPaymentType = -1;
            m_intResponseCode = -1;
            m_strResponseDescription = "";
            m_objProperties = new System.Collections.Generic.Dictionary<string, string>();
            ///''what is this? Lingfei
            //'debuging 1231 this is path of registry. LfZ
            SetupBase("PCICharge", "EEPM", 2);
            m_objEEPG = null;

            m_strGatewaySecurityProfile = "";
            //'12/4/13 added Lingfei
        }

        //protected override void Finalize()
        ~EEGateway()
        {
            m_intGatewayID = 0;
            m_strGatewayURL = "";
            m_strMerchantLogin = "";
            m_strMerchantPassword = "";
            m_intPaymentType = 0;
            m_intResponseCode = 0;
            m_strResponseDescription = "";
            m_objProperties = null;
            m_objEEPG = null;

            m_strGatewaySecurityProfile = "";
            //'12/4/13 added Lingfei
        }

        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Public Functions
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // "PublicFunctions"

        public virtual bool Authorize()
        {

            m_objLog.LogMessage("EEGateway: Authorize()", 40);

            //'!!!LZ debuging 1231 replaced with next line 
            if (!(CheckInternalLicenses(4)))
                return false;

            if (!(CheckReady()))
                return false;
            if (!(DetermineGatewayObject()))
                return false;
            return m_objEEPG.Authorize(ref m_objProperties);

        }

        //' 12/4/2013 adding Tokenize() -- tokenize without sale (i.e.adding card and storing as a token) 
        //'2/14/2014  change interface of token in line with encrypt
        public virtual string Tokenize(string strPlainText)
        {
            // return the token uid string given by gateway
            m_objLog.LogMessage("EEGateway: Tokenize()", 40);

            // If Not (CheckInternalLicenses(123) Then Return False   '''' licensing
            if (!CheckReady())
                return "ErrorNotReady";
            if (!DetermineGatewayObject())
                return "ErrorGatewayObject";
            return m_objEEPG.Tokenize(strPlainText, ref m_objProperties);

        }



        public virtual bool Capture()
        {

            m_objLog.LogMessage("EEGateway: Capture()", 40);

            if (!CheckInternalLicenses(14, Convert.ToInt32(Math.Round(Convert.ToDecimal(GetNameValue("TRANSACTIONAMOUNT")),MidpointRounding.ToEven))))
                return false;
            if (!CheckReady())
                return false;
            if (!DetermineGatewayObject())
                return false;
            return m_objEEPG.Capture(ref m_objProperties);

        }

        public virtual bool DirectSale()
        {

            m_objLog.LogMessage("EEGateway: DirectSale()", 40);

            //using banker's rounding, since it is to adding up the captured total.
            if (!CheckInternalLicenses(52, Convert.ToInt32(Math.Round(Convert.ToDecimal(GetNameValue("TRANSACTIONAMOUNT")),MidpointRounding.ToEven))))
                return false;
            if (!CheckReady())
                return false;
            if (!DetermineGatewayObject())
                return false;
            return m_objEEPG.DirectSale(ref m_objProperties);

        }

        public virtual bool Credit()
        {

            m_objLog.LogMessage("EEGateway: Credit()", 40);

            if (!CheckInternalLicenses(74))
                return false;
            if (!CheckReady())
                return false;
            if (!DetermineGatewayObject())
                return false;
            return m_objEEPG.Credit(ref m_objProperties);

        }

        public virtual bool Void()
        {

            m_objLog.LogMessage("EEGateway: Void()", 40);

            if (!CheckInternalLicenses(83))
                return false;
            if (!CheckReady())
                return false;
            if (!DetermineGatewayObject())
                return false;
            return m_objEEPG.VoidTransaction(ref m_objProperties);

        }


        public void AddNameValue(string strName, string strValue)
        {
            m_objLog.LogMessage("EEGateway: AddNameValue(): " + strName + "-" + strValue, 100);

            if ((strName == "!#INSTANCE"))
            {
                m_strProductInstance = strValue;
            }
            else
            {
                if ((m_objProperties.ContainsKey(strName)))
                    m_objProperties.Remove(strName);
                m_objProperties.Add(strName, strValue);
            }
        }

        public string GetNameValue(string strName)
        {
            m_objLog.LogMessage("EEGateway: GetNameValue(): " + strName, 100);
            if ((m_objProperties.ContainsKey(strName)))
                return m_objProperties[strName];
            else
                return "";
        }

        public void ClearNameValue()
        {
            m_objLog.LogMessage("EEGateway: ClearNameValue()", 40);
            m_objProperties.Clear();
        }



        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Protected\Private Functions
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // "Protected\PrivateFunctions"

        // ###################################################################################################################################################################
        //   Protected Functions
        // ###################################################################################################################################################################
        protected virtual bool CheckReady()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EEGateway: CheckReady()", 40);

            // Check if needed information has been setup to process the request
            ///''''''12/3/2013 adding strSecurityProfile check
            if ((m_intGatewayID == -1) || (m_intPaymentType == -1) || ((m_strGatewaySecurityProfile != "Encrpytion") && (m_strGatewaySecurityProfile != "Tokens")))
            {
                m_intResponseCode = 98000;
                m_strResponseDescription = "Needed variable(s) not set: ";
                if (m_intGatewayID == -1)
                    m_strResponseDescription += "Gateway ID";
                if (m_intPaymentType == -1)
                    m_strResponseDescription += "Payment Type";
                if (string.IsNullOrEmpty(m_strGatewaySecurityProfile))
                {
                    m_strResponseDescription += "GatewaySecurityProfile";
                }
                //depricated: actually will be checked by the DeterminGatewayObject 3-level Swith Cases
                //If ((m_strGatewaySecurityProfile <> "Encrpytion") And (m_strGatewaySecurityProfile <> "Tokens")) Then
                //    m_strResponseDescription &= "SecurityType (not Encrpytion or Tokens)"
                //End IFF
                blnReturn = false;
            }

            m_objLog.LogMessage("EEGateway: CheckReady(): " + blnReturn, 40);

            return blnReturn;
        }
        /// nsoftware.InPay
        //    public enum IchargeGateways
        //    { gwNoGateway = 0,  gwAuthorizeNet = 1, gwEprocessing = 2, gwIntellipay = 3, gwITransact = 4, gwNetBilling = 5, gwPayFlowPro = 6, gwUSAePay = 7, gwPlugNPay = 8, gwPlanetPayment = 9, gwMPCS = 10,
        //      gwRTWare = 11, gwECX = 12, gwBankOfAmerica = 13, gwInnovative = 14, gwMerchantAnywhere = 15, gwSkipjack = 16, gwIntuitPaymentSolutions = 17, gw3DSI = 18, gwTrustCommerce = 19, gwPSIGate = 20, 
        //      gwPayFuse = 21, gwPayFlowLink = 22, gwOrbital = 23, gwLinkPoint = 24, gwMoneris = 25, gwUSight = 26, gwFastTransact = 27, gwNetworkMerchants = 28, gwOgone = 29, gwPRIGate = 30, gwMerchantPartners = 31, 
        //      gwCyberCash = 32, gwFirstData = 33, gwYourPay = 34, gwACHPayments = 35, gwPaymentsGateway = 36, gwCyberSource = 37, gwEway = 38, gwGoEMerchant = 39, gwTransFirst = 40, gwChase = 41, gwNexCommerce = 42,
        //      gwWorldPay = 43, gwTransactionCentral = 44, gwSterling = 45, gwPayJunction = 46, gwSECPay = 47, gwPaymentExpress = 48, gwMyVirtualMerchant = 49, gwSagePayments = 50, gwSecurePay = 51, gwMonerisUSA = 52,
        //      gwBeanstream = 53, gwVerifi = 54, gwSagePay = 55, gwMerchantESolutions = 56, gwPayLeap = 57, gwPayPoint = 58, gwWorldPayXML = 59, gwProPay = 60, gwQBMS = 61, gwHeartland = 62, gwLitle = 63, 
        //      gwBrainTree = 64,  gwJetPay = 65, gwHSBC = 66, gwBluePay = 67, gwAdyen = 68, gwBarclay = 69, gwPayTrace = 70, gwYKC = 71, gwCyberbit = 72, gwGoToBilling = 73, gwTransNationalBankcard = 74, gwNetbanx = 75, 
        //      gwMIT = 76, gwDataCash = 77, gwACHFederal = 78, gwGlobalIris = 79, gwFirstDataE4 = 80, gwFirstAtlantic = 81, gwBluefin = 82, gwPayscape = 83, gwPayDirect = 84, 
        // nsoftware.InPay.IchargeGateways.gwPayFlowPro = 6
        protected virtual bool DetermineGatewayObject()
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EEGateway: DetermineDatewayObject: " + m_intGatewayID + "-" + m_intPaymentType, 40);

            switch (m_intGatewayID)
            {
                case 0:
                    errorGatewayUnsupported(m_intGatewayID);
                    blnReturn = false;
                    break;
                case 10:  
                    // PayFlow Pro
                    switch (m_intPaymentType)
                    {
                        case 0:
                            m_objEEPG = new EEPGGWCCPayFlowPro(Convert.ToInt32(nsoftware.InPay.IchargeGateways.gwPayFlowPro), m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }    
                    break;
                case 29:
                    // Orbital Gateway  '02182014 LfZ number now is 23! Paymentech Orbital Gateway V5.6 (23). 29 is ogone as of nsoft V6.0.0.5xxx
                    switch (m_intPaymentType)
                    {
                        case 0:
                            m_objEEPG = new EEPMGWCCOrbital(Convert.ToInt32(nsoftware.InPay.IchargeGateways.gwOrbital), m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
                case 38:
                    // EWay Gateway
                    switch (m_intPaymentType)
                    {
                        case 0:
                            m_objEEPG = new EEPMGWCCEWay(Convert.ToInt32(nsoftware.InPay.IchargeGateways.gwEway), m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
                case 42:
                    // First Data Global Gateway '02102014 LfZ the number in nsoftware is 33!! Void is not functioning reported by Cat. Maybe use 207 FD E4.
                    switch (m_intPaymentType)
                    {
                        case 0:
                            m_objEEPG = new EEPMGWCCFDGlobal(Convert.ToInt32(nsoftware.InPay.IchargeGateways.gwFirstData), m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
                case 46:
                    // Cybersource
                    switch (m_intPaymentType)
                    {
                        case 0:
                            m_objEEPG = new EEPMGWCCCyberSource(Convert.ToInt32(nsoftware.InPay.IchargeGateways.gwCyberSource), m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
                case 69:
                    // Barclays ePDQ
                    switch (m_intPaymentType)
                    {
                        case 0:
                            m_objEEPG = new EEPMGWCCBarclayEDPQ(Convert.ToInt32(nsoftware.InPay.IchargeGateways.gwBarclay), m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
                case 100:
                    // PayPal Direct Payments
                    switch (m_intPaymentType)
                    {
                        case 0:
                            m_objEEPG = new EEPMGWCCPayPal(m_intGatewayID, m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                            break;
                        //Case 1
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
                case 101:
                    // Chase PaymentTech Direct Payment using Ptech.dll, depricated. 02212014. NSfotware's new integrator is called Direct integrator V6 
                    switch (m_intPaymentType)
                    {   
                        case 0:
                            errorGatewayUnsupported(m_intGatewayID); //added 02282014 LfZ
                        // commented  02212014 LfZ
                        //    m_objEEPG = new EEPMGWCCPaymentTech(m_intGatewayID, m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, m_objLog);
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
                case 205:
                    // Ogone
                    switch (m_intPaymentType)
                    {
                        // commented as the override base is not well consistant with C# strong type class stuff. Ogone is also supported so I need to develop a nSoftware way too.
                        ////TODO TODO 02212014 LfZ
                        //case 0:
                        //    m_objEEPG = new EEPGGWCCOgone(m_intGatewayID, m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                        //    break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
                ///'Case 67 SagePay with/out Tokenization  12/4/2013 adding  sagepay pathway  tokenization Case 
                ///'' SagePay with/out Tokenization  12/4/2013 adding  sagepay pathway  tokenization Case
                //' should be 55 01022014 LfZ
                case 206:
                    // SagePay with or without Tokenization
                    switch (m_intPaymentType)
                    {
                        case 0:
                            switch (m_strGatewaySecurityProfile)
                            {
                                case "Tokens":
                                    ///' TODO test
                                    m_objEEPG = new EEPMGWCCSagePayToken(Convert.ToInt32(nsoftware.InPay.IchargeGateways.gwSagePay), m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                                    break;
                                case "Encrpytion":
                                    ///''TODO test
                                    m_objEEPG = new EEPMGWCCSagePay(Convert.ToInt32(nsoftware.InPay.IchargeGateways.gwSagePay), m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                                    break;
                                default:
                                    errorSecurityProfileUnsupported(m_intGatewayID);
                                    blnReturn = false;
                                    break;
                            }
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }                   
                    break;
                case 207:
                    // nsoftware.InPay.IchargeGateways.gwFirstDataE4 '(80) added  02102014 LfZ
                    switch (m_intPaymentType)
                    {
                        case 0:
                            m_objEEPG = new EEPMGWCCFDE4(Convert.ToInt32(nsoftware.InPay.IchargeGateways.gwFirstDataE4), m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
                default:
                    switch (m_intPaymentType)
                    {
                        case 0:
                            m_objEEPG = new EEPMGWCCGenericBase(m_intGatewayID, m_strGatewayURL, m_strMerchantLogin, m_strMerchantPassword, ref m_objLog);
                            break;
                        default:
                            errorPaymentTypeUnsupported(m_intPaymentType);
                            blnReturn = false;
                            break;
                    }
                    break;
            }

            m_objLog.LogMessage("EEGateway: DetermineDatewayObject: " + blnReturn, 40);

            return blnReturn;
        }

        // ###################################################################################################################################################################
        //   Private Functions
        // ###################################################################################################################################################################

        private void errorGatewayUnsupported(int intGateway)
        {
            m_objLog.LogMessage("EEGateway: errorGatewayUnsupported: " + intGateway, 40);
            try
            {
                m_intResponseCode = 98001;
                m_strResponseDescription = "Gateway " + Convert.ToString(intGateway) + " not supported";
            }
            catch
            {
            }
        }

        private void errorPaymentTypeUnsupported(int intPaymentType)
        {
            m_objLog.LogMessage("EEGateway: errorPaymentTypeUnsupported: " + intPaymentType, 40);
            try
            {
                m_intResponseCode = 98002;
                m_strResponseDescription = "Payment type " + Convert.ToString(intPaymentType) + " not supported";
            }
            catch
            {
            }
        }

        ///' 12/04/2013 adding Tokeization for Sagepay and future possible pathway support
        private void errorSecurityProfileUnsupported(int strGatewaySecurityProfile)
        {
            m_objLog.LogMessage("EEGateway: errorSecurityProfileUnsupported: " + m_strGatewaySecurityProfile, 40);
            try
            {
                m_intResponseCode = 98522;
                //new error code, but better use 980xx for consistency with those from other functions in this class.
                m_strResponseDescription = "Gateway Security Profile type " + strGatewaySecurityProfile + " not supported";
            }
            catch
            {
            }
        }


        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Member Variables\Functions
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // "MemberVariables\Properties"

        // ###################################################################################################################################################################
        //   Property Functions
        // ###################################################################################################################################################################
        public int GatewayID
        {
            get { return m_intGatewayID; }
            set { m_intGatewayID = value; }
        }

        public int PaymentType
        {
            get { return m_intPaymentType; }
            set { m_intPaymentType = value; }
        }

        public string GatewayURL
        {
            get { return m_strGatewayURL; }
            set { m_strGatewayURL = value; }
        }

        public string GatewayLogin
        {
            get { return m_strMerchantLogin; }
            set { m_strMerchantLogin = value; }
        }

        public string GatewayPassword
        {
            get { return m_strMerchantPassword; }
            set { m_strMerchantPassword = value; }
        }

        public override int ResponseCode
        {
            get
            {
                if ((m_objEEPG == null))
                {
                    return m_intResponseCode;
                }
                else
                {
                    return m_objEEPG.ResponseCode;
                }
            }
        }

        public override string ResponseDescription
        {
            get {
				string strResponse = "";
				if ((m_objEEPG == null)) {
					strResponse = m_strResponseDescription.Replace("nsoftware", "PCICharge");
				} else {
                    strResponse = m_objEEPG.ResponseDescription.Replace("nsoftware", "PCICharge");
				}
				return strResponse;
			}
        }

        ///''''' 12/3/2013 adding property for SecurityProfile ( either Tokens or Encrpytion)
        public string GatewaySecurityProfile
        {
            get { return m_strGatewaySecurityProfile; }
            set { m_strGatewaySecurityProfile = value; }
        }

        // ###################################################################################################################################################################
        //   Private Member Variables
        // ###################################################################################################################################################################
        // Gateway object.
        protected EEPMGWBase m_objEEPG;

        // Gateway ID used to know what gateway to create
        protected int m_intGatewayID;
        // Payment Type is in terms of 1=CreditCard, 2=ECheck, etc.... and 2 E-Check is never implemented said Mike 12/3/2013. Lingfei
        protected int m_intPaymentType;
        // Gateway URL
        protected string m_strGatewayURL;
        // Gateway Login
        protected string m_strMerchantLogin;
        // Gateway Password
        protected string m_strMerchantPassword;

        ///'''''''''12/3  adding  member securityprofile for tokenization/encrytion(usual) switch '''''''''''
        // either "Encrpytion" or "Tokens"
        protected string m_strGatewaySecurityProfile;
        //Protected m_strSecurityProfile As String ' Security Profile ('Encryption', 'Tokens')


        protected System.Collections.Generic.Dictionary<string, string> m_objProperties;
        protected const int m_intPaymentTypeCC = 0;

        protected const int m_intPaymentTypeECheck = 0;




    }

}
