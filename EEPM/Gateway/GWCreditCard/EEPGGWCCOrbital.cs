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
	public class EEPMGWCCOrbital : EEPMGWCCGenericBase
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################
		public EEPMGWCCOrbital(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog) : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
		{
		}

		// ###################################################################################
		// Protected functions
		// ###################################################################################
		protected override bool SetGatewayCredentials(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			try {
                m_objNSoftwareGW.Gateway = (IchargeGateways) m_intGatewayID;
				if ((!string.IsNullOrEmpty(m_strGatewayURL)))
					m_objNSoftwareGW.GatewayURL = m_strGatewayURL;
				m_objNSoftwareGW.MerchantLogin = m_strMerchantLogin;
				m_objNSoftwareGW.MerchantPassword = m_strMerchantPassword;
				// Default terminal in orbital is 001.  If another terminal must be set then set the property TERMINALNUMBER to this value.
				if (ContainKeyCheck(ref objProperties, "TERMINALNUMBER")) {
					m_objNSoftwareGW.Config("TerminalID=" + ProcessKey(ref objProperties, "TERMINALNUMBER"));
				}

			//' ''OrbitalConnectionPassword:   Orbital Connection Password field used by the Orbital gateway.
			//' ''If OrbitalConnectionPassword is set, the OrbitalConnectionUsername should also be set. 
			//' ''OrbitalConnectionUsername :   Orbital Connection Username field used by the Orbital gateway.
			// ''If ContainKeyCheck(ref objProperties, "ORBITALCONNECTIONPASSWORD") Then
			// ''    m_objNSoftwareGW.Config("OrbitalConnectionPassword=" + ProcessKey(ref objProperties, "ORBITALCONNECTIONPASSWORD"))
			// ''End If
			// ''If ContainKeyCheck(ref objProperties, "ORBITALCONNECTIONUSERNAME") Then
			// ''    m_objNSoftwareGW.Config("OrbitalConnectionUsername=" + ProcessKey(ref objProperties, "ORBITALCONNECTIONUSERNAME"))
			// ''End If

			} catch (System.Exception err) {
				m_intEEPGResponseCode = 98003;
				m_strEEPGResponseDescription = "Error:  "+ err.Message;
				blnReturn = false;
			}
			return blnReturn;
		}

        //C# version: created 02212014 LfZ
        //this version is on BarclayEDPQ, EWay, Obital, Ogone
        //"12,345.605" -> "1234561" ;"123" -> "12300" etc..
		protected override string FormatAmount(string strAmount)
		{
			string strReturn = strAmount;
            m_objLog.LogMessage("EEPGGWCCOrbital: FormatAmount()" + strReturn, 40);
			strReturn = strReturn.Replace(" ", "");
			strReturn = strReturn.Replace( ",", "");
            int intAmount = 0;
            intAmount = Convert.ToInt32(100 * Math.Round(Convert.ToDecimal(strReturn), 2, MidpointRounding.AwayFromZero));
            strReturn = Convert.ToString(intAmount);

            m_objLog.LogMessage("EEPGGWCCOrbital: FormatAmount()" + strReturn, 40);
			return strReturn;
		}

	}

}
