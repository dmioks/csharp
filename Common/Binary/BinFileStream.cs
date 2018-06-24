using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Server;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Binary
{
    public delegate void DelegateFileChunkReceivedCallback(byte[] arrChunkBytes, bool bIsFinal);

    public enum eBinFileStreamType
    {
        WriteOnly,
        ReadOnly
    }

    public class BinFileStream
    {
        public readonly BinSocketClient BinSocketClient;
        public readonly int     FileId;
        public readonly string  Context;
        public readonly string  FileName;
        public readonly eBinFileStreamType StreamType;

        protected long m_lBufPosition = 0;
        protected int m_iChunkId = 0;
        protected long m_lBytesCount = 0;
        protected bool m_bIsFinalized = false;

        public event DelegateFileChunkReceivedCallback FileChunkReceived;

        protected readonly object m_objLocker = new object();

        // Read Only Constructor
        internal protected BinFileStream(BinSocketClient bsc, string sContext, string sFileName, int iFileId, eBinFileStreamType ebfst)
        {
            Debug.Assert(bsc != null);
            Debug.Assert(!string.IsNullOrEmpty(sContext));
            Debug.Assert(!string.IsNullOrEmpty(sFileName));
            Debug.Assert(0 < iFileId);
            //Debug.Assert(dfcrCallback != null);

            this.BinSocketClient = bsc;

            this.Context = sContext;
            this.FileName = sFileName;
            this.FileId = iFileId;

            this.StreamType = ebfst;
        }

        public int LastChunkId
        {
            get
            {
                lock (m_objLocker)
                {
                    return m_iChunkId;
                }
            }
        }

        public long BytesCount
        {
            get
            {
                lock (m_objLocker)
                {
                    return m_lBytesCount;
                }
            }
        }

        public bool IsFinalized
        {
            get
            {
                lock (m_objLocker)
                {
                    return m_bIsFinalized;
                }
            }
        }

        public void ProcessReadFileChunk (FileMessageBody fmb)
        {
            Debug.Assert(this.FileId == fmb.FileId);
            ExcpHelper.ThrowIf<InvalidOperationException>(this.StreamType == eBinFileStreamType.WriteOnly, $"BinFileStream.ProcessReadFileChunk() ERROR: This stream is write only. {this}");
            ExcpHelper.ThrowIf<InvalidDataException>(m_bIsFinalized, $"BinFileStream.ProcessReadFileChunk() ERROR: This stream is finalized. {this}");

            lock (m_objLocker)
            {
                int iExpectedChunkId = m_iChunkId + 1;

                ExcpHelper.ThrowIf<InvalidOperationException>(fmb.ChunkId != iExpectedChunkId, $"BinFileStream.ProcessReadFileChunk() ERROR: Inconsistent chunk id (Expected={iExpectedChunkId}) in {fmb}");

                m_iChunkId ++;
                m_lBytesCount += fmb.FileChunk.Length;

                if (this.FileChunkReceived != null)
                {
                    this.FileChunkReceived(fmb.FileChunk, fmb.IsFinalChunk);
                }
            }
        }

        public void WriteByOnceChunk(byte[] arrChunk)
        {
            this.Write(arrChunk, true);
        }

        public void Write(byte[] arrChunk, bool bIsFinal)
        {
            ExcpHelper.ThrowIf<InvalidOperationException>(this.StreamType == eBinFileStreamType.ReadOnly, $"BinFileStream.Write() ERROR: This stream is read only. {this}");
            ExcpHelper.ThrowIf<InvalidDataException>(arrChunk == null || arrChunk.Length == 0, $"BinFileStream.Write() ERROR: Either null or empty array. {this}");
            ExcpHelper.ThrowIf<InvalidDataException>(m_bIsFinalized, $"BinFileStream.Write() ERROR: This stream is finalized. {this}");

            AsyncResponse ar = new AsyncResponse();

            lock (m_objLocker)
            {
                FileMessageBody fmb = FileMessageBody.Create(this.FileId, this.Context, this.FileName, ++ m_iChunkId, bIsFinal);
                fmb.FileChunk = arrChunk;

                BinMessage bm = new BinMessage();
                bm.MessageType = eMessageType.File;
                bm.FileBody = fmb;
                bm.AsyncResponse = ar;

                this.BinSocketClient.TryEnqueue(bm);
                ar.WaitResponse();
                ar.Reset();

                m_lBytesCount += arrChunk.Length;

                m_bIsFinalized |= bIsFinal;
            }
        }

        public override string ToString()
        {
            lock (m_objLocker)
            {
                return $"{this.GetType().Name} {{{this.StreamType}, FileId={this.FileId}, Context={this.Context}, FileName={this.FileName}, LastChunkId={m_iChunkId}, BytesCount={m_lBytesCount}, IsFinalized={m_bIsFinalized}}}";
            }
        }
    }
}
