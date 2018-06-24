using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Entity;
using Dmioks.Common.Server;
using NLog.LayoutRenderers.Wrappers;

namespace Dmioks.Common.Binary
{
    public interface IMessage
    {
        AsyncResponse AsyncResponse { get; set; } 
        long Id { get; set; }
        eMessageType MessageType { get; }
        long Time { get; }
        long RequestId { get; set; }
        long RequestTime { get; set; }
        FileMessageBody FileBody { get; }
    }

    public interface IWrite
    {
        void WriteByte(byte bt);
        void WriteChar(char ch);
        void WriteString(string sValue);
    }

    public interface IRead
    {
        char PeekChar();
        char ReadChar();
    }

    public sealed class BinHeaders
    {
        public const byte OBJECT_TYPE = 10;

        private BinHeaders()
        {

        }
    }

    public interface IBinWrite : IWrite
    {
        byte[] UlongBuffer { get; }
        void WriteUshort(uint uiValue, bool bFlag);
        void WriteShortInt(int iValue);
        void WriteUlong(ulong ulValue);
        void WriteLong(long lValue);
        void WriteStringBin(string sValue);
        void WriteBinary(byte[] arr);
        void WriteObject(SimpleEntity si);
    }

    public interface IBinRead : IRead
    {
        byte ReadByte();
        uint ReadUshort(out bool bFlag);
        int ReadShortInt();
        ulong ReadUlong();
        long ReadLong();
        string ReadStringBin();
        byte[] ReadBinary();
        SimpleEntity ReadObject();
    }

    public interface IBinSerializable
    {
        void Serialize(IBinWrite bw, int iFormat);
    }
}
