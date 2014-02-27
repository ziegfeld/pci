using nsoftware.InPay;
using System;
using System.Runtime.InteropServices;

namespace EEPM
{

	//Partial Public Class EEGateway
	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class EEPGGWCCPayFlowPro : EEPMGWCCGenericBase
	{

		// ###################################################################################
		// Constructors\Destructors
		// ###################################################################################
		public EEPGGWCCPayFlowPro(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog) : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
		{
		}

		// ###################################################################################
		// Protected functions
		// ###################################################################################
		protected override bool SetGatewayCredentials(ref System.Collections.Generic.Dictionary<string, string> objProperties)
		{
			bool blnReturn = true;
			m_objLog.LogMessage("PayFlowPro: SetGatewayCredentials()", 40);

			try {
                m_objNSoftwareGW.Gateway = (IchargeGateways) m_intGatewayID;
				if ((!string.IsNullOrEmpty(m_strGatewayURL)))
					m_objNSoftwareGW.GatewayURL = m_strGatewayURL;
				m_objNSoftwareGW.MerchantLogin = m_strMerchantLogin;
				m_objNSoftwareGW.MerchantPassword = m_strMerchantPassword;
				// By default the MerchantLogin is the "Merchant".  We still need to setup the USER and override the Partner from Navision.
				if (!(ContainKeyCheck(ref objProperties, "PARTNER")))
					throw new Exception("You are missing the PARTNER key in the Gateway Additional Key setup.");

                m_objNSoftwareGW.AddSpecialField("Partner", ProcessKey(ref objProperties, "PARTNER"));
                // above line added 02242014 after reading http://www.nsoftware.com/kb/help/BPN6-A/pg_icgatewaysetup.rst :
                //The default "Partner" special field is set to "PayPal". You may be required to change it depending on your account setup. 
                // If your User ID and Vendor ID (Merchant Login ID) are different, supply the Vendor ID to MerchantLogin and add the User ID
                // like so: AddSpecialField("USER","User ID Value").
				////m_objNSoftwareGW.SpecialFields(1).Value = ProcessKey(ref objProperties, "PARTNER"));
                // What is this??? ^^^^^^^^^^^^
                //'maybe it means m_objNSoftwareGW.AddSpecialField("1","CStr(ProcessKey(objProperties, "PARTNER"))") ??
                //'nsoftware.inpay.icharge.SpecialFields(1).Value ??? special field has only get, no set!
				if ((ContainKeyCheck(ref objProperties, "USER")))
					m_objNSoftwareGW.AddSpecialField("USER", ProcessKey(ref objProperties, "USER"));
            }
            catch (System.Exception err)
            {
				m_intEEPGResponseCode = 98003;
				m_strEEPGResponseDescription = "Error:  " + err.Message;
				blnReturn = false;
			}

			m_objLog.LogMessage("PayFlowPro: SetGatewayCredentials(): " + blnReturn, 40);

			return blnReturn;
		}

	}

}

