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

using System.Xml;
using System.Runtime.InteropServices;

namespace EEPM
{

    //Partial Public Class EEGateway
    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public class EEPMGWCCBarclayEDPQ : EEPMGWCCGenericBase
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################
        public EEPMGWCCBarclayEDPQ(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog)
            : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword,ref objLog)
        {
        }

        // ###################################################################################
        // Protected functions
        // ###################################################################################

        protected override bool GatewaySpecificMessageSetup(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: GatewaySpecificMessageSetup()", 40);

            try
            {
                if (ContainKeyCheck(ref objProperties, "CURRENCY"))
                    m_objNSoftwareGW.Config("CurrencyCode=" + ProcessKey(ref objProperties, "CURRENCY"));
                m_objLog.LogMessage("Config return (CurrencyCode): " + m_objNSoftwareGW.Config("CurrencyCode"), 35);
                if (ContainKeyCheck(ref objProperties, "USERID"))
                    m_objNSoftwareGW.Config("UserId=" + ProcessKey(ref objProperties, "USERID"));
                m_objLog.LogMessage("Config return (UserId): " + m_objNSoftwareGW.Config("UserId"), 35);
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98321;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
                m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: GatewaySpecificMessageSetup(): Exception: " + err.Message, 50);
                blnReturn = false;
            }

            m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: GatewaySpecificMessageSetup(): " + blnReturn, 40);

            return blnReturn;
        }

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

            m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: FormatAmount()" + strReturn, 40);
            return strReturn;
        }


        protected override bool ReadGatewayResponse(ref System.Collections.Generic.Dictionary<string, string> objProperties)
        {
            bool blnReturn = true;

            m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: ReadGatewayResponse.", 40);

            try
            {
                nsoftware.InPay.EPResponse objNSoftwareResponse = new nsoftware.InPay.EPResponse();
                objNSoftwareResponse = m_objNSoftwareGW.Response;

                m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: Response.Data: " + objNSoftwareResponse.Data, 35);
                m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: Response.Var: STATUS: " + m_objNSoftwareGW.GetResponseVar("STATUS"), 35);


                // A STATUS respone of 46 denotes a request for 3D secure.  We need to log the data as needed.
                if (!(objNSoftwareResponse.Approved) && (m_objNSoftwareGW.GetResponseVar("STATUS") != "46"))
                {
                    // The two following variables are included to log someday
                    m_strGatewayResponseCode = objNSoftwareResponse.ErrorCode;
                    m_strGatewayResponseRawData = objNSoftwareResponse.Data;
                    //m_strGatewayResponseDescription = objNSoftwareResponse.ErrorText
                    m_strGatewayResponseDescription = objNSoftwareResponse.Text;
                    // The two following variables are what is sent back to the caller
                    m_intEEPGResponseCode = 98013;
                    m_strEEPGResponseDescription = "Error:  " + objNSoftwareResponse.Code + " : " + objNSoftwareResponse.Text;
                    m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: ReadGatewayResponse: ResponseCode: " + m_strGatewayResponseCode, 50);
                    m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: ReadGatewayResponse: ResponseText: " + m_strGatewayResponseDescription, 50);
                    m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: ReadGatewayResponse: ResponseErrorText: " + objNSoftwareResponse.ErrorText, 50);
                    m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: ReadGatewayResponse: ResponseRawData: " + m_strGatewayResponseRawData, 50);
                    blnReturn = false;
                }
                else
                {
                    objProperties.Add("TRANSACTIONID", objNSoftwareResponse.TransactionId);
                    objProperties.Add("AVSRESULT", objNSoftwareResponse.AVSResult);
                    objProperties.Add("STATUS", m_objNSoftwareGW.GetResponseVar("STATUS"));
                    if ((m_objNSoftwareGW.GetResponseVar("STATUS") == "46"))
                    {
                        XmlNode objXMLNode = null;
                        XmlDocument objXMLDocument = new XmlDocument();
                        objXMLDocument.LoadXml(objNSoftwareResponse.Data);
                        objXMLNode = objXMLDocument.SelectSingleNode("/ogone/ncresponse/HTML_ANSWER");
                        if ((objXMLNode != null))
                        {
                            m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: Response.Var: HTML_ANSWER: objXMLNode.InnerXml: " + objXMLNode.InnerXml, 50);
                            objProperties.Add("HTML_ANSWER64", objXMLNode.InnerXml);
                            objProperties.Add("HTML_ANSWER", System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(objXMLNode.InnerXml)));
                            m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: Response.Var: HTML_ANSWER: " + objProperties["HTML_ANSWER"], 50);
                        }
                        else
                        {
                            m_intEEPGResponseCode = 98066;
                            m_strEEPGResponseDescription = "HTML_ANSWER node expected in gateway response, but not found.";
                            m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: ReadGatewayResponse: Error: " + m_strEEPGResponseDescription, 35);
                            blnReturn = false;
                        }
                    }
                }
            }
            catch (System.Exception err)
            {
                m_intEEPGResponseCode = 98014;
                m_strEEPGResponseDescription = "Error:  " + err.Message;
                m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: Exception: " + m_strEEPGResponseDescription, 35);
                blnReturn = false;
            }

            m_objLog.LogMessage("EEPMGWCCBarclayEDPQ: ReadGatewayResponse: " + blnReturn, 40);

            return blnReturn;
        }

    }

}
