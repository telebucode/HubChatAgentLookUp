using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubChatAgentLookUp
{
    internal class CustomerData
    {
        private int _accountId;
        private int _widgetId;
        private int _conversationId;
        private string _widgetUUID;
        private string _channel;
        internal int AccountId { get { return _accountId; } set { _accountId = value; } }
        internal int WidgetId { get { return _widgetId; } set { _widgetId = value; } }
        internal int ConversationId { get { return _conversationId; } set { _conversationId = value; } }
        internal string WidgetUUID { get { return _widgetUUID; } set { _widgetUUID = value; } }
        internal string Channel { get { return _channel; } set { _channel = value; } }
    }
}
