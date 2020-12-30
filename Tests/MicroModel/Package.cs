using CK.Setup;
using CK.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroModel
{
    [SqlitePackage(ResourcePath = "Res", Database = typeof(SqliteDefaultDatabase))]
    public class Package : SqlitePackage
    {
    }
}
