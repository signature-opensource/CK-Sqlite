using System;
using CK.Core;
using CK.Setup;

namespace CK.Sqlite.Setup
{
    /// <summary>
    /// Driver for <see cref="SqliteDatabaseConnectionItem"/> item. 
    /// </summary>
    public class SqliteDatabaseConnectionItemDriver : SetupItemDriver
    {
        readonly ISqliteManagerProvider _sqliteProvider;
        ISqliteManagerBase _connection;

        /// <summary>
        /// Initializes a new <see cref="SqliteDatabaseConnectionItem"/>.
        /// </summary>
        /// <param name="info">Driver build information (required by base SetupItemDriver).</param>
        /// <param name="sqliteProvider">The sql manager provider.</param>
        public SqliteDatabaseConnectionItemDriver( BuildInfo info, ISqliteManagerProvider sqliteProvider )
            : base( info )
        {
            _sqliteProvider = sqliteProvider;
        }

        /// <summary>
        /// Masked Item to formally be associated to a <see cref="SqliteDatabaseConnectionItem"/> item.
        /// </summary>
        public new SqliteDatabaseConnectionItem Item => (SqliteDatabaseConnectionItem)base.Item;

        /// <summary>
        /// Gets the Sql manager. This is initialized by <see cref="ExecutePreInit"/>.
        /// </summary>
        public ISqliteManagerBase SqliteManager => _connection;

        /// <summary>
        /// Initializes the <see cref="SqlManager"/> based on the <see cref="Item"/>'s <see cref="SqliteDatabaseConnectionItem.SqliteDatabase"/>.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <returns>True on success, false if the database can not be found in the <see cref="ISqliteManagerProvider"/>.</returns>
        protected override bool ExecutePreInit( IActivityMonitor monitor )
        {
            _connection = FindManager( _sqliteProvider, monitor, Item.SqliteDatabase );
            return _connection != null;
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="beforeHandlers">Initialization is done after the handlers (when true, this method does nothing).</param>
        /// <returns>True on success, false if an error occurred.</returns>
        protected override bool Init( IActivityMonitor monitor, bool beforeHandlers )
        {
            // TODO: ...Well, nothing.
            return true;
        }

        static ISqliteManagerBase FindManager( ISqliteManagerProvider sqlite, IActivityMonitor monitor, SqliteDatabase db )
        {
            ISqliteManagerBase c = null;
            if( !string.IsNullOrWhiteSpace( db.ConnectionString ) )
            {
                c = sqlite.FindManagerByConnectionString( db.ConnectionString );
            }
            if( c == null )
            {
                c = sqlite.FindManagerByName( db.Name );
            }
            if( c == null )
            {
                monitor.Error( $"Database '{db.Name}' not available." );
            }
            else if( !db.IsDefaultDatabase && db.InstallCore )
            {
                c.EnsureCKCoreIsInstalled( monitor );
            }
            return c;
        }
    }

}
