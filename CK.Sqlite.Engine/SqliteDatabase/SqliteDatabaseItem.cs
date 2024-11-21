using System;
using System.Collections.Generic;
using CK.Core;
using CK.Setup;

namespace CK.Sqlite.Setup;

/// <summary>
/// Setup item that models the Sql database.
/// </summary>
public class SqliteDatabaseItem : StObjDynamicContainerItem
{
    /// <summary>
    /// All <see cref="SqliteDatabaseItem"/> share the same name: "SqliteDatabase".
    /// Their full names are defined only by context and location (the logical databasename): 
    /// "[context]dbName^SqliteDatabase".
    /// </summary>
    public static string SqlDatabaseItemName = "SqliteDatabase";

    internal readonly SqliteDatabaseConnectionItem ConnectionItem;

    class Model : ISetupItem, IDependentItemGroup, IDependentItemGroupRef
    {
        readonly SqliteDatabaseItem _holder;

        public Model( SqliteDatabaseItem h )
        {
            _holder = h;
        }

        public IDependentItemContainerRef Container => null;

        public string Context => _holder.Context;

        public string Location => _holder.Location;

        public string Name => "Model." + _holder.Name;

        public string FullName => DefaultContextLocNaming.Format( _holder.Context, _holder.Location, Name );

        string IContextLocNaming.TransformArg => null;

        public IDependentItemRef Generalization => null;

        public IEnumerable<IDependentItemGroupRef> Groups => null;

        public IEnumerable<IDependentItemRef> RequiredBy => null;

        public IEnumerable<IDependentItemRef> Requires => new[] { _holder.ConnectionItem };

        public string TransformArg => null;

        public bool Optional => false;

        public IEnumerable<IDependentItemRef> Children => null;

        public object StartDependencySort( IActivityMonitor m ) => null;

        /// <inheritdoc />
        public IContextLocNaming CombineName( string n ) => new ContextLocName( new ContextLocNameStructImpl( this ).CombineName( n ) );
    }

    /// <summary>
    /// >Initializes a new <see cref="SqliteDatabaseItem"/>.
    /// </summary>
    /// <param name="monitor">The monitor to use.</param>
    /// <param name="data">The setup data from actual object.</param>
    public SqliteDatabaseItem( IActivityMonitor monitor, IStObjSetupData data )
        : base( monitor, data, typeof( SqliteDatabaseItemDriver ) )
    {
        Context = string.Empty;
        Location = ActualObject.Name;
        Name = SqlDatabaseItemName;
        ConnectionItem = new SqliteDatabaseConnectionItem( this );
        Requires.Add( new Model( this ) );
    }

    /// <summary>
    /// Masked to return a <see cref="SqliteDatabase"/>.
    /// </summary>
    public new SqliteDatabase ActualObject => (SqliteDatabase)base.ActualObject;

    /// <summary>
    /// Gets the name of the SqlDatabaseItem based on the context and location.
    /// </summary>
    /// <param name="contextLocName">The non null context-locaton-name.</param>
    /// <returns>The associated database item name.</returns>
    public static string ItemNameFor( IContextLocNaming contextLocName )
    {
        return DefaultContextLocNaming.Format( contextLocName.Context, contextLocName.Location, SqlDatabaseItemName );
    }
}
