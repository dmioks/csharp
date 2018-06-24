using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NLog;


namespace Dmioks.Common.Logs
{
    public enum eLogType
    {
        NLog = 1,
    }

    public static class LogFactory
    {
        public const eLogType DEFAULT_LOGGER = eLogType.NLog;
        private static readonly ILogManager m_logManager;

        public static ILog GetLogger(Type type)
        {
            return m_logManager.GetLogger(type);
        }

        static LogFactory()
        {
            eLogType elt = GetLogType();

            switch (elt)
            {
                case eLogType.NLog:

                    m_logManager = NLogManagerWrapper.Instance;
                    break;

                default:

                    Debug.Assert(false, $"Unknown Logger Type ({elt})");
                    break;
            }
        }

        private static eLogType GetLogType()
        {
            return DEFAULT_LOGGER;
        }
    }

    public sealed class NLogManagerWrapper : ILogManager
    {
        public static readonly NLogManagerWrapper Instance = new NLogManagerWrapper();

        private NLogManagerWrapper()
        {

        }

        public ILog GetLogger(Type type)
        {
            var logger = NLog.LogManager.GetLogger(type.Name, type);

            return new NLogLogger(logger);
        }
    }

    internal sealed class NLogLogger : ILog
    {
        protected readonly NLog.ILogger m_logger;

        internal protected NLogLogger(NLog.ILogger logger)
        {
            m_logger = logger;
        }


        public void Trace(string sMessage, params object[] args)
        {
            m_logger.Trace(sMessage, args);
        }

        public void Debug(string sMessage, params object[] args)
        {
            m_logger.Debug(sMessage, args);
        }

        public void Info(string sMessage, params object[] args)
        {
            m_logger.Info(sMessage, args);
        }

        public void Warn(string sMessage, params object[] args)
        {
            m_logger.Warn(sMessage, args);
        }

        public void Error(string sMessage, params object[] args)
        {
            m_logger.Error(sMessage, args);
        }

        public void Error(Exception excp, string sMessage, params object[] args)
        {
            m_logger.Error(excp, sMessage, args);
        }

        public void Fatal(string sMessage, params object[] args)
        {
            m_logger.Fatal(sMessage, args);
        }
    }
}
