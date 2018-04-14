using CK.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Sqlite.Tests
{
    class TemporarySqliteDatabase : IDisposable
    {
        readonly TemporaryFile _file;
        readonly string _connectionString;

        public TemporarySqliteDatabase()
        {
            _file = new TemporaryFile( ".sqlite" );
            _connectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = _file.Path,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();
        }

        public string ConnectionString => _connectionString;

        public void Dispose()
        {
            _file.Dispose();
        }
    }
}
