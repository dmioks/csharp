using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Json;
using Dmioks.Common.Logs;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Server
{
    public class JsonEntitySocketClient : SocketClient<JsonMessage>
    {
        JsonParser m_jsonParser = new JsonParser();
        private static ILog m_logger = LogFactory.GetLogger(typeof(JsonEntitySocketClient));

        protected readonly IConnectionCallback<JsonMessage> m_cc;

        public JsonEntitySocketClient(TcpClient tc, Stream stream, string sName, int iUniqueId, IConnectionCallback<JsonMessage> cc, int iAlivePeriodMilles, int iWriteQueueCapacity) : base(tc, stream, sName, iUniqueId, cc, iAlivePeriodMilles, iWriteQueueCapacity)
        {
            Debug.Assert(cc != null);

            m_cc = cc;
        }

        protected override JsonMessage ReadMessage()
        {
            try
            {
                JsonMessage jsonMessage = m_jsonParser.Parse<JsonMessage>(m_br, new JsonMessage(), ref m_bStop);
                Debug.Assert(jsonMessage != null);

                return jsonMessage;
            }
            catch (Exception e)
            {
                m_logger.Error(e, e.Message);
                throw;
            }
        }

        protected override void WriteMessageImp(JsonMessage objMessage)
        {
            objMessage.Serialize(m_bw, 0);
            m_bw.WriteByte(0);
            m_bw.WriteByte(0);
        }

        protected override JsonMessage Create(eMessageType emt)
        {
            return JsonMessage.Create(emt);
        }

        protected override void ProcessEvent(JsonMessage objEvent)
        {

        }

        protected override void ProcessPong(JsonMessage objPong)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            m_jsonParser.Dispose();
        }
    }
}
