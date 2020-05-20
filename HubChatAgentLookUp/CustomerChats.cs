using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HubChatAgentLookUp
{
    internal class CustomerChats
    {
        private Mutex _queueMutex = new Mutex();
        private Publisher _publisher = null;
        private bool _shouldIPoll = true;
        

        internal CustomerChats(Publisher publisherObj)
        {
            this._publisher = publisherObj;
        }

        public void AgentCustomerMapper()
        {
            try
            {
                //int agentId = 0;
                CustomerData CusObj = null;
                Logger.Info("Agent Customer Mapper Started ");

                while (!SharedClass.HasStopSignal && this._shouldIPoll)
                {
                    try
                    {
                        if (this.QueueCount() > 0)
                        {
                            Logger.Info("Processing customer chats,Queue count : " + this.QueueCount().ToString());
                            CusObj = new CustomerData();
                            CusObj = this.DeQueue();
                            Logger.Info("Processing customer chats :" + CusObj.Channel);

                            (int agentId, bool IsPingAgent) = this.GetAvailableAgent(CusObj);
                            if (agentId > 0)
                            {
                                JObject wsMessageObj = new JObject();
                                JObject dataObj = new JObject();

                                dataObj = new JObject(new JProperty("AccountId", CusObj.AccountId),
                                                    new JProperty("WidgetId", CusObj.WidgetId),
                                                    new JProperty("WidgetUUID", CusObj.WidgetUUID),
                                                    new JProperty("CustomerChannel", CusObj.Channel),
                                                    new JProperty("ConversationId", CusObj.ConversationId),
                                                    new JProperty("MaxAttempts", CusObj.WidgetId));

                                wsMessageObj = new JObject(new JProperty("Module", "Chat"),
                                                            new JProperty("Event", "NewChat"),
                                                            new JProperty("Channel_Name", "Agent_" + agentId.ToString()),
                                                            new JProperty("Data", dataObj));

                                this._publisher.EnQueue(wsMessageObj);
                                Logger.Info(string.Format("Enqueued the websocket Message : {0}", wsMessageObj));

                            }
                            else
                            {
                                this.EnQueue(CusObj);
                                SharedClass.ThreadSleep(1000);
                            }

                        }
                        else
                            SharedClass.ThreadSleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("Error at AgentCustomerMapper , Error : {0}", ex.ToString()));
                    }
                    finally
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error at AgentCustomerMapper , Error : {0}", ex.ToString()));
            }

        }
        private (int, bool) GetAvailableAgent(CustomerData chat)
        {
            int agentId = 0;
            bool IsPing = false;
            SqlConnection sqlCon = new SqlConnection(SharedClass.ConnectionString);
            try
            {

                SqlCommand sqlCommand = new SqlCommand(StoredProcedure.GET_AVAILABLE_AGENT, sqlCon);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.Add(ProcedureParameter.ACCOUNT_ID, SqlDbType.Int).Value = chat.AccountId;
                sqlCommand.Parameters.Add(ProcedureParameter.WIDGET_ID, SqlDbType.BigInt).Value = chat.WidgetId;
                sqlCommand.Parameters.Add(ProcedureParameter.CONVERSATION_ID, SqlDbType.BigInt).Value = chat.ConversationId;
                sqlCommand.Parameters.Add(ProcedureParameter.AGENT_ID, SqlDbType.Int).Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(ProcedureParameter.IS_PING, SqlDbType.Bit).Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(ProcedureParameter.SUCCESS, SqlDbType.Bit).Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(ProcedureParameter.MESSAGE, SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                sqlCon.Open();
                sqlCommand.ExecuteNonQuery();
                sqlCon.Close();

                if (Convert.ToBoolean(sqlCommand.Parameters[ProcedureParameter.SUCCESS].Value) == true)
                {
                    if (!Convert.IsDBNull(sqlCommand.Parameters[ProcedureParameter.AGENT_ID].Value) && Convert.ToInt32(sqlCommand.Parameters[ProcedureParameter.AGENT_ID].Value) > 0)
                    {
                        agentId = Convert.ToInt32(sqlCommand.Parameters[ProcedureParameter.AGENT_ID].Value);
                        IsPing = Convert.ToBoolean(sqlCommand.Parameters[ProcedureParameter.IS_PING].Value);
                    }
                }
                else
                {
                    Logger.Error(string.Format("No Data returned from database for Proc {0} , AccountId : {1} AND Message From DB : {2}", StoredProcedure.GET_AVAILABLE_AGENT, chat.AccountId, sqlCommand.Parameters[ProcedureParameter.MESSAGE].Value));
                    SharedClass.ThreadSleep(1000);
                }
                //Logger.Info(string.Format("Agentid : {0}", agentId));
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Error at GetAvailableAgent Error : {0}", ex.ToString()));
                SharedClass.ThreadSleep(1000);
            }
            finally
            {
                if (sqlCon.State == ConnectionState.Open)
                    sqlCon.Close();
            }
            return (agentId,IsPing);
        }

        internal bool EnQueue(CustomerData Chat)
        {
            bool enQueued = false;
            try
            {
                while (!this._queueMutex.WaitOne())
                    Thread.Sleep(20);
                SharedClass.Customerqueue.Enqueue(Chat);
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Exception while EnQueuing Message. {0}", e.ToString()), LogTarget.PUBLISHER);
                enQueued = false;
            }
            finally
            {
                this._queueMutex.ReleaseMutex();
            }
            return enQueued;
        }
        private int QueueCount()
        {
            int count = 0;
            try
            {
                while (!this._queueMutex.WaitOne())
                    Thread.Sleep(20);
                count = SharedClass.Customerqueue.Count;
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Exception querying the Queue Count. {0}", e.ToString()), LogTarget.PUBLISHER);
            }
            finally
            {
                this._queueMutex.ReleaseMutex();
            }
            return count;
        }
        private CustomerData DeQueue()
        {
            CustomerData Chat = null;
            try
            {
                while (!this._queueMutex.WaitOne())
                    Thread.Sleep(20);
                if (SharedClass.Customerqueue.Count > 0)
                    Chat = SharedClass.Customerqueue.Dequeue();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Exception DeQueing Message. {0}", e.ToString()), LogTarget.PUBLISHER);
            }
            finally
            {
                this._queueMutex.ReleaseMutex();
            }
            return Chat;
        }
    }
}
