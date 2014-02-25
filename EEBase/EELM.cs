using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace Enterprise
{

    [ComVisible(false), ClassInterface(ClassInterfaceType.None)]
    public class EELog
    {

        // ###################################################################################
        // ###################################################################################
        // Public Constructors
        // ###################################################################################
        // ###################################################################################
        public EELog()
        {
            InitializeMembers();
        }

        public EELog(string strRoot, string strProductHive, string strProductVersion, string strInstance)
        {
            InitializeMembers(strRoot, strProductHive, strProductVersion, strInstance);
        }

        // ###################################################################################
        // ###################################################################################
        // Public Functions
        // ###################################################################################
        // ###################################################################################
        public void LogMessage(string strMessage)
        {
            LogMessage(strMessage, "");
        }

        public void LogMessage(string strMessage, int intLogLevel)
        {
            LogMessage(strMessage, "", intLogLevel);
        }

        public void LogMessage(string strMessage, string strThreadName)
        {
            LogMessage(strMessage, "", 10);
        }

        public void LogMessage(string strMessage, string strThreadName, int intLogLevel)
        {
            if (LogEnabled)
            {
                if ((intLogLevel > 0) && (intLogLevel <= LogLevel))
                {
                    SetStreamObject();
                    strThreadName.PadRight(8, ' ');
                    string strLogMessage = null;
                    strLogMessage = DateTime.Now.ToString("hh:mm:ss-0fff") + '\t' + strThreadName + '\t' + '\t' + strMessage;

                    lock (m_objLock)
                    {
                        m_objFileStream.WriteLine(strLogMessage);
                        m_objFileStream.Flush();
                    }
                    CloseStreamObject();
                }
            }
        }

        public virtual void SetRegistryInformation(string strRoot = "", string strProductHive = "", string strProductVersion = "", string strInstance = "")
        {
            if ((!string.IsNullOrEmpty(strRoot)))
                m_objRegistry.Root = strRoot;
            if ((!string.IsNullOrEmpty(strProductHive)))
                m_objRegistry.ProductHive = strProductHive;
            if ((!string.IsNullOrEmpty(strProductVersion)))
                m_objRegistry.ProductVersion = strProductVersion;
            if ((!string.IsNullOrEmpty(strInstance)))
                m_objRegistry.Instance = strInstance;
            LogPath = m_objRegistry.ProductGetKeyValue("LogLocation");
            if ((string.IsNullOrEmpty(LogPath)))
                LogPath = "C:\\Logs\\";
            if ((!string.IsNullOrEmpty(m_objRegistry.ProductGetKeyValue("LogEnabled"))))
                LogEnabled = m_objRegistry.ProductGetKeyValue("LogEnabled") == "1";
            if ((!string.IsNullOrEmpty(m_objRegistry.ProductGetKeyValue("LogLevel"))))
                LogLevel = Convert.ToInt32(m_objRegistry.ProductGetKeyValue("LogLevel"));
        }

        public virtual void OverrideRegistryInformation(string strLogPath = "c:\\Logs\\", bool blnLogEnabled = false, int intLogLevel = 0)
        {
            LogPath = strLogPath;
            LogEnabled = blnLogEnabled;
            LogLevel = intLogLevel;
        }

        // ###################################################################################
        // ###################################################################################
        // Private Functions
        // ###################################################################################
        // ###################################################################################
        private void InitializeMembers(string strRoot = "", string strProductHive = "", string strProductVersion = "", string strInstance = "")
        {
            m_objFileStream = null;
            m_objLock = new object();
            m_strCurrentLogDate = DateTime.Now;
            m_objRegistry = new Enterprise.EERegistry();
            LogLevel = 0;
            LogEnabled = false;
            SetRegistryInformation(strRoot, strProductHive, strProductVersion, strInstance);
        }

        private void SetFileName()
        {
            string strDate = DateTime.Now.ToString("yyyyMMdd");
            FileName = "EELog_" + m_objRegistry.ProductHive + "_" + strDate + ".esl";
        }

        private void SetStreamObject()
        {
            CloseStreamObject();
            SetFileName();
            m_objFileStream = new System.IO.StreamWriter(LogPath + "\\" + FileName, true);
        }

        private void CloseStreamObject()
        {
            if ((m_objFileStream != null))
                m_objFileStream.Close();
            m_objFileStream = null;
        }

        private bool CheckLogDate()
        {
            //if ((m_strCurrentLogDate.Month != DateAndTime.Month(DateAndTime.Now())) || (m_strCurrentLogDate.Day != DateAndTime.Day(DateAndTime.Now())) || (m_strCurrentLogDate.Year != DateAndTime.Year(DateAndTime.Now())))
            if ( m_strCurrentLogDate.ToShortDateString() != DateTime.Now.ToShortDateString())
                SetStreamObject();
        }

        // ###################################################################################
        // ###################################################################################
        // Property Functions
        // ###################################################################################
        // ###################################################################################
        public string LogPath
        {
            get { return m_strLogPath; }
            set { m_strLogPath = value; }
        }

        public bool LogEnabled
        {
            get { return m_blnLogEnabled; }
            set { m_blnLogEnabled = value; }
        }

        public int LogLevel
        {
            get { return m_blnLogLevel; }
            set { m_blnLogLevel = value; }
        }

        protected string FileName
        {
            get { return m_strFileName; }
            set { m_strFileName = value; }
        }

        protected System.DateTime CurrentLogDate
        {
            get { return m_strCurrentLogDate; }
            set { m_strCurrentLogDate = value; }
        }

        // ###################################################################################
        // ###################################################################################
        // Member Variables
        // ###################################################################################
        // ###################################################################################
        private string m_strLogPath;
        private bool m_blnLogEnabled;
        private int m_blnLogLevel;
        private string m_strFileName;

        private System.DateTime m_strCurrentLogDate;

        private Enterprise.EERegistry m_objRegistry;
        protected object m_objLock;
        protected System.IO.StreamWriter m_objFileStream;
    }

}
