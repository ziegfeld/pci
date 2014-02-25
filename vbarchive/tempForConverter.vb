catch (System.Exception err)
m_strEEPGResponseDescription = "Error:  " + err.Message;

(IchargeGateways) m_intGatewayID

	m_strVersion = m_strVersion.Replace(".", "");
	m_strProductKey = m_strProductKey.Replace("-", "");
	
	m_strVersion.Length
			
			
			
strReturn = strReturn.Replace(" ", "");
strReturn = strReturn.Replace( ",", "");
strReturn = Convert.ToString(Math.Round(Convert.ToDecimal(strReturn), 2, MidpointRounding.AwayFromZero));

if ((strReturn.IndexOf(".") == (strReturn.Length - 2)) & (strReturn.Length != 0))



.Replace("nsoftware", "PCICharge")

            if ((strYear.Length > 2))
				strYear = strYear.Substring(strYear.Length - 2, 2);


(Strings.Mid(strSpecificString, 72, 4)   ->     strSpecificString.Substring(71,4) 

int intChar = int.Parse( tmp, System.Globalization.NumberStyles.HexNumber);

added 02242014 LfZ

string[] arrPathPieces = strCurrentPath.Split(new[]{"\\"},StringSplitOptions.None);

strEntryName.Substring(0,11)

	for (int intIndex = 0; intIndex < strKeyValue.Length / 2; intIndex++)
	{
		int intChar = int.Parse(strKeyValue.Substring(intIndex * 2, 2), System.Globalization.NumberStyles.HexNumber);
		intChar = intChar ^ 111;
		strReturn += Convert.ToChar(intChar);
	}

string strChar = intChar.ToString("X").PadLeft(2,'0');                
strReturn += strChar.Substring(strChar.Length - 2, 2);
//strReturn += Strings.Right("0" + Conversion.Hex(intChar), 2);


protected Directpayment m_objNSoftwareGW;
protected Reauthcapture m_objNSoftwareGWPPRefund;
protected Refundtransaction m_objNsoftwareGWPPTx;

        Convert.ToString(DateAndTime.Now().Millisecond).PadLeft(4, "0");