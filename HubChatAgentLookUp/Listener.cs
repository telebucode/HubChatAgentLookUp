using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;

namespace HubChatAgentLookUp
{
    internal class Listener
    {
        private string _ip = string.Empty;
        private int _port = 0;
        private List<string> _endPoints = null;
        private HttpListener _listener = null;
        private System.Threading.Thread _listeningThread = null;
        HttpListenerContext context = null;
        JObject responseJSonObject = new JObject();
        JArray responseJSonArray = new JArray();
        JObject tempJObject = new JObject();
        bool isListenerStarted = false;
        private List<string> _prefixes = null;
        private Publisher _publisher = null;
        internal void Initialize(Publisher publisher)
        {
            this._publisher = publisher;
            this._prefixes = new List<string>();
            this._prefixes.Add("http://" + this._ip + ":" + this._port.ToString() + "/");

            this._endPoints = new List<string>();
            foreach (string key in System.Configuration.ConfigurationManager.AppSettings.AllKeys)
                if (key.StartsWith("EndPoint_"))
                    this._endPoints.Add(string.Format("http://{0}:{1}/{2}/", this._ip, this._port, key.Replace("EndPoint_", "")));



            this._listeningThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.Start));
            this._listeningThread.Name = "Listener";
            this._listeningThread.Start();
        }
        private void Start()
        {
            this._listener = new HttpListener();
            foreach (string uriPrefix in this._prefixes)
            {
                Logger.Info("Adding Url " + uriPrefix + " To Listener");
                try
                {
                    this._listener.Prefixes.Add(uriPrefix);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error Adding prefix To Listener, Reason : " + ex.ToString());
                }
            }
            if (this._endPoints.Count == 2) //0)
                Logger.Warn("NO END_POINTS Configured");
            else
            {
                if (this._listener.Prefixes.Count == 0)
                    Logger.Error("No end points were bounded to the listener. Terminating Listener");
                else
                {
                    Logger.Info(string.Format("Starting Listener On {0}:{1}", this._ip, this._port));
                    try
                    {
                        this._listener.Start();
                        isListenerStarted = true;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(string.Format("Exception while starting listener. {0}", e.ToString()));
                    }
                    if (isListenerStarted)
                    {
                        while (!SharedClass.HasStopSignal)
                        {
                            try
                            {
                                try
                                {
                                    context = null;
                                    context = _listener.GetContext();
                                }
                                catch (HttpListenerException e)
                                {
                                    Logger.Error(string.Format("Exception while listening. {0}", e.ToString()));
                                    continue;
                                }
                                if (context != null)
                                {
                                    responseJSonArray.RemoveAll();
                                    responseJSonObject.RemoveAll();
                                    tempJObject.RemoveAll();
                                    Logger.Info(string.Format("New Request from {0}:{1} To {2}", context.Request.RemoteEndPoint.Address.ToString(), context.Request.RemoteEndPoint.Port.ToString(), context.Request.RawUrl));
                                    string data = new System.IO.StreamReader(context.Request.InputStream).ReadToEnd();
                                    JObject dataObj = new JObject();
                                    dataObj = JObject.Parse(data);
                                    Logger.Info("Listener, received Data : " + dataObj.ToString());
                                    HandleUIAction(dataObj);
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error(string.Format("Exception while processing request. {0}", e.ToString()));
                                responseJSonObject = new JObject(new JProperty("Success", false),
                                                                 new JProperty("Message", string.Format("Exception while Processing Request. {0}", e.ToString())));
                            }
                            finally
                            {
                                try
                                {
                                    if (context != null)
                                    {
                                        Logger.Info(string.Format("Response Payload: {0}", responseJSonObject));
                                        context.Response.OutputStream.Write(System.Text.Encoding.UTF8.GetBytes(responseJSonObject.ToString()), 0, System.Text.Encoding.UTF8.GetByteCount(responseJSonObject.ToString()));
                                        context.Response.Close();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Error(string.Format("Exception while writing response. {0}", e.ToString()));
                                }
                            }
                        }// While End
                    }
                } // EndPoints > 0
            }
        }
        
        private void HandleUIAction(JObject requestPayload)
        {
            string Action = string.Empty;
            int AccountId = 0, WidgetId = 0, ConversationId= 0;
            string WidgetUUID, Channel;
            try
            {
                if (requestPayload.SelectToken("Action") == null || requestPayload.SelectToken("Channel") == null || requestPayload.SelectToken("ConversationId") == null || requestPayload.SelectToken("AccountId") == null || requestPayload.SelectToken("WidgetId") == null)
                {
                    responseJSonObject = new JObject(new JProperty("Success", false),
                       new JProperty("Message", "Insufficient request payload"));
                    return;
                }
                Action = requestPayload.SelectToken("Action").ToString();
                AccountId = Convert.ToInt32(requestPayload.SelectToken("AccountId").ToString());
                Channel = requestPayload.SelectToken("Channel").ToString();
                WidgetId = Convert.ToInt32(requestPayload.SelectToken("WidgetId").ToString());
                ConversationId = Convert.ToInt32(requestPayload.SelectToken("ConversationId").ToString());
                switch (Action)
                {
                    case "NEWCHAT":
                        CustomerData chat = new CustomerData();
                        chat.AccountId = AccountId;
                        chat.Channel = Channel;
                        chat.WidgetId = WidgetId;
                        chat.ConversationId = ConversationId;
                        SharedClass.Customerqueue.Enqueue(chat);

                        break;
                    case "INPROGRESS":
                        //if (SharedClass.IsCampaignInstanceActive(campaignInstanceId))
                        //{
                        //    responseJSonObject = new JObject(new JProperty("Success", false),
                        //        new JProperty("Campaign already active"));
                        //}
                        //else
                        //{
                        //    Logger.Info(string.Format("Campaign InstanceId : {0} And Status : {1}", campaignInstanceId, campaignStatus));
                        //    if (!SharedClass.IsCampaignInstanceActive(campaignInstanceId) && "INPROGRESS,RESUME".Split(new char[] { ',' }).Contains(campaignStatus))
                        //    {
                        //        responseJSonObject = ResumeCampaign(campaignInstanceId);
                        //    }
                        //}
                        break;
                    default:
                        responseJSonObject = new JObject(new JProperty("Success", false),
                            new JProperty("Message", "Invalid Campaign Status"));
                        break;
                }
            }
            catch (Exception e)
            {
                responseJSonObject = new JObject(new JProperty("Success", false),
                    new JProperty("Message", "Unable to perform action."));
            }
        }
        private void ConfigureEndPoints()
        {
            foreach (string uri in this._endPoints)
            {
                try
                {
                    this._listener.Prefixes.Add(uri);
                }
                catch (Exception e)
                {
                    Logger.Error(string.Format("Exception while binding end point to listener object. {0}", e.ToString()));
                }
            }
        }
        public string Ip { get { return this._ip; } set { this._ip = value; } }
        public int Port { get { return this._port; } set { this._port = value; } }
    }
}
