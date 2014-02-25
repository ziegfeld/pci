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
    public class EEPMGWCCEWay : EEPMGWCCGenericBase
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################
        public EEPMGWCCEWay(int intGatewayID, string strGatewayURL, string strMerchantLogin, string strMerchantPassword, ref Enterprise.EELog objLog)
            : base(intGatewayID, strGatewayURL, strMerchantLogin, strMerchantPassword, ref objLog)
        {
        }

        // ###################################################################################
        // Protected functions
        // ###################################################################################

        //C# version: created 02212014 LfZ
        //this version is on BarclayEDPQ, EWay, Obital, Ogone
        //"12,345.605" -> "1234561" ;"123" -> "12300" etc..
		protected override string FormatAmount(string strAmount)
		{
			string strReturn = strAmount;
            m_objLog.LogMessage("EEPMGWCCEWay: FormatAmount()" + strReturn, 40);
			strReturn = strReturn.Replace(" ", "");
			strReturn = strReturn.Replace( ",", "");
            int intAmount = 0;
            intAmount = Convert.ToInt32(100 * Math.Round(Convert.ToDecimal(strReturn), 2, MidpointRounding.AwayFromZero));
            strReturn = Convert.ToString(intAmount);

            m_objLog.LogMessage("EEPMGWCCEWay: FormatAmount()" + strReturn, 40);
			return strReturn;
		}

    }

}
