using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace Enterprise
{

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public class EERegistry
    {

        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Constructors\Destructors
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        #region "Constructors\Destructors"

        public EERegistry()
        {
            InitializeMembers();
        }

        public EERegistry(string strRoot, string strProductHive, string strProductVersion)
        {
            InitializeMembers(strRoot, strProductHive, strProductVersion);
        }

        public EERegistry(string strRoot, string strProductHive, string strProductVersion, string strInstance)
        {
            InitializeMembers(strRoot, strProductHive, strProductVersion, strInstance);
        }

        #endregion


        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Public Functions
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        #region "PublicFunctions"

        public bool PathExists(string strPath)
        {
            bool blnReturn = false;
            lock (m_objLock)
            {
                blnReturn = CheckPathExists(strPath);
            }
            return blnReturn;
        }

        public bool ProductPathExists(string strRoot = "", string strProductHive = "", string strProductVersion = "")
        {
            //string strCheckStr = "";
            lock (m_objLock)
            {
                if ((!string.IsNullOrEmpty(strRoot)))
                    Root = strRoot;
                if ((!string.IsNullOrEmpty(strProductHive)))
                    ProductHive = strProductHive;
                if ((!string.IsNullOrEmpty(strProductVersion)))
                    ProductVersion = strProductVersion;
            }
            return CheckPathExists(KeyPath, true);
        }

        public string ProductGetLatestVersion(string strRoot = "", string strInstance = "", string strProductHive = "")
        {
            string strReturn = "";
            strReturn = ReturnHighestVersion(strRoot, strInstance, strProductHive);
            return strReturn;
        }

        public string GetKeyValue(string strPath, string strKeyName)
        {
            string strReturn = "";
            lock (m_objLock)
            {
                strReturn = GetValueEntry(strPath, strKeyName);
            }
            return strReturn;
        }

        public string ProductGetKeyValue(string strKeyName, string strRoot = "", string strProductHive = "", string strProductVersion = "", string strInstance = "")
        {
            string strReturn = "";
            lock (m_objLock)
            {
                if ((!string.IsNullOrEmpty(strRoot)))
                    Root = strRoot;
                if ((!string.IsNullOrEmpty(strProductHive)))
                    ProductHive = strProductHive;
                if ((!string.IsNullOrEmpty(strProductVersion)))
                    ProductVersion = strProductVersion;
                if ((!string.IsNullOrEmpty(strInstance)))
                    Instance = strInstance;
                strReturn = GetValueEntry(KeyPath, strKeyName);
            }
            return strReturn;
        }

        public bool CreatePath(string strPath)
        {
            bool blnReturn = false;
            RegistryKey objRegKey = null;
            objRegKey = CreateRegistryPath(strPath);
            if ((objRegKey != null))
                blnReturn = true;
            return blnReturn;
        }

        public bool ProductCreatePath(string strRoot = "", string strProductHive = "", string strProductVersion = "", string strInstance = "")
        {
            bool blnReturn = false;
            RegistryKey objRegKey = null;
            lock (m_objLock)
            {
                if ((!string.IsNullOrEmpty(strRoot)))
                    Root = strRoot;
                if ((!string.IsNullOrEmpty(strProductHive)))
                    ProductHive = strProductHive;
                if ((!string.IsNullOrEmpty(strProductVersion)))
                    ProductVersion = strProductVersion;
                objRegKey = CreateRegistryPath(KeyPath);
            }
            if ((objRegKey != null))
                blnReturn = true;
            return blnReturn;
        }

        public bool CreateKey(string strPath, string strKeyName, string strKeyValue)
        {
            bool blnReturn = false;
            lock (m_objLock)
            {
                blnReturn = CreateRegistryKey(strPath, strKeyName, strKeyValue);
            }
            return blnReturn;
        }

        public bool ProductCreateKey(string strKeyName, string strKeyValue, string strRoot = "", string strProductHive = "", string strProductVersion = "", string strInstance = "")
        {
            bool blnReturn = false;
            lock (m_objLock)
            {
                if ((!string.IsNullOrEmpty(strRoot)))
                    Root = strRoot;
                if ((!string.IsNullOrEmpty(strProductHive)))
                    ProductHive = strProductHive;
                if ((!string.IsNullOrEmpty(strProductVersion)))
                    ProductVersion = strProductVersion;
                blnReturn = CreateRegistryKey(KeyPath, strKeyName, strKeyValue);
            }
            return blnReturn;
        }

        public bool ProductCreateValueEntry(string strValueName, string strValueValue, bool blnUpdateExisting = true, string strRoot = "", string strProductHive = "", string strProductVersion = "", string strInstance = "")
        {
            bool blnReturn = false;
            lock (m_objLock)
            {
                if ((!string.IsNullOrEmpty(strRoot)))
                    Root = strRoot;
                if ((!string.IsNullOrEmpty(strProductHive)))
                    ProductHive = strProductHive;
                if ((!string.IsNullOrEmpty(strProductVersion)))
                    ProductVersion = strProductVersion;
                blnReturn = (!string.IsNullOrEmpty(SetValueEntry(KeyPath, strValueName, strValueValue, blnUpdateExisting, false)));
            }
            return blnReturn;
        }

        #endregion


        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Protected\Private Functions
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        #region "Protected\PrivateFunctions"

        // ###################################################################################################################################################################
        //   Protected Functions
        // ###################################################################################################################################################################

        protected void InitializeMembers(string strRoot = "", string strProductHive = "", string strProductVersion = "", string strInstance = "")
        {
            Root = strRoot;
            ProductHive = strProductHive;
            ProductVersion = strProductVersion;
            Instance = strInstance;
            if ((m_objLock == null))
                m_objLock = new object();
        }

        protected bool CheckPathExists(string strPath, bool blnAddBasePath = false)
        {
            bool blnReturn = false;
            RegistryKey objCheckKey = null;
            m_objRegKey = Registry.LocalMachine;
            if ((blnAddBasePath))
                strPath = m_strBaseKeyPath + strPath;
            try
            {
                objCheckKey = m_objRegKey.OpenSubKey(strPath, false);
                if ((objCheckKey != null))
                {
                    blnReturn = true;
                }
            }
            catch
            {
            }
            return blnReturn;
        }

        protected string ReturnHighestVersion(string strRoot = "", string strInstance = "", string strProductHive = "")
        {
            string strReturn = "";
            RegistryKey objProductPath = null;
            if ((string.IsNullOrEmpty(strRoot)))
                strRoot = Root;
            if ((string.IsNullOrEmpty(strInstance)))
                strInstance = Instance;
            if ((string.IsNullOrEmpty(strProductHive)))
                strProductHive = ProductHive;

            string strPath = m_strBaseKeyPath + "\\";
            if ((!string.IsNullOrEmpty(strRoot)))
                strPath += strRoot + "\\";
            if ((!string.IsNullOrEmpty(strInstance)))
                strPath += strInstance + "\\";
            if (strPath.Length <= 1)
                strPath = "";

            if ((CheckPathExists(strPath + strProductHive)))
            {
                objProductPath = GetPath(strPath + strProductHive);
                string[] strSubKeys = objProductPath.GetSubKeyNames();
                if (strSubKeys.Length != 0)
                {
                    strReturn = strSubKeys[strSubKeys.Length - 1];
                }
            }
            return strReturn;
        }

        protected RegistryKey GetPath(string strPath)
        {
            m_objRegKey = Registry.LocalMachine;
            try
            {
                m_objRegKey = m_objRegKey.OpenSubKey(strPath, true);
            }
            catch
            {
                m_objRegKey = null;
            }
            return m_objRegKey;
        }

        protected string GetValueEntry(string strPath, string strEntryName)
        {
            string strReturn = "";
            object objValue = null;
            RegistryKey objRegKey = GetPath(strPath);
            if ((objRegKey == null))
            {
                return strReturn;
            }
            try
            {
                objValue = objRegKey.GetValue(strEntryName);
                if (strEntryName.Substring(0,11) == "SpecificKey")
                    strReturn = IlluminateKey(objValue.ToString());
                else
                    strReturn = objValue.ToString();
            }
            catch
            {
                strReturn = "";
            }
            return strReturn;
        }

        protected string SetValueEntry(string strPath, string strEntryName, string strEntryValue, bool blnUpdateExisting = true, bool blnAddBasePath = true)
        {
            string strReturn = "";
            object objValue = null;
            RegistryKey objRegKey = GetPath(strPath);
            if ((objRegKey == null))
                return strReturn;
            try
            {
                objValue = objRegKey.GetValue(strEntryName);
                if ((!string.IsNullOrEmpty(objValue.ToString())))
                {
                    if ((blnUpdateExisting))
                    {
                        if (strEntryName.Substring(0, 11) == "SpecificKey")
                            objRegKey.SetValue(strEntryName, ObfuscateKey(strEntryValue));
                        else
                            objRegKey.SetValue(strEntryName, strEntryValue);
                    }
                }
                else
                {
                    if (strEntryName.Substring(0, 11) == "SpecificKey")
                        objRegKey.SetValue(strEntryName, ObfuscateKey(strEntryValue));
                    else
                        objRegKey.SetValue(strEntryName, strEntryValue);
                }
                objValue = objRegKey.GetValue(strEntryName);
                if ((objValue.ToString() != strEntryValue))
                    strReturn = "";
                else
                    strReturn = objValue.ToString();
            }
            catch
            {
                strReturn = "";
            }
            return strReturn;
        }

        // ###################################################################################################################################################################
        //   Private Functions
        // ###################################################################################################################################################################

        private RegistryKey CreateRegistryKey(string strPath, string strKey)
        {
            RegistryKey objReturn = null;
            try
            {
                m_objRegKey = m_objRegKey.OpenSubKey(strPath, true);
                objReturn = m_objRegKey.CreateSubKey(strKey);
            }
            catch
            {
                objReturn = null;
            }
            return objReturn;
        }

        private RegistryKey CreateRegistryPath(string strPath, bool blnAddBasePath = false)
        {
            RegistryKey objReturn = null;
            int intCounter = 0;
            string strCurrentPath = "";
            string strLastKnownGoodPath = "";
            string[] arrPathPieces = strCurrentPath.Split(new[]{'\\'},StringSplitOptions.None);            
            m_objRegKey = Registry.LocalMachine;
            if ((blnAddBasePath))
                strPath = m_strBaseKeyPath + strPath;
            objReturn = GetPath(strPath);
            if ((objReturn != null))
                return objReturn;
            try
            {
                while (intCounter < arrPathPieces.Length)
                {
                    if (!string.IsNullOrEmpty(arrPathPieces[intCounter]))
                    {
                        strCurrentPath = strCurrentPath + arrPathPieces[intCounter];
                        if (!CheckPathExists(strCurrentPath))
                        {
                            objReturn = CreateRegistryKey(strLastKnownGoodPath, arrPathPieces[intCounter]);
                            strLastKnownGoodPath = strLastKnownGoodPath + "\\" + arrPathPieces[intCounter];
                        }
                        else
                        {
                            strLastKnownGoodPath = strCurrentPath;
                        }
                        strCurrentPath = strCurrentPath + "\\";
                        
                    }
                    ++intCounter;
                };
            }
            catch
            {
                objReturn = null;
            }
            return objReturn;
        }

        private bool CreateRegistryKey(string strPath, string strKeyName, string strKeyValue)
        {
            bool blnReturn = false;
            RegistryKey objRegKey = GetPath(strPath);
            if ((objRegKey == null))
                objRegKey = CreateRegistryPath(strPath);
            if ((objRegKey == null))
                return blnReturn;
            try
            {
                if (strKeyName.Substring(0, 11) == "SpecificKey")
                    objRegKey.SetValue(strKeyName, ObfuscateKey(strKeyValue));
                else
                    objRegKey.SetValue(strKeyName, strKeyValue);
                objRegKey.Close();
                blnReturn = true;
            }
            catch
            {
            }
            return blnReturn;
        }

        private string ObfuscateKey(string strKeyValue)
        {
            string strReturn = "";

            if (string.IsNullOrEmpty(strKeyValue)) return strReturn;

            for (int intIndex = 0; intIndex < strKeyValue.Length; intIndex++)
            {
                int intChar = (int)strKeyValue[intIndex];
                intChar = intChar ^ 111; // 111= binary 111 1011 LfZ.
                string strChar = intChar.ToString("X").PadLeft(2,'0');                
                strReturn += strChar.Substring(strChar.Length - 2, 2);
                //strReturn += Strings.Right("0" + Conversion.Hex(intChar), 2);
            }

            return strReturn;
        }

        private string IlluminateKey(string strKeyValue)
        {
            string strReturn = "";

            if ((string.IsNullOrEmpty(strKeyValue)))
                return strReturn;

            for (int intIndex = 0; intIndex < strKeyValue.Length / 2; intIndex++)
            {
                int intChar = int.Parse(strKeyValue.Substring(intIndex * 2, 2), System.Globalization.NumberStyles.HexNumber);
                intChar = intChar ^ 111;
                strReturn += Convert.ToChar(intChar);
            }

            return strReturn;
        }

        #endregion

        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // 
        //    Member Variables\Functions
        //
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        // ###################################################################################################################################################################
        #region "MemberVariables\Properties"

        // ###################################################################################################################################################################
        //   Property Functions
        // ###################################################################################################################################################################

        public virtual string ProductHive
        {
            get { return m_strProductHive; }
            set { m_strProductHive = value; }
        }

        public virtual string ProductVersion
        {
            get { return m_strProductVersion; }
            set
            {
                value = value.Replace( ".", "");
                if (value.Length >= 3)
                    value = value.Substring(1, 3);
                m_strProductVersion = value;
            }
        }

        public virtual string Root
        {
            get { return m_strRoot; }
            set { m_strRoot = value; }
        }

        public virtual string Instance
        {
            get { return m_strInstance; }
            set { m_strInstance = value; }
        }

        public virtual string KeyPath
        {
            get
            {
                string strReturn = "";
                //if (!string.IsNullOrEmpty(m_strBaseKeyPath))     strReturn += "\\\\" + m_strBaseKeyPath;
                //if (!string.IsNullOrEmpty(m_strRoot))            strReturn += "\\\\" + m_strRoot;
                //if (!string.IsNullOrEmpty(m_strInstance))        strReturn += "\\\\" + m_strInstance;
                //if (!string.IsNullOrEmpty(m_strProductHive))     strReturn += "\\\\" + m_strProductHive;
                //if (!string.IsNullOrEmpty(m_strProductVersion))  strReturn += "\\\\" + m_strProductVersion;
                //if (strReturn.IndexOf('\\') == 0)                strReturn = strReturn.Substring(2);
                //if (strReturn.Length != 0 )                      strReturn += "\\\\";
                if (!string.IsNullOrEmpty(m_strBaseKeyPath)) strReturn += m_strBaseKeyPath + "\\\\";
                if (!string.IsNullOrEmpty(m_strRoot)) strReturn += m_strRoot + "\\\\";
                if (!string.IsNullOrEmpty(m_strInstance)) strReturn += m_strInstance + "\\\\";
                if (!string.IsNullOrEmpty(m_strProductHive)) strReturn += m_strProductHive + "\\\\";
                if (!string.IsNullOrEmpty(m_strProductVersion)) strReturn += m_strProductVersion + "\\\\";
                return strReturn;
            }
        }

        // ###################################################################################################################################################################
        //   Private Member Variables
        // ###################################################################################################################################################################


        private RegistryKey m_objRegKey;
        private string m_strRoot;
        private string m_strInstance;
        private string m_strProductHive;

        private string m_strProductVersion;

        protected string m_strBaseKeyPath = "SOFTWARE";

        protected static object m_objLock;
        #endregion

    }

}
