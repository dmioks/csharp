using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Entity;
using Dmioks.Common.Server;

namespace Dmioks.Common.Binary
{
    public class FileMessageBody : SimpleEntity
    {
        public const int VERY_FIRST_CHUNK_ID = 1;

        public static readonly PropertyKey<string>     PK_CONTEXT            = new PropertyKey<string>(2, "Context", PropertyTypes.STRING); // Sent only once in very first message
        public static readonly PropertyKey<int>        PK_FILE_ID            = new PropertyKey<int>(6, "FileId", PropertyTypes.INT_32); // Unique Id of file in current socket
        public static readonly PropertyKey<string>     PK_FILE_NAME          = new PropertyKey<string>(8, "FileName", PropertyTypes.STRING); // Sent only once in very first message
        //public static readonly PropertyKey<long>       PK_FILE_SIZE          = new PropertyKey<long>(12, "FileSize", PropertyTypes.INT_64);
        public static readonly PropertyKey<int>        PK_CHUNK_ID           = new PropertyKey<int>(16, "ChunkId", PropertyTypes.INT_32);
        public static readonly PropertyKey<byte[]>     PK_FILE_CHUNK         = new PropertyKey<byte[]>(18, "FileChunk", PropertyTypes.BYTE_ARRAY);
        public static readonly PropertyKey<bool>       PK_IS_FINAL_CHUNK     = new PropertyKey<bool>(22, "IsFinalChunk",  PropertyTypes.BOOL);

        public static readonly DelegateCreateNewObject CREATE_FILE_MESSAGE_BODY = delegate (object objParam) { return new FileMessageBody(); };
        public const int BIN_TYPE = 30;

        public static readonly ObjectType OBJECT_TYPE = new ObjectType(BIN_TYPE, typeof(BinMessageBody).Name, CREATE_FILE_MESSAGE_BODY, new PropertyKey[]
        {
            PK_CONTEXT,
            PK_FILE_ID,
            PK_FILE_NAME,
            //PK_FILE_SIZE,
            PK_CHUNK_ID,
            PK_FILE_CHUNK,
            PK_IS_FINAL_CHUNK,
        });

        public FileMessageBody() : base(FileMessageBody.OBJECT_TYPE)
        {
        }

        public FileMessageBody(ObjectType ot) : base(ot)
        {
        }

        public static FileMessageBody Create (int iFileId, string sContext, string sFileName, int iChunkId, bool bIsFinalChunk)
        {
            FileMessageBody fmb = new FileMessageBody();

            PK_FILE_ID.Set(fmb, iFileId);
            PK_CHUNK_ID.Set(fmb, iChunkId);

            if (iChunkId == VERY_FIRST_CHUNK_ID)
            {
                PK_CONTEXT.Set(fmb, sContext);
                PK_FILE_NAME.Set(fmb, sFileName);
            }

            PK_IS_FINAL_CHUNK.Set(fmb, bIsFinalChunk);

            return fmb;
        }

        public static BinMessageBody Create(eResponseResult err)
        {
            BinMessageBody bmBody = new BinMessageBody();

            bmBody.Result = err;

            return bmBody;
        }

        public string Context
        {
            get { return PK_CONTEXT.Value(this);}
            set { PK_CONTEXT.Set(this, value); }
        }

        public int FileId
        {
            get { return PK_FILE_ID.Value(this); }
            set { PK_FILE_ID.Set(this, value); }
        }

        public string FileName
        {
            get { return PK_FILE_NAME.Value(this); }
            set { PK_FILE_NAME.Set(this, value); }
        }

        /*
        public long FileSize
        {
            get { return PK_FILE_SIZE.Value(this); }
            set { PK_FILE_SIZE.Set(this, value); }
        }
        */

        public int ChunkId
        {
            get { return PK_CHUNK_ID.Value(this); }
            set { PK_CHUNK_ID.Set(this, value); }
        }

        public byte[] FileChunk
        {
            get { return PK_FILE_CHUNK.Value(this); }
            set { PK_FILE_CHUNK.Set(this, value); }
        }

        public bool IsFinalChunk
        {
            get { return PK_IS_FINAL_CHUNK.Value(this); }
            set { PK_IS_FINAL_CHUNK.Set(this, value); }
        }
    }
}
