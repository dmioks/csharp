using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Json
{
    public delegate JsonEntity DelegateCreateJsonObject();
    public delegate JsonEntity DelegateCreateJsonArray();

    public class JsonEntityMapper
    {
        private Dictionary<string, DelegateCreateJsonObject> m_diObjects = new Dictionary<string, DelegateCreateJsonObject>();
        private Dictionary<string, DelegateCreateJsonArray>  m_diArrays = new Dictionary<string, DelegateCreateJsonArray>();

        public static readonly JsonEntityMapper Instance = new JsonEntityMapper();

        public JsonObject CreateObject(string sKey)
        {
            if (m_diObjects.TryGetValue(sKey, out DelegateCreateJsonObject d))
            {
                JsonObject jo = d() as JsonObject;
                Debug.Assert(jo != null, $"JasonEntityMapper.CreateObject('{sKey}') returns null.");

                return jo;
            }

            return DefaultCreateJsonObject();
        }

        public JsonArray CreateArray(string sKey)
        {
            if (m_diArrays.TryGetValue(sKey, out DelegateCreateJsonArray d))
            {
                JsonArray ja = d() as JsonArray;
                Debug.Assert(ja != null, $"JasonEntityMapper.CreateArray('{sKey}') returns null.");

                return ja;
            }

            return DefaultCreateJsonArray();
        }

        public void AddCreateJsonObjectDelegate(string sKey, DelegateCreateJsonObject dcjo)
        {
            Debug.Assert(dcjo != null);
            m_diObjects.Add(sKey, dcjo);
        }

        public void AddCreateJsonArrayDelegate(string sKey, DelegateCreateJsonArray dcja)
        {
            Debug.Assert(dcja != null);
            m_diArrays.Add(sKey, dcja);
        }

        protected virtual JsonObject DefaultCreateJsonObject()
        {
            return new JsonObject();
        }

        protected virtual JsonArray DefaultCreateJsonArray()
        {
            return new JsonSimpleArray();
        }
    }
}
