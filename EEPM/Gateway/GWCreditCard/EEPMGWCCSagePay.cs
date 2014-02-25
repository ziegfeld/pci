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

// new error code set 985xx here 01/07/2014 LfZ

using System.Runtime.InteropServices;

///'  EEPMGWCCSagePay : no token involved
///'  EEPMGWCCSagePayToken: must have token involved
///'  'Supported Methods: 

//Sale 'AuthOnly 'Capture
//Credit 'using VoidTransaction
//Refund 'here Refund() uses nSoft Refund(TxId,Amount), equivalent to sagepay TxType=REFUND 
//' option for Refund VoidTransaction with SagePayUseAbort=False Config can CANCEL an captured tx, i.e. Credit with the whole amount. before it is settled
//Force  '' Not implemented. 
//'also TxType=Manual, DIRECTREFUND, and VOID not implemented LfZ 01092014
//MerchantLogin is a required property. MerchantPassword is not applicable.

//'  formatAmount: can only be 1.00 for $1.00 required by nSoftware (maybe not by Sagepay)
// it is said only several gateways want "100" for "1.00"  http://www.nsoftware.com/kb/help/BPN6-A/ICharge_p_TransactionAmount.rst sagepay not included.
// but actually only 1.00 works. 100 will be traslated as $100.00.

//A Security key (10char of Aa0-9) which Sage Pay uses to generate a MD5 Hash for to sign the Notification message (B3 below).
//The signature is called VPSSignature. This value is used to allow detection of tampering with notifications from the Sage Pay gateway. 
//It must be kept secret from the customer and held in your database. 

//Please ensure the VendorName is lower case prior to hashing.

namespace EEPM
{

	//Partial Public Class EEGateway
	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	// will only implement the part that has no token involved. GatewaySecurityProfile <> "Tokens"
	public class EEPMGWCCSagePay : EEPMGWCCGenericBase
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################
		public EEPMGWCCSagePay(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog) : 
            base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
		{
			m_strSecurityKey = "";
		}


		// ###################################################################################
		// Protected variables
		// ###################################################################################

		protected string m_strSecurityKey = "";
		// ###################################################################################
		// Protected functions
		// ###################################################################################

