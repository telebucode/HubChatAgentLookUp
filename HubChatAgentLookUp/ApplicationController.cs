using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Configuration;

namespace HubChatAgentLookUp
{
    class ApplicationController
    {
        private Thread _ChatsThread = null;
        private Thread[] _publishers = null;
        Publisher publisherCls = null;
        internal ApplicationController()
        {
            this.LoadConfig();
        }
        internal void Start()
        {
            Logger.Info("Starting");

            SharedClass.IsServiceCleaned = false;

            publisherCls = new Publisher();

            CustomerChats chat = new CustomerChats(publisherCls);
            this._ChatsThread = new Thread(new ThreadStart(chat.AgentCustomerMapper));
            this._ChatsThread.Name = "ChatsProcessor";
            this._ChatsThread.Start();

            Logger.Info("Poller thread Started");

            if (!SharedClass.WEB_SOCKET_END_POINT.IsEmpty() && SharedClass.NUMBER_OF_PUBLISHERS > 0)
            {
                this._publishers = new System.Threading.Thread[SharedClass.NUMBER_OF_PUBLISHERS];
                for (byte index = 1; index <= SharedClass.NUMBER_OF_PUBLISHERS; index++)
                {
                    this._publishers[index - 1] = new System.Threading.Thread(publisherCls.Start);
                    this._publishers[index - 1].Name = string.Format("Publisher_{0}", index);
                    this._publishers[index - 1].Start();
                }
            }
            if (SharedClass.Listener.Ip.Length > 7 && SharedClass.Listener.Port > 0)
            {
                Logger.Info("Starting Listener");
                SharedClass.Listener.Initialize(publisherCls);
            }
        }
        internal void Stop()
        {
            try
            {
                Logger.Info("STOP_SIGNAL Received");

                while (!this._ChatsThread.ThreadState.Equals(System.Threading.ThreadState.Stopped))
                {
                    Logger.Info(string.Format("Thread {0} not yet stopped. ThreadState: {1}", this._ChatsThread.Name, this._ChatsThread.ThreadState));
                    if (this._ChatsThread.ThreadState.Equals(System.Threading.ThreadState.WaitSleepJoin))
                        this._ChatsThread.Interrupt();
                    Thread.Sleep(2000);
                }

                

                if (this._publishers != null && this._publishers.Count() > 0)
                {
                    for (byte index = 0; index < this._publishers.Count(); index++)
                    {
                        while (!this._publishers[index].ThreadState.Equals(System.Threading.ThreadState.Stopped))
                        {
                            if (this._publishers[index].ThreadState.Equals(System.Threading.ThreadState.WaitSleepJoin))
                                this._publishers[index].Interrupt();
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                }
                Logger.Info("STOP_SIGNAL processed");
                SharedClass.IsServiceCleaned = true;
            }
            catch (ThreadAbortException ex)
            {
                Logger.Error("thread abort exception at ApplicationController Stop Method");
            }
            catch (ThreadInterruptedException ex)
            {
                Logger.Error("ThreadInterruptedException at ApplicationController Stop Method");
            }
            catch (Exception ex)
            {
                Logger.Error("Exception at ApplicationController Stop : " + ex.ToString());
            }

        }
        

        private void LoadConfig()
        {
            Logger.InitiaLizeLogger();
            SharedClass.Listener.Ip = ConfigurationManager.AppSettings["ListenerIp"] == null ? "" : ConfigurationManager.AppSettings["ListenerIp"].ToString();
            SharedClass.Listener.Port = ConfigurationManager.AppSettings["ListenerPort"] == null ? 0 : Convert.ToInt16(ConfigurationManager.AppSettings["ListenerPort"]);
        }

    }

}
