using System.Runtime.InteropServices;

[Guid("F9C2DABB-F95E-4f55-8158-3A1FFAD6B49D")]
public interface IEECCValidator
{

    int GetCardType(string strCCNumber);
    bool ValidateCard(string strCCNumber, int strCCExpMonth, int strCCExpYear);

    int ResponseCode { get; }

    string ResponseDescription { get; }
}