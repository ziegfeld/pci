using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.IO;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

using nsoftware.InPay;

namespace EEPM
{

    // ###################################################################################
    // ###################################################################################
    // ###################################################################################
    //   Specific Gateway Override Classes
    // ###################################################################################
    // ###################################################################################
    // ###################################################################################

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public class CustomGWCard // : EPCard  commented 02242014 LfZ for the "Use new keyword if hiding was intended" related problems 
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################

        public CustomGWCard(Enterprise.EELog objLog)
        {
            m_objLog = objLog;
        }

        // ###################################################################################
        // Public functions
        // ###################################################################################

        public virtual string BuildRequest()
        {
            string strReturn = "";

            return strReturn;
        }

        // ###################################################################################
        // Protected functions
        // ###################################################################################

        // ###################################################################################
        // Public property functions
        // ###################################################################################

        public string CardType
        {
            get { return m_strCardType; }
            set { m_strCardType = value; }
        }

        public string Number
        {
            get { return m_strCardNumber; }
            set { m_strCardNumber = value; }
        }

        public string ExpMonth
        {
            get { return m_strCardExpMonth; }
            set { m_strCardExpMonth = value; }
        }

        public string ExpYear
        {
            get { return m_strCardExpYear; }
            set { m_strCardExpYear = value; }
        }

        public string CVVData
        {
            get { return m_strCardExpCVV2Data; }
            set { m_strCardExpCVV2Data = value; }
        }


        // ###################################################################################
        // Private variables
        // ###################################################################################


