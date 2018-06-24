using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Server;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Json
{
    public class JsonMessage : JsonObject, IMessage
    {
        public const int    ERROR_ID   = - 1;

        private static readonly JsonEntityMapper m_mapper = new JsonEntityMapper();

        public static readonly DelegateCreateJsonObject CREATE_BODY_DTO = delegate () { return new JsonMessageBody(); };

        static JsonMessage()
        {
            m_mapper.AddCreateJsonObjectDelegate(KEY_BODY, CREATE_BODY_DTO);
        }

        public JsonMessage ()
        {

        }

      

        public AsyncResponse AsyncResponse { get; set; }

        public long Id
        {
            get { return this.GetLong(KEY_ID); }
            set { this.Set<long>(KEY_ID, value); }
        }

        public const string KEY_REQUEST_ID = "RequestId";
        public long RequestId
        {
            get
            {
                object objId = this.TryGet(KEY_REQUEST_ID, ERROR_ID);

                return Convert.ToInt64(objId);
            }
            set
            {
                this.Set<long>(KEY_REQUEST_ID, value);
            }
        }

        public const string KEY_TYPE = "Type";
        public eMessageType MessageType
        {
            get
            {
                int iType = this.GetInt(KEY_TYPE);
                Debug.Assert(Enum.IsDefined(typeof(eMessageType), iType));

                eMessageType emType = (eMessageType)iType;

                return emType;
            }
            set
            {
                this.Set<int>(KEY_TYPE, (int)value);
            }
        }

        public const string KEY_HANDLER = "Handler";
        public int Handler {
            get { return this.GetInt(KEY_HANDLER); }
            set { this.Set<int>(KEY_HANDLER, value); }
        }

        public const string KEY_BODY = "Body";
        public JsonMessageBody Body
        {
            get
            {
                if (this.TryGetValue(KEY_BODY, out object objBody))
                {
                    return objBody as JsonMessageBody;
                }

                return null;
            }

            set { this.Set<JsonMessageBody>(KEY_BODY, value); }
        }

        public const string KEY_TIME = "Time";
        public long Time
        {
            get { return this.GetLong(KEY_TIME); }
            set { this.Set<long>(KEY_TIME, value); }
        }

        public const string KEY_REQUEST_TIME = "RequestTime";
        public long RequestTime
        {
            get { return this.GetLong(KEY_REQUEST_TIME); }
            set { this.Set<long>(KEY_REQUEST_TIME, value); }
        }

        public FileMessageBody FileBody
        {
            get { return null; }
        }

        public static JsonMessage Create(eMessageType emt)
        {
            JsonMessage jmAlive = new JsonMessage();

            jmAlive.MessageType = emt;
            jmAlive.Time = TimeStamp.GetUnixTimestamp();

            return jmAlive;
        }

        public static JsonMessage CreateResponseMessage(JsonMessage jmRequest, JsonMessageBody body)
        {
            JsonMessage jmResponse = new JsonMessage();
            jmResponse.MessageType = eMessageType.Response;
            jmResponse.Time = TimeStamp.GetUnixTimestamp();
            jmResponse.Body = body;

            // Help Props
            jmResponse.RequestId = jmRequest.Id;
            jmResponse.RequestTime = jmRequest.Time;

            return jmResponse;
        }

        public static JsonMessage CreateEventMessage(int iHandler, JsonMessageBody body)
        {
            JsonMessage jmEvent = new JsonMessage();
            jmEvent.MessageType = eMessageType.Event;
            jmEvent.Handler = iHandler;
            jmEvent.Time = TimeStamp.GetUnixTimestamp();
            jmEvent.Body = body;

            return jmEvent;
        }
    }
}
