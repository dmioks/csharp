using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Entity;
using Dmioks.Common.Server;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Binary
{
    public class BinMessageBody : SimpleEntity
    {
        public static readonly PropertyKey<int> PK_RESULT = new PropertyKey<int>(2, "Result", PropertyTypes.INT_32);

        public static readonly DelegateCreateNewObject CREATE_BIN_BODY_MESSAGE = delegate (object objParam) { return new BinMessageBody(); };
        public const int BIN_TYPE = 20;

        public static readonly ObjectType OBJECT_TYPE = new ObjectType(BIN_TYPE, typeof(BinMessageBody).Name, CREATE_BIN_BODY_MESSAGE, new PropertyKey[]
        {
            PK_RESULT,
        });

        public BinMessageBody() : base(BinMessageBody.OBJECT_TYPE)
        {
        }

        public BinMessageBody(ObjectType ot) : base(ot)
        {
        }

        public static BinMessageBody Create(eResponseResult err)
        {
            BinMessageBody bmBody = new BinMessageBody();

            bmBody.Result = err;

            return bmBody;
        }

        public eResponseResult Result
        {
            get
            {
                int iType = PK_RESULT.Value(this);
                Debug.Assert(Enum.IsDefined(typeof(eResponseResult), iType));

                eResponseResult err = (eResponseResult)iType;

                return err;
            }
            set
            {
                PK_RESULT.Set(this, (int)value);
            }
        }
    }
}
