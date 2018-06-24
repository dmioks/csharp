using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Json
{
    public enum eJsonParserState
    {
        // Root State - When parser is waiting next object or array
        ExpectingObjectOrArray = 0, // '{', '[', 

        // Parsing Object States
        ExpectingNameOrObjectEnd = 110, // '"', '}',
        ExpectingSeparatorInObject = 120, // ':'
        ExpectingValueInObject = 130, // Object, Array, Digit, String, true, false, null
        ExpectingCommaOrObjectEnd = 140, // ',', '}'
        ExpectingNameInObject = 150, // '"'

        // Parsing Array States
        ExpectingArrayValueOrEnd = 210, // Digit, String, true, false, null, '{', '[', ']'
        ExpectingCommaOrArrayEnd = 220, // ',', ']'
        ExpectingArrayValue = 230, // Digit, String, true, false, null, '{', '['
    }

    public class ParserStackRecord
    {
        public eJsonParserState State;
        public string CurrentName = string.Empty;
        public readonly JsonObject JsonObject;
        public readonly JsonArray JsonArray;

        protected internal ParserStackRecord(eJsonParserState jpState, JsonObject jo, JsonArray ja)
        {
            this.State = jpState;

            Debug.Assert((jo != null && ja == null) || (jo == null && ja != null));

            this.JsonObject = jo;
            this.JsonArray = ja;
        }

        protected internal void SetStateAndName(eJsonParserState jps, string sName)
        {
            this.State = jps;
            this.CurrentName = sName;
        }

        protected internal JsonEntity JsonEntity
        {
            get
            {
                if (this.JsonObject != null)
                {
                    return this.JsonObject;
                }

                Debug.Assert(this.JsonArray != null);

                return this.JsonArray;
            }
        }
    }

    public class ParserStack
    {
        protected readonly List<ParserStackRecord> m_lStack = new List<ParserStackRecord>();
        protected ParserStackRecord m_psrCurrent = null;

        protected internal void Add(ParserStackRecord psr)
        {
            m_lStack.Add(psr);
            m_psrCurrent = psr;
        }

        protected internal ParserStackRecord Remove()
        {
            int i = m_lStack.Count - 1;
            ParserStackRecord psr = m_lStack[i];
            m_lStack.RemoveAt(i);

            m_psrCurrent = 0 < m_lStack.Count ? m_lStack[m_lStack.Count - 1] : null;

            return psr;
        }

        protected internal ParserStackRecord Current
        {
            get { return m_psrCurrent; }
        }

        protected internal eJsonParserState CurrentState
        {
            get { return m_psrCurrent != null ? m_psrCurrent.State : eJsonParserState.ExpectingObjectOrArray; }
        }

        protected internal int Count
        {
            get { return m_lStack.Count; }
        }
    }

    public class JsonParser : IDisposable
    {
        /*
        private static readonly char[] ARR_SPECIAL_CHARS = new Char[]
        {
            '\b', //  8
            '\t', //  9
            '\f', // 12
            '\n', // 10
            '\r', // 13
            '"',  // 34
            '\\', // 92
        };

        public static readonly HashSet<char> SET_OF_SPECIAL_CHARS = new HashSet<char>(ARR_SPECIAL_CHARS);
        */

        static readonly HashSet<char> SET_OF_NAME_CHARACTERS =
            new HashSet<char>("._-0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());

        static readonly HashSet<char> SET_OF_DIGIT_CHARACTERS = new HashSet<char>("-.0123456789".ToCharArray());

        public static readonly char[] ARR_CHARS_NULL = new Char[] {'n', 'u', 'l', 'l'};
        public static readonly char[] ARR_CHARS_TRUE = new Char[] {'t', 'r', 'u', 'e'};
        public static readonly char[] ARR_CHARS_FALSE = new Char[] {'f', 'a', 'l', 's', 'e'};

        protected readonly ParserStack m_Stack = new ParserStack();
        protected StringBuilder m_sb = StringBuilderPool.Instance.Get();

        public void Dispose()
        {
            StringBuilderPool.Instance.Put(m_sb);
            m_sb = null;
        }

        public int CurrentDepth
        {
            get { return m_Stack.Count; }
        }

        protected string ParseName(IRead br, ref bool bIsToStop)
        {
            Debug.Assert(m_sb.Length == 0);

            try
            {
                while (!bIsToStop)
                {
                    char ch = br.ReadChar();

                    if (ch == '"')
                    {
                        string sName = m_sb.ToString();

                        if (sName.Length == 0)
                        {
                            throw new Exception();
                        }

                        return sName;
                    }

                    if (!SET_OF_NAME_CHARACTERS.Contains(ch))
                    {
                        string sParsedName = m_sb.ToString();
                        throw new Exception($"Invalid character in name {sParsedName} '{ch}'");
                    }

                    m_sb.Append(ch);
                }
            }
            finally
            {
                m_sb.Clear();
            }

            Debug.Assert(false);

            return null;
        }

        protected string ParseString(IRead br, ref bool bIsToStop)
        {
            Debug.Assert(m_sb.Length == 0);

            try
            {
                while (!bIsToStop)
                {
                    char ch = br.ReadChar();

                    if (ch == '"')
                    {
                        return m_sb.ToString();
                    }
                    else if (ch == '\\')
                    {
                        // Read special Symbol
                        ch = br.ReadChar();

                        switch (ch)
                        {
                            case 'b': // '\b' 8

                                m_sb.Append((char) 8);
                                break;

                            case 't': // '\t' 9

                                m_sb.Append((char) 9);
                                break;

                            case 'f': // '\f' 12

                                m_sb.Append((char) 12);
                                break;

                            case 'n': // '\n' 10

                                m_sb.Append((char) 10);
                                break;

                            case 'r': // '\r' 13

                                m_sb.Append((char) 13);
                                break;

                            case '"': // '"' 34

                                m_sb.Append((char) 34);
                                break;

                            case '\\': // '\\' 92

                                m_sb.Append((char) 92);
                                break;

                            default:

                                string sParsedName = m_sb.ToString();
                                throw new Exception($"Invalid character in string after '\\' {sParsedName} '{ch}'");
                                break;
                        }

                        continue;
                    }

                    m_sb.Append(ch);
                }
            }
            finally
            {
                m_sb.Clear();
            }

            Debug.Assert(false);

            return null;
        }

        protected string ParseDigitAsString(IRead br, char chFirst, ref bool bContainsFloat, ref bool bIsToStop)
        {
            Debug.Assert(m_sb.Length == 0);
            Debug.Assert(SET_OF_DIGIT_CHARACTERS.Contains(chFirst));

            m_sb.Append(chFirst);

            try
            {
                while (!bIsToStop)
                {
                    int iChar = br.PeekChar();

                    if (iChar < 0)
                    {
                        throw new Exception();
                    }

                    char ch = (char) iChar;

                    if (!SET_OF_DIGIT_CHARACTERS.Contains(ch))
                    {
                        return m_sb.ToString();
                    }

                    char chCHeck = br.ReadChar();
                    Debug.Assert(chCHeck == ch);

                    if (ch == '.' || ch == ',')
                    {
                        bContainsFloat = true;
                    }

                    m_sb.Append(ch);
                }
            }
            finally
            {
                m_sb.Clear();
            }

            Debug.Assert(false);

            return null;
        }

        protected object ParseDigit(IRead br, char chFirst, ref bool bIsToStop)
        {
            bool bContainsFloat = false;
            string sDigit = ParseDigitAsString(br, chFirst, ref bContainsFloat, ref bIsToStop);

            if (bContainsFloat)
            {
                sDigit = sDigit.Replace('.', ',');

                if (decimal.TryParse(sDigit, out decimal dcResult))
                {
                    return dcResult;
                }

                throw new Exception();
            }

            if (long.TryParse(sDigit, out long lResult))
            {
                return lResult;
            }

            throw new Exception();
        }

        protected static void ParseTemplate(IRead br, char[] arrTemplate)
        {
            for (int i = 1; i < arrTemplate.Length; i++)
            {
                char ch = br.ReadChar();

                if (ch != arrTemplate[i])
                {
                    throw new Exception();
                }
            }
        }

        protected static void ParseObjectEnd(ParserStackRecord psrFromStack, ParserStackRecord psrParseCompleted)
        {
            if (psrFromStack.JsonObject != null)
            {
                Debug.Assert(psrFromStack.JsonArray == null);
                Debug.Assert(psrFromStack.State == eJsonParserState.ExpectingValueInObject);
                Debug.Assert(!string.IsNullOrEmpty(psrFromStack.CurrentName));

                psrFromStack.JsonObject.Set(psrFromStack.CurrentName, psrParseCompleted.JsonEntity);
                psrFromStack.SetStateAndName(eJsonParserState.ExpectingCommaOrObjectEnd, string.Empty);
            }
            else if (psrFromStack.JsonArray != null)
            {
                Debug.Assert(psrFromStack.JsonObject == null);
                Debug.Assert(psrFromStack.State == eJsonParserState.ExpectingArrayValueOrEnd ||
                             psrFromStack.State == eJsonParserState.ExpectingArrayValue);

                psrFromStack.JsonArray.Add(psrParseCompleted.JsonEntity);
                psrFromStack.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
            }
        }

        public JsonEntity Parse(IRead br, ref bool bIsToStop)
        {
            int iChar = br.PeekChar();
            char ch = (char)iChar;

            switch (ch)
            {
                case '{': return this.Parse<JsonObject>(br, new JsonObject(), ref bIsToStop);
                case '[': return this.Parse<JsonSimpleArray>(br, new JsonSimpleArray(), ref bIsToStop);
            }

            throw new Exception($"Parse() ERROR. Incorrect first character '{ch}'");
        }

        public T Parse<T>(IRead br, T objNewInstance, ref bool bIsToStop) where T:JsonEntity
        {
            Debug.Assert(br != null);
            Debug.Assert(objNewInstance != null);

            while (!bIsToStop)
            {
                char ch = br.ReadChar();

                switch (m_Stack.CurrentState)
                {
                    // Root State - When parser is waiting next object or array
                    case eJsonParserState.ExpectingObjectOrArray: // '{', '[',
                        {
                            switch (ch)
                            {
                                case '{':

                                    JsonObject joNew = objNewInstance as JsonObject;
                                    Debug.Assert(joNew != null);
                                    m_Stack.Add(new ParserStackRecord(eJsonParserState.ExpectingNameOrObjectEnd, joNew, null));
                                    break;

                                case '[':

                                    JsonArray jaNew = objNewInstance as JsonArray;
                                    Debug.Assert(jaNew != null);
                                    m_Stack.Add(new ParserStackRecord(eJsonParserState.ExpectingArrayValueOrEnd, null, jaNew));
                                    break;
                            }
                        }

                        break;

                    // Parsing Object States
                    case eJsonParserState.ExpectingNameOrObjectEnd: // '"', '}',
                        {
                            switch (ch)
                            {
                                case '"':

                                    string sName = this.ParseName(br, ref bIsToStop);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingSeparatorInObject, sName);
                                    break;

                                case '}':

                                    ParserStackRecord psrParseCompleted = m_Stack.Remove();

                                    if (m_Stack.Count == 0)
                                    {
                                        return psrParseCompleted.JsonEntity as T;
                                    }

                                    ParseObjectEnd(m_Stack.Current, psrParseCompleted);
                                    break;

                                case '\r':
                                case '\n':
                                case '\t':
                                case ' ':

                                    // Do nothing here
                                    break;

                                default:

                                    throw new Exception("Invalid Character");
                                    break;
                            }
                        }

                        break;

                    case eJsonParserState.ExpectingSeparatorInObject: // ':'
                        {
                            switch (ch)
                            {
                                case ':':

                                    m_Stack.Current.State = eJsonParserState.ExpectingValueInObject;
                                    break;

                                case '\r':
                                case '\n':
                                case '\t':
                                case ' ':

                                    // Do nothing here
                                    break;

                                default:

                                    throw new Exception("Invalid Character");
                                    break;
                            }
                        }

                        break;

                    case eJsonParserState.ExpectingValueInObject: // Object, Array, Digit, String, true, false, null
                        {
                            Debug.Assert(!string.IsNullOrEmpty(m_Stack.Current.CurrentName));

                            switch (ch)
                            {
                                case '{': // Object

                                    JsonObject joNew = JsonEntityMapper.Instance.CreateObject(m_Stack.Current.CurrentName);

                                    //m_Stack.Add(new ParserStackRecord(eJsonParserState.ExpectingNameOrObjectEnd, joNew, null));
                                    break;

                                case '[': // Array

                                    JsonArray jaNew = JsonEntityMapper.Instance.CreateArray(m_Stack.Current.CurrentName);

                                    //m_Stack.Add(new ParserStackRecord(eJsonParserState.ExpectingArrayValueOrEnd, null, jaNew));
                                    break;

                                case '-': // Digit
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':

                                    object objDigit = this.ParseDigit(br, ch, ref bIsToStop);

                                    m_Stack.Current.JsonObject.Set(m_Stack.Current.CurrentName,objDigit);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrObjectEnd, string.Empty);
                                    break;

                                case '"': // String

                                    string sValue = this.ParseString(br, ref bIsToStop);

                                    m_Stack.Current.JsonObject.Set(m_Stack.Current.CurrentName,sValue);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrObjectEnd, string.Empty);
                                    break;

                                case 't': // true

                                    ParseTemplate(br, ARR_CHARS_TRUE);

                                    m_Stack.Current.JsonObject.Set(m_Stack.Current.CurrentName, true);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrObjectEnd, string.Empty);
                                    break;

                                case 'f': // false

                                    ParseTemplate(br, ARR_CHARS_FALSE);

                                    m_Stack.Current.JsonObject.Set(m_Stack.Current.CurrentName,false);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrObjectEnd, string.Empty);
                                    break;

                                case 'n': // null

                                    ParseTemplate(br, ARR_CHARS_NULL);

                                    m_Stack.Current.JsonObject.Set<JsonObject>(m_Stack.Current.CurrentName, null);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrObjectEnd, string.Empty);
                                    break;

                                case '\r':
                                case '\n':
                                case '\t':
                                case ' ':

                                    // Do nothing here
                                    break;

                                default:

                                    throw new Exception("Invalid Character");
                                    break;
                            }
                        }

                        break;

                    case eJsonParserState.ExpectingCommaOrObjectEnd: // ',', '}'
                        {
                            switch (ch)
                            {
                                case ',':

                                    m_Stack.Current.State = eJsonParserState.ExpectingNameInObject;
                                    break;

                                case '}':

                                    ParserStackRecord psrParseCompleted = m_Stack.Remove();

                                    if (m_Stack.Count == 0)
                                    {
                                        return psrParseCompleted.JsonEntity as T;
                                    }

                                    ParseObjectEnd(m_Stack.Current, psrParseCompleted);
                                    break;

                                case '\r':
                                case '\n':
                                case '\t':
                                case ' ':

                                    // Do nothing here
                                    break;

                                default:

                                    throw new Exception("Invalid Character");
                                    break;
                            }
                        }

                        break;

                    case eJsonParserState.ExpectingNameInObject: // '"'
                        {
                            switch (ch)
                            {
                                case '"':

                                    string sName = this.ParseName(br, ref bIsToStop);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingSeparatorInObject, sName);
                                    break;

                                case '\r':
                                case '\n':
                                case '\t':
                                case ' ':

                                    // Do nothing here
                                    break;

                                default:

                                    throw new Exception("Invalid Character");
                                    break;
                            }
                        }

                        break;

                    // Parsing Array States
                    case eJsonParserState.ExpectingArrayValueOrEnd: // '{', '[', ']', Digit, String, true, false, null, 
                        {
                            Debug.Assert(m_Stack.Current.JsonArray != null);

                            switch (ch)
                            {
                                case '{': // Object

                                    JsonObject joNew = (JsonObject) m_Stack.Current.JsonArray.CreateObject();

                                    m_Stack.Add(new ParserStackRecord(eJsonParserState.ExpectingNameOrObjectEnd, joNew, null));
                                    break;

                                case '[': // Array

                                    JsonArray jaNew = (JsonArray) m_Stack.Current.JsonArray.CreateObject();

                                    m_Stack.Add(new ParserStackRecord(eJsonParserState.ExpectingArrayValueOrEnd, null, jaNew));
                                    break;

                                case ']':

                                    ParserStackRecord psrParseCompleted = m_Stack.Remove();

                                    if (m_Stack.Count == 0)
                                    {
                                        return psrParseCompleted.JsonEntity as T;
                                    }

                                    ParseObjectEnd(m_Stack.Current, psrParseCompleted);
                                    break;

                                case '-': // Digit
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':

                                    object objDigit = this.ParseDigit(br, ch, ref bIsToStop);

                                    m_Stack.Current.JsonArray.Add(objDigit);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case '"': // String

                                    string sValue = this.ParseString(br, ref bIsToStop);

                                    m_Stack.Current.JsonArray.Add(sValue);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case 't': // true

                                    ParseTemplate(br, ARR_CHARS_TRUE);

                                    m_Stack.Current.JsonArray.Add(true);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case 'f': // false

                                    ParseTemplate(br, ARR_CHARS_FALSE);

                                    m_Stack.Current.JsonArray.Add(false);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case 'n': // null

                                    ParseTemplate(br, ARR_CHARS_NULL);

                                    m_Stack.Current.JsonArray.Add(null);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case '\r':
                                case '\n':
                                case '\t':
                                case ' ':

                                    // Do nothing here
                                    break;

                                default:

                                    throw new Exception("Invalid Character");
                                    break;
                            }
                        }

                        break;

                    case eJsonParserState.ExpectingCommaOrArrayEnd: // ',', ']'
                        {
                            switch (ch)
                            {
                                case ',':

                                    m_Stack.Current.State = eJsonParserState.ExpectingArrayValue;
                                    break;

                                case ']': // Array

                                    ParserStackRecord psrParseCompleted = m_Stack.Remove();

                                    if (m_Stack.Count == 0)
                                    {
                                        return psrParseCompleted.JsonEntity as T;
                                    }

                                    ParseObjectEnd(m_Stack.Current, psrParseCompleted);

                                    break;

                                case '\r':
                                case '\n':
                                case '\t':
                                case ' ':

                                    // Do nothing here
                                    break;

                                default:

                                    throw new Exception("Invalid Character");
                                    break;
                            }
                        }
                        break;

                    case eJsonParserState.ExpectingArrayValue: // Digit, String, true, false, null, '{', '['
                        {
                            Debug.Assert(m_Stack.Current.JsonArray != null);

                            switch (ch)
                            {
                                case '{': // Object

                                    JsonObject joNew = (JsonObject)m_Stack.Current.JsonArray.CreateObject();

                                    m_Stack.Add(new ParserStackRecord(eJsonParserState.ExpectingNameOrObjectEnd, joNew, null));
                                    break;

                                case '[': // Array

                                    JsonArray jaNew = (JsonArray)m_Stack.Current.JsonArray.CreateObject();

                                    m_Stack.Add(new ParserStackRecord(eJsonParserState.ExpectingArrayValueOrEnd, null, jaNew));
                                    break;

                                case '-': // Digit
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':

                                    object objDigit = this.ParseDigit(br, ch, ref bIsToStop);

                                    m_Stack.Current.JsonArray.Add(objDigit);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case '"': // String

                                    string sValue = this.ParseString(br, ref bIsToStop);

                                    m_Stack.Current.JsonArray.Add(sValue);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case 't': // true

                                    ParseTemplate(br, ARR_CHARS_TRUE);

                                    m_Stack.Current.JsonArray.Add(true);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case 'f': // false

                                    ParseTemplate(br, ARR_CHARS_FALSE);

                                    m_Stack.Current.JsonArray.Add(false);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case 'n': // null

                                    ParseTemplate(br, ARR_CHARS_NULL);

                                    m_Stack.Current.JsonArray.Add(null);
                                    m_Stack.Current.SetStateAndName(eJsonParserState.ExpectingCommaOrArrayEnd, string.Empty);
                                    break;

                                case '\r':
                                case '\n':
                                case '\t':
                                case ' ':

                                    // Do nothing here
                                    break;

                                default:

                                    throw new Exception("Invalid Character");
                                    break;
                            }
                        }
                        break;

                    default:

                        Debug.Assert(false);
                        break;
                }
            }

            return null;
        }
    }
}
