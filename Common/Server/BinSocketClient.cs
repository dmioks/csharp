using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Dmioks.Common.Binary;
using Dmioks.Common.Collections;
using Dmioks.Common.Entity;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Server
{
    public class BinSocketClient : SocketClient<BinMessage>
    {
        public BinSocketClient(TcpClient tc, Stream stream, string sName, int iUniqueId, IConnectionCallback<BinMessage> cc, int iAlivePeriodMilles, int iWriteQueueCapacity) : base (tc, stream, sName, iUniqueId, cc, iAlivePeriodMilles, iWriteQueueCapacity)
        {
            m_br.ClassFactory.Add(BinMessage.BIN_MESSAGE_TYPE, BinMessage.CREATE_BIN_MESSAGE);
        }

        public BinFileStream CreateWriteFileStream(string sContext, string sFileName)
        {
           return new BinFileStream(this, sContext, sFileName, this.GetNextFileId(), eBinFileStreamType.WriteOnly);
        }

        protected override BinMessage ReadMessage()
        {
            SimpleEntity seEntity = m_br.ReadObject();
            BinMessage bm = seEntity as BinMessage;

            Debug.Assert(bm != null);

            return bm;
        }

        public void AddToFactory(int iBinClassType, DelegateCreateNewObject dcno)
        {
            m_br.ClassFactory.Add(iBinClassType, dcno);
        }

        protected override void WriteMessageImp(BinMessage objMessage)
        {
            m_bw.WriteObject(objMessage);
        }

        protected override BinMessage Create(eMessageType emt)
        {
            return BinMessage.Create(emt);
        }

        protected override void ProcessEvent(BinMessage objEvent)
        {

        }

        protected override void ProcessPong(BinMessage objPong)
        {

        }
    }
}