		//Everything in objProperties after this method and before actualy tx (Authorize, Capture, Credit, etc) is cleared,
		//In the case some special properties are needed for tx's like Capture, Credit, Void, 
		// you should store properties in protected member variables like m_strSecurityKey in this method.
		protected override bool GatewaySpecificMessageSetup(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;

			m_objLog.LogMessage("EEPMGWCCSagePay: GatewaySpecificMessageSetup()", 40);

			try {
				//Sagepay supports CurrencyCode setting. default value is "USD".
				if (ContainKeyCheck(ref objProperties, "CURRENCY"))
					m_objNSoftwareGW.Config("CurrencyCode=" + ProcessKey(ref objProperties, "CURRENCY"));
				m_objLog.LogMessage("Config return (CurrencyCode): " + m_objNSoftwareGW.Config("CurrencyCode"), 35);

				m_objNSoftwareGW.MerchantPassword = "";
				//  ''MerchantPassword is not applicable.
				m_objNSoftwareGW.AddSpecialField("VPSProtocol", "3.00");
				//VPSProtocol was 2.23 and that still works for non-token.

				//Since ProcessKey(objPro,"key") will check contain, the If ContainKeyCheck is unnecessary if we do not want to throw Error if non-exist.

				//"RelatedSecurityKey" and "RelatedVendorTXCode" special fields are required for Refunds.
				// "SecurityKey" and AuthCode "TxAuthNo" are required for voids and captures.

				m_strSecurityKey = ProcessKey(ref objProperties, "SECURITYKEY");
				//charge.Response.ProcessorCode)

				m_objNSoftwareGW.AuthCode = ProcessKey(ref objProperties, "TXAUTHNO");
				//m_objNSoftwareGW.AddSpecialField("TxAuthNo", m_objNSoftwareGW.AuthCode)

				if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER")) {
					string strVendorTXCode = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
					//m_objNSoftwareGW.AddSpecialField("VendorTXCode", VendorTXCode) not required since InvoiceNumber=VendorTxCode
					m_objNSoftwareGW.InvoiceNumber = strVendorTXCode;
				} else {
					m_objLog.LogMessage("EEPMGWCCSagePay: WARNING: TransactionDocumentNumber (Vendor side Invoice No.), parsed as VendorTxCode, must be specified for a Credit and should not be used before", 40);
				}
				//' commeted 01/06/14 m_objNSoftwareGW.AddSpecialField("RelatedVendorTXCode", VendorTXCode)


				if ((!ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))) {
					m_intEEPGResponseCode = 98503;
					m_strEEPGResponseDescription = "Transaction Description must be specified, SagePay will throw 3013 : The Description is missing.";
					m_objLog.LogMessage("EEPMGWCCSagePay: GatewaySpecificMessageSetup(): Error: Transaction Description must be specified", 50);
					blnReturn = false;
				}

            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98504;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
                m_objLog.LogMessage("EEPMGWCCSagePay: GatewaySpecificMessageSetup(): Exception: " + err.Message, 50);
				blnReturn = false;
			}

			m_objLog.LogMessage("EEPMGWCCSagePay: GatewaySpecificMessageSetup(): " + blnReturn, 40);

			return blnReturn;
		}

		protected override bool GatewaySpecificCredit(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
			string strTransactionAmount = "";
			m_objLog.LogMessage("EEPMGWCCSagePay: GatewaySpecificCredit.", 40);
			try {
				//security key(if exists, is not needed for credit! so clear it to make it disappear from raw request.
				//m_objNSoftwareGW.ClearSpecialField("SecurityKey") 'wrong need to not set "SecurityKey" in GWSpecFieldSetup 
				if ((!ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))) {
					m_intEEPGResponseCode = 98505;
					m_strEEPGResponseDescription = "Amount must be specified for a Credit.";
					blnReturn = false;
				} else {
					strTransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
					m_objNSoftwareGW.TransactionAmount = strTransactionAmount;
					if ((!ContainKeyCheck(ref objProperties, "TRANSACTIONID"))) {
						m_intEEPGResponseCode = 98506;
						m_strEEPGResponseDescription = "Transaction ID must be specified for a Credit. It is the TxID of the ORIGINAL tx, sent as RelatedVPSTxID in REFUND raw request. ";
						blnReturn = false;
					} else {
						strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
						//for refund, transactionID for itself is not needed, relatedVPSTxID is needed.
						//But m_objNSoftwareGW.AddSpecialField("RelatedVPSTxID", "TRANSACTIONID") ' is redundant since 
						//calling nSoftwareGW.Refund(strTransactionID,amount) will set strTransactionID to that "RepaltedVPSTxID" field
						// ^^ last 2 lines seems untrue when testing after MikeSimple meeting 02172014. So adding Special field again.
						m_objNSoftwareGW.AddSpecialField("RelatedVPSTxID", strTransactionID);

						if ((!ContainKeyCheck(ref objProperties, "RELATEDTRANSACTIONDOCUMENTNUMBER"))) {
							m_intEEPGResponseCode = 98507;
							//new error code nedded
							m_strEEPGResponseDescription = "Related Transaction Document Number (Vendor side Invoice No.) must be specified for a Credit and should not be used before.";
							blnReturn = false;
						} else {
							m_objNSoftwareGW.AddSpecialField("RelatedVendorTXCode", ProcessKey(ref objProperties, "RELATEDTRANSACTIONDOCUMENTNUMBER"));

							if ((!ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))) {
								m_intEEPGResponseCode = 98508;
								m_strEEPGResponseDescription = "Transaction Description must be specified for a Credit.";
								blnReturn = false;
							} else {
								m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");

								if ((string.IsNullOrEmpty(m_objNSoftwareGW.AuthCode))) {
									m_intEEPGResponseCode = 98509;
									//new error code needed
									m_strEEPGResponseDescription = "TxAuthNo (AuthCode must be specified for a Credit.";
									blnReturn = false;

								} else {
									//RelatedSecurityKey required. Note the keyword RELATED!!
									m_objNSoftwareGW.AddSpecialField("RelatedSecurityKey", m_strSecurityKey);

									//a InviceNo for refund itself is not required for refund, but it was set in GWSpecMsgSetup. Set it again here will null it.
									//m_objNSoftwareGW.InvoiceNumber = CStr(ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))

									//normal credit, amount can be specified and less than Captured amount.
									m_objNSoftwareGW.Refund(strTransactionID, strTransactionAmount);
									// 

									//Option 2: using Void (before bank processor settled the payment, but no other coice of amount but the exact original amount captured is refunded.
									//m_objNSoftwareGW.Config("SagePayUseAbort=False") is as default 
									//m_objNSoftwareGW.VoidTransaction(strTransactionID)

									//'From Sagepay server&direct shared protocols 3.00 pdf: VOID an authorised transaction.
									// If you have taken a PAYMENT or RELEASED a DEFERRED transaction and do not wish it to be settled because the customer has 
									//cancelled their order (or you wish to), you can send a VOID message to prevent the transaction from ever being settled. 
									// This only works BEFORE the transaction has been settled. As soon as a transaction has been settled, 
									// it can no longer be VOIDed and must be REFUNDed instead. The advantage with a VOID over a REFUND 
									//is that because the transaction is never settled, you will not be charged Merchant Fees by your bank. 
									//In a REFUND situation you will normally be charged for both the initial payment and the refund. 

									m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePay: GatewaySpecificCredit: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
									objProperties.Clear();
								}
							}
						}
					}
				}
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98510;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePay: GatewaySpecificCredit: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
				blnReturn = false;
			}

			m_objLog.LogMessage("EEPMGWCCSagePay: GatewaySpecificCredit: " + blnReturn, 40);

			return blnReturn;
		}

		protected override bool GatewaySpecificVoidTransaction(ref Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
            //string strTransactionAmount = "";

			m_objLog.LogMessage("EEPMGWCCSagePay: GatewaySpecificVoidTransaction.", 40);
			try {
				if ((string.IsNullOrEmpty(m_strSecurityKey))) {
					m_intEEPGResponseCode = 98510;
					m_strEEPGResponseDescription = "Security Key (Response.ProcessorCode) must be specified for a Void.";
					blnReturn = false;
				} else {
					m_objNSoftwareGW.AddSpecialField("SecurityKey", m_strSecurityKey);
					if ((!ContainKeyCheck(ref objProperties, "TRANSACTIONID"))) {
						m_intEEPGResponseCode = 98510;
						m_strEEPGResponseDescription = "Transaction ID must be specified for a Credit. It is the TxID of the ORIGINAL tx, sent as VPSTxID in ABORT raw request. ";
						blnReturn = false;
					} else {
						strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");

						// TransactionAmount is not applicable -- whole authorize is voided (ABORTed).
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
							m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");

						//for sagepay, to void an AuthOnly is called "ABORT an deferred trancation".                    '
						// using icharge.Config("SagePayUseAbort=True") sets txtype = ABORT
						//FYI: void (with the default SagePayUseAbor=False") is sued only for RELEASED tx's (captured auth or sale).
						m_objNSoftwareGW.Config("SagePayUseAbort=True");
						m_objNSoftwareGW.VoidTransaction(strTransactionID);

						m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePay: GatewaySpecificVoidTransaction: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
						objProperties.Clear();
						blnReturn = true;
					}
				}
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98511;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePay: GatewaySpecificVoidTransaction: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool GatewaySpecificCapture(ref Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
			string strTransactionAmount = "";

			m_objLog.LogMessage("EEPMGWCCSagePay: GatewaySpecificCapture.", 40);
			try {
				if ((!ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))) {
					m_intEEPGResponseCode = 98511;
					m_strEEPGResponseDescription = "Amount must be specified for a Capture.";
					blnReturn = false;
				} else {
					strTransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
					m_objNSoftwareGW.TransactionAmount = strTransactionAmount;
					if ((string.IsNullOrEmpty(m_strSecurityKey))) {
						m_intEEPGResponseCode = 98512;
						m_strEEPGResponseDescription = "Security Key (Response.ProcessorCode) must be specified for a Void.";
						blnReturn = false;
					} else {
						m_objNSoftwareGW.AddSpecialField("SecurityKey", m_strSecurityKey);
						if ((!ContainKeyCheck(ref objProperties, "TRANSACTIONID"))) {
							m_intEEPGResponseCode = 98513;
							m_strEEPGResponseDescription = "Transaction ID must be specified for a Capture. It is the TxID of the ORIGINAL tx, sent as VPSTxID in RELEASE raw request. ";
							blnReturn = false;
						} else {
							strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
							if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
								m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
							m_objNSoftwareGW.Capture(strTransactionID, strTransactionAmount);
							m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePay: GatewaySpecificCapture: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
							objProperties.Clear();
							blnReturn = true;
						}
					}
				}
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98514;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				m_objLog.LogMessage(ScrubForLog("EEPMGWCCSagePay: GatewaySpecificCapture: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
				blnReturn = false;
			}
			return blnReturn;
		}



		///' 12/30/13 LZ
		//Protox and SagePay Responce Code -- Description 
		//OK --  Process executed without error. The DEFERRED payment was released. 
		//MALFORMED -- Input message was malformed - normally will only occur during development. 
		//INVALID -- Unable to authenticate you or find the transaction, or the data provided is invalid. If the Deferred payment was already released, an INVALID response is returned.  
		protected override bool ReadGatewayResponse(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;

			m_objLog.LogMessage("EEPMGWCCSagePay: ReadGatewayResponse.", 40);

			try {
				//'DEBUG:    m_objLog.LogMessage("Lingfei 010614: Full Request: " + m_objNSoftwareGW.Config("FullRequest"))
				nsoftware.InPay.EPResponse objNSoftwareResponse = new nsoftware.InPay.EPResponse();
				objNSoftwareResponse = m_objNSoftwareGW.Response;
				m_objLog.LogMessage("EEPMGWCCSagePay: Response.Data: " + objNSoftwareResponse.Data, 35);
				m_objLog.LogMessage("EEPMGWCCSagePay: Response.Var: STATUS: " + m_objNSoftwareGW.GetResponseVar("STATUS"), 35);
				if ((!objNSoftwareResponse.Approved)) {
					// The two following variables are included to log someday
					objProperties.Add("STATUS", m_objNSoftwareGW.GetResponseVar("STATUS"));
					m_strGatewayResponseCode = objNSoftwareResponse.ErrorCode;
					m_strGatewayResponseRawData = objNSoftwareResponse.Data;
					//m_strGatewayResponseDescription = objNSoftwareResponse.ErrorText
					m_strGatewayResponseDescription = objNSoftwareResponse.Text;
					// The two following variables are what is sent back to the caller
					m_intEEPGResponseCode = 98515;
					m_strEEPGResponseDescription = "Error:  " + objNSoftwareResponse.Code + " : " + objNSoftwareResponse.Text;
					m_objLog.LogMessage("EEPMGWCCSagePay: ReadGatewayResponse: ResponseCode: " + m_strGatewayResponseCode, 50);
					m_objLog.LogMessage("EEPMGWCCSagePay: ReadGatewayResponse: ResponseText: " + m_strGatewayResponseDescription, 50);
					m_objLog.LogMessage("EEPMGWCCSagePay: ReadGatewayResponse: ResponseErrorText: " + objNSoftwareResponse.ErrorText, 50);
					m_objLog.LogMessage("EEPMGWCCSagePay: ReadGatewayResponse: ResponseRawData: " + m_strGatewayResponseRawData, 50);
					blnReturn = false;
				} else {
					objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult);
					objProperties.Add("GATEWAYRESPONSE", m_strGatewayResponseRawData);

					// sagepay's VendorTXCode, vendor side doc no. unique. and sageapy requires it for refund and abort(void)
					// Note that this is actually the same info that feeds in this dll from NAV or w/e
					objProperties.Add("TRANSACTIONDOCUMENTNUMBER", m_objNSoftwareGW.InvoiceNumber);

					//'sagepay response raw data VPSTxId = nSoft GW Response.TransactionId = TRANSACTIONID
					objProperties.Add("TRANSACTIONID", objNSoftwareResponse.TransactionId);

					//'raw data TxAuthNo = Response.ApprovalCode  = TxAuthNo
					//'raw data SecurityKey = Response.ProcessorCode = SECURITYKEY
					if ((!string.IsNullOrEmpty(objNSoftwareResponse.ProcessorCode)))
						objProperties.Add("SECURITYKEY", objNSoftwareResponse.ProcessorCode);
					if ((!string.IsNullOrEmpty(objNSoftwareResponse.ApprovalCode)))
						objProperties.Add("TXAUTHNO", objNSoftwareResponse.ApprovalCode);
					// is this so?

				}
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98516;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				m_objLog.LogMessage("EEPMGWCCSagePay: Exception: " + m_strEEPGResponseDescription, 35);
				blnReturn = false;
			}
			//inspect Watch variables here!!
			m_objLog.LogMessage("EEPMGWCCSagePay: ReadGatewayResponse: " + blnReturn, 40);
			//inspect Watch variables here!!
			return blnReturn;
		}


	}

}
