using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HubChatAgentLookUp
{
    internal class Publisher
    {
        private Queue<JObject> _queue = new Queue<JObject>();
        private Mutex _queueMutex = new Mutex();
        internal void Start()
        {
            try
            {
                Logger.Info(string.Format("Started using EndPoint {0}", SharedClass.WEB_SOCKET_END_POINT));
                Logger.Info(string.Format("Started using EndPoint {0}", SharedClass.WEB_SOCKET_END_POINT), LogTarget.PUBLISHER);
            connect:
                Task t = BoradCastMessageToSubscriber();
                t.Wait();
                if (!SharedClass.HasStopSignal)
                    goto connect;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString(), LogTarget.PUBLISHER);
            }
        }
        private async Task BoradCastMessageToSubscriber()
        {
            try
            {
                using (ClientWebSocket ws = new ClientWebSocket())
                {
                    Uri serverUri = new Uri(SharedClass.WEB_SOCKET_END_POINT);
                    await ws.ConnectAsync(serverUri, CancellationToken.None);
                    Logger.Info("Connected To WebSocket", LogTarget.PUBLISHER);
                    JObject message = null;
                    while (ws.State == WebSocketState.Open && !SharedClass.HasStopSignal)
                    {
                        Logger.Info("While block in broadcastMsgToSubscriber QueueCount :" + this.QueueCount().ToString());
                        if (this.QueueCount() > 0)
                        {
                            message = this.DeQueue();
                            Logger.Info(string.Format("Processing ws notifications : {0}", message));
                            if (message != null)
                            {
                                ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message.ToString()));
                                await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                        else
                            System.Threading.Thread.Sleep(2000);
                    }
                    Logger.Info(string.Format("WebSocket Client Disconnected from the server. HasStopSignal : {0}", SharedClass.HasStopSignal), LogTarget.PUBLISHER);
                }
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Exception {0}", e.ToString()), LogTarget.PUBLISHER);
            }
        }
        internal bool EnQueue(JObject message)
        {
            bool enQueued = false;
            try
            {
                while (!this._queueMutex.WaitOne())
                    Thread.Sleep(20);
                this._queue.Enqueue(message);
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
                count = this._queue.Count;
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
        private JObject DeQueue()
        {
            JObject message = null;
            try
            {
                while (!this._queueMutex.WaitOne())
                    Thread.Sleep(20);
                if (this._queue.Count > 0)
                    message = this._queue.Dequeue();
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Exception DeQueing Message. {0}", e.ToString()), LogTarget.PUBLISHER);
            }
            finally
            {
                this._queueMutex.ReleaseMutex();
            }
            return message;
        }
    }
}
