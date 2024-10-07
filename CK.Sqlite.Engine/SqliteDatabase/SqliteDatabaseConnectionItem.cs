using System.Collections.Generic;
using CK.Setup;
using CK.Core;

namespace CK.Sqlite.Setup;

/// <summary>
/// Defines the connection object.
/// Its driver is <see cref="SqliteDatabaseConnectionItemDriver"/>.
/// </summary>
public class SqliteDatabaseConnectionItem : ISetupItem, IDependentItemRef
{
    readonly SqliteDatabaseItem _db;

    /// <summary>
    /// Initializes a new <see cref="SqliteDatabaseConnectionItem"/>.
    /// </summary>
    /// <param name="db">The database item.</param>
    public SqliteDatabaseConnectionItem( SqliteDatabaseItem db )
    {
        _db = db;
    }

    /// <summary>
    /// Gets the <see cref="SqliteDatabase"/> object instance.
    /// </summary>
    public SqliteDatabase SqliteDatabase => _db.ActualObject;

    /// <summary>
    /// Gets the full name of this connection: : it is the FullName of the <see cref="SqliteDatabase"/> suffixed with ".Connection".
    /// </summary>
    public string FullName => _db.FullName + ".Connection";

    /// <summary>
    /// Gets the name of this connection: it is the Name of the <see cref="SqliteDatabase"/> suffixed with ".Connection".
    /// </summary>
    public string Name => _db.Name + ".Connection";

    IDependentItemContainerRef IDependentItem.Container => null;

    IDependentItemRef IDependentItem.Generalization => null;

    IEnumerable<IDependentItemRef> IDependentItem.Requires => null;

    IEnumerable<IDependentItemGroupRef> IDependentItem.Groups => null;

    IEnumerable<IDependentItemRef> IDependentItem.RequiredBy => null;

    object IDependentItem.StartDependencySort( IActivityMonitor m ) => typeof( SqliteDatabaseConnectionItemDriver );

    bool IDependentItemRef.Optional => false;

    /// <summary>
    /// Gets the context name.
    /// </summary>
    public string Context => _db.Context;

    /// <summary>
    /// Gets the location.
    /// </summary>
    public string Location => _db.Location;

    string IContextLocNaming.TransformArg => null;

    /// <inheritdoc />
    public IContextLocNaming CombineName( string n ) => new ContextLocName( new ContextLocNameStructImpl( this ).CombineName( n ) );
}
