using System;
using System.Runtime.InteropServices;

namespace EEPM
{

	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class EEPMGWBase
	{

		// ###################################################################################
		// Constructors/Destructors
		// ###################################################################################
		public EEPMGWBase(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog)
		{
			m_intGatewayID = intGatewayID;
			m_strGatewayResponseCode = "";
			m_strGatewayResponseDescription = "";
			m_intEEPGResponseCode = -1;
			m_strEEPGResponseDescription = "";
			m_strGatewayURL = strGatewayURL;
			m_strMerchantLogin = strMerchantLogin;
			m_strMerchantPassword = strMerchantPassword;
			m_objLog = objLog;

			m_objSetGW = null;

			m_objNSoftwareGW = null;
			m_objNSoftwareCard = null;
			m_objNSoftwareCustomer = null;

			BuildSetGW();
			BuildScrubList();

		}

		//protected override void Finalize()
        ~EEPMGWBase()
		{
			m_intGatewayID = 0;
			m_strGatewayResponseCode = "";
			m_strGatewayResponseDescription = "";
			m_intEEPGResponseCode = 0;
			m_strEEPGResponseDescription = "";

			m_objSetGW = null;
			m_objScrubList = null;
			m_objScrubData = null;

			m_objNSoftwareGW = null;
			m_objNSoftwareCard = null;
			m_objNSoftwareCustomer = null;

		}

		// ###################################################################################
		// Public functions
		// ###################################################################################
		public virtual bool Authorize(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: Authorize()", 40);
			m_intEEPGResponseCode = 98009;
			m_strEEPGResponseDescription = "Required function Authorize not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: Authorize(): " + blnReturn, 40);
			return blnReturn;
		}

		///' 12/10 Commented: maybe I do not need to touch this, instead just do EEPMOverrideBase.vb and add Tokenize() there!
		///' 12/4/2013 adding Tokenize generic function -- tokenize without sale (i.e.adding card and storing as a token) 
		///' TODO ? As String 
		public virtual string Tokenize(string strPlainText, ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			m_objLog.LogMessage("EEPMGWBase: Tokenize()", 40);
			m_intEEPGResponseCode = 98500;
			//' new error code starting from 98500 -- 01/07/2014 LfZ
			m_strEEPGResponseDescription = "Required function Tokenize() not Overridden";
			m_objLog.LogMessage("EEPMGWBase: Tokenize(): ", 40);
			return "";
			//Throw New NotImplementedException
		}

