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
    public class EEPMGWCCFDGlobal : EEPMGWCCGenericBase
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################
        public EEPMGWCCFDGlobal(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog)
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
            m_objLog.LogMessage("FDGlobal: SetGatewayCredentials()", 40);
            nsoftware.InPay.Certificate objNSoftwareCertificate = default(nsoftware.InPay.Certificate);
            try
            {
                m_objNSoftwareGW.Gateway = (IchargeGateways)m_intGatewayID; 
                if ((!string.IsNullOrEmpty(m_strGatewayURL)))
                    m_objNSoftwareGW.GatewayURL = m_strGatewayURL;
                m_objLog.LogMessage("FDGlobal: GatewayURL: " + m_strGatewayURL);
                m_objNSoftwareGW.MerchantLogin = m_strMerchantLogin;
                m_objLog.LogMessage("FDGlobal: MerchantLogin: " + m_strMerchantLogin);
                m_objNSoftwareGW.MerchantPassword = m_strMerchantPassword;
                m_objLog.LogMessage("FDGlobal: MerchantPassword: " + m_strMerchantPassword);
                if (ContainKeyCheck(ref objProperties, "SSLCERT"))
                {
                    objNSoftwareCertificate = new nsoftware.InPay.Certificate(nsoftware.InPay.CertStoreTypes.cstPEMKeyFile, ProcessKey(ref objProperties, "SSLCERT"), "", "*");
                    m_objLog.LogMessage("FDGlobal: SSLCert Subject: " + objNSoftwareCertificate.Subject, 35);
                    m_objNSoftwareGW.SSLCert = objNSoftwareCertificate;
                    if ((string.IsNullOrEmpty(objNSoftwareCertificate.Subject)))
                    {
                        throw new Exception("Failed to load SSLCert file.");
                    }
                }
                else
                {
                    string strAppPath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".dll", "");
                    strAppPath = strAppPath + m_strMerchantLogin + ".pem";
                    m_objLog.LogMessage("FDGlobal: SetGatewayCredentials: AppPath: " + strAppPath, 35);
                    objNSoftwareCertificate = new nsoftware.InPay.Certificate(nsoftware.InPay.CertStoreTypes.cstPEMKeyFile, Convert.ToString(strAppPath), "", "*");
                    m_objLog.LogMessage("FDGlobal: SSLCert Subject: " + objNSoftwareCertificate.Subject, 35);
                    m_objNSoftwareGW.SSLCert = objNSoftwareCertificate;
                    if ((string.IsNullOrEmpty(objNSoftwareCertificate.Subject)))
                    {
                        throw new Exception("Failed to load SSLCert file.");
                    }
                    //Throw New Exception("SSLCert file information is missing. (file should be a .pem file.)")
                }
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98003;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
                blnReturn = false;
                m_objLog.LogMessage("FDGlobal: SetGatewayCredentials(): " + m_strEEPGResponseDescription, 1);
            }
            m_objLog.LogMessage("FDGlobal: SetGatewayCredentials(): " + blnReturn, 40);
            return blnReturn;
        }

        protected override bool ReadGatewayResponse(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;
            m_objLog.LogMessage("FDGlobal: ReadGatewayResponse()", 40);
            try
            {
                nsoftware.InPay.EPResponse objNSoftwareResponse = new nsoftware.InPay.EPResponse();
                objNSoftwareResponse = m_objNSoftwareGW.Response;

                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): ApprovalCode: " + objNSoftwareResponse.ApprovalCode, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): Approved: " + objNSoftwareResponse.Approved, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): AVSResult: " + objNSoftwareResponse.AVSResult, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): Code: " + objNSoftwareResponse.Code, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): CVVResult: " + objNSoftwareResponse.CVVResult, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): Data: " + objNSoftwareResponse.Data, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): ErrorCode: " + objNSoftwareResponse.ErrorCode, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): ErrorText: " + objNSoftwareResponse.ErrorText, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): InvoiceNumber: " + objNSoftwareResponse.InvoiceNumber, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): ProcessorCode: " + objNSoftwareResponse.ProcessorCode, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): Text: " + objNSoftwareResponse.Text, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): TransactionId: " + objNSoftwareResponse.TransactionId, 35);
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): Timestamp: " + m_objNSoftwareGW.GetResponseVar("/root/r_tdate"), 35);

                if (!(objNSoftwareResponse.Approved))
                {
                    // The two following variables are included to log someday
                    m_strGatewayResponseCode = objNSoftwareResponse.ErrorCode;
                    m_strGatewayResponseRawData = objNSoftwareResponse.Data;
                    m_strGatewayResponseDescription = objNSoftwareResponse.ErrorText;
                    // The two following variables are what is sent back to the caller
                    m_intEEPGResponseCode = 98013;
                    m_strEEPGResponseDescription = "Error:  " + objNSoftwareResponse.Code + " : " + objNSoftwareResponse.Text;
                    blnReturn = false;
                }
                else
                {
                    objProperties.Add("TRANSACTIONID", objNSoftwareResponse.InvoiceNumber);
                    objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult);
                    // This field was added to retrieve the timestamp of a transaction.
                    // This timestamp is needed for Voids of Authorizations to be possible.
                    if ((!string.IsNullOrEmpty(m_objNSoftwareGW.GetResponseVar("/root/r_tdate"))))
                        objProperties.Add("tdate", m_objNSoftwareGW.GetResponseVar("/root/r_tdate"));
                }
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98014;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
                blnReturn = false;
                m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): " + m_strEEPGResponseDescription, 1);
            }
            m_objLog.LogMessage("FDGlobal: ReadGatewayResponse(): " + blnReturn, 40);
            return blnReturn;
        }

    }

}
