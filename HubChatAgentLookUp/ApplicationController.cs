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

            CustomerChats chat = new CustomerChats();
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
        /// <summary>
        /// Starts the polling.
        /// </summary>
        private void StartPolling()
        {
            try
            {

                Logger.Info("Started polling " + SharedClass.HasStopSignal.ToString());
                SqlConnection sqlConnection = new SqlConnection(SharedClass.ConnectionString);
                //SqlCommand sqlCommand = new SqlCommand(StoredProcedure.GET_PENDING_DIALOUT_CAMPAIGN_INSTANCES, sqlConnection);
                //SqlDataAdapter da = null;
                //DataSet ds = null;
                //while (!SharedClass.HasStopSignal)
                //{
                    
                //    try
                //    {
                //        sqlCommand.Parameters.Clear();
                //        sqlCommand.Parameters.Add(ProcedureParameter.LAST_SLNO, SqlDbType.BigInt).Value = _campaignInstanceLastSlno;
                //        sqlCommand.Parameters.Add(ProcedureParameter.SUCCESS, SqlDbType.Bit).Direction = ParameterDirection.Output;
                //        sqlCommand.Parameters.Add(ProcedureParameter.MESSAGE, SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                //        da = new SqlDataAdapter(sqlCommand);
                //        da.Fill(ds = new DataSet());
                        
                //        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                //        {
                            
                //            foreach (DataRow campaignInstanceRow in ds.Tables[0].Rows)
                //            {
                //                try
                //                {
                //                    Campaign campaign = new Campaign();
                //                    CampaignInstance instance = new CampaignInstance();
                //                    bool isExcelDataPollingStart = false;

                //                    TimeZone timeZone = new TimeZone();
                //                    CampaignInstanceProcessor instanceProcessor = null;
                //                    instance.Id = Convert.ToInt32(campaignInstanceRow["InstanceId"]);
                //                    timeZone.TimeZoneName = campaignInstanceRow["TimeZone"].ToString();
                //                    timeZone.UtcOffSetInMinutes = Convert.ToInt32(campaignInstanceRow["UtcOffSetInMinutes"]);
                //                    timeZone.Id = Convert.ToByte(campaignInstanceRow["TargetTimeZoneId"]);
                //                    campaign.Id = Convert.ToInt32(campaignInstanceRow["CampaignId"]);
                //                    campaign.TimeZone = timeZone;
                //                    campaign.FromTime = campaignInstanceRow["FromTime"].ToString();
                //                    campaign.ToTime = campaignInstanceRow["ToTime"].ToString();
                //                    campaign.MaxLines = Convert.ToByte(campaignInstanceRow["MaximumLines"]);
                //                    campaign.AllowOtherAgentsToJoin = Convert.ToBoolean(campaignInstanceRow["AllowOtherAgentsToJoin"]);
                //                    instance.InitializationType = Convert.ToByte(campaignInstanceRow["InitializationTypeID"]);
                //                    if (campaignInstanceRow.Table.Columns.Contains("IsSkillGroupBasedDistribution") && campaignInstanceRow["IsSkillGroupBasedDistribution"] != DBNull.Value)
                //                        campaign.IsSkillGroupBasedDistribution = Convert.ToBoolean(campaignInstanceRow["IsSkillGroupBasedDistribution"].ToString());
                //                    if (instance.InitializationType == 0)
                //                    {
                //                        Logger.Info(string.Format("Campaign Instance Id : {0} conatains initializationType as 0 so Skipping", instance.Id));
                //                        continue;
                //                    }
                //                    Logger.Info(string.Format("Initialization Id : {0} And CampaignId : {1},FilesCount : {2}", instance.InitializationType, campaign.Id, ds.Tables[1].Rows.Count));
                //                    if (!Convert.ToBoolean(campaignInstanceRow["IsInstanceAlreadyExists"]))
                //                    {
                //                        if (ds.Tables[1].Rows.Count > 0 && instance.InitializationType == 1)
                //                        {
                //                            isExcelDataPollingStart = true;
                //                            foreach (DataRow campaignDataFile in ds.Tables[1].Select("CampaignId = " + campaign.Id.ToString()))
                //                            {
                //                                Logger.Info(string.Format("Files fetching : {0} And FileMetdata : {1}", campaignDataFile["FilePath"], campaignDataFile["FileMetadata"]));
                //                                ExcelFileInfo dataFileInfo = new ExcelFileInfo();
                //                                dataFileInfo.FilePath = Convert.ToString(campaignDataFile["FilePath"]);
                //                                dataFileInfo.MetaData = Convert.ToString(campaignDataFile["FileMetadata"]);
                //                                campaign.DataFiles.Add(dataFileInfo);
                //                            }
                //                        }
                //                        else if (ds.Tables[1].Rows.Count > 0)
                //                        {   
                //                            if(instance.InitializationType==2 || instance.InitializationType==3)
                //                            {
                //                                isExcelDataPollingStart = false;
                //                                Logger.Info(string.Format("Campaign relaunching with initialization type : {0} and Instace Id : {1}", instance.InitializationType == 2 ? "Disposition" : "End reason", instance.Id));
                //                            }
                                            
                //                        }
                //                    }

                //                    instance.Campaign = campaign;
                //                    _campaignInstanceLastSlno = instance.Id;
                //                    instanceProcessor = new CampaignInstanceProcessor(instance, publisherCls, isExcelDataPollingStart);
                //                    instance.Processor = instanceProcessor;
                //                    System.Threading.Thread cipt = new Thread(new ThreadStart(instance.Processor.Start));
                //                    cipt.Name = "CampaignInstanceProcessor_" + instance.Id.ToString();
                //                    cipt.Start();
                //                }
                //                catch (Exception e)
                //                {
                //                    Logger.Error(string.Format("Error In For Loop. {0}", e.ToString()));
                //                }
                //            }
                //        }
                //        try
                //        {
                //            System.Threading.Thread.Sleep(SharedClass.POLL_INTERVAL_IN_SECONDS * 1000);
                //        }
                //        catch (System.Threading.ThreadAbortException e)
                //        {

                //        }
                //        catch (System.Threading.ThreadInterruptedException e)
                //        {

                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        Logger.Error(string.Format("Error while getting pending campaign instances, {0}", e.ToString()));
                //    }
                //    finally
                //    {
                //        if (sqlConnection.State == ConnectionState.Open)
                //            sqlConnection.Close();

                //        if (sqlCommand != null)
                //            sqlCommand.Dispose();
                //    }
                //}
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            Logger.Info("Exit");
        }

        private void LoadConfig()
        {
            Logger.InitiaLizeLogger();
            SharedClass.Listener.Ip = ConfigurationManager.AppSettings["ListenerIp"] == null ? "" : ConfigurationManager.AppSettings["ListenerIp"].ToString();
            SharedClass.Listener.Port = ConfigurationManager.AppSettings["ListenerPort"] == null ? 0 : Convert.ToInt16(ConfigurationManager.AppSettings["ListenerPort"]);
        }

    }

}
