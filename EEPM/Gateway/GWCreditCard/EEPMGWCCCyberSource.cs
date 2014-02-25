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

namespace EEPM
{

	//Partial Public Class EEGateway
	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class EEPMGWCCCyberSource : EEPMGWCCGenericBase
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################
		public EEPMGWCCCyberSource(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog) : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
		{
		}

		// ###################################################################################
		// Protected functions
		// ###################################################################################
		protected override bool GatewaySpecificCredit(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
			string strTransactionAmount = "";

			m_objLog.LogMessage("CyberSource: GatewaySpecificCredit.", 40);

			try {
				if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT")) {
					strTransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONID")) {
						strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
							m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
							m_objNSoftwareGW.InvoiceNumber = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
						m_objNSoftwareGW.Config("CyberSourceVoidMode=Void");
						// This line was added after nSoftware produced a "Hot fix" for the component to support "Void" (Credit)
						m_objNSoftwareGW.Refund(strTransactionID, strTransactionAmount);
						m_objLog.LogMessage(ScrubForLog("CyberSource: GatewaySpecificCredit: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
						objProperties.Clear();
					} else {
						m_intEEPGResponseCode = 98030;
						m_strEEPGResponseDescription = "Transaction ID must be specified for a Credit.";
						blnReturn = false;
					}
				} else {
					m_intEEPGResponseCode = 98029;
					m_strEEPGResponseDescription = "Amount must be specified for a Credit.";
					blnReturn = false;
				}
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98031;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				m_objLog.LogMessage(ScrubForLog("CyberSource: GatewaySpecificCredit: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
				blnReturn = false;
			}

			m_objLog.LogMessage("CyberSource: GatewaySpecificCredit: " + blnReturn, 40);

			return blnReturn;
		}

		protected override bool GatewaySpecificVoidTransaction(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";

			m_objLog.LogMessage("CyberSource: GatewaySpecificVoidTransaction.", 40);

			try {
				if (ContainKeyCheck(ref objProperties, "TRANSACTIONID")) {
					strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))
						m_objNSoftwareGW.TransactionAmount = ProcessKey(ref objProperties, "TRANSACTIONAMOUNT");
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
						m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
						m_objNSoftwareGW.InvoiceNumber = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
					m_objNSoftwareGW.Config("CyberSourceVoidMode=Reverse");
					// This line was added after nSoftware produced a "Hot fix" for the component to support "Full Authorization Reversal"
					m_objNSoftwareGW.VoidTransaction(strTransactionID);
					m_objLog.LogMessage(ScrubForLog("CyberSource: GatewaySpecificVoidTransaction: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
					objProperties.Clear();
				} else {
					m_intEEPGResponseCode = 98048;
					m_strEEPGResponseDescription = "Transaction ID must be specified for a Void.";
					blnReturn = false;
				}
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98049;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				m_objLog.LogMessage(ScrubForLog("CyberSource: GatewaySpecificVoidTransaction: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
				blnReturn = false;
			}

			m_objLog.LogMessage("CyberSource: GatewaySpecificVoidTransaction: " + blnReturn, 40);

			return blnReturn;
		}

		protected override bool GatewaySpecificMessageSetup(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("CyberSource: GatewaySpecificMessageSetup.", 40);

			try {
				if (ContainKeyCheck(ref objProperties, "ADDRESS2"))
					m_objNSoftwareCustomer.Address = m_objNSoftwareCustomer.Address + " " + ProcessKey(ref objProperties, "ADDRESS2");
			} catch (Exception err) {
				m_intEEPGResponseCode = 98300;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				blnReturn = false;
			}

			m_objLog.LogMessage("CyberSource: GatewaySpecificMessageSetup : " + blnReturn, 40);
			return blnReturn;
		}

		protected override bool SetGatewayCredentials(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			string strPassKey = "";
			int intIndex = 1;

			m_objLog.LogMessage("CyberSource: SetGatewayCredentials.", 40);

			try {
				do {
					if (ContainKeyCheck(ref objProperties, "EXP_PASSWORD_EXT_" + Convert.ToString(intIndex))) {
						strPassKey = strPassKey + ProcessKey(ref objProperties, "EXP_PASSWORD_EXT_" + Convert.ToString(intIndex));
					} else {
						break; // TODO: might not be correct. Was : Exit Do
					}
					intIndex = intIndex + 1;
				} while (true);
				if ((string.IsNullOrEmpty(strPassKey))) {
					m_intEEPGResponseCode = 99001;
					m_strEEPGResponseDescription = "You have set the EXP_PASSWORD_EXT_ additional keys incorrectly or not at all.";
					blnReturn = false;
				} else {
					m_objNSoftwareGW.Gateway = (IchargeGateways) m_intGatewayID;
					if ((!string.IsNullOrEmpty(m_strGatewayURL)))
						m_objNSoftwareGW.GatewayURL = m_strGatewayURL;
					m_objNSoftwareGW.MerchantLogin = m_strMerchantLogin;
					m_objNSoftwareGW.MerchantPassword = strPassKey;
				}
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98003;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool SetSpecialFields(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			//  Special Fields may be allowed for Cybersource, and in the future they may be added, but if they are not monitored, as to what gets
			//    added then users will received a 500 Internal Server Error from Cybersource, as they are incapable of dealing with unknown 
			//    input parameters.
			return (blnReturn);
		}

	}

}
