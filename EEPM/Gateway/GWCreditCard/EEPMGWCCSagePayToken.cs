using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using nsoftware;
using nsoftware.InPay;
//using nsoftware.InPay.Icharge;
//using nsoftware.InPay.EPCard;
//using nsoftware.InPay.EPCustomer;
//using nsoftware.InPay.EPResponse;
using System.Runtime.InteropServices;

//Tokens will automatically be removed when the card expiry date has passed.  
//They will also be removed if you do not indicate SagePayStoreToken=True after each use

//Please note that the Sage Pay Token System does not validate any customer information 
//associated with the token and therefore it is important for you to store the correct token 
//with the customer’s details to ensure that payment is taken from the correct card.

//'02172014 LfZ to set it externally: use AddNameValue("CCNUMBER","yourTokenValue"); to delete it: AddNameValue("CCNUMBER","")
// to override the CCNUMBER fed to Nsoftcard.number, another overriding PrepareGatewayMessage is coded.

//' get TokenGuid here to passed back to NAV through NameValue ("Token")
//  strTokenGuid = m_objNSoftwareGW.Config("SagePayToken")
//' to acess: use GetNameValue("Token")
///' to set it externally: use AddNameValue("Token","yourTokenValue" or "" to delete it)
// cannot use process key.. do not know if this line is good. LZ

namespace EEPM
{

	//Partial Public Class EEGateway
	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	// will only implement the part that has token involved. GatewaySecurityProfile = "Tokens"
	public class EEPMGWCCSagePayToken : EEPMGWCCSagePay
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################
		public EEPMGWCCSagePayToken(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog) : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
		{
		}

		// ###################################################################################
		// Protected variables
		// ###################################################################################

		protected string m_strTokenGuid = "";
		// ###################################################################################
		// Protected functions
		// ###################################################################################

		////Create a token without a sale (icharge is an configed instance of nSoftware.InPay.Icharge()
		//icharge.AddSpecialField("VPSProtocol", "3.00");
		//icharge.TransactionAmount = "1.00"; 'TODO must be 1 unit of the currency?
		//icharge.TransactionDesc = "Token";
		//icharge.Config("SagePayStoreToken=True");
		//icharge.Config("SagePayCreateToken");
		protected override string GatewaySpecificTokenize(string strCardNumber, ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
            //string strTransactionID = "";
			//Dim strTransactionAmount As String = ""

			m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificTokenize.", 40);

			try {
				m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");

				m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificTokenize: Option 1 detected: Request a new Token", 40);


				if ((!string.IsNullOrEmpty(m_objNSoftwareCard.Number))) {
					m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePayToken: GatewaySpecificTokenize: Warning! Credit Card Number alread set to " + m_objNSoftwareCard.Number + "  through the normal way (NameValue CCNUMBER), and will be overwritten by the parameter str in Tokenize(str): " + strCardNumber));
				}
				m_objNSoftwareCard.Number = strCardNumber;
				//m_strTokenGuid


				m_objNSoftwareGW.Config("SagePayStoreToken=True");
				//This line create a new token for the card (no matter it has a token already or not) and you can access that token at icharge1.Config("SagePayToken")
				m_objNSoftwareGW.Config("SagePayCreateToken");
				m_strTokenGuid = m_objNSoftwareGW.Config("SagePayToken");

				m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePayToken: GatewaySpecificTokenize: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
				m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePayToken: GatewaySpecificTokenize: Token Received = " + m_strTokenGuid));

				objProperties.Clear();
			//objProperties.Add("TOKEN", m_strTokenGuid) 
			//blnReturn = True
			//Else
			//    '------option 2--------Remove an existing token
			//    m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificDirectSale: Option 2 detected: Remove Token =" + strTokenGuid, 40)
			//    m_objNSoftwareGW.Config("SagePayToken=" + strTokenGuid)
			//    m_objNSoftwareGW.Config("SagePayRemoveToken")
			//    m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePayToken: GatewaySpecificTokenize: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50)
			//    objProperties.Clear()
			//    'blnReturn = True
			//End If
			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98517;
				m_strEEPGResponseDescription = "Error:  "+ err.Message;
				m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePayToken: GatewaySpecificCredit: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
			}

			m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificTokenize - TokenGuid: " + m_strTokenGuid, 40);

			return m_strTokenGuid;
		}

