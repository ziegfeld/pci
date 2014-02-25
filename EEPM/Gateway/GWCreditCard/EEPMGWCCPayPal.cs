using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using nsoftware;
using nsoftware.Sys;
using nsoftware.InPayPal;

using System.Runtime.InteropServices;

namespace EEPM
{

	//Partial Public Class EEGateway
	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class EEPMGWCCPayPal : EEPMGWCCGenericBase
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################
        public EEPMGWCCPayPal(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog, bool blnInitGWObject = true, bool blnInitGWCardObject = true, bool blnInitGWCustomerObject = true) :
            base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
		{
            
		}

		// ###################################################################################
		// Public functions
		// ###################################################################################
		protected override bool GatewaySpecificAuthorize(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			try {
				if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT")) {
					m_objNSoftwareGW.OrderTotal = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
						m_objNSoftwareGW.OrderDescription = ProcessKey(ref objProperties, "TRANSACTIONDESC");
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
						m_objNSoftwareGW.InvoiceNumber = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
					m_objNSoftwareGW.Authorize();
					objProperties.Clear();
				} else {
					m_intEEPGResponseCode = 98040;
					m_strEEPGResponseDescription = "Amount must be specified for an Authorization.";
					blnReturn = false;
				}
			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98041;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool GatewaySpecificCapture(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
			string strTransactionAmount = "";
			string strTransactionOriginalAmount = "";

			try {
				if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT")) {
					strTransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONID")) {
						strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
                            m_objNSoftwareGWPPRefund.Note = ProcessKey(ref objProperties, "TRANSACTIONDESC");
                            //m_objNSoftwareGW.OrderDescription = ProcessKey(ref objProperties, "TRANSACTIONDESC");
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
							ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONORIGINALAMOUNT")) {
							strTransactionOriginalAmount = ProcessKey(ref objProperties, "TRANSACTIONORIGINALAMOUNT");
							if ((Convert.ToDouble(strTransactionAmount) < Convert.ToDouble(strTransactionOriginalAmount))) {
								m_objNSoftwareGWPPRefund.IsPartialCapture = true;
							} else {
                                m_objNSoftwareGWPPRefund.IsPartialCapture = false;
							}
						}
                        m_objNSoftwareGWPPRefund.Capture(strTransactionID, strTransactionAmount);
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
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98044;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool GatewaySpecificDirectSale(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
            //string strTransactionAmount = "";
			try {
				if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT")) {
					m_objNSoftwareGW.OrderTotal = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
						m_objNSoftwareGW.OrderDescription = ProcessKey(ref objProperties, "TRANSACTIONDESC");
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
						m_objNSoftwareGW.InvoiceNumber = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
					m_objNSoftwareGW.Sale();
					objProperties.Clear();
				} else {
					m_intEEPGResponseCode = 98045;
					m_strEEPGResponseDescription = "Amount must be specified for an Direct Sale.";
					blnReturn = false;
				}
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98046;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool GatewaySpecificCredit(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
			string strTransactionAmount = "";
			string strTransactionOriginalAmount = "";

			try {
				if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT")) {
					strTransactionAmount = ProcessKey(ref objProperties, "TRANSACTIONAMOUNT");
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONID")) {
						strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
                            m_objNSoftwareGWPPTx.Memo = ProcessKey(ref objProperties, "TRANSACTIONDESC");
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
							ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
						if (ContainKeyCheck(ref objProperties, "TRANSACTIONORIGINALAMOUNT")) {
							strTransactionOriginalAmount = ProcessKey(ref objProperties, "TRANSACTIONORIGINALAMOUNT");
							if ((Convert.ToDouble(strTransactionAmount) < Convert.ToDouble(strTransactionOriginalAmount))) {
                                m_objNSoftwareGWPPTx.RefundType = RefundtransactionRefundTypes.rtPartial;
							} else {
                                m_objNSoftwareGWPPTx.RefundType = RefundtransactionRefundTypes.rtFull;
							}
						}
                        m_objNSoftwareGWPPTx.Refund(strTransactionID);
						objProperties.Clear();
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
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98052;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool GatewaySpecificVoidTransaction(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			string strTransactionID = "";
			//string strTransactionAmount = "";

			try {
				if (ContainKeyCheck(ref objProperties, "TRANSACTIONID")) {
					strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))
						ProcessKey(ref objProperties, "TRANSACTIONAMOUNT");
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
                        m_objNSoftwareGWPPRefund.Note = ProcessKey(ref objProperties, "TRANSACTIONDESC");
					if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
						ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
                    m_objNSoftwareGWPPRefund.VoidTransaction(strTransactionID);
					objProperties.Clear();
				} else {
					m_intEEPGResponseCode = 98050;
					m_strEEPGResponseDescription = "Transaction ID must be specified for a Void.";
					blnReturn = false;
				}
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98051;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}

		// ###################################################################################
		// Protected functions
		// ###################################################################################
		protected override bool SetGatewayCredentials(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			if ((objProperties.ContainsKey("PPSIGNATURE"))) {
				try {
					m_objNSoftwareGW.URL = m_strGatewayURL;
					m_objNSoftwareGW.User = m_strMerchantLogin;
					m_objNSoftwareGW.Password = m_strMerchantPassword;
					m_objNSoftwareGW.Signature = objProperties["PPSIGNATURE"];
                }
                catch (System.Exception err)
                {
					m_intEEPGResponseCode = 98054;
                    m_strEEPGResponseDescription = "Error:  " + err.Message;
					blnReturn = false;
				}
			} else {
				m_intEEPGResponseCode = 98055;
				m_strEEPGResponseDescription = "Needed Key (PPSIGNATURE) not set.";
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool PrepareGatewayMessage(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			// Build the card object
			if ((m_objNSoftwareCard != null)) {
				try {
					if (ContainKeyCheck(ref objProperties, "CCTYPE"))
						m_objNSoftwareCard.CardType =  (CardTypes) Convert.ToInt32(ProcessKey(ref objProperties, "CCTYPE"));
					if (ContainKeyCheck(ref objProperties, "CCNUMBER"))
						m_objNSoftwareCard.Number = ProcessKey(ref objProperties, "CCNUMBER");
					if (ContainKeyCheck(ref objProperties, "CCEXPMONTH"))
						m_objNSoftwareCard.ExpMonth = Convert.ToInt32(ProcessKey(ref objProperties, "CCEXPMONTH"));
					if (ContainKeyCheck(ref objProperties, "CCEXPYEAR"))
						m_objNSoftwareCard.ExpYear = Convert.ToInt32(ProcessKey(ref objProperties, "CCEXPYEAR"));
					if (ContainKeyCheck(ref objProperties, "CCCCV"))
						m_objNSoftwareCard.CVV = ProcessKey(ref objProperties, "CCCCV");
					m_objNSoftwareGW.Card = m_objNSoftwareCard;
				} catch (System.Exception err)
                {
					m_intEEPGResponseCode = 98056;
                    m_strEEPGResponseDescription = "Error:  " + err.Message;
					blnReturn = false;
				}
			} else {
				if (ContainKeyCheck(ref objProperties, "CCTYPE"))
					ProcessKey(ref objProperties, "CCTYPE");
				if (ContainKeyCheck(ref objProperties, "CCNUMBER"))
					ProcessKey(ref objProperties, "CCNUMBER");
				if (ContainKeyCheck(ref objProperties, "CCEXPMONTH"))
					ProcessKey(ref objProperties, "CCEXPMONTH");
				if (ContainKeyCheck(ref objProperties, "CCEXPYEAR"))
					ProcessKey(ref objProperties, "CCEXPYEAR");
				if (ContainKeyCheck(ref objProperties, "CCCCV"))
					ProcessKey(ref objProperties, "CCCCV");
			}
			// Build the customer object
			if ((m_objNSoftwareCustomer != null)) {
				try {
					if (ContainKeyCheck(ref objProperties, "FNAME"))
						m_objNSoftwareCustomer.FirstName = ProcessKey(ref objProperties, "FNAME");
					if (ContainKeyCheck(ref objProperties, "LNAME"))
						m_objNSoftwareCustomer.LastName = ProcessKey(ref objProperties, "LNAME");
					// Address number and Adress passed in seperately because certain gateways will require this.  They can override this function.
					// Address2 might also be passed, but there is no place for it in the default Customer object.  To use this override this function
					if ((ContainKeyCheck(ref objProperties, "ADDRESSNUMBER")) && (ContainKeyCheck(ref objProperties, "ADDRESS"))) {
						m_objNSoftwareCustomer.Street1 = ProcessKey(ref objProperties, "ADDRESSNUMBER") + " " + ProcessKey(ref objProperties, "ADDRESS");
					}
					if (ContainKeyCheck(ref objProperties, "CITY"))
						m_objNSoftwareCustomer.City = ProcessKey(ref objProperties, "CITY");
					if (ContainKeyCheck(ref objProperties, "STATE"))
						m_objNSoftwareCustomer.State = ProcessKey(ref objProperties, "STATE");
					if (ContainKeyCheck(ref objProperties, "ZIPCODE"))
						m_objNSoftwareCustomer.Zip = ProcessKey(ref objProperties, "ZIPCODE");
					if (ContainKeyCheck(ref objProperties, "COUNTRYCODE"))
						m_objNSoftwareCustomer.CountryCode = ProcessKey(ref objProperties, "COUNTRYCODE");
					if (ContainKeyCheck(ref objProperties, "EMAIL"))
						m_objNSoftwareCustomer.Email = ProcessKey(ref objProperties, "EMAIL");
					m_objNSoftwareGW.Payer = m_objNSoftwareCustomer;
				} catch (System.Exception err) {
					m_intEEPGResponseCode = 98057;
					m_strEEPGResponseDescription = "Error:  "+ err.Message;
					blnReturn = false;
				}
			} else {
				if (ContainKeyCheck(ref objProperties, "FNAME"))
					ProcessKey(ref objProperties, "FNAME");
				if (ContainKeyCheck(ref objProperties, "ADDRESSNUMBER"))
					ProcessKey(ref objProperties, "ADDRESSNUMBER");
				if (ContainKeyCheck(ref objProperties, "ADDRESS"))
					ProcessKey(ref objProperties, "ADDRESS");
				if (ContainKeyCheck(ref objProperties, "CITY"))
					ProcessKey(ref objProperties, "CITY");
				if (ContainKeyCheck(ref objProperties, "STATE"))
					ProcessKey(ref objProperties, "STATE");
				if (ContainKeyCheck(ref objProperties, "ZIPCODE"))
					ProcessKey(ref objProperties, "ZIPCODE");
				if (ContainKeyCheck(ref objProperties, "COUNTRYCODE"))
					ProcessKey(ref objProperties, "COUNTRYCODE");
				if (ContainKeyCheck(ref objProperties, "EMAIL"))
					ProcessKey(ref objProperties, "EMAIL");
			}

			blnReturn = GatewaySpecificMessageSetup(ref objProperties);
			blnReturn = SetSpecialFields(ref objProperties);

			return blnReturn;
		}

		protected override bool ReadGatewayResponse(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			try {
                if (m_objNSoftwareGWPPRefund != null)
                {
                    Payment objNSoftwareResponse = new nsoftware.InPayPal.Payment();
                    if ((m_objNSoftwareGW.Ack != "Success") && (m_objNSoftwareGW.Ack != "SuccessWithWarning"))
                    {
                        // The two following variables are included to log someday
                        m_strGatewayResponseCode = "999999";
                        m_strGatewayResponseRawData = m_objNSoftwareGW.Ack;
                        m_strGatewayResponseDescription = "Failed request from PayPal";
                        // The two following variables are what is sent back to the caller
                        m_intEEPGResponseCode = 98058;
                        m_strEEPGResponseDescription = "Error:  " + m_objNSoftwareGW.Ack;
                        blnReturn = false;
                    }
                    else
                    {
                        objProperties.Add("TRANSACTIONID", objNSoftwareResponse.TransactionId);
                        objProperties.Add("TRANSACTIONAMOUNT", objNSoftwareResponse.GrossAmount);
                        if ((m_objNSoftwareGWPPRefund.Ack == "SuccessWithWarning"))
                        {
                            m_intEEPGResponseCode = 98060;
                            m_strEEPGResponseDescription = "Warning:  " + m_objNSoftwareGWPPRefund.Ack;
                        }
                        //objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult)
                    }
				} else {
                    //Response objNSoftwareResponse = new nsoftware.InPayPal.Response();
                    if (m_objNSoftwareGWPPTx != null)
                    {
                        RefundResponse objNSoftwareResponse = m_objNSoftwareGWPPTx.Response;
                        if ((m_objNSoftwareGWPPTx.Ack != "Success") && (m_objNSoftwareGWPPTx.Ack != "SuccessWithWarning"))
                        {
                            // The two following variables are included to log someday
                            m_strGatewayResponseCode = "999999";
                            m_strGatewayResponseRawData = m_objNSoftwareGW.Ack;
                            m_strGatewayResponseDescription = "Failed request from PayPal";
                            // The two following variables are what is sent back to the caller
                            m_intEEPGResponseCode = 98058;
                            m_strEEPGResponseDescription = "Error:  " + m_objNSoftwareGW.Ack;
                            blnReturn = false;
                        }
                        else
                        {                            
                            objProperties.Add("TRANSACTIONID", objNSoftwareResponse.TransactionId);
                            objProperties.Add("TRANSACTIONAMOUNT", objNSoftwareResponse.GrossAmount);
                            if ((m_objNSoftwareGW.Ack == "SuccessWithWarning"))
                            {
                                m_intEEPGResponseCode = 98060;
                                m_strEEPGResponseDescription = "Warning: SuccessWithWarning "; // + m_objNSoftwareGWPPTx.Ack; 02242014 LfZ
                            }
                            //objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult)
                        }
                    }
                    else
                    {
                        Response objNSoftwareResponse = m_objNSoftwareGW.Response;
                        if ((m_objNSoftwareGW.Ack != "Success") && (m_objNSoftwareGW.Ack != "SuccessWithWarning"))
                        {
                            // The two following variables are included to log someday
                            m_strGatewayResponseCode = "999999";
                            m_strGatewayResponseRawData = m_objNSoftwareGW.Ack;
                            m_strGatewayResponseDescription = "Failed request from PayPal";
                            // The two following variables are what is sent back to the caller
                            m_intEEPGResponseCode = 98058;
                            m_strEEPGResponseDescription = "Error:  " + m_objNSoftwareGW.Ack;
                            blnReturn = false;
                        }
                        else
                        {
                            objProperties.Add("TRANSACTIONID", objNSoftwareResponse.TransactionId);
                            objProperties.Add("TRANSACTIONAMOUNT", objNSoftwareResponse.Amount);
                            if ((m_objNSoftwareGW.Ack == "SuccessWithWarning"))
                            {
                                m_intEEPGResponseCode = 98060;
                                m_strEEPGResponseDescription = "Warning: SuccessWithWarning "; // + m_objNSoftwareGWPPTx.Ack; 02242014 LfZ
                            }
                            //objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult)
                        }
                    }
				}
				
			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98059;
				m_strEEPGResponseDescription = "Error:  "+ err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override bool SetSpecialFields(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			try {
				if (objProperties.Count > 0) {
					foreach (KeyValuePair<string, string> kvPair in objProperties) {
						if ((kvPair.Key != "TRANSACTIONID") && (kvPair.Key != "TRANSACTIONAMOUNT") && (kvPair.Key != "TRANSACTIONDESC") && (kvPair.Key != "TRANSACTIONDOCUMENTNUMBER") && (kvPair.Key != "TRANSACTIONORIGINALAMOUNT")) {
							m_objNSoftwareGW.AddCustomField(Convert.ToString(kvPair.Key), Convert.ToString(kvPair.Value));
						}
					}
				}
			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98061;
				m_strEEPGResponseDescription = "Error:  "+ err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}
                
		protected override bool SetGWObject(string strCase)
		{
			bool blnReturn = true;
			try {
				switch (strCase) {
                    case "PAYMENT":                        
                        m_objNSoftwareGW = new nsoftware.InPayPal.Directpayment(); //02212014 LfZ
                        m_objNSoftwareCard = new nsoftware.InPayPal.Card();
                        m_objNSoftwareCustomer = new nsoftware.InPayPal.DirectPaymentPayer();
                        break;
                        
                    case "TRANSACTION":                        
                        m_objNSoftwareGWPPRefund = new nsoftware.InPayPal.Reauthcapture();
                        break;
                        
                    case "REFUND":                        
                        m_objNSoftwareGWPPTx = new nsoftware.InPayPal.Refundtransaction();
                        break;
                        
					default:
						m_intEEPGResponseCode = 98038;
						m_strEEPGResponseDescription = "Unknown object type requested";
						blnReturn = false;
				}
				m_objNSoftwareGW.RuntimeLicense = "42314E334141315355425241315355424348394535303330000000000000000000000000000000004B56553458483550000054565346504D334247574D550000";
			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98062;
				m_strEEPGResponseDescription = "Error:  "+ err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}

		protected override void BuildSetGW()
		{
			m_objSetGW = new System.Collections.Generic.Dictionary<string, string>();
			m_objSetGW.Add("AUTHORIZE", "PAYMENT");
			m_objSetGW.Add("CAPTURE", "TRANSACTION");
			m_objSetGW.Add("DIRECT", "PAYMENT");
			m_objSetGW.Add("CREDIT", "REFUND");
			m_objSetGW.Add("VOID", "TRANSACTION");
		}

        // ###################################################################################
        // Protected variables
        // ###################################################################################

        protected new Directpayment m_objNSoftwareGW;
        protected Reauthcapture m_objNSoftwareGWPPRefund;
        protected Refundtransaction m_objNSoftwareGWPPTx;
        protected new Card m_objNSoftwareCard;
        protected new DirectPaymentPayer m_objNSoftwareCustomer;

	}

}
