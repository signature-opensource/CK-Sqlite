using System;
using CK.Core;
using CK.Setup;

namespace CK.Sqlite.Setup;

/// <summary>
/// Sql table item.
/// </summary>
public class SqliteTableItem : SqlitePackageBaseItem
{
    /// <summary>
    /// Initializes a new <see cref="SqliteTableItem"/>.
    /// </summary>
    /// <param name="monitor">The monitor to use.</param>
    /// <param name="data">The StObj data.</param>
    public SqliteTableItem( IActivityMonitor monitor, IStObjSetupData data )
        : base( monitor, data )
    {
        Name = data.FullNameWithoutContext;
    }

    /// <summary>
    /// Masked to formally be associated to <see cref="SqliteTable"/>.
    /// </summary>
    public new SqliteTable ActualObject => (SqliteTable)base.ActualObject;

}
