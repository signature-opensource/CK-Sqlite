using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CK.Core;
using CK.Setup;
using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace CK.Sqlite.Setup
{
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
            var monitor = m.Monitor;
            using( monitor.OpenTrace( "Installing SqlVersionedItemRepository store." ) )
            {
                m.ExecuteNonQuery( CreateVersionTableScript );
            }
        }

        public IReadOnlyCollection<VersionedTypedName> GetOriginalVersions( IActivityMonitor monitor )
        {
            var result = new List<VersionedTypedName>();
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
                    Version v;
                    if( !Version.TryParse( r.GetString( 2 ), out v ) )
                    {
                        throw new Exception( $"Unable to parse version for {fullName}: '{r.GetString( 2 )}'." );
                    }
                    result.Add( new VersionedTypedName( fullName, r.GetString( 1 ), v ) );
                }
            }
            return result;
        }


        public VersionedName OnVersionNotFound( IVersionedItem item, Func<string, VersionedTypedName> originalVersions )
        {
            // Maps "Model.XXX" to "XXX" versions for default context and database.
            if( item.FullName.StartsWith( "[]db^Model.", StringComparison.Ordinal ) )
            {
                return originalVersions( "[]db^" + item.FullName.Substring( 11 ) );
            }
            // Old code: Handle non-prefixed FullName when not found.
            return item.FullName.StartsWith( "[]db^", StringComparison.Ordinal )
                    ? originalVersions( item.FullName.Substring( 5 ) )
                    : null;
        }

        public VersionedName OnPreviousVersionNotFound( IVersionedItem item, VersionedName prevVersion, Func<string, VersionedTypedName> originalVersions )
        {
            // Maps "Model.XXX" to "XXX" versions for default context and database.
            if( prevVersion.FullName.StartsWith( "[]db^Model.", StringComparison.Ordinal ) )
            {
                return originalVersions( "[]db^" + prevVersion.FullName.Substring( 11 ) );
            }
            // Old code: Handle non-prefixed FullName when not found.
            return prevVersion.FullName.StartsWith( "[]db^", StringComparison.Ordinal )
                    ? originalVersions( prevVersion.FullName.Substring( 5 ) )
                    : null;
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
