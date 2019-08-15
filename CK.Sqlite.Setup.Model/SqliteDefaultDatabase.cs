using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Sqlite
{
    /// <summary>
    /// Typed <see cref="SqliteDatabase"/> for the default <see cref="SqliteDatabase"/>.
    /// </summary>
    public class SqliteDefaultDatabase : SqliteDatabase, IAmbientObject
    {
        /// <summary>
        /// Initializes the default database. Its name is <see cref="SqliteDatabase.DefaultDatabaseName"/>.
        /// </summary>
        public SqliteDefaultDatabase()
            : base( DefaultDatabaseName )
        {
        }

        void StObjConstruct( string connectionString )
        {
            ConnectionString = connectionString;
        }
    }
}
