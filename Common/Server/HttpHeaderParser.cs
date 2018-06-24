using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Dmioks.Common;
using System.Diagnostics;

namespace Dmioks.Common.Server
{
    public sealed class HttpHeaderParser
    {
        public static readonly char[] ARR_EOL = new char[] { '\r', '\n' };
        public static readonly char[] ARR_EOL_EOL = new char[] { '\r', '\n', '\r', '\n' };

        private const string REPLY_TEMPLATE = "HTTP/1.1 101 Switching Protocols\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            "Sec-WebSocket-Accept: ";

        protected const string GUID_STR = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        protected const string VERSION_NAME = "sec-websocket-version";
        protected const string KEY_NAME     = "sec-websocket-key";
        protected const string HOST_NAME    = "host";
        protected const int SUPPORTED_VERSION = 8;
        protected static char[] ARR_SEP = new char[] { ':' };
        protected Dictionary<string, string> m_di = new Dictionary<string, string>();
        protected string m_sStartLine = string.Empty;
        protected int    m_iVersion = 0;
        protected string m_sHost = null;
        protected string m_sSecWebsocketKey = null;

        private HttpHeaderParser ()
        {

        }

        public static HttpHeaderParser Parse(string sHeader)
        {
            try
            {
                HttpHeaderParser parser = new HttpHeaderParser();
                parser.ParseImp(sHeader);

                return parser;
            }
            catch (Exception excp)
            {

            }

            return null;
        }

        public bool IsValidForRFC6455
        {
            get
            {
                return SUPPORTED_VERSION <= m_iVersion && !string.IsNullOrEmpty(m_sSecWebsocketKey) && !string.IsNullOrEmpty(m_sHost);
            }
        }

        public string GenerateReply ()
        {
            var sha1 = SHA1.Create();
            string sAccept = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(m_sSecWebsocketKey + GUID_STR)));

            return REPLY_TEMPLATE + sAccept + "\r\n\r\n";
        }

        private void ParseImp (string sHeader)
        {
            string[] arrParts = sHeader.Split(ARR_EOL, StringSplitOptions.RemoveEmptyEntries);

            foreach (string sPart in arrParts)
            {
                int iIndex = sPart.IndexOf(':');

                if (0 < iIndex)
                {
                    string sKey = sPart.Substring(0, iIndex);
                    string sVal = sPart.Substring(iIndex + 1);

                    Debug.Assert(!m_di.ContainsKey(sKey));
                    m_di[sKey.Trim().ToLower()] = sVal.Trim();
                }
                else
                {
                    Debug.Assert(string.IsNullOrEmpty(m_sStartLine));
                    m_sStartLine = sPart;
                }
            }

            string sVersion = m_di[VERSION_NAME];

            if (!int.TryParse(sVersion, out m_iVersion))
            {
                m_iVersion = -1; // Error Parsed
            }

            m_di.TryGetValue(KEY_NAME, out m_sSecWebsocketKey);
            m_di.TryGetValue(HOST_NAME, out m_sHost);
        }

        public string this[string key]
        {
            get { return m_di[key]; }
        }

        public int Version
        {
            get
            {
                return m_iVersion;
            }
        }
    }
}
