
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.Runtime.InteropServices;

[Guid("C618C8CD-3C21-427a-9577-6CD87A696E61")]
public interface IEEGateway
{

    bool Authorize();
    bool Capture();
    bool DirectSale();
    bool Credit();
    bool Void();

    //request a token without a sale" lz as in nsfotware help doc
    //'2/14/2014  change interface of token in line with encrypt
    string Tokenize(string strPlainText);

    void AddNameValue(string strName, string strValue);
    string GetNameValue(string strName);

    void ClearNameValue();
    int GatewayID { get; set; }
    int PaymentType { get; set; }
    string GatewayURL { get; set; }
    string GatewayLogin { get; set; }
    string GatewayPassword { get; set; }
    int ResponseCode { get; }
    string ResponseDescription { get; }
    //' 12/4/2013 adding property for SecurityProfile (either Tokens or Encrpytion)

    string GatewaySecurityProfile { get; set; }
}