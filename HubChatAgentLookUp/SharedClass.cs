using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HubChatAgentLookUp
{
    internal static class SharedClass
    {
        #region PRIVATE VARIABLES        
        private static bool _hasStopSignal = false;
        private static bool _isServiceCleaned = true;
        private static string _connectionString = null;
        //private static Dictionary<long, CampaignInstance> _activeCampaigns = new Dictionary<long, CampaignInstance>();
        private static Listener _listener = null;
        private static Queue<CustomerData> _Customerqueue = null;
        private static List<KeyValuePair<long, string>> _campaignInstanceStatuese = new List<KeyValuePair<long, string>>();


        #endregion
        #region READONLY VARIABLES
        internal static readonly byte POLL_INTERVAL_IN_SECONDS = GetApplicationKey("PollIntervalSeconds") == null ? Convert.ToByte(10) : Convert.ToByte(GetApplicationKey("PollIntervalInSeconds"));
        internal static readonly string WEB_SOCKET_END_POINT = GetApplicationKey("WebSocketEndPoint") == null ? string.Empty : GetApplicationKey("WebSocketEndPoint");
        internal static readonly byte NUMBER_OF_PUBLISHERS = GetApplicationKey("NumberOfPublishers") == null ? Convert.ToByte(1) : Convert.ToByte(GetApplicationKey("NumberOfPublishers"));
        internal static readonly Int32 QUEUE_COUNT_LIMIT = GetApplicationKey("QueueCountLimit") == null ? Convert.ToInt32(100) : Convert.ToInt32(GetApplicationKey("QueueCountLimit"));
        internal static readonly string EXCEL_FILE_PATH = GetApplicationKey("ExcelFilePath") == null ? string.Empty : GetApplicationKey("ExcelFilePath");
        #endregion

        #region PRIVATE METHODS
        private static string GetApplicationKey(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }
        #endregion
        #region PROPERTIES
        internal static bool IsServiceCleaned { get { return _isServiceCleaned; } set { _isServiceCleaned = value; } }
        internal static bool HasStopSignal { get { return _hasStopSignal; } set { _hasStopSignal = value; } }
        internal static Listener Listener
        {
            get
            {
                if (SharedClass._listener == null)
                    SharedClass._listener = new Listener();
                return SharedClass._listener;
            }
            set
            {
                SharedClass._listener = value;
            }
        }
        internal static Queue<CustomerData> Customerqueue
        {
            get
            {
                if (SharedClass._Customerqueue == null)
                    SharedClass._Customerqueue = new Queue<CustomerData>();
                return SharedClass._Customerqueue;
            }
            set
            {
                SharedClass._Customerqueue = value;
            }
        }

        internal static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                    _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                return _connectionString;
            }
        }


        #endregion
        #region INTERNAL METHODS
        
        internal static void ThreadSleep(int sleepTimeMilliSeconds)
        {
            if (sleepTimeMilliSeconds > 0)
            {
                try
                {
                    System.Threading.Thread.Sleep(sleepTimeMilliSeconds);
                }
                catch (System.Threading.ThreadAbortException) { }
                catch (System.Threading.ThreadInterruptedException) { }
            }
        }

        #endregion
    }
}
