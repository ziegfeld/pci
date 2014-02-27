using System;

using System.Runtime.InteropServices;

namespace EEPM
{

    [ComVisible(true), ClassInterface(ClassInterfaceType.None), Guid("0895EC80-215A-44A0-B35F-A240977F704E")] //Guid("5BBD9E4E-895E-418c-8CD4-668177666BF4")]
    public class EECCValidator : Enterprise.EEBase, IEECCValidator
    {

        // ###################################################################################
        // Constructors\Destructors
        // ###################################################################################
        public EECCValidator()
            : base()
        {
            m_intResponseCode = -1;
            m_strResponseDescription = "";
            SetupBase("PCICharge", "EEPM", 2);
            m_objNSoftwareCCValidator = new nsoftware.InPay.Cardvalidator();
            m_objNSoftwareCCValidator.RuntimeLicense = "42504E3641413153554252413153554243483945353033300000000000000000000000000000000058584436334D594500005357595253564B4D5A5338580000";
            // Version 6 license
            // "42504E354141315355425241315355424348394535303330000000000000000000000000000000004B5655345848355000003733583348365758435A4A4D0000" ' Version 5 license

        }

        //protected override void Finalize()
        ~EECCValidator()
        {
            m_objNSoftwareCCValidator = null;
        }

        // ###################################################################################
        // Public functions
        // ###################################################################################
        public virtual int GetCardType(string strCCNumber)
        {
            int intReturn = 0;
            try
            {
                m_objNSoftwareCCValidator.CardNumber = strCCNumber;
                m_objNSoftwareCCValidator.CardExpMonth = 1;
                m_objNSoftwareCCValidator.CardExpYear = (DateTime.Now.Year) % 100 + 1; //Convert.ToInt32(Strings.Right(Convert.ToString(DateTime.Year(DateTime.Now())) + 1, 2));
                m_objNSoftwareCCValidator.ValidateCard();
                // why setting all the wrong info and calling validateCard? only .CardType is needed. Linfei
                intReturn =  Convert.ToInt32(m_objNSoftwareCCValidator.CardType);
            }
            catch (nsoftware.InPay.InPayException err)
            {
                DetermineError(err);
                intReturn = -1;
            }
            return intReturn;
        }

        public virtual bool ValidateCard(string strCCNumber, int strCCExpMonth, int strCCExpYear)
        {
            bool blnReturn = true;
            m_objNSoftwareCCValidator.CardNumber = strCCNumber;
            m_objNSoftwareCCValidator.CardExpMonth = strCCExpMonth;
            m_objNSoftwareCCValidator.CardExpYear = strCCExpYear;
            m_objNSoftwareCCValidator.ValidateCard();
            if ((blnReturn))
                blnReturn = m_objNSoftwareCCValidator.DateCheckPassed;
            //its calling validateCard method of its superclass -- nSoftware inpay card validator ?
            if ((blnReturn))
                blnReturn = m_objNSoftwareCCValidator.DigitCheckPassed;
            // marked by lingfei
            return blnReturn;
        }

        // ###################################################################################
        // Public property functions
        // ###################################################################################
        public override int ResponseCode
        {
            get { return m_intResponseCode; }
        }

        public override string ResponseDescription
        {
            get {
				if ((!string.IsNullOrEmpty(m_strResponseDescription)))
                    m_strResponseDescription = m_strResponseDescription.Replace("nsoftware", "PCICharge");
                    // had a variable of CompareMethod.Text in VB version 02212014 LfZ
				return m_strResponseDescription;
			}
        }

        // ###################################################################################
        // Protected functions
        // ###################################################################################

        protected virtual void DetermineError(nsoftware.InPay.InPayException objError)
        {
            if ((objError.Code == 504)) 
            {
                m_intResponseCode = 98032;
                m_strResponseDescription = "Card failed the Luhn digit check.  Check the number entered.";
            }
            if ((objError.Code == 505))
            {
                m_intResponseCode = 98035;
                m_strResponseDescription = "Expiration month entered is invalid.";
            }
            if ((objError.Code == 506))
            {
                m_intResponseCode = 98036;
                m_strResponseDescription = "Expiration year entered is invalid.";
            }
            if ((objError.Code == 703))
            {
                m_intResponseCode = 98033;
                m_strResponseDescription = "Invalid characters entered into the card number.";
            }
            if ((objError.Code == 704))
            {
                m_intResponseCode = 98034;
                m_strResponseDescription = "Number appears valid, but card type was not determined.";
            }

        }

        // ###################################################################################
        // Public variables
        // ###################################################################################


        // ###################################################################################
        // Protected variables
        // ###################################################################################

        protected nsoftware.InPay.Cardvalidator m_objNSoftwareCCValidator;
    }

}