using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace HubChatAgentLookUp
{   
    internal static class Logger
    {
        private static ILog _defaultLogger = null;
        private static ILog _traceLogger = null;
        private static ILog _publishLogger = null;
        internal static void InitiaLizeLogger()
        {
            GlobalContext.Properties["LogName"] = DateTime.Now.ToString("yyyyMMdd");
            log4net.Config.XmlConfigurator.Configure();
            _defaultLogger = LogManager.GetLogger("Log");
            _traceLogger = LogManager.GetLogger("TraceLogger");
            _publishLogger = LogManager.GetLogger("PublishLogger");
        }
        internal static void Info(string message, LogTarget target = LogTarget.DEFAULT)
        {
            switch(target)
            {
                case LogTarget.DEFAULT:
                    _defaultLogger.Info(message);
                    break;
                case LogTarget.PUBLISHER:
                    _publishLogger.Info(message);
                    break;
                case LogTarget.TRACE:
                    _traceLogger.Info(message);
                    break;
                default:
                    _defaultLogger.Info(message);
                    break;
            }
        }
        internal static void Error(string message, LogTarget target = LogTarget.DEFAULT)
        {
            switch (target)
            {
                case LogTarget.DEFAULT:
                    _defaultLogger.Error(message);
                    break;
                case LogTarget.PUBLISHER:
                    _publishLogger.Error(message);
                    break;
                case LogTarget.TRACE:
                    _traceLogger.Error(message);
                    break;
                default:
                    _defaultLogger.Error(message);
                    break;
            }
        }
        internal static void Debug(string message, LogTarget target = LogTarget.DEFAULT)
        {
            switch (target)
            {
                case LogTarget.DEFAULT:
                    _defaultLogger.Debug(message);
                    break;
                case LogTarget.PUBLISHER:
                    _publishLogger.Debug(message);
                    break;
                case LogTarget.TRACE:
                    _traceLogger.Debug(message);
                    break;
                default:
                    _defaultLogger.Debug(message);
                    break;
            }
        }
        internal static void Warn(string message, LogTarget target = LogTarget.DEFAULT)
        {
            switch (target)
            {
                case LogTarget.DEFAULT:
                    _defaultLogger.Warn(message);
                    break;
                case LogTarget.PUBLISHER:
                    _publishLogger.Warn(message);
                    break;
                case LogTarget.TRACE:
                    _traceLogger.Warn(message);
                    break;
                default:
                    _defaultLogger.Warn(message);
                    break;
            }
        }
    }
}
