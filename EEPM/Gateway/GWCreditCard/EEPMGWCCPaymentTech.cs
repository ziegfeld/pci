using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using nsoftware;
using nsoftware.Sys;
using nsoftware.InPtech;

using System.Runtime.InteropServices;

namespace EEPM
{

	//Partial Public Class EEGateway
	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class EEPMGWCCPaymentTech : EEPMGWCCGenericBase
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################
		public EEPMGWCCPaymentTech(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog, bool blnInitGWObject = true, bool blnInitGWCardObject = true, bool blnInitGWCustomerObject = true) : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, objLog)
		{
		}

		// ###################################################################################
		// Public functions
		// ###################################################################################
		protected override bool GatewaySpecificAuthorize(ref System.Collections.Generic.Dictionary<object, object> objProperties)
		{
			bool blnReturn = true;

			try {
				if (ContainKeyCheck(objProperties, "TRANSACTIONAMOUNT")) {
					m_objNSoftwareGW.TransactionAmount = FormatAmount(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONAMOUNT")));
					ProcessKey(objProperties, "TRANSACTIONDESC");
					if (ContainKeyCheck(objProperties, "TRANSACTIONDOCUMENTNUMBER"))
						m_objNSoftwareGW.InvoiceNumber = Strings.Right(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONDOCUMENTNUMBER")), 16);
					m_objNSoftwareGW.AuthOnly();
					objProperties.Clear();
				} else {
					m_intEEPGResponseCode = 98040;
					m_strEEPGResponseDescription = "Amount must be specified for an Authorization.";
					blnReturn = false;
				}
			} catch {
				m_intEEPGResponseCode = 98041;
				m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool GatewaySpecificCapture(ref System.Collections.Generic.Dictionary<object, object> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
			string strTransactionAmount = "";
			string strTransactionOriginalAmount = "";

			try {
				if (ContainKeyCheck(objProperties, "TRANSACTIONAMOUNT")) {
					strTransactionAmount = FormatAmount(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONAMOUNT")));
					if (ContainKeyCheck(objProperties, "TRANSACTIONID")) {
						strTransactionID = Convert.ToString(ProcessKey(objProperties, "TRANSACTIONID"));
						if (ContainKeyCheck(objProperties, "TRANSACTIONDOCUMENTNUMBER"))
							m_objNSoftwareGW.InvoiceNumber = Strings.Right(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONDOCUMENTNUMBER")), 16);
						m_objNSoftwareGW.TransactionAmount = strTransactionAmount;
						m_objNSoftwareGW.Capture(strTransactionID);
						objProperties.Clear();
					} else {
						m_intEEPGResponseCode = 98042;
						m_strEEPGResponseDescription = "Transaction ID must be specified for an Capture.";
						blnReturn = false;
					}
				} else {
					m_intEEPGResponseCode = 98043;
					m_strEEPGResponseDescription = "Amount must be specified for an Capture.";
					blnReturn = false;
				}
			} catch {
				m_intEEPGResponseCode = 98044;
				m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool GatewaySpecificDirectSale(ref System.Collections.Generic.Dictionary<object, object> objProperties)
		{
			bool blnReturn = true;
			string strTransactionAmount = "";

			try {
				if (ContainKeyCheck(objProperties, "TRANSACTIONAMOUNT")) {
					m_objNSoftwareGW.TransactionAmount = FormatAmount(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONAMOUNT")));
					if (ContainKeyCheck(objProperties, "TRANSACTIONDOCUMENTNUMBER"))
						m_objNSoftwareGW.InvoiceNumber = Strings.Right(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONDOCUMENTNUMBER")), 16);
					m_objNSoftwareGW.Sale();
					objProperties.Clear();
				} else {
					m_intEEPGResponseCode = 98045;
					m_strEEPGResponseDescription = "Amount must be specified for an Direct Sale.";
					blnReturn = false;
				}
			} catch {
				m_intEEPGResponseCode = 98046;
				m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool GatewaySpecificCredit(ref System.Collections.Generic.Dictionary<object, object> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
			string strInvoiceNumber = "";
			string strTransactionAmount = "";
			string strTransactionOriginalAmount = "";

			try {
				if (ContainKeyCheck(objProperties, "TRANSACTIONAMOUNT")) {
					strTransactionAmount = FormatAmount(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONAMOUNT")));
					if (ContainKeyCheck(objProperties, "TRANSACTIONID")) {
						strTransactionID = Convert.ToString(ProcessKey(objProperties, "TRANSACTIONID"));
						if (ContainKeyCheck(objProperties, "TRANSACTIONDOCUMENTNUMBER"))
							strInvoiceNumber = Strings.Right(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONDOCUMENTNUMBER")), 16);

						// First call to void the transaction.
						// If this transaction is on the open batch then a void must be called.
						// If this transaction has already been settled then this call will fail
						//   and a credit must be issued.
						m_objNSoftwareGW.VoidTransaction(strTransactionID, "");
						objProperties.Clear();
						blnReturn = ReadGatewayResponse(ref objProperties);

						if (!(blnReturn)) {
							// Call to void failed.  Call to credit the customer's money.
							m_objNSoftwareGW.TransactionAmount = strTransactionAmount;
							m_objNSoftwareGW.Credit();
							objProperties.Clear();
						}
					} else {
						m_intEEPGResponseCode = 98047;
						m_strEEPGResponseDescription = "Transaction ID must be specified for a Credit.";
						blnReturn = false;
					}
				} else {
					m_intEEPGResponseCode = 98053;
					m_strEEPGResponseDescription = "Amount must be specified for a Credit.";
					blnReturn = false;
				}
			} catch {
				m_intEEPGResponseCode = 98052;
				m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool GatewaySpecificVoidTransaction(ref System.Collections.Generic.Dictionary<object, object> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
			string strTransactionAmount = "";

			try {
				if (ContainKeyCheck(objProperties, "TRANSACTIONID")) {
					strTransactionID = FormatAmount(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONID")));
					if (ContainKeyCheck(objProperties, "TRANSACTIONDOCUMENTNUMBER"))
						m_objNSoftwareGW.InvoiceNumber = Strings.Right(Convert.ToString(ProcessKey(objProperties, "TRANSACTIONDOCUMENTNUMBER")), 16);

					m_objNSoftwareGW.TransactionAmount = "0.00";
					m_objNSoftwareGW.Capture(strTransactionID);
					objProperties.Clear();
					blnReturn = ReadGatewayResponse(ref objProperties);

					if ((blnReturn)) {
						// Call to void failed.  Call to credit the customer's money.
						m_objNSoftwareGW.TransactionAmount = strTransactionAmount;
						m_objNSoftwareGW.Credit();
						objProperties.Clear();
					} else {
						m_intEEPGResponseCode = 98999;
						m_strEEPGResponseDescription = "Capture call before void failed.  Please check status of transaction on live server.";
						blnReturn = false;
					}
				} else {
					m_intEEPGResponseCode = 98050;
					m_strEEPGResponseDescription = "Transaction ID must be specified for a Void.";
					blnReturn = false;
				}
			} catch {
				m_intEEPGResponseCode = 98051;
				m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
				blnReturn = false;
			}
			return blnReturn;
		}

		// ###################################################################################
		// Protected functions
		// ###################################################################################
		protected override bool SetGatewayCredentials(ref System.Collections.Generic.Dictionary<object, object> objProperties)
		{
			bool blnReturn = true;
			string strMissingKeys = "";

			if ((objProperties.ContainsKey("MERCHANTNUMBER")) & (objProperties.ContainsKey("TERMINALNUMBER")) & (objProperties.ContainsKey("CLIENTNUMBER"))) {
				try {
					m_objNSoftwareGW.Server = m_strGatewayURL;
					m_objNSoftwareGW.UserId = m_strMerchantLogin;
					m_objNSoftwareGW.Password = m_strMerchantPassword;
					m_objNSoftwareGW.MerchantNumber = ProcessKey(objProperties, "MERCHANTNUMBER");
					m_objNSoftwareGW.TerminalNumber = ProcessKey(objProperties, "TERMINALNUMBER");
					m_objNSoftwareGW.ClientNumber = ProcessKey(objProperties, "CLIENTNUMBER");
				} catch {
					m_intEEPGResponseCode = 98054;
					m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
					blnReturn = false;
				}
			} else {
				if ((objProperties.ContainsKey("MERCHANTNUMBER")))
					strMissingKeys = "," + "MERCHANTNUMBER";
				if ((objProperties.ContainsKey("TERMINALNUMBER")))
					strMissingKeys = "," + "TERMINALNUMBER";
				if ((objProperties.ContainsKey("CLIENTNUMBER")))
					strMissingKeys = "," + "CLIENTNUMBER";
				m_intEEPGResponseCode = 98055;
				m_strEEPGResponseDescription = "Needed Key(s) (" + Strings.Mid(strMissingKeys, 2) + ") not set.";
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool PrepareGatewayMessage(ref System.Collections.Generic.Dictionary<object, object> objProperties)
		{
			bool blnReturn = true;
			// Build the card object
			try {
				ProcessKey(objProperties, "CCTYPE");
				// Left in place to ensure it is removed from the objProperties variable and isn't added as an extra key later in the process
				if (ContainKeyCheck(objProperties, "CCNUMBER"))
					m_objNSoftwareCard.Number = Convert.ToString(ProcessKey(objProperties, "CCNUMBER"));
				if (ContainKeyCheck(objProperties, "CCEXPMONTH"))
					m_objNSoftwareCard.ExpMonth = Convert.ToInt32(ProcessKey(objProperties, "CCEXPMONTH"));
				if (ContainKeyCheck(objProperties, "CCEXPYEAR"))
					m_objNSoftwareCard.ExpYear = Convert.ToInt32(ProcessKey(objProperties, "CCEXPYEAR"));
				if (ContainKeyCheck(objProperties, "CCCCV"))
					m_objNSoftwareCard.CVVData = Convert.ToString(ProcessKey(objProperties, "CCCCV"));
				if (ContainKeyCheck(objProperties, "CCENTRYTYPE"))
					m_objNSoftwareCard.EntryDataSource = Convert.ToInt32(ProcessKey(objProperties, "CCENTRYTYPE"));
				else
					m_objNSoftwareCard.EntryDataSource = 1;
				m_objNSoftwareGW.Card = m_objNSoftwareCard;
			} catch {
				m_intEEPGResponseCode = 98056;
				m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
				blnReturn = false;
			}
			// Build the customer object
			if (ContainKeyCheck(objProperties, "FNAME"))
				ProcessKey(objProperties, "FNAME");
			if (ContainKeyCheck(objProperties, "ADDRESSNUMBER"))
				ProcessKey(objProperties, "ADDRESSNUMBER");
			if (ContainKeyCheck(objProperties, "ADDRESS"))
				ProcessKey(objProperties, "ADDRESS");
			if (ContainKeyCheck(objProperties, "CITY"))
				ProcessKey(objProperties, "CITY");
			if (ContainKeyCheck(objProperties, "STATE"))
				ProcessKey(objProperties, "STATE");
			if (ContainKeyCheck(objProperties, "ZIPCODE"))
				ProcessKey(objProperties, "ZIPCODE");
			if (ContainKeyCheck(objProperties, "COUNTRYCODE"))
				ProcessKey(objProperties, "COUNTRYCODE");
			if (ContainKeyCheck(objProperties, "EMAIL"))
				ProcessKey(objProperties, "EMAIL");

			blnReturn = GatewaySpecificMessageSetup(objProperties);
			blnReturn = SetSpecialFields(ref objProperties);

			return blnReturn;
		}

		protected override bool ReadGatewayResponse(ref System.Collections.Generic.Dictionary<object, object> objProperties)
		{
			bool blnReturn = true;
			try {
				nsoftware.InPtech.PTChargeResponse objNSoftwareResponse = default(nsoftware.InPtech.PTChargeResponse);
				objNSoftwareResponse = m_objNSoftwareGW.Response;
				if ((objNSoftwareResponse.Code != "A")) {
					// The two following variables are included to log someday
					m_strGatewayResponseCode = 999999;
					m_strGatewayResponseRawData = objNSoftwareResponse.Data;
					m_strGatewayResponseDescription = "Failed request from PaymentTech";

					// The two following variables are what is sent back to the caller
					m_intEEPGResponseCode = 98058;
					m_strEEPGResponseDescription = "Error:  " + objNSoftwareResponse.Text;
					blnReturn = false;
				} else {
					objProperties.Add("TRANSACTIONID", objNSoftwareResponse.RetrievalNumber);
					objProperties.Add("TRANSACTIONAMOUNT", objNSoftwareResponse.AuthorizedAmount);
					objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult);
				}
			} catch {
				m_intEEPGResponseCode = 98059;
				m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool SetSpecialFields(ref System.Collections.Generic.Dictionary<object, object> objProperties)
		{
			bool blnReturn = true;
			try {
				// 0 retail
				// 1 Direct Marketing
				// 2 ECommerce
				// 3 Restaurant
				// 4 Hotel
				if (ContainKeyCheck(objProperties, "CCINDUSTRYTYPE"))
					m_objNSoftwareGW.IndustryType = Convert.ToInt32(ProcessKey(objProperties, "CCINDUSTRYTYPE"));
				else
					m_objNSoftwareGW.IndustryType = 1;
			} catch {
				m_intEEPGResponseCode = 98061;
				m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override string FormatAmount(string strAmount)
		{
			string strReturn = "";
			bool blnDecimalExists = false;
			int intDecimalLength = 0;

			Strings.Replace(strAmount, "$", "");
			blnDecimalExists = Strings.InStr(strAmount, ".") > 0;
			intDecimalLength = (Strings.Len(strAmount) - Strings.InStr(strAmount, "."));

			if ((intDecimalLength == 2) & blnDecimalExists) {
				strReturn = strAmount;
			} else {
				if ((blnDecimalExists)) {
					if (intDecimalLength > 2) {
						strReturn = Strings.Mid(strAmount, 1, (Strings.InStr(strAmount, ".") + 2));
					} else {
						strReturn = strAmount + "0";
					}
				} else {
					strReturn = strAmount + ".00";
				}
			}
			return strReturn;
		}

		protected override bool SetGWObject(string strCase)
		{
			bool blnReturn = true;
			try {
				switch (strCase) {
					default:
						m_objNSoftwareGW = new nsoftware.InPtech.Ptcharge();
						m_objNSoftwareCard = new nsoftware.InPtech.PTCardType();
						break;
				}
				m_objNSoftwareGW.RuntimeLicense = "42544E364141315355425241315355424348394535303330000000000000000000000000000000004B5655345848355000005956584A57545932325833320000";
			} catch {
				m_intEEPGResponseCode = 98062;
				m_strEEPGResponseDescription = "Error:  " + Err().Number + " : " + Err().Description;
				blnReturn = false;
			}
			return blnReturn;
		}

	}

}
