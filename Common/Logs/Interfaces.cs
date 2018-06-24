using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Logs
{
    public interface ILogManager
    {
        ILog GetLogger(Type type);
    }

    public interface ILog
    {
      
        void Trace(string sMessage, params object[] args);
        void Debug(string sMessage, params object[] args);
        void Info(string sMessage, params object[] args);
        void Warn(string sMessage, params object[] args);
        void Error(string sMessage, params object[] args);
        void Error(Exception excp, string sMessage, params object[] args);
        void Fatal(string sMessage, params object[] args);
    }
}
