using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Entity;
using Dmioks.Common.Server;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Binary
{
    public class BinMessage : SimpleEntity, IMessage
    {
        public static readonly PropertyKey<long>                  PK_ID               = new PropertyKey<long>(1, "Id", PropertyTypes.INT_64);
        public static readonly PropertyKey<long>                  PK_REQUEST_ID       = new PropertyKey<long>(2, "RequestId", PropertyTypes.INT_64);
        public static readonly PropertyKey<int>                   PK_MESSAGE_TYPE     = new PropertyKey<int>(6, "MessageType", PropertyTypes.INT_32);
        public static readonly PropertyKey<int>                   PK_HANDLER          = new PropertyKey<int>(8, "Handler", PropertyTypes.INT_32);
        public static readonly PropertyKey<SimpleEntity>          PK_BODY             = new PropertyKey<SimpleEntity>(12, "Body", PropertyTypes.OBJECT);
        public static readonly PropertyKey<long>                  PK_TIME             = new PropertyKey<long>(16, "Time", PropertyTypes.INT_64);
        public static readonly PropertyKey<long>                  PK_REQUEST_TIME     = new PropertyKey<long>(17, "RequestTime", PropertyTypes.INT_64);

        public static readonly DelegateCreateNewObject CREATE_BIN_MESSAGE = delegate (object objParam) { return new BinMessage(); };
        public const int BIN_MESSAGE_TYPE = 10;

        public static readonly ObjectType OBJECT_TYPE = new ObjectType(BIN_MESSAGE_TYPE, typeof(BinMessage).Name, CREATE_BIN_MESSAGE, new PropertyKey[]
        {
            PK_ID,
            PK_REQUEST_ID,
            PK_MESSAGE_TYPE,
            PK_HANDLER,
            PK_BODY,
            PK_TIME,
            PK_REQUEST_TIME,
        });

        public BinMessage() : base(BinMessage.OBJECT_TYPE)
        {
        }

        public static BinMessage Create(eMessageType emt)
        {
            BinMessage bmAlive = new BinMessage();

            bmAlive.MessageType = emt;
            bmAlive.Time = TimeStamp.GetUnixTimestamp();

            return bmAlive;
        }

        public static BinMessage CreateResponse(BinMessage bmRequest, BinMessageBody bmBody)
        {
            BinMessage bmResponse = new BinMessage();

            bmResponse.MessageType = eMessageType.Response;
            bmResponse.Time = TimeStamp.GetUnixTimestamp();

            if (bmBody != null)
            {
                bmResponse.Body = bmBody;
            }

            // Help Props
            bmResponse.RequestId = bmRequest.Id;
            bmResponse.RequestTime = bmRequest.Time;

            return bmResponse;
        }

        public static BinMessage CreateEvent(int iHandler, BinMessageBody body)
        {
            BinMessage bmEvent = new BinMessage();
            bmEvent.MessageType = eMessageType.Event;
            bmEvent.Handler = iHandler;
            bmEvent.Time = TimeStamp.GetUnixTimestamp();
            bmEvent.Body = body;

            return bmEvent;
        }

        public AsyncResponse AsyncResponse { get; set; }

        public long Id
        {
            get { return PK_ID.Value(this); }
            set { PK_ID.Set(this, value); }
        }

        public long RequestId
        {
            get { return PK_REQUEST_ID.Value(this); }
            set { PK_REQUEST_ID.Set(this, value); }
        }

        public eMessageType MessageType
        {
            get
            {
                int iType = PK_MESSAGE_TYPE.Value(this);
                Debug.Assert(Enum.IsDefined(typeof(eMessageType), iType));

                eMessageType emType = (eMessageType)iType;

                return emType;
            }
            set
            {
                PK_MESSAGE_TYPE.Set(this, (int)value);
            }
        }

        public int Handler
        {
            get { return PK_HANDLER.Value(this); }
            set { PK_HANDLER.Set(this, value); }
        }

        public BinMessageBody Body
        {
            get { return PK_BODY.ObjectValue<BinMessageBody>(this); }
            set { PK_BODY.Set(this, value); }
        }

        public FileMessageBody FileBody
        {
            get { return PK_BODY.ObjectValue<FileMessageBody>(this); }
            set { PK_BODY.Set(this, value); }
        }

        public long Time
        {
            get { return PK_TIME.Value(this); }
            set { PK_TIME.Set(this, value); }
        }

        public long RequestTime
        {
            get { return PK_REQUEST_TIME.Value(this); }
            set { PK_REQUEST_TIME.Set(this, value); }
        }
    }
}
