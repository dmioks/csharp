using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;

namespace Dmioks.Common.Server
{
    public interface IConnectionCallback<T> where T : IMessage
    {
        void ProcessRequest(SocketClient<T> client, T objRequestMessage);
        //void BeginProcessFile(BinFileStream binFileStream);
    }
}
