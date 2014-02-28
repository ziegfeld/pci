using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.Xml;
using System.Runtime.InteropServices;

namespace EEPM
{

	//Partial Public Class EEGateway
	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class EEPGGWCCOgone : EEPMGWCCGenericBase
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################
		public EEPGGWCCOgone(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog) : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
		{           
		}

        // ###################################################################################
        // Protected variables
        // ###################################################################################
        //redefine the class of these 3 objects to customed ones.
        protected new OgoneGateway m_objNSoftwareGW;
        protected new OgoneCard m_objNSoftwareCard;
        protected new OgoneCustomer m_objNSoftwareCustomer;


		// ###################################################################################
		// Protected functions
		// ###################################################################################

		protected override bool SetGWObject(string strCase)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPGGWCCOgone: SetGWObject: " + strCase, 40);
			try {
				switch (strCase) {
					default:
						m_objNSoftwareGW = new OgoneGateway(m_objLog);
						m_objNSoftwareCard = new OgoneCard(m_objLog);
						m_objNSoftwareCustomer = new OgoneCustomer(ref m_objLog);
						break;
				}
			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98062;
				m_strEEPGResponseDescription = "Error:  "+ err.Message;
				blnReturn = false;
			}
			m_objLog.LogMessage("EEPGGWCCOgone: SetGWObject: " + blnReturn, 40);
			return blnReturn;
		}

		protected override bool ReadGatewayResponse(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;

			m_objLog.LogMessage("EEPGGWCCOgone: ReadGatewayResponse.", 40);

			try {
				OgoneResponse objNSoftwareResponse = (OgoneResponse) m_objNSoftwareGW.Response;

				m_objLog.LogMessage("EEPGGWCCOgone: Response.Data: " + objNSoftwareResponse.Data, 35);

				if (!(objNSoftwareResponse.Approved)) {
					// The two following variables are included to log someday
					m_strGatewayResponseCode = objNSoftwareResponse.ErrorCode;
					m_strGatewayResponseRawData = objNSoftwareResponse.Data;
					m_strGatewayResponseDescription = objNSoftwareResponse.ErrorText;
					// The two following variables are what is sent back to the caller
					m_intEEPGResponseCode = 98013;
					//m_strEEPGResponseDescription = "Error:  " + "Danger Will Robinson."
					m_strEEPGResponseDescription = "Error:  " + objNSoftwareResponse.Code + " : " + objNSoftwareResponse.Text;
					blnReturn = false;
				} else {
					objProperties.Add("TRANSACTIONID", objNSoftwareResponse.TransactionId);
					objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult);
				}
			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98014;
				m_strEEPGResponseDescription = "Error:  "+ err.Message;
				blnReturn = false;
			}

			m_objLog.LogMessage("EEPGGWCCOgone: ReadGatewayResponse: " + blnReturn, 40);

			return blnReturn;
		}

		//returning 432100 for $4,321.0

        //C# version: created 02212014 LfZ
        //this version is on BarclayEDPQ, EWay, Obital, Ogone
        //"12,345.605" -> "1234561" ;"123" -> "12300" etc..
        protected override string FormatAmount(string strAmount)
        {
            string strReturn = strAmount;
            m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: FormatAmount()" + strReturn, 40);
            strReturn = strReturn.Replace(" ", "");
            strReturn = strReturn.Replace(",", "");
            int intAmount = 0;
            intAmount = Convert.ToInt32(100 * Math.Round(Convert.ToDecimal(strReturn), 2, MidpointRounding.AwayFromZero));
            strReturn = Convert.ToString(intAmount);

			m_objLog.LogMessage("EEPGGWCCOgone: FormatAmount()" + strReturn, 40);
			return strReturn;
		}

	}
    
	// ###################################################################################
	// ###################################################################################
	// ###################################################################################
	//   Specific Gateway Override Classes
	// ###################################################################################
	// ###################################################################################
	// ###################################################################################

	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class OgoneCard : CustomGWPostCard
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################

		public OgoneCard(Enterprise.EELog objLog) : base(objLog)
		{
			SetupPostValueArray();
		}

		// ###################################################################################
		// Protected functions
		// ###################################################################################

		protected override void SetupPostValueArray()
		{
			m_arrPostValues = new string[] {
				"",
				"CARDNO",
				"",
				"",
				"CVC"
			};
		}

		protected override string BuildGatewaySpecificPost()
		{
			string strReturn = "";
			string strYear = ExpYear;
            if ((strYear.Length > 2))
				strYear = strYear.Substring(strYear.Length - 2, 2);
			strReturn = "&ED=" + ExpMonth + "/" + strYear;
			return strReturn;
		}

	}

	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class OgoneCustomer : CustomGWPostCustomer
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################

		public OgoneCustomer(ref Enterprise.EELog objLog) : base(ref objLog)
		{
		}

		// ###################################################################################
		// Protected functions
		// ###################################################################################

		protected override void SetupPostValueArray()
		{
			m_arrPostValues = new string[] {
				"",
				"ECOM_BILLTO_POSTAL_NAME_FIRST",
				"ECOM_BILLTO_POSTAL_NAME_LAST",
				"ECOM_BILLTO_POSTAL_STREET_LINE1",
				"ECOM_BILLTO_POSTAL_STREET_LINE2",
				"ECOM_BILLTO_POSTAL_CITY",
				"",
				"ECOM_BILLTO_POSTAL_POSTALCODE",
				"ECOM_BILLTO_POSTAL_COUNTRYCODE",
				"",
				"EMAIL"
			};
		}

	}

	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class OgoneGateway : CustomGWPostGateway
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################
		public OgoneGateway(Enterprise.EELog objLog) : base(objLog)
		{
			Response = new OgoneResponse();
		}

		// ###################################################################################
		// Public functions
		// ###################################################################################

		public override void AuthOnly()
		{
			AddSpecialField("Operation", "RES");
			PostDataToGateway();
			Response.ParseResponseData();
		}


		public override void Capture(string strTransactionID, string strTransactionAmount)
		{
			TransactionId = strTransactionID;
			TransactionAmount = strTransactionAmount;

			AddSpecialField("Operation", "SAS");
			// Override gateway URL.  This is done here as the gateway requires different URLs for different transaction types.
			//GatewayURL() = "https://secure.ogone.com/ncol/test/maintenancedirect.asp"
			PostDataToGateway();
			Response.ParseResponseData();
		}


		public override void Sale()
		{
			m_objLog.LogMessage("OgoneGateway: Sale: ", 40);

			AddSpecialField("Operation", "SAL");
			PostDataToGateway();
			Response.ParseResponseData();

			m_objLog.LogMessage("OgoneGateway: Sale: ", 40);
		}


		public override void Refund(string strTransactionID, string strTransactionAmount)
		{
			m_objLog.LogMessage("OgoneGateway: Refund: ", 40);

			TransactionId = strTransactionID;
			TransactionAmount = strTransactionAmount;

			AddSpecialField("Operation", "RFD");
			//GatewayURL() = "https://secure.ogone.com/ncol/test/maintenancedirect.asp"
			PostDataToGateway();
			Response.ParseResponseData();

			m_objLog.LogMessage("OgoneGateway: Refund: ", 40);

		}


		public override void VoidTransaction(string strTransactionID)
		{
			TransactionId = strTransactionID;

			AddSpecialField("Operation", "DES");
			//GatewayURL() = "https://secure.ogone.com/ncol/test/maintenancedirect.asp"
			PostDataToGateway();
			Response.ParseResponseData();
		}

		// ###################################################################################
		// Protected functions
		// ###################################################################################

		protected override void SetupPostValueArray()
		{
			m_arrPostValues = new string[] {
				"",
				"",
				"PSPID",
				"PSWD",
				"amount",
				"COM",
				"ORDERID",
				"",
				""
			};
		}

		protected override string BuildGatewaySpecificPost()
		{
			string strReturn = "";
			//If (GetSpecialField("CURRENCY") = "") Then
			//RemoveSpecialField("CURRENCY")
			//AddSpecialField("CURRENCY", "EUR")
			//End If
			if ((ContainsSpecialField("ADDRESS2")))
				RemoveSpecialField("ADDRESS2");
			return strReturn;
		}

	}

	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class OgoneResponse : CustomGWResponse
	{

		// ###################################################################################
		// Protected functions
		// ###################################################################################

		public override bool ParseResponseData()
		{
			bool blnReturn = true;

			try {
                XmlDocument objXMLDocument = new XmlDocument();
                objXMLDocument.LoadXml(Data);
                XmlElement objXMLElement = objXMLDocument.SelectSingleNode("ncresponse") as XmlElement;
				// Throw Exeption if these required attributes are not present.
				// Status 5 - Authorization was successful
				// Status 9 - Direct sales was sucessful.
				// Status 61 - Void to process offline.  No error reported.
				// Status 91 - Capture to process offline.  No error reported.
                if ((objXMLElement.HasAttribute("STATUS")))
                {
                    string strAttribute = objXMLElement.GetAttribute("STATUS");
                    Approved = (strAttribute == "5") || (strAttribute == "9") || (strAttribute == "61") || (strAttribute == "91");
                }
                else
                    throw new Exception("Failed loading response from gateway.  Missing attribute: 'STATUS'");
				if (objXMLElement.HasAttribute("PAYID"))
					TransactionId = objXMLElement.GetAttribute("PAYID");
				else
					throw new Exception("Failed loading response from gateway.  Missing attribute: 'PAYID'");
				if (objXMLElement.HasAttribute("NCERROR"))
					ErrorCode = objXMLElement.GetAttribute("NCERROR");
				else
					throw new Exception("Failed loading response from gateway.  Missing attribute: 'NCERROR'");
				if (objXMLElement.HasAttribute("NCERRORPLUS"))
					ErrorText = objXMLElement.GetAttribute("NCERRORPLUS");
				else
					throw new Exception("Failed loading response from gateway.  Missing attribute: 'NCERRORPLUS'");

				// These are optional attributes.  Save data if present, but do not error if not.
				if (objXMLElement.HasAttribute("AAVCHECK"))
					AVSResult = objXMLElement.GetAttribute("AAVCHECK");
			} catch (Exception err) {
				blnReturn = false;
				throw new Exception("Failed loading response from gateway: " + Data + " . Error message : " + err.Message);
			}
			return blnReturn;
		}

	}

}