        protected Enterprise.EELog m_objLog;
        private string m_strCardType;
        private string m_strCardNumber;
        private string m_strCardExpMonth;
        private string m_strCardExpYear;
        private string m_strCardExpCVV2Data;
    }

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public class CustomGWCustomer // : EPCustomer //commented 02242014 LfZ for the "Use new keyword if hiding was intended" related problems
    {

        public CustomGWCustomer(Enterprise.EELog objLog)
        {
            m_objLog = objLog;
        }

        // ###################################################################################
        // Public functions
        // ###################################################################################

        public virtual string BuildRequest()
        {
            string strReturn = "";

            return strReturn;
        }

        // ###################################################################################
        // Protected functions
        // ###################################################################################


        // ###################################################################################
        // Public property functions
        // ###################################################################################

        public string Id
        {
            get { return m_strCustomerGuid; }
            set { m_strCustomerGuid = value; }
        }

        public string FirstName
        {
            get { return m_strCustomerFirstName; }
            set { m_strCustomerFirstName = value; }
        }

        public string LastName
        {
            get { return m_strCustomerLastName; }
            set { m_strCustomerLastName = value; }
        }

        public string Address
        {
            get { return m_strCustomerAddress; }
            set { m_strCustomerAddress = value; }
        }

        public string City
        {
            get { return m_strCustomerCity; }
            set { m_strCustomerCity = value; }
        }

        public string State
        {
            get { return m_strCustomerState; }
            set { m_strCustomerState = value; }
        }

        public string Zip
        {
            get { return m_strCustomerZipCode; }
            set { m_strCustomerZipCode = value; }
        }

        public string Country
        {
            get { return m_strCustomerCountry; }
            set { m_strCustomerCountry = value; }
        }

        public string Phone
        {
            get { return m_strCustomerPhone; }
            set { m_strCustomerPhone = value; }
        }

        public string Email
        {
            get { return m_strCustomerEmail; }
            set { m_strCustomerEmail = value; }
        }


        // ###################################################################################
        // Private variables
        // ###################################################################################


        protected Enterprise.EELog m_objLog;
        private string m_strCustomerGuid;
        private string m_strCustomerFirstName;
        private string m_strCustomerLastName;
        private string m_strCustomerAddress;
        private string m_strCustomerCity;
        private string m_strCustomerState;
        private string m_strCustomerZipCode;
        private string m_strCustomerCountry;
        private string m_strCustomerPhone;
        private string m_strCustomerEmail;
    }

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public class CustomGWGateway //: Icharge // commented 02242014 LfZ
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################
        public CustomGWGateway(Enterprise.EELog objLog)
        {
            m_objLog = objLog;
            m_objResponse = new CustomGWResponse();
            m_dictSpecialFields = new System.Collections.Generic.Dictionary<string, string>();
        }

        //protected override void Finalize()
        ~CustomGWGateway()
        {
            m_objResponse = null;
            m_dictSpecialFields = null;
        }


        // ###################################################################################
        // Public functions
        // ###################################################################################

        //added 02212014 LfZ
        public virtual string Config(string str)
        {
            if (str == "RawRequest")
                return RawRequest;
            else return "";
        }

        public virtual void AuthOnly()
        {
            Response.Approved = false;
            Response.ErrorCode = "78950";
            Response.ErrorText = "Custom GW AuthOnly not overridden.";
        }

        ///' TODO ? add overridable Tokenize()


        public virtual void Capture(string strTransactionID, string strTransactionAmount)
        {
            Response.Approved = false;
            Response.ErrorCode = "78951";
            Response.ErrorText = "Custom GW Capture not overridden.";
        }

        public virtual void Sale()
        {
            Response.Approved = false;
            Response.ErrorCode = "78952";
            Response.ErrorText = "Custom GW Sale not overridden.";
        }

        public virtual void Refund(string strTransactionID, string strTransactionAmount)
        {
            Response.Approved = false;
            Response.ErrorCode = "78953";
            Response.ErrorText = "Custom GW Credit not overridden.";
        }

        public virtual void VoidTransaction(string strTransactionID)
        {
            Response.Approved = false;
            Response.ErrorCode = "78954";
            Response.ErrorText = "Custom GW VoidTransaction not overridden.";
        }

        public virtual void AddSpecialField(string strKey, string strValue)
        {
            m_objLog.LogMessage("CustomGWGateway: AddSpecialField: Entering", 40);

            if (m_dictSpecialFields.ContainsKey(strKey))
            {
                m_dictSpecialFields[strKey] = strValue;
            }
            else
            {
                m_dictSpecialFields.Add(strKey, strValue);
            }

            m_objLog.LogMessage("CustomGWGateway: AddSpecialField: Exiting", 40);
        }

        protected virtual void RemoveSpecialField(string strKey)
        {
            if (m_dictSpecialFields.ContainsKey(strKey))
                m_dictSpecialFields.Remove(strKey);
        }

        protected virtual bool ContainsSpecialField(string strKey)
        {
            return m_dictSpecialFields.ContainsKey(strKey);
        }

        protected virtual string GetSpecialField(string strKey)
        {
            if ((ContainsSpecialField(strKey)))
                return m_dictSpecialFields[strKey];
            else
                return "";
        }

        // ###################################################################################
        // Protected functions
        // ###################################################################################

        protected virtual string BuildRequest()
        {
            string strReturn = "";

            return strReturn;
        }


        // ###################################################################################
        // Public property functions
        // ###################################################################################

        public string Gateway
        {
            get { return m_strGatewayId; }
            set { m_strGatewayId = value; }
        }

        public string GatewayURL
        {
            get { return m_strGatewayURL; }
            set { m_strGatewayURL = value; }
        }

        public string MerchantLogin
        {
            get { return m_strGatewayLogin; }
            set { m_strGatewayLogin = value; }
        }

        public string MerchantPassword
        {
            get { return m_strGatewayPassword; }
            set { m_strGatewayPassword = value; }
        }

        public string InvoiceNumber
        {
            get { return m_strInvoiceNumber; }
            set { m_strInvoiceNumber = value; }
        }

        public string TransactionAmount
        {
            get { return m_strTransactionAmount; }
            set { m_strTransactionAmount = value; }
        }

        public string TransactionDesc
        {
            get { return m_strTransactionDescription; }
            set { m_strTransactionDescription = value; }
        }

        public string TransactionId
        {
            get { return m_strTransactionID; }
            set { m_strTransactionID = value; }
        }

        //02212014 LfZ VB code below is converted to a public method.
        //public string Config
        //Public ReadOnly Property Config(ByVal strString As String) As String
        //   Get
        //        If (strString = "RawRequest") Then Return RawRequest() Else Return ""
        //    End Get
        //End Property
        //}

        public CustomGWCard Card
        {
            get { return m_objCard; }
            set { m_objCard = value; }
        }

        public CustomGWCustomer Customer
        {
            get { return m_objCustomer; }
            set { m_objCustomer = value; }
        }

        public CustomGWResponse Response
        {
            get { return m_objResponse; }
            set { m_objResponse = value; }
        }

        public string RawRequest
        {
            get { return m_strRawRequest; }
            set { m_strRawRequest = value; }
        }

        // ###################################################################################
        // Protected variables
        // ###################################################################################


        protected System.Collections.Generic.Dictionary<string, string> m_dictSpecialFields;

        protected Enterprise.EELog m_objLog;
        // ###################################################################################
        // Private variables
        // ###################################################################################

        private string m_strGatewayId;
        private string m_strGatewayURL;
        private string m_strGatewayLogin;

        private string m_strGatewayPassword;
        private string m_strInvoiceNumber;
        private string m_strTransactionAmount;
        private string m_strTransactionDescription;

        private string m_strTransactionID;
        private CustomGWCard m_objCard;
        private CustomGWCustomer m_objCustomer;
        private CustomGWResponse m_objResponse;

        private string m_strRawRequest;
    }

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)] 
    public class CustomGWResponse // : EPResponse commented 02242014 LfZ
    {

        // ###################################################################################
        // Public functions
        // ###################################################################################

        public virtual bool ParseResponseData()
        {
            bool blnReturn = false;

            return blnReturn;
        }

        // ###################################################################################
        // Public property functions
        // ###################################################################################

        public bool Approved
        {
            get { return m_blnApproved; }
            set { m_blnApproved = value; }
        }

        public string TransactionId
        {
            get { return m_strTransactionId; }
            set { m_strTransactionId = value; }
        }

        public string AVSResult
        {
            get { return m_strAVSResult; }
            set { m_strAVSResult = value; }
        }

        public string ErrorCode
        {
            get { return m_strErrorCode; }
            set { m_strErrorCode = value; }
        }

        public string Code
        {
            get { return ErrorCode; }
        }

        public string ErrorText
        {
            get { return m_strErrorText; }
            set { m_strErrorText = value; }
        }

        public string Text
        {
            get { return ErrorText; }
        }

        public string Data
        {
            get { return m_strRawData; }
            set { m_strRawData = value; }
        }

        // ###################################################################################
        // Private variables
        // ###################################################################################


        protected Enterprise.EELog m_objLog;
        private bool m_blnApproved;
        private string m_strTransactionId;
        private string m_strAVSResult;
        private string m_strErrorCode;
        private string m_strErrorText;
        private string m_strRawData;
    }

    
    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public abstract class CustomGWPostCard : CustomGWCard
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################

        public CustomGWPostCard(Enterprise.EELog objLog)
            : base(objLog)
        {
            SetupPostValueArray();
        }

        // ###################################################################################
        // Public functions
        // ###################################################################################

        //'building request in the HTTP POST link adress format Lingfei
        public override string BuildRequest()
        {
            string strReturn = "";
            if ((!string.IsNullOrEmpty(m_arrPostValues[0])))
                strReturn += "&" + m_arrPostValues[0] + "=" + CardType;
            if ((!string.IsNullOrEmpty(m_arrPostValues[1])))
                strReturn += "&" + m_arrPostValues[1] + "=" + Number;
            if ((!string.IsNullOrEmpty(m_arrPostValues[2])))
                strReturn += "&" + m_arrPostValues[2] + "=" + ExpMonth;
            if ((!string.IsNullOrEmpty(m_arrPostValues[3])))
                strReturn += "&" + m_arrPostValues[3] + "=" + ExpYear;
            if ((!string.IsNullOrEmpty(m_arrPostValues[4])))
                strReturn += "&" + m_arrPostValues[4] + "=" + CVVData;
            strReturn += BuildGatewaySpecificPost();
            if ((!string.IsNullOrEmpty(strReturn)))
                strReturn = strReturn.Substring(1);
            return strReturn;
        }


        // ###################################################################################
        // Protected functions
        // ###################################################################################

        // CardType	    (0)
        // Number		(1)
        // ExpMonth	    (2)
        // ExpYear		(3)
        // CVV2Data	    (4)
        protected abstract void SetupPostValueArray();

        protected virtual string BuildGatewaySpecificPost()
        {
            string strReturn = "";
            return strReturn;
        }

        // ###################################################################################
        // Protected variables
        // ###################################################################################

        protected string[] m_arrPostValues;
    }

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public abstract class CustomGWPostCustomer : CustomGWCustomer
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################

        public CustomGWPostCustomer(ref Enterprise.EELog objLog)
            : base(objLog)
        {
            SetupPostValueArray();
        }


        // ###################################################################################
        // Public functions
        // ###################################################################################

        public override string BuildRequest()
        {
            string strReturn = "";
            if ((!string.IsNullOrEmpty(m_arrPostValues[0])))
                strReturn += "&" + m_arrPostValues[0] + "=" + Id;
            if ((!string.IsNullOrEmpty(m_arrPostValues[1])))
                strReturn += "&" + m_arrPostValues[1] + "=" + FirstName;
            if ((!string.IsNullOrEmpty(m_arrPostValues[2])))
                strReturn += "&" + m_arrPostValues[2] + "=" + LastName;
            if ((!string.IsNullOrEmpty(m_arrPostValues[3])))
                strReturn += "&" + m_arrPostValues[3] + "=" + Address;
            //If (m_arrPostValues(4) <> "") Then strReturn &= "&" + m_arrPostValues(4) + "=" + Address2()
            if ((!string.IsNullOrEmpty(m_arrPostValues[5])))
                strReturn += "&" + m_arrPostValues[5] + "=" + City;
            if ((!string.IsNullOrEmpty(m_arrPostValues[6])))
                strReturn += "&" + m_arrPostValues[6] + "=" + State;
            if ((!string.IsNullOrEmpty(m_arrPostValues[7])))
                strReturn += "&" + m_arrPostValues[7] + "=" + Zip;
            if ((!string.IsNullOrEmpty(m_arrPostValues[8])))
                strReturn += "&" + m_arrPostValues[8] + "=" + Country;
            if ((!string.IsNullOrEmpty(m_arrPostValues[9])))
                strReturn += "&" + m_arrPostValues[9] + "=" + Phone;
            if ((!string.IsNullOrEmpty(m_arrPostValues[10])))
                strReturn += "&" + m_arrPostValues[10] + "=" + Email;
            strReturn += BuildGatewaySpecificPost();
            if ((!string.IsNullOrEmpty(strReturn)))
                strReturn = strReturn.Substring(1);
            return strReturn;
        }


        // ###################################################################################
        // Protected functions
        // ###################################################################################

        // Id		    (0)
        // FirstName	    (1)
        // LastName	    (2)
        // Address		(3)
        // Address2	    (4)
        // City		    (5)
        // State		    (6)
        // Zip		    (7)
        // Country		(8)
        // Phone		    (9)
        // Email		    (10)
        protected abstract void SetupPostValueArray();

        protected virtual string BuildGatewaySpecificPost()
        {
            string strReturn = "";
            return strReturn;
        }

        // ###################################################################################
        // Protected variables
        // ###################################################################################

        protected string[] m_arrPostValues;
    }

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public abstract class CustomGWPostGateway : CustomGWGateway
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################

        public CustomGWPostGateway(Enterprise.EELog objLog)
            : base(objLog)
        {
            SetupPostValueArray();
        }


        // ###################################################################################
        // Protected functions
        // ###################################################################################

        protected override string BuildRequest()
        {
            string strReturn = "";
            if ((!string.IsNullOrEmpty(m_arrPostValues[0])))
                strReturn += "&" + m_arrPostValues[0] + "=" + Gateway;
            if ((!string.IsNullOrEmpty(m_arrPostValues[1])))
                strReturn += "&" + m_arrPostValues[1] + "=" + GatewayURL;
            if ((!string.IsNullOrEmpty(m_arrPostValues[2])))
                strReturn += "&" + m_arrPostValues[2] + "=" + MerchantLogin;
            if ((!string.IsNullOrEmpty(m_arrPostValues[3])))
                strReturn += "&" + m_arrPostValues[3] + "=" + MerchantPassword;
            if ((!string.IsNullOrEmpty(m_arrPostValues[4])))
                strReturn += "&" + m_arrPostValues[4] + "=" + TransactionAmount;
            if ((!string.IsNullOrEmpty(m_arrPostValues[5])))
                strReturn += "&" + m_arrPostValues[5] + "=" + TransactionDesc;
            if ((!string.IsNullOrEmpty(m_arrPostValues[6])))
                strReturn += "&" + m_arrPostValues[6] + "=" + InvoiceNumber;
            strReturn += BuildGatewaySpecificPost();
            strReturn += AddSpecialFieldsToRequest();
            if ((!string.IsNullOrEmpty(strReturn)))
                strReturn = strReturn.Substring(1);
            return strReturn;
        }

        protected virtual string AddSpecialFieldsToRequest()
        {
            string strReturn = "";

            foreach (KeyValuePair<string, string> kvPair in m_dictSpecialFields)
            {
                strReturn += "&" + Convert.ToString(kvPair.Key) + "=" + Convert.ToString(kvPair.Value);
            }

            return strReturn;
        }

        // ###################################################################################
        // Protected functions
        // ###################################################################################

        // Gateway			    (0)
        // GatewayURL		    (1)
        // MerchantLogin		    (2)
        // MerchantPassword	    (3)
        // TransactionAmount	    (4)
        // TransactionDesc		(5)
        // InvoiceNumber		    (6)
        // TransactionId         (7)

        /// virtual function
        protected abstract void SetupPostValueArray();

        protected virtual string BuildGatewaySpecificPost()
        {
            string strReturn = "";
            return strReturn;
        }

        /// sending HTTP POST request  lingfei  using System.Net.WebRequest and .WebResponse
        protected virtual bool PostDataToGateway()
        {
            bool blnReturn = true;
            string strPostData = "";

            m_objLog.LogMessage("CustomGWPostGateway: PostDataToGateway: Entering.", 40);


            try
            {
                strPostData += BuildRequest();
                strPostData += "&" + Card.BuildRequest();
                strPostData += "&" + Customer.BuildRequest();

                blnReturn = !string.IsNullOrEmpty(strPostData);
                if ((blnReturn))
                {
                    RawRequest = strPostData;
                    byte[] bytPostDataArray = Encoding.UTF8.GetBytes(strPostData);

                    m_objWebRequest = WebRequest.Create(GatewayURL);
                    m_objWebRequest.Method = "POST";
                    m_objWebRequest.ContentType = "application/x-www-form-urlencoded";
                    m_objWebRequest.ContentLength = bytPostDataArray.Length;

                    m_objDataStream = m_objWebRequest.GetRequestStream();
                    m_objDataStream.Write(bytPostDataArray, 0, bytPostDataArray.Length);
                    m_objDataStream.Close();

                    m_objWebResponse = m_objWebRequest.GetResponse();
                    if ( Convert.ToInt32( ((HttpWebResponse)m_objWebResponse).StatusCode) == 200)
                    {
                        m_objDataStream = m_objWebResponse.GetResponseStream();
                        m_objDataStreamReader = new StreamReader(m_objDataStream);

                        Response.Data = m_objDataStreamReader.ReadToEnd();
                        m_objDataStreamReader.Close();
                        m_objDataStream.Close();
                        m_objWebResponse.Close();
                    }
                    else
                    {
                        blnReturn = false;
                        Response.Approved = false;
                        Response.ErrorCode = "78981";
                        Response.ErrorText = "PostDataToGateway : Communication Error : " + ((HttpWebResponse)m_objWebResponse).StatusCode + " : " + ((HttpWebResponse)m_objWebResponse).StatusDescription;
                    }

                    m_objDataStream = null;
                    m_objDataStreamReader = null;

                    m_objWebRequest = null;
                    m_objWebResponse = null;
                    // TODO Remove me after initial test
                    blnReturn = false;
                }
            }
            catch (Exception ex)
            {
                blnReturn = false;
                Response.Approved = false;
                Response.ErrorCode = "78980";
                Response.ErrorText = "Internal Error : PostDataToGateway : " + ex.Message;
            }

            m_objLog.LogMessage("CustomGWPostGateway: PostDataToGateway: Exiting: " + blnReturn, 40);

            return blnReturn;
        }


        // ###################################################################################
        // Protected variables
        // ###################################################################################

        protected Stream m_objDataStream;
        protected StreamReader m_objDataStreamReader;
        protected WebRequest m_objWebRequest;

        protected WebResponse m_objWebResponse;
        protected string[] m_arrPostValues;
    }

}
