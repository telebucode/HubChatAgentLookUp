using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubChatAgentLookUp
{
    internal static class StoredProcedure
    {
        internal const string GET_PENDING_DIALOUT_CAMPAIGN_INSTANCES = "PD_GetPendingDialOutCampaignInstances";
        internal const string GET_AVAILABLE_AGENT = "GetAvailableAgentForChat";
    }

    internal static class ProcedureParameter
    {
        internal const string UUID = "@UUID";
        internal const string ACCOUNT_CALLER_ID = "@CallerID";
        internal const string ACCOUNT_ID = "@AccountId";
        internal const string WIDGET_ID = "@WidgetId";
        internal const string CONVERSATION_ID = "@ConversationId";
        internal const string INVALID_COUNT = "@InvalidCount";
        internal const string LAST_SLNO = "@LastSlno";
        internal const string AGENT_ID = "@AgentId";
        internal const string STATUS = "@Status";
        internal const string SUCCESS = "@Success";
        internal const string MESSAGE = "@Message";
    }

    internal static class Label
    {
        internal const string SUCCESS = "Success";
        internal const string OUTPUT_PARAMETERS = "OutputParameters";
    }
}