		public virtual bool Capture(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: Capture()", 40);
			m_intEEPGResponseCode = 98010;
			m_strEEPGResponseDescription = "Required function Capture not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: Capture(): " + blnReturn, 40);
			return blnReturn;
		}

		public virtual bool DirectSale(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: DirectSale()", 40);
			m_intEEPGResponseCode = 98011;
			m_strEEPGResponseDescription = "Required function DirectSale not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: DirectSale(): " + blnReturn, 40);
			return blnReturn;
		}

		public virtual bool Credit(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: Credit()", 40);
			m_intEEPGResponseCode = 98012;
			m_strEEPGResponseDescription = "Required function Credit not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: Credit(): " + blnReturn, 40);
			return blnReturn;
		}

		public virtual bool VoidTransaction(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: VoidTransaction()", 40);
			m_intEEPGResponseCode = 98019;
			m_strEEPGResponseDescription = "Required function VoidTransaction not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: VoidTransaction(): " + blnReturn, 40);
			return blnReturn;
		}

		// ###################################################################################
		// Public property functions
		// ###################################################################################
		public int GatewayID {
			get { return m_intGatewayID; }
		}

		public int ResponseCode {
			get { return m_intEEPGResponseCode; }
		}

		public string ResponseDescription {
			get { return m_strEEPGResponseDescription; }
		}

		// ###################################################################################
		// Protected functions
		// ###################################################################################
		protected virtual bool SetGatewayCredentials(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: SetGatewayCredentials()", 40);
			m_intEEPGResponseCode = 98006;
			m_strEEPGResponseDescription = "Required function SetGatewayCredentials not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: SetGatewayCredentials(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual bool PrepareGatewayMessage(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: PrepareGatewayMessage()", 40);
			m_intEEPGResponseCode = 98007;
			m_strEEPGResponseDescription = "Required function PrepareGatewayMessage not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: PrepareGatewayMessage(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual bool ReadGatewayResponse(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: ReadGatewayResponse()", 40);
			m_intEEPGResponseCode = 98008;
			m_strEEPGResponseDescription = "Required function ReadGatewayResponse not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: ReadGatewayResponse(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual bool GatewaySpecificMessageSetup(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificMessageSetup(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual bool GatewaySpecificAuthorize(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificAuthorize()", 40);
			m_intEEPGResponseCode = 98020;
			m_strEEPGResponseDescription = "Optional function GatewaySpecificAuthorize not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificAuthorize(): " + blnReturn, 40);
			return blnReturn;
		}

		///' added  12/23   template for gw-specific tokenize ( called if it is override for spec gw, otherwize execute generic tokenize as in EEPMGWGenericBase.vb)
		protected virtual string GatewaySpecificTokenize(string strPlainText, ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificTokenize()", 40);
			m_intEEPGResponseCode = 98501;
			m_strEEPGResponseDescription = "Optional function GatewaySpecificTokenize not Overridden";
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificTokenize(): NULL", 40);
			return "";
		}

		protected virtual bool GatewaySpecificCapture(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificCapture()", 40);
			m_intEEPGResponseCode = 98021;
			m_strEEPGResponseDescription = "Optional function GatewaySpecificCapture not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificCapture(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual bool GatewaySpecificDirectSale(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificDirectSale()", 40);
			m_intEEPGResponseCode = 98022;
			m_strEEPGResponseDescription = "Optional function GatewaySpecificDirectSale not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificDirectSale(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual bool GatewaySpecificCredit(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificCredit()", 40);
			m_intEEPGResponseCode = 98023;
			m_strEEPGResponseDescription = "Optional function GatewaySpecificCredit not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificCredit(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual bool GatewaySpecificVoidTransaction(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificVoidTransaction()", 40);
			m_intEEPGResponseCode = 98024;
			m_strEEPGResponseDescription = "Optional function GatewaySpecificVoidTransaction not Overridden";
			blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: GatewaySpecificVoidTransaction(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual bool SetSpecialFields(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: SetSpecialFields(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual bool ContainKeyCheck(ref System.Collections.Generic.Dictionary<string, string> objProperties, string strKey)
		{
			bool blnReturn = false;
			m_objLog.LogMessage("EEPMGWBase: ContainKeyCheck(): " + strKey, 40);
			if (objProperties.ContainsKey(strKey))                 
				if (String.IsNullOrEmpty(objProperties[strKey])) {
					objProperties.Remove(strKey);
					blnReturn = false;
				} else
                    blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: ContainKeyCheck(): " + blnReturn, 40);
			return blnReturn;
		}

		// Read the key value out and REMOVE it from objProperties Dictionary. LZ 122713
		protected virtual string ProcessKey(ref System.Collections.Generic.Dictionary<string, string> objProperties, string strKey)
		{
			string strReturn = "";
			m_objLog.LogMessage("EEPMGWBase: ProcessKey(): " + strKey, 40);
			if (objProperties.ContainsKey(strKey)) {
				if (!string.IsNullOrEmpty(objProperties[strKey]))
					strReturn = objProperties[strKey];
				objProperties.Remove(strKey);
			}
			m_objLog.LogMessage("EEPMGWBase: ProcessKey(): " + strReturn, 100);
			return strReturn;
		}

		protected virtual bool SetGWObject(string strCase)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWBase: SetGWObject(): " + blnReturn, 40);
			return blnReturn;
		}

		protected virtual void BuildSetGW()
		{
			m_objLog.LogMessage("EEPMGWBase: BuildSetGW()", 40);
			m_objSetGW = new System.Collections.Generic.Dictionary<string, string>();
			m_objSetGW.Add("AUTHORIZE", "");
			m_objSetGW.Add("CAPTURE", "");
			m_objSetGW.Add("DIRECT", "");
			m_objSetGW.Add("CREDIT", "");
			m_objSetGW.Add("VOID", "");
		}

		//format amout as 12345.67 no comma(,) for thousand groups
        // 02202014 cahnged line 5, and first if. LfZ
		protected virtual string FormatAmount(string strAmount)
		{
			string strReturn = strAmount;
			m_objLog.LogMessage("EEPMGWBase: FormatAmount()" + strReturn, 40);
			strReturn = strReturn.Replace(" ", "");
			strReturn = strReturn.Replace( ",", "");
            strReturn = Convert.ToString(Math.Round(Convert.ToDecimal(strReturn), 2, MidpointRounding.AwayFromZero));

            if ((strReturn.IndexOf(".") == (strReturn.Length - 2)) && (strReturn.Length != 0))
            {
				strReturn = strReturn + "0";
			} else {
                if (strReturn.IndexOf(".") < 0)
					strReturn = strReturn + ".00";
			}

			m_objLog.LogMessage("EEPMGWBase: FormatAmount()" + strReturn, 40);
			return strReturn;
		}

		// This function is used to scrub log messages that could potentially have a credit card number.
		protected string ScrubForLog(string strMessage)
		{
			string strValue = "";

			foreach (string strValue_loopVariable in m_objScrubData) {
				strValue = strValue_loopVariable;
				// Replace key values set in the BuildScrubList function
				strMessage = strMessage.Replace(strValue, "xx-replaced-xx");
			}
			// Always replace password
            if (!string.IsNullOrEmpty(m_strMerchantPassword))
			    strMessage = strMessage.Replace(m_strMerchantPassword, "xx-replaced-xx");
			return strMessage;
		}

		protected void AddToScrub(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			string strKey = "";

			foreach (string strKey_loopVariable in m_objScrubList) {
				strKey = strKey_loopVariable;
				if ((ContainKeyCheck(ref objProperties, strKey))) {
					if (!(m_objScrubData.Contains(objProperties[strKey]))) {
						m_objScrubData.Add(objProperties[strKey]);
					}
				}
			}
		}

		protected void BuildScrubList()
		{
			m_objLog.LogMessage("EEPMGWBase: BuildScrubList()", 40);
			m_objScrubList = new System.Collections.Generic.List<string>();
			m_objScrubList.Add("CCNUMBER");
			m_objScrubList.Add("EXP_PASSWORD_EXT_1");
			m_objScrubList.Add("EXP_PASSWORD_EXT_2");
			m_objScrubList.Add("EXP_PASSWORD_EXT_3");
			m_objScrubData = new System.Collections.Generic.List<string>();
		}

		// ###################################################################################
		// Protected variables
		// ###################################################################################

			// Gateway ID used to know what gateway to create
		protected int m_intGatewayID;
		protected string m_strGatewayURL;
		protected string m_strMerchantLogin;

		protected string m_strMerchantPassword;
		protected string m_strGatewayResponseCode;
		protected string m_strGatewayResponseRawData;

		protected string m_strGatewayResponseDescription;
			// Internal response code to calling process.
		protected int m_intEEPGResponseCode;
			// Internal response message to calling process.
		protected string m_strEEPGResponseDescription;


		protected Enterprise.EELog m_objLog;

		protected System.Collections.Generic.Dictionary<string, string> m_objSetGW;
		protected System.Collections.Generic.List<string> m_objScrubList;
                
		protected System.Collections.Generic.List<string> m_objScrubData;


        //TODO!!!!!//TODO!!!!!//TODO!!!!! 02202014 LfZ
		// 'TODO: Delete line when build complete
        protected nsoftware.InPay.Icharge m_objNSoftwareGW;
		//nsoftware.InPay.EPCard ' TODO: Change to an Object when build complete
        protected nsoftware.InPay.EPCard m_objNSoftwareCard;
	    // ' TODO: Change to an Object when build complete
        protected nsoftware.InPay.EPCustomer m_objNSoftwareCustomer;


	}

}
