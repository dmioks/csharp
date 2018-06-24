using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Server
{
    public enum eResponseResult
    {
        None = -1,
        Succeeded        = 0,
        PartiallyFailed  = 1,
        Failed_SessionExpired = 1010, // Session expired. User should proceed to login page.
        Failed_IncorrectCredentials = 1020, // Either user name or password failed.
        Failed_InvalidArguments = 1025, // Data given as argument is not correct
        Failed_DataNotFound = 1030,
        Failed_DocumentNotFound = 1040,
        Failed_UnauthorizedOperation = 1050,
        Failed_ServerError = 1210,
        Failed_HandlerDoesNotExist = 1220,
        Failed_HandlerNotImplemented = 1230,
        Failed_Timeout = 1240,
        Validation_Error = 1250
    }
}
