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
    public class EEPMGWCCGenericBase : EEPMGWBase
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################
        public EEPMGWCCGenericBase(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog)
            : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
        {
        }

        // ###################################################################################
        // Public functions
        // ###################################################################################
        public override bool Authorize(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;

            m_objLog.LogMessage("Generic: Authorize.");

            AddToScrub(ref objProperties);
            if ((blnReturn))
                blnReturn = SetGWObject("AUTHORIZE");
            if ((blnReturn))
                blnReturn = SetGatewayCredentials(ref objProperties);
            if ((blnReturn))
                blnReturn = PrepareGatewayMessage(ref objProperties);
            if ((blnReturn))
                blnReturn = GatewaySpecificAuthorize(ref objProperties);
            if (!(blnReturn))
            {
                // If the function GatewaySpecificAuthorize has not been overriden then we make the generic call.
                // A non-overridden GatewaySpecificAuthorize is expected to return Error 98020
                if ((m_intEEPGResponseCode == 98020))
                {
                    m_intEEPGResponseCode = -1;
                    m_strEEPGResponseDescription = "";
                    blnReturn = true;
                    if ((blnReturn))
                    {
                        m_objLog.LogMessage("Generic: Authorize Calling.");
                        try
                        {
                            if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))
                            {
                                m_objNSoftwareGW.TransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
                                if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
                                    m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
                                if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
                                    m_objNSoftwareGW.InvoiceNumber = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
                                //'the next line moved to SagePay setGatewayCredentials;and last line .InvoiceNumber goes along since that TransDocNum filed will be cleared. 123014 LZ
                                //m_objNSoftwareGW.AddSpecialField("RelatedVendorTXCode", m_objNSoftwareGW.InvoiceNumber)

                                //m_objNSoftwareGW.MerchantLogin = m_strMerchantLogin
                                //m_objLog.LogMessage("Generic: MerchantLogin: " + m_objNSoftwareGW.MerchantLogin)
                                //m_objNSoftwareGW.MerchantPassword = m_strMerchantPassword
                                //m_objLog.LogMessage("Generic: MerchantPassword: " + m_objNSoftwareGW.MerchantPassword)
                                //m_objLog.LogMessage("Generic: GatewayURL: " + m_objNSoftwareGW.GatewayURL)
                                m_objNSoftwareGW.AuthOnly();
                                m_objLog.LogMessage(ScrubForLog("Generic: Authorize. Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                                objProperties.Clear();
                            }
                            else
                            {
                                m_intEEPGResponseCode = 98016;
                                m_strEEPGResponseDescription = "Amount must be specified for an Authorization.";
                                blnReturn = false;
                            }
                        }
                        catch (System.Exception err)
                        {
                            m_intEEPGResponseCode = 98025;
                            m_strEEPGResponseDescription = "Error:  " + err.Message;
                            m_objLog.LogMessage(ScrubForLog("Generic: Authorize: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                            blnReturn = false;
                        }
                    }
                }
            }
            if ((blnReturn))
                blnReturn = ReadGatewayResponse(ref objProperties);
            return blnReturn;
        }

        //'12/23/2013 adding  tokenize without sale (i.e.adding card and storing as a token) 
        public override string Tokenize(string strPlainText, ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            string strTokenGuid = "";
            m_objLog.LogMessage("Generic: Tokenize.");

            AddToScrub(ref objProperties);
            if ((blnReturn))
                blnReturn = SetGWObject("TOKENIZE");
            if ((blnReturn))
                blnReturn = SetGatewayCredentials(ref objProperties);
            if ((blnReturn))
                blnReturn = PrepareGatewayMessage(ref objProperties);
            if ((blnReturn))
            {
                strTokenGuid = GatewaySpecificTokenize(strPlainText, ref objProperties);
                blnReturn = (!string.IsNullOrEmpty(strTokenGuid));
            }

            //' TODO generic tokenize without sale (i.e.adding card and storing as a token) not implemented 12/23/2013
            //' Since we only see tokenization in SagePay; not enough info exists for creating a generic method.
            if (!(blnReturn))
            {
                //    'If the function GatewaySpecificTokenize has not been overriden then we make the generic call.
                //    'A non-overridden GatewaySpecificTokenize is expected to return Error 98501

                if ((m_intEEPGResponseCode == 98501))
                {
                    m_objLog.LogMessage("Generic: Tokenize Calling.");
                    m_intEEPGResponseCode = 98502;
                    // new error code 01/07/2014 LfZ
                    m_strEEPGResponseDescription = "Generic: Tokenize not Implemented (only 55 SagePay works for Tokenize).";
                    blnReturn = false;

                }
            }
            if ((blnReturn))
                blnReturn = ReadGatewayResponse(ref objProperties);
            if ((blnReturn))
                return strTokenGuid;
            return "ERROR!";
        }


        public override bool Capture(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            string strTransactionID = "";
            string strTransactionAmount = "";

            m_objLog.LogMessage("EEPMGWCCGenericBase: Capture(): Entering", 40);

            AddToScrub(ref objProperties);
            if ((blnReturn))
                blnReturn = SetGWObject("CAPTURE");
            if ((blnReturn))
                blnReturn = SetGatewayCredentials(ref objProperties);
            if ((blnReturn))
                blnReturn = PrepareGatewayMessage(ref objProperties);
            if ((blnReturn))
                blnReturn = GatewaySpecificCapture(ref objProperties);
            if (!(blnReturn))
            {
                // If the function GatewaySpecificCapture has not been overriden then we make the generic call.
                // A non-overridden GatewaySpecificCapture is expected to return Error 98021
                if ((m_intEEPGResponseCode == 98021))
                {
                    m_intEEPGResponseCode = -1;
                    m_strEEPGResponseDescription = "";
                    blnReturn = true;
                    if ((blnReturn))
                    {
                        try
                        {
                            if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))
                            {
                                strTransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
                                if (ContainKeyCheck(ref objProperties, "TRANSACTIONID"))
                                {
                                    strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
                                    if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
                                        m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
                                    if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
                                        m_objNSoftwareGW.InvoiceNumber = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
                                    m_objNSoftwareGW.Capture(strTransactionID, strTransactionAmount);
                                    m_objLog.LogMessage(ScrubForLog("Generic: Capture. Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                                    objProperties.Clear();
                                }
                                else
                                {
                                    m_intEEPGResponseCode = 98018;
                                    m_strEEPGResponseDescription = "Transaction ID must be specified for an Capture.";
                                    blnReturn = false;
                                }
                            }
                            else
                            {
                                m_intEEPGResponseCode = 98017;
                                m_strEEPGResponseDescription = "Amount must be specified for an Capture.";
                                blnReturn = false;
                            }
                        }
                        catch (System.Exception err)
                        {
                            m_intEEPGResponseCode = 98026;
                            m_strEEPGResponseDescription = "Error:  " + err.Message;
                            m_objLog.LogMessage(ScrubForLog("EEPMGWCCGenericBase: Capture: Exception: URL: " + Convert.ToString(m_objNSoftwareGW.GatewayURL)), 50);
                            m_objLog.LogMessage(ScrubForLog("EEPMGWCCGenericBase: Capture: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                            blnReturn = false;
                        }
                    }
                }
            }
            if ((blnReturn))
                blnReturn = ReadGatewayResponse(ref objProperties);
            m_objLog.LogMessage("EEPMGWCCGenericBase: Capture(): Exiting: " + blnReturn, 40);
            return blnReturn;
        }

        public override bool DirectSale(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            //string strTransactionAmount = "";

            AddToScrub(ref objProperties);
            if ((blnReturn))
                blnReturn = SetGWObject("DIRECT");
            if ((blnReturn))
                blnReturn = SetGatewayCredentials(ref objProperties);
            if ((blnReturn))
                blnReturn = PrepareGatewayMessage(ref objProperties);
            if ((blnReturn))
                blnReturn = GatewaySpecificDirectSale(ref objProperties);
            if (!(blnReturn))
            {
                // If the function GatewaySpecificDirectSale has not been overriden then we make the generic call.
                // A non-overridden GatewaySpecificDirectSale is expected to return Error 98022
                if ((m_intEEPGResponseCode == 98022))
                {
                    m_intEEPGResponseCode = -1;
                    m_strEEPGResponseDescription = "";
                    blnReturn = true;
                    if ((blnReturn))
                    {
                        try
                        {
                            if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))
                            {
                                m_objNSoftwareGW.TransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
                                if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
                                    m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
                                if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
                                    m_objNSoftwareGW.InvoiceNumber = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
                                m_objNSoftwareGW.Sale();
                                m_objLog.LogMessage(ScrubForLog("Generic: DirectSale. Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                                objProperties.Clear();
                            }
                            else
                            {
                                m_intEEPGResponseCode = 98027;
                                m_strEEPGResponseDescription = "Amount must be specified for an Direct Sale.";
                                blnReturn = false;
                            }
                        }
                        catch (System.Exception err)
                        {
                            m_intEEPGResponseCode = 98028;
                            m_strEEPGResponseDescription = "Error:  " + err.Message;
                            m_objLog.LogMessage("Generic: DirectSale. Exception: " + err.Message, 1);
                            m_objLog.LogMessage(ScrubForLog("Generic: DirectSale. Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                            blnReturn = false;
                        }
                    }
                }
            }
            if ((blnReturn))
                blnReturn = ReadGatewayResponse(ref objProperties);
            return blnReturn;
        }

        public override bool Credit(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            string strTransactionID = "";
            string strTransactionAmount = "";

            AddToScrub(ref objProperties);
            if ((blnReturn))
                blnReturn = SetGWObject("CREDIT");
            if ((blnReturn))
                blnReturn = SetGatewayCredentials(ref objProperties);
            if ((blnReturn))
                blnReturn = PrepareGatewayMessage(ref objProperties);
            if ((blnReturn))
                blnReturn = GatewaySpecificCredit(ref objProperties);
            if (!(blnReturn))
            {
                // If the function GatewaySpecificCredit has not been overriden then we make the generic call.
                // A non-overridden GatewaySpecificCredit is expected to return Error 98023
                if ((m_intEEPGResponseCode == 98023))
                {
                    m_intEEPGResponseCode = -1;
                    m_strEEPGResponseDescription = "";
                    blnReturn = true;
                    if ((blnReturn))
                    {
                        try
                        {
                            if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))
                            {
                                strTransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
                                if (ContainKeyCheck(ref objProperties, "TRANSACTIONID"))
                                {
                                    strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
                                    if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
                                        m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
                                    if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
                                        m_objNSoftwareGW.InvoiceNumber = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
                                    m_objNSoftwareGW.Refund(strTransactionID, strTransactionAmount);
                                    m_objLog.LogMessage(ScrubForLog("Generic: Credit: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                                    objProperties.Clear();
                                }
                                else
                                {
                                    m_intEEPGResponseCode = 98030;
                                    m_strEEPGResponseDescription = "Transaction ID must be specified for a Credit.";
                                    blnReturn = false;
                                }
                            }
                            else
                            {
                                m_intEEPGResponseCode = 98029;
                                m_strEEPGResponseDescription = "Amount must be specified for a Credit.";
                                blnReturn = false;
                            }
                        }
                        catch (System.Exception err)
                        {
                            m_intEEPGResponseCode = 98031;
                            m_strEEPGResponseDescription = "Error:  " + err.Message;
                            m_objLog.LogMessage(ScrubForLog("Generic: Credit: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                            blnReturn = false;
                        }
                    }
                }
            }
            if ((blnReturn))
                blnReturn = ReadGatewayResponse(ref objProperties);
            return blnReturn;
        }

        public override bool VoidTransaction(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            string strTransactionID = "";

            AddToScrub(ref objProperties);
            if ((blnReturn))
                blnReturn = SetGWObject("VOID");
            if ((blnReturn))
                blnReturn = SetGatewayCredentials(ref objProperties);
            if ((blnReturn))
                blnReturn = PrepareGatewayMessage(ref objProperties);
            if ((blnReturn))
                blnReturn = GatewaySpecificVoidTransaction(ref objProperties);
            if (!(blnReturn))
            {
                // If the function GatewaySpecificCredit has not been overriden then we make the generic call.
                // A non-overridden GatewaySpecificCredit is expected to return Error 98023
                if ((m_intEEPGResponseCode == 98024))
                {
                    m_intEEPGResponseCode = -1;
                    m_strEEPGResponseDescription = "";
                    blnReturn = true;
                    if ((blnReturn))
                    {
                        try
                        {
                            if (ContainKeyCheck(ref objProperties, "TRANSACTIONID"))
                            {
                                strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
                                if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))
                                    m_objNSoftwareGW.TransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
                                if (ContainKeyCheck(ref objProperties, "TRANSACTIONDESC"))
                                    m_objNSoftwareGW.TransactionDesc = ProcessKey(ref objProperties, "TRANSACTIONDESC");
                                if (ContainKeyCheck(ref objProperties, "TRANSACTIONDOCUMENTNUMBER"))
                                    m_objNSoftwareGW.InvoiceNumber = ProcessKey(ref objProperties, "TRANSACTIONDOCUMENTNUMBER");
                                m_objNSoftwareGW.VoidTransaction(strTransactionID);
                                m_objLog.LogMessage(ScrubForLog("Generic: Void: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                                objProperties.Clear();
                            }
                            else
                            {
                                m_intEEPGResponseCode = 98048;
                                m_strEEPGResponseDescription = "Transaction ID must be specified for a Void.";
                                blnReturn = false;
                            }
                        }
                        catch (System.Exception err)
                        {
                            m_intEEPGResponseCode = 98049;
                            m_strEEPGResponseDescription = "Error:  " + err.Message;
                            m_objLog.LogMessage(ScrubForLog("Generic: Void: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                            blnReturn = false;
                        }
                    }
                }
            }
            if ((blnReturn))
                blnReturn = ReadGatewayResponse(ref objProperties);
            return blnReturn;
        }

        // ###################################################################################
        // Protected functions
        // ###################################################################################
        protected override bool SetGatewayCredentials(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            try
            {
                m_objNSoftwareGW.Gateway = (IchargeGateways) m_intGatewayID;
                // Allow for a special key to change the default gateway URL.
                if ((ContainKeyCheck(ref objProperties, "!#GATEWAY")))
                {
                    m_objNSoftwareGW.GatewayURL = ProcessKey(ref objProperties, "!#GATEWAY");
                }
                else if ((!string.IsNullOrEmpty(m_strGatewayURL)))
                {
                    m_objNSoftwareGW.GatewayURL = m_strGatewayURL;
                }
                m_objNSoftwareGW.MerchantLogin = m_strMerchantLogin;
                m_objNSoftwareGW.MerchantPassword = m_strMerchantPassword;
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98003;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
                blnReturn = false;
            }
            return blnReturn;
        }

        protected override bool PrepareGatewayMessage(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;

            m_objLog.LogMessage("Generic: PrepareGatewayMessage.", 40);

            // Build the card object
            try
            {
                if (ContainKeyCheck(ref objProperties, "CCTYPE"))
                    m_objNSoftwareCard.CardType =  (TCardTypes) Convert.ToInt32(ProcessKey(ref objProperties, "CCTYPE"));
                if (ContainKeyCheck(ref objProperties, "CCNUMBER"))
                    m_objNSoftwareCard.Number = ProcessKey(ref objProperties, "CCNUMBER");
                if (ContainKeyCheck(ref objProperties, "CCEXPMONTH"))
                    m_objNSoftwareCard.ExpMonth = Convert.ToInt32(ProcessKey(ref objProperties, "CCEXPMONTH"));
                if (ContainKeyCheck(ref objProperties, "CCEXPYEAR"))
                    m_objNSoftwareCard.ExpYear = Convert.ToInt32(ProcessKey(ref objProperties, "CCEXPYEAR"));
                if (ContainKeyCheck(ref objProperties, "CCCCV"))
                    m_objNSoftwareCard.CVVData = ProcessKey(ref objProperties, "CCCCV");
                m_objNSoftwareGW.Card = m_objNSoftwareCard;
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98004;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
                blnReturn = false;
            }
            // Build the customer object
            try
            {
                if (ContainKeyCheck(ref objProperties, "FNAME"))
                    m_objNSoftwareCustomer.FirstName = ProcessKey(ref objProperties, "FNAME");
                if (ContainKeyCheck(ref objProperties, "LNAME"))
                    m_objNSoftwareCustomer.LastName = ProcessKey(ref objProperties, "LNAME");
                // Address number and Adress passed in seperately because certain gateways will require this.  They can override this function.
                // Address2 might also be passed, but there is no place for it in the default Customer object.  To use this override this function
                if ((ContainKeyCheck(ref objProperties, "ADDRESSNUMBER")) && (ContainKeyCheck(ref objProperties, "ADDRESS")))
                {
                    m_objNSoftwareCustomer.Address = ProcessKey(ref objProperties, "ADDRESSNUMBER") + " " + ProcessKey(ref objProperties, "ADDRESS");
                }
                if (ContainKeyCheck(ref objProperties, "CITY"))
                    m_objNSoftwareCustomer.City = ProcessKey(ref objProperties, "CITY");
                if (ContainKeyCheck(ref objProperties, "STATE"))
                    m_objNSoftwareCustomer.State = ProcessKey(ref objProperties, "STATE");
                if (ContainKeyCheck(ref objProperties, "ZIPCODE"))
                    m_objNSoftwareCustomer.Zip = ProcessKey(ref objProperties, "ZIPCODE");
                if (ContainKeyCheck(ref objProperties, "COUNTRYCODE"))
                    m_objNSoftwareCustomer.Country = ProcessKey(ref objProperties, "COUNTRYCODE");
                if (ContainKeyCheck(ref objProperties, "PHONE"))
                    m_objNSoftwareCustomer.Phone = ProcessKey(ref objProperties, "PHONE");
                if (ContainKeyCheck(ref objProperties, "EMAIL"))
                    m_objNSoftwareCustomer.Email = ProcessKey(ref objProperties, "EMAIL");
                if (ContainKeyCheck(ref objProperties, "CUSTOMERNUMBER"))
                    m_objNSoftwareCustomer.Id = ProcessKey(ref objProperties, "CUSTOMERNUMBER");
                m_objNSoftwareGW.Customer = m_objNSoftwareCustomer;
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98005;
                m_strEEPGResponseDescription = "Error:  "+ err.Message;
                blnReturn = false;
            }

            blnReturn = GatewaySpecificMessageSetup(ref objProperties);
            blnReturn = SetSpecialFields(ref objProperties);

            m_objLog.LogMessage("Generic: PrepareGatewayMessage : " + blnReturn, 40);

            return blnReturn;
        }

        protected override bool ReadGatewayResponse(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;

            m_objLog.LogMessage("Generic: ReadGatewayResponse.", 40);

            try
            {
                nsoftware.InPay.EPResponse objNSoftwareResponse = new nsoftware.InPay.EPResponse();
                objNSoftwareResponse = m_objNSoftwareGW.Response;

                m_objLog.LogMessage("Generic: Response.Data: " + objNSoftwareResponse.Data, 35);

                if (!(objNSoftwareResponse.Approved))
                {
                    // The two following variables are included to log someday
                    m_strGatewayResponseCode = objNSoftwareResponse.ErrorCode;
                    m_strGatewayResponseRawData = objNSoftwareResponse.Data;
                    //m_strGatewayResponseDescription = objNSoftwareResponse.ErrorText
                    m_strGatewayResponseDescription = objNSoftwareResponse.Text;
                    // The two following variables are what is sent back to the caller
                    m_intEEPGResponseCode = 98013;
                    m_strEEPGResponseDescription = "Error:  " + objNSoftwareResponse.Code + " : " + objNSoftwareResponse.Text;
                    m_objLog.LogMessage("Generic: ReadGatewayResponse: ResponseCode: " + m_strGatewayResponseCode, 50);
                    m_objLog.LogMessage("Generic: ReadGatewayResponse: ResponseText: " + m_strGatewayResponseDescription, 50);
                    m_objLog.LogMessage("Generic: ReadGatewayResponse: ResponseErrorText: " + objNSoftwareResponse.ErrorText, 50);
                    m_objLog.LogMessage("Generic: ReadGatewayResponse: ResponseRawData: " + m_strGatewayResponseRawData, 50);
                    blnReturn = false;
                }
                else
                {
                    objProperties.Add("TRANSACTIONID", objNSoftwareResponse.TransactionId);
                    objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult);
                }
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98014;
                m_strEEPGResponseDescription = "Error:  "+ err.Message;
                blnReturn = false;
            }

            m_objLog.LogMessage("Generic: ReadGatewayResponse: " + blnReturn, 40);

            return blnReturn;
        }

        protected override bool SetSpecialFields(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            try
            {
                if (objProperties.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvPair in objProperties)
                    {
                        if ((kvPair.Key != "TRANSACTIONID") && (kvPair.Key != "TRANSACTIONAMOUNT") && (kvPair.Key != "TRANSACTIONDESC") && (kvPair.Key != "TRANSACTIONDOCUMENTNUMBER") && (kvPair.Key != "TRANSACTIONORIGINALAMOUNT") && (kvPair.Key != "RELATEDTRANSACTIONDOCUMENTNUMBER"))
                        {
                            // 01092014 LfZ "RELATEDTRANSACTIONDOCUMENTNUMBER" added for credit (nsoftware refund(tx,amount) for sagepay )

                            m_objLog.LogMessage("Generic: SetSpecialFields(): " + Convert.ToString(kvPair.Key) + "-" + Convert.ToString(kvPair.Value), 100);

                            m_objNSoftwareGW.AddSpecialField(Convert.ToString(kvPair.Key), Convert.ToString(kvPair.Value));
                        }
                    }
                }
                //objProperties.Clear() ' 
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98015;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
                blnReturn = false;
            }
            return blnReturn;
        }

        protected override bool SetGWObject(string strCase)
        {
            bool blnReturn = true;
            try
            {
                switch (strCase)
                {
                    default:
                        m_objNSoftwareGW = new Icharge();
                        m_objNSoftwareCard = new EPCard();
                        m_objNSoftwareCustomer = new EPCustomer();
                        m_objNSoftwareGW.RuntimeLicense = "42504E3641413153554252413153554243483945353033300000000000000000000000000000000058584436334D594500005357595253564B4D5A5338580000";
                        // Version 6 license
                        break;
                    // "42504E354141315355425241315355424348394535303330000000000000000000000000000000004B5655345848355000003733583348365758435A4A4D0000" ' Version 5 license
                }
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98062;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
                blnReturn = false;
            }

            return blnReturn;
        }


    }

}
