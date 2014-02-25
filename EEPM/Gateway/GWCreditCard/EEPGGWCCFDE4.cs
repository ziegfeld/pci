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
    public class EEPMGWCCFDE4 : EEPMGWCCGenericBase
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################
        public EEPMGWCCFDE4(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog)
            : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
        {
        }

        // ###################################################################################
        // Protected functions
        // ###################################################################################

        //Protected Overrides Function GatewaySpecificVoidTransaction(ByRef objProperties As System.Collections.Generic.Dictionary(Of Object, Object)) As Boolean
        //    Dim blnReturn As Boolean = True

        //    m_intEEPGResponseCode = 98000
        //    m_strEEPGResponseDescription = "Error:  Void is currently unsupported for First Data."
        //    blnReturn = False

        //    Return blnReturn
        //End Function

        protected override bool SetGatewayCredentials(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            m_objLog.LogMessage("FirstDataE4: SetGatewayCredentials", 40);
            //Dim objNSoftwareCertificate As nsoftware.InPay.Certificate
            string strFullName = "";
            string strFDMSKeyID = "";
            string strHashSecret = "";
            try
            {
                m_objNSoftwareGW.Gateway = (IchargeGateways)m_intGatewayID; // 02212014 LfZ : or  m_objNSoftwareGW.Gateway = Enum.ToObject(typeof(IchargeGateways), m_intGatewayID);
                if ((!string.IsNullOrEmpty(m_strGatewayURL)))
                    m_objNSoftwareGW.GatewayURL = m_strGatewayURL;
                m_objLog.LogMessage("FirstDataE4: GatewayURL: " + m_strGatewayURL);
                m_objNSoftwareGW.MerchantLogin = m_strMerchantLogin;
                m_objLog.LogMessage("FirstDataE4: MerchantLogin: " + m_strMerchantLogin);
                m_objNSoftwareGW.MerchantPassword = m_strMerchantPassword;
                m_objLog.LogMessage("FirstDataE4: MerchantPassword: " + m_strMerchantPassword);

                //nSoft: The FullName is also required to be specified and an exception will be thrown if not set. 
                //FullName is transfered to Firstdata: CardHoldersName. FirstName and LastName is Not used.  02112014 LfZ
                //m_objNSoftwareCustomer.FullName = m_objNSoftwareCustomer.FirstName.ToString() + " " + m_objNSoftwareCustomer.LastName.ToString()
                //this function is called before PrepareGatewayMessage so that .firstName and .LastName is not set yet.
                if (ContainKeyCheck(ref objProperties, "FNAME"))
                    strFullName = ProcessKey(ref objProperties, "FNAME");
                if (ContainKeyCheck(ref objProperties, "LNAME"))
                {
                    if ((string.IsNullOrEmpty(strFullName)))
                    {
                        strFullName = ProcessKey(ref objProperties, "LNAME");
                    }
                    else
                    {
                        strFullName = strFullName + " " + ProcessKey(ref objProperties, "LNAME");
                    }
                }
                //If (strFullName = "") Then
                //    m_intEEPGResponseCode = 98530
                //    m_strEEPGResponseDescription = "Customer name not set: both FNAME and LAST name is null."
                //    m_objLog.LogMessage("FirstDataE4: SetGatewayCredentials(): Error: " + Err.Number + " : " + Err.Description, 50)
                //    blnReturn = False
                //Else
                m_objNSoftwareCustomer.FullName = strFullName;
                //End If

                //NSoft: AuthCode = FDE4 Transaction_Tag = NSoft.response(ApprovalCode) = NAV "TxAuthNo" 02122014 LfZ
                m_objNSoftwareGW.AuthCode = ProcessKey(ref objProperties, "TXAUTHNO");
                //NsoftGW.TransactionId = FDE4 Authorization_Num = NAV "TRANSACTIONID" will be handled in general Capture,Void,Refund()

                if ((!ContainKeyCheck(ref objProperties, "FDMSKEYID")))
                {
                    m_intEEPGResponseCode = 98531;
                    m_strEEPGResponseDescription = "FirstData Key ID must be specified; login to FirstData web admin portal - Terminals tab - select xxxECOMM - API Access tab, and copy that Key id (all numbers).";
                    m_objLog.LogMessage("FirstDataE4: SetGatewayCredentials(): Error: " + m_intEEPGResponseCode + " : " + m_strEEPGResponseDescription, 50);
                    blnReturn = false;
                }
                else
                {
                    strFDMSKeyID = ProcessKey(ref objProperties, "FDMSKEYID");
                    m_objNSoftwareGW.Config("FDMSKeyID=" + strFDMSKeyID);
                    m_objLog.LogMessage("FirstDataE4: FDMSKeyID: " + strFDMSKeyID, 35);
                    if ((!ContainKeyCheck(ref objProperties, "HASHSECRET")))
                    {
                        m_intEEPGResponseCode = 98532;
                        m_strEEPGResponseDescription = "FirstData HMAC Key ID must be specified; login to FirstData web admin portal - Terminals tab - select xxxECOMM - API Access tab, and copy that HMAC KEY (~32digit).";
                        m_objLog.LogMessage("FirstDataE4: SetGatewayCredentials(): Error: " + m_intEEPGResponseCode + " : " + m_strEEPGResponseDescription, 50);
                        blnReturn = false;
                    }
                    else
                    {
                        strHashSecret = ProcessKey(ref objProperties, "HASHSECRET");
                        m_objNSoftwareGW.Config("HASHSECRET=" + strHashSecret);
                        m_objLog.LogMessage("FirstDataE4: HASHSECRET: " + strHashSecret, 35);
                    }
                }

            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98533;
                m_strEEPGResponseDescription = "Error:  "+ err.Message;
                blnReturn = false;
                m_objLog.LogMessage("FirstDataE4: SetGatewayCredential: " + m_strEEPGResponseDescription, 1);
            }
            m_objLog.LogMessage("FirstDataE4: SetGatewayCredentials: " + blnReturn, 40);
            return blnReturn;
        }

        protected override bool ReadGatewayResponse(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse", 40);
            try
            {
                nsoftware.InPay.EPResponse objNSoftwareResponse = new nsoftware.InPay.EPResponse();
                objNSoftwareResponse = m_objNSoftwareGW.Response;

                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: ApprovalCode: " + objNSoftwareResponse.ApprovalCode, 35);
                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: Approved: " + objNSoftwareResponse.Approved, 35);
                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: AVSResult: " + objNSoftwareResponse.AVSResult, 35);
                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: Code: " + objNSoftwareResponse.Code, 35);
                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: CVVResult: " + objNSoftwareResponse.CVVResult, 35);
                m_strGatewayResponseRawData = objNSoftwareResponse.Data;
                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: ResponseRawData: " + m_strGatewayResponseRawData, 35);

                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: InvoiceNumber: " + objNSoftwareResponse.InvoiceNumber, 35);
                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: ProcessorCode: " + objNSoftwareResponse.ProcessorCode, 35);
                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: Text: " + objNSoftwareResponse.Text, 35);
                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: TransactionId: " + objNSoftwareResponse.TransactionId, 35);
                if ((!objNSoftwareResponse.Approved))
                {
                    // The two following variables are included to log someday
                    m_strGatewayResponseCode = objNSoftwareResponse.ErrorCode;

                    m_strGatewayResponseDescription = objNSoftwareResponse.ErrorText;
                    // The two following variables are what is sent back to the caller
                    m_intEEPGResponseCode = 98534;
                    m_strEEPGResponseDescription = "Error:  " + objNSoftwareResponse.Code + " : " + objNSoftwareResponse.Text;
                    m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: ErrorCode: " + m_strGatewayResponseCode, 50);
                    m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: ResponseText: " + m_strGatewayResponseDescription, 50);
                    m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: ErrorText: " + m_strGatewayResponseDescription, 50);

                    blnReturn = false;
                }
                else
                {
                    //NsoftGW.TransactionId = FDE4 Authorization_Num  = NAV "TRANSACTIONID" will be handled in general Capture,Void,Refund()
                    objProperties.Add("TRANSACTIONID", objNSoftwareResponse.TransactionId);
                    objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult);
                    //NSoft: AuthCode = FDE4 Transaction_Tag   = NSoft.response(ApprovalCode) = NAV "TxAuthNo" 02122014 LfZ
                    //ApprovalCode here = Transaction_Tag  which cam be used for tagged completion/void/refund.
                    if ((!string.IsNullOrEmpty(objNSoftwareResponse.ApprovalCode)))
                        objProperties.Add("TXAUTHNO", objNSoftwareResponse.ApprovalCode);

                }
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98535;
                m_strEEPGResponseDescription = "Error:  "+ err.Message;
                blnReturn = false;
                m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: " + m_strEEPGResponseDescription, 1);
            }
            m_objLog.LogMessage("FirstDataE4: ReadGatewayResponse: " + blnReturn, 40);
            return blnReturn;
        }

        protected override bool GatewaySpecificVoidTransaction(ref Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            string strTransactionID = "";
            //string strTransactionAmount = "";

            m_objLog.LogMessage("FirstDataE4: GatewaySpecificVoidTransaction.", 40);
            try
            {
                if ((!ContainKeyCheck(ref objProperties, "TRANSACTIONID")))
                {
                    m_intEEPGResponseCode = 98536;
                    m_strEEPGResponseDescription = "Transaction ID must be specified for a Credit. It is the TxID of the ORIGINAL tx, sent as VPSTxID in ABORT raw request. ";
                    blnReturn = false;
                }
                else
                {
                    strTransactionID = ProcessKey(ref objProperties, "TRANSACTIONID");
                    m_objNSoftwareGW.TransactionId = strTransactionID;
                    //NsoftGW.TransactionId = FDE4 Authorization_Num = NAV "TRANSACTIONID" 

                    // FDE4 Transaction_Tag  = NSoft.response(ApprovalCode) = NAV "TxAuthNo" 02122014 LfZ

                    if (ContainKeyCheck(ref objProperties, "TRANSACTIONAMOUNT"))
                        m_objNSoftwareGW.TransactionAmount = FormatAmount(ProcessKey(ref objProperties, "TRANSACTIONAMOUNT"));
                    // transaction description is not supported

                    m_objNSoftwareGW.VoidTransaction(strTransactionID);

                    m_objLog.LogMessage(ScrubForLog("FirstDataE4: GatewaySpecificVoidTransaction: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                    objProperties.Clear();
                    blnReturn = true;
                }
            }
            catch (System.Exception err)
            {
                strTransactionID = m_objNSoftwareGW.Config("FullRequest");
                m_intEEPGResponseCode = 98537;
                m_strEEPGResponseDescription = "Error:  "+ err.Message;
                m_objLog.LogMessage(ScrubForLog("FirstDataE4: GatewaySpecificVoidTransaction: Exception: Raw Request: " + m_objNSoftwareGW.Config("RawRequest")), 50);
                blnReturn = false;
            }
            return blnReturn;
        }

    }

}
