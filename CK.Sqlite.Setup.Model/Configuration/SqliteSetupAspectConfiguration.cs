using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CK.Setup
{
    /// <summary>
    /// Aspect configuration object.
    /// </summary>
    public class SqliteSetupAspectConfiguration : IStObjEngineAspectConfiguration
    {
        /// <summary>
        /// Default database name is "sqlite".
        /// </summary>
        public const string DefaultDatabaseName = "sqlite";

        readonly List<SqliteDatabaseDescriptor> _databases;

        /// <summary>
        /// Initializes a new <see cref="SqlSetupAspectConfiguration"/>.
        /// </summary>
        public SqliteSetupAspectConfiguration()
        {
            _databases = new List<SqliteDatabaseDescriptor>();
        }

        static readonly XName xDatabases = XNamespace.None + "Databases";
        static readonly XName xDatabase = XNamespace.None + "Database";
        static readonly XName xDefaultDatabaseConnectionString = XNamespace.None + "DefaultDatabaseConnectionString";

        /// <summary>
        /// Initializes a new <see cref="SqlSetupAspectConfiguration"/> from its xml representation.
        /// </summary>
        /// <param name="e">The element.</param>
        public SqliteSetupAspectConfiguration( XElement e )
        {
            _databases = e.Elements( xDatabases ).Elements( xDatabase ).Select( d => new SqliteDatabaseDescriptor( d ) ).ToList();
            DefaultDatabaseConnectionString = e.Element( xDefaultDatabaseConnectionString )?.Value;
        }

        /// <summary>
        /// Serializes its content in the provided <see cref="XElement"/> and returns it.
        /// The <see cref="SqlSetupAspectConfiguration(XElement)"/> constructor will be able to read this element back.
        /// </summary>
        /// <param name="e">The element to populate.</param>
        /// <returns>The <paramref name="e"/> element.</returns>
        public XElement SerializeXml( XElement e )
        {
            e.Add( new XElement( xDatabases, _databases.Select( d => d.Serialize( new XElement( xDatabase ) ) ) ),
                   new XElement( xDefaultDatabaseConnectionString, DefaultDatabaseConnectionString ) );
            return e;
        }

        /// <summary>
        /// Gets or sets the default database connection string.
        /// </summary>
        public string DefaultDatabaseConnectionString { get; set; }

        /// <summary>
        /// Gets the list of available <see cref="SqlDatabaseDescriptor"/>.
        /// </summary>
        public List<SqliteDatabaseDescriptor> Databases => _databases;

        /// <summary>
        /// Finds a configured connection string by its name.
        /// It may be the <see cref="DefaultDatabaseConnectionString"/> (default database name is 'db') or one of the registered <see cref="Databases"/>.
        /// </summary>
        /// <param name="name">Logical name of the connection string to find.</param>
        /// <returns>Configured connection string or null if not found.</returns>
        public string FindConnectionStringByName( string name )
        {
            if( name == DefaultDatabaseName ) return DefaultDatabaseConnectionString;
            foreach( var desc in Databases ) if( desc.LogicalDatabaseName == name ) return desc.ConnectionString;
            return null;
        }

        string IStObjEngineAspectConfiguration.AspectType => "CK.Sqlite.Setup.SqliteSetupAspect, CK.Sqlite.Setup.Runtime";


    }
}
