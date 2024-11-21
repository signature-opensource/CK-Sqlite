using CK.Core;
using CK.Sqlite;

namespace MicroModel;

[SqliteTable( "tTests", Package = typeof( Package ) )]
[Versions( "1.0.0, 1.1.0" )]
public class TestsTable : SqliteTable
{

}
