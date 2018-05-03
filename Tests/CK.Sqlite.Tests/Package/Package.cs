using CK.Setup;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Sqlite.Tests.Package
{
    [SqlitePackage(ResourcePath = "Res", Database = typeof(SqliteDefaultDatabase))]
    public class Package : SqlitePackage
    {
    }

    [SqliteTable("tTests", Package = typeof(Package))]
    [Versions("1.0.0, 1.1.0")]
    public class TestsTable : SqliteTable
    {

    }
}
