using System;
using System.Collections.Generic;
using CK.Core;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace CK.Sqlite.Setup
{

    class SqliteManagerProvider : ISqliteManagerProvider, IDisposable
    {
        readonly IActivityMonitor _monitor;
        readonly Dictionary<string, Item> _items;
        readonly Action<ISqliteManagerBase> _dbConfigurator;

        class Item
        {
            public string ConnectionString;
            public ISqliteManager Manager;

            public override string ToString() => $"{Manager != null} - {ConnectionString}";
        }

        public SqliteManagerProvider( IActivityMonitor monitor, Action<ISqliteManagerBase> dbConfigurator = null )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            _monitor = monitor;
            _items = new Dictionary<string, Item>();
            _dbConfigurator = dbConfigurator ?? Util.ActionVoid;
        }

        public void Add( string name, string connectionString )
        {
            Item i = new Item() { ConnectionString = connectionString };
            _items.Add( name, i );
            _items[connectionString] = i;
        }

        public ISqliteManagerBase FindManagerByName( string name )
        {
            if( !string.IsNullOrWhiteSpace( name ) )
            {
                Item i;
                if( _items.TryGetValue( name, out i ) )
                {
                    if( i.Manager == null ) CreateManager( i );
                    return i.Manager;
                }
            }
            return null;
        }

        void CreateManager( Item i )
        {
            SqliteManager m = new SqliteManager( _monitor );
            if( !m.OpenFromConnectionString( i.ConnectionString ) )
            {
                throw new CKException( "Unable to open database for '{0}'.", i.ConnectionString );
            }
            _dbConfigurator( m );
            i.Manager = m;
        }

        public ISqliteManager FindManagerByConnectionString( string connectionString )
        {
            if( !String.IsNullOrWhiteSpace( connectionString ) )
            {
                Item i;
                if( _items.TryGetValue( connectionString, out i ) )
                {
                    if( i.Manager == null ) CreateManager( i );
                    return i.Manager;
                }
            }
            return null;
        }

        public void Dispose()
        {
            if( _items.Count > 0 )
            {
                foreach( var item in _items )
                {
                    if( item.Key != null && item.Value.Manager != null ) item.Value.Manager.Dispose();
                }
                _items.Clear();
            }
        }

        ISqliteManagerBase ISqliteManagerProvider.FindManagerByName( string logicalName ) => FindManagerByName( logicalName );

        ISqliteManagerBase ISqliteManagerProvider.FindManagerByConnectionString( string connectionString ) => FindManagerByConnectionString( connectionString );

    }
}
