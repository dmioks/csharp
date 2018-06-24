using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Server
{
    public enum eMessageType
    {
        EmptyAlive = 0,
        Ping = 1,
        Pong = 2,
        Request = 3,
        Response = 4,
        Event = 5,
        File = 7,
        Close = 10,
    }
}
