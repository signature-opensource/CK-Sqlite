using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CK.Core;
using CK.Setup;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using CK.Text;

namespace CK.Sqlite.Setup
{
    /// <summary>
    /// Implements <see cref="IVersionedItemReader"/>.
    /// </summary>
    public class SqliteVersionedItemReader : IVersionedItemReader
    {
        /// <summary>
        /// Gets the current version of this store.
        /// </summary>
        public static int CurrentVersion { get; } = 1;

        bool _initialized;

        public SqliteVersionedItemReader( ISqliteManager manager )
        {
            Manager = manager ?? throw new ArgumentNullException( "manager" );
        }

        internal readonly ISqliteManager Manager;

        public static void AutoInitialize( ISqliteManager m )
        {
            using( m.Monitor.OpenTrace( "Installing SqlVersionedItemRepository store." ) )
            {
                m.ExecuteNonQuery( CreateVersionTableScript );
            }
        }

        public OriginalReadInfo GetOriginalVersions( IActivityMonitor monitor )
        {
            var items = new List<VersionedTypedName>();
            var features = new List<VFeature>();
            if( !_initialized )
            {
                AutoInitialize( Manager );
                _initialized = true;
            }
            using( var c = new SqliteCommand( "select FullName, ItemType, ItemVersion from CKCore_tItemVersionStore where FullName <> 'CK.SqlVersionedItemRepository'" ) { Connection = Manager.Connection } )
            using( var r = c.ExecuteReader() )
            {
                while( r.Read() )
                {
                    string fullName = r.GetString( 0 );
                    string itemType = r.GetString( 1 );
                    if( itemType == "VFeature" ) features.Add( new VFeature( fullName, CSemVer.SVersion.Parse( r.GetString( 2 ) ) ) );
                    else items.Add( new VersionedTypedName( fullName, itemType, Version.Parse(  r.GetString( 2 ) ) ) );
                }
            }
            monitor.Trace( $"Existing VFeatures: {features.Select( f => f.ToString() ).Concatenate()}" );
            return new OriginalReadInfo( items, features );
        }


        public VersionedName OnVersionNotFound( IVersionedItem item, Func<string, VersionedTypedName> originalVersions )
        {
            // Maps "Model.XXX" to "XXX" versions for default context and database.
            if( item.FullName.StartsWith( "[]db^Model.", StringComparison.Ordinal ) )
            {
                return originalVersions( "[]db^" + item.FullName.Substring( 11 ) );
            }
            return null;
        }

        public VersionedName OnPreviousVersionNotFound( IVersionedItem item, VersionedName prevVersion, Func<string, VersionedTypedName> originalVersions )
        {
            // Maps "Model.XXX" to "XXX" versions for default context and database.
            if( prevVersion.FullName.StartsWith( "[]db^Model.", StringComparison.Ordinal ) )
            {
                return originalVersions( "[]db^" + prevVersion.FullName.Substring( 11 ) );
            }
            // Old code: Handle non-prefixed FullName when not found.
            return null;
        }

        internal static string CreateTemporaryTableScript = @"
pragma temp_store = MEMORY;
create temporary table if not exists TMP_T
(
	F text not null PRIMARY KEY,
	T text not null,
	V text not null
);
";

        internal static string MergeTemporaryTableScript = @"
insert or replace into CKCore_tItemVersionStore( FullName, ItemType, ItemVersion ) select F, V, T from TMP_T;";

        internal static string CreateVersionTableScript = @"
create table if not exists CKCore_tItemVersionStore
(
	FullName text not null PRIMARY KEY,
	ItemType text not null,
	ItemVersion text not null
);
insert or replace into CKCore_tItemVersionStore( FullName, ItemType, ItemVersion ) values( 'CK.SqlVersionedItemRepository', '', '0' );
";

    }
}
