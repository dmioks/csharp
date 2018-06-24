using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Server;

namespace Dmioks.Common.Json
{
    public class JsonMessageBody : JsonObject
    {

        public JsonMessageBody(JsonDictionary diItems) : base(diItems)
        {
        }

        public JsonMessageBody() : base()
        {
        }

        public const string KEY_RESULT = "Result";
        public eResponseResult Result
        {
            get
            {
                int iResult = this.GetInt(KEY_RESULT);

                if (Enum.IsDefined(typeof(eResponseResult), iResult))
                {
                    return (eResponseResult)iResult;
                }

                return eResponseResult.None;
            }

            set { this.Set<int>(KEY_RESULT, (int)value); }
        }

        public const string KEY_ERROR_MESSAGE = "ErrorMessage";
        public string ErrorMessage
        {
            get { return this.GetString(KEY_ERROR_MESSAGE); }
            set { this.Set<string>(KEY_ERROR_MESSAGE, value); }
        }

        public const string KEY_TOKEN = "Token";
        public string Token
        {
            get { return this.GetString(KEY_TOKEN); }
            set { this.Set<string>(KEY_TOKEN, value); }
        }

        public const string KEY_COMPANY_ID = "CompanyId";
        public int CompanyId
        {
            get { return this.GetInt(KEY_COMPANY_ID); }
            set { this.Set(KEY_COMPANY_ID, value); }
        }
        
        public const string KEY_INVOICE_ID = "InvoiceId";
        public long InvoiceId
        {
            get { return this.GetInt(KEY_INVOICE_ID); }
            set { this.Set(KEY_INVOICE_ID, value); }
        }

        public const string KEY_CONTENT = "Content";
        public JsonObject Content
        {
            get { return this.GetObject<JsonObject>(KEY_CONTENT); }
            set { this.SetObject<JsonObject>(KEY_CONTENT, value); }
        }

        public const string KEY_CONTENT_ARRAY = "ContentArray";
        public JsonArray<object> ContentArray
        {
            get { return this.GetJsonArray<object>(KEY_CONTENT_ARRAY); }
            set { this.SetJsonArray<object>(KEY_CONTENT_ARRAY, value); }
        }
    }
}
