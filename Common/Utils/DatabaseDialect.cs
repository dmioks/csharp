using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.DAL
{
    public enum eDatabaseDialect
    {
        Undefined = -1,
        MsSql = 0, // Microsoft SQL Server
        MySql = 1, // MySql
        PgSql = 2, // PostGreSql
    };
}