		protected override bool GatewaySpecificDirectSale(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificDirectSale.", 40);
			try {
				if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT")) {
					m_objNSoftwareGW.TransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONID")) {
						m_objNSoftwareGW.TransactionId = ProcessKey(ref objProperties, "TRANSACTIONID");
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC")) {
							m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
							m_objNSoftwareGW.Config("SagePayStoreToken=True");
							//------option 2--------Make a sale with a token
							m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificDirectSale: Option 2 detected: Make a Sale Using an Existing Token =" + m_strTokenGuid, 40);
							m_objNSoftwareGW.Config("SagePayToken=" + m_strTokenGuid);
							m_objNSoftwareGW.Config("SagePayRequestToken=False");
							m_objNSoftwareGW.Sale();
							objProperties.Clear();

							m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePayToken: GatewaySpecificDirectSale: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
						//(End of sale's 2 options(new sale and create a token) and (sale using a token)
						} else {
							m_intEEPGResponseCode = 98518;
							m_strEEPGResponseDescription = "Transaction Desc must be specified for a DirectSale using/creating Token; SagePay will throw error 3013 : The Description is missing.";
						}

					} else {
						m_intEEPGResponseCode = 98519;
						m_strEEPGResponseDescription = "Transaction ID must be specified for a DirectSale using/creating Token.";
						blnReturn = false;
					}
				} else {
					m_intEEPGResponseCode = 98520;
					m_strEEPGResponseDescription = "Amount must be specified for a DirectSale using/creating Token.";
					blnReturn = false;
				}
			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98521;
				m_strEEPGResponseDescription = "Error:  "+ err.Message;
				m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePayToken: GatewaySpecificDirectSale: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
				blnReturn = false;
			}

			m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificDirectSale: " + blnReturn, 40);


			return blnReturn;
		}

		//icharge1.Config("SagePayStoreToken=False")  
		//  Once a transaction has completed as either successful or failed, the token is considered as used. 
		//For successful transactions the token is deleted unless a request is made as per the transaction 
		//registration post StoreToken=1.For failed transactions the token will continue to be stored unless 
		//subsequent attempts are successful or a REMOVETOKEN request is made

		protected override bool GatewaySpecificAuthorize(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificAuthorize.", 40);
			try {
				if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT")) {
					m_objNSoftwareGW.TransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONID")) {
						m_objNSoftwareGW.TransactionId = ProcessKey(ref objProperties, "TRANSACTIONID");
						//' TxDocNo / InvoiceNumber already set in GWSpecMsgSetup(): ' m_objNSoftwareGW.InvoiceNumber = CStr(ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC")) {
							m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
							m_objNSoftwareGW.Config("SagePayStoreToken=True");

							//------option 2------- Authorize with a token
							m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificAuthorize: Option 2 detected: Authorize Using an Existing Token =" + m_strTokenGuid, 40);
							m_objNSoftwareGW.Config("SagePayToken=" + m_strTokenGuid);
							m_objNSoftwareGW.Config("SagePayRequestToken=False");
							m_objNSoftwareGW.AuthOnly();
							objProperties.Clear();
							m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePayToken: GatewaySpecificAuthorize: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
						//(End of auth's 2 options(auth and create a token) and (auth using a token)
						} else {
							m_intEEPGResponseCode = 98522;
							m_strEEPGResponseDescription = "Transaction Desc must be specified for Authorize using/creating Token; SagePay will throw error 3013 : The Description is missing.";
							blnReturn = false;
						}

					} else {
						m_intEEPGResponseCode = 98523;
						m_strEEPGResponseDescription = "Transaction ID must be specified for Authorize using/creating Token.";
						blnReturn = false;
					}
				} else {
					m_intEEPGResponseCode = 98524;
					m_strEEPGResponseDescription = "Amount must be specified for Authorize using/creating Token.";
					blnReturn = false;
				}
			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98525;
				m_strEEPGResponseDescription = "Error:  "+ err.Message;
				m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePayToken: GatewaySpecificAuthorize: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
				blnReturn = false;
			}

			m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificAuthorize: " + blnReturn, 40);

			return blnReturn;
		}


		protected override bool PrepareGatewayMessage(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			if (ContainKeyCheck(ref objProperties, "CCNUMBER")) {
				m_strTokenGuid = ProcessKey(ref objProperties, "CCNUMBER");

				if ((m_strTokenGuid.Length == 38)) {
					m_objLog.LogMessage("EEPMGWCCSagePayToken: PrepareGatewayMessage: Token Received =" + m_strTokenGuid, 40);
					//Else
					// meaning it is truely a cred card number
				}
			}
			return base.PrepareGatewayMessage(ref objProperties);
		}

		//' query message setup (special keys, currency.. etc)
		protected override bool GatewaySpecificMessageSetup(ref Dictionary<string, string> objProperties)
		{
			//same as super class SagePay (no token version) now
			if ((!base.GatewaySpecificMessageSetup(ref objProperties)))
				return false;
			bool blnReturn = true;

			m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificMessageSetup(): No Additional Info Set", 40);

			m_objLog.LogMessage("EEPMGWCCSagePayToken: GatewaySpecificMessageSetup(): " + blnReturn, 40);
			return blnReturn;
		}

	}

}

