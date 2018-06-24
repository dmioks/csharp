using System;
using System.Diagnostics;
using Dmioks.Common.DAL;

namespace Dmioks.Common.Utils
{
    public class ConnectionConfig
    {
        public const string REPLACE_WITH_DB_NAME = "REPLACE_WITH_DB_NAME";
        public eDatabaseDialect Dialect { get; set; }
        public string Name { get; set; }
        public string Database { get; set; }
        public string Connection { get; set; }

        protected readonly int m_iHashCode;
        protected readonly string m_sToString;

        public ConnectionConfig()
        {

        }

        public ConnectionConfig (eDatabaseDialect dd, string sTagName, string sDatabase, string sConnection)
        {
            Debug.Assert(dd != eDatabaseDialect.Undefined);
            Debug.Assert(!string.IsNullOrEmpty(sTagName));
            Debug.Assert(!string.IsNullOrEmpty(sDatabase));
            Debug.Assert(!string.IsNullOrEmpty(sConnection));

            this.Dialect = dd;
            this.Name = sTagName;
            this.Database = sDatabase;
            this.Connection = sConnection;

            m_iHashCode = this.Name.GetHashCode() ^ this.Connection.GetHashCode() ^ this.Database.GetHashCode() ^ this.Dialect.GetHashCode();
        }

        public override int GetHashCode()
        {
            return m_iHashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            ConnectionConfig cc = obj as ConnectionConfig;

            return cc != null && this.Name.Equals(cc.Name) && this.Database.Equals(cc.Database) && Connection.Equals(cc.Connection) && this.Dialect.Equals(cc.Dialect);
        }

        public string GetConnectionString()
        {
            return Connection.Replace(REPLACE_WITH_DB_NAME, this.Database);
        }

        public string GetMasterConnectionString()
        {
            string sMasterDbName = GetSystemDatabaseName(this.Dialect);
            return Connection.Replace(REPLACE_WITH_DB_NAME, sMasterDbName);
        }

        public override string ToString()
        {
            return $"Connection {{Dialect={this.Dialect}, TagName='{this.Name}', Database='{this.Database}' ConnectionString='{Connection}'}}";
        }

        public const string MS_SQL_MASTER_NAME = "master";
        public const string PG_SQL_MASTER_NAME = "postgres";

        public static string GetSystemDatabaseName(eDatabaseDialect dd)
        {
            switch (dd)
            {
                case eDatabaseDialect.MsSql: return MS_SQL_MASTER_NAME;

                case eDatabaseDialect.PgSql: return PG_SQL_MASTER_NAME;

                case eDatabaseDialect.MySql:

                    throw new NotImplementedException();

                default:

                    Debug.Assert(false);
                    break;
            }

            return string.Empty;
        }
    }
}
