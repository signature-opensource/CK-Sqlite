using CK.Setup;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Sqlite.Setup
{
    [Setup(
        ItemKind = DependentItemKindSpec.Group,
        TrackAmbientProperties = TrackAmbientPropertiesMode.AddPropertyHolderAsChildren,
        ItemTypeName = "CK.Sqlite.Setup.SqliteDatabaseItem,CK.Sqlite.Setup.Runtime"
        )]
    public class SqliteDatabase : ISqliteConnectionStringProvider
    {
        /// <summary>
        /// Default database name is "sqlite": this is the name of the <see cref="SqliteDefaultDatabase"/> type.
        /// </summary>
        public const string DefaultDatabaseName = SqliteSetupAspectConfiguration.DefaultDatabaseName;
        bool _installCore;

        public SqliteDatabase( string name )
        {
            if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "Must be not null, empty, nor whitespace.", nameof( name ) );
            Name = name;
        }

        /// <summary>
        /// Gets or sets the connection string.
        /// This can be automatically configured during setup (if the specialized class implements a StObjConstruct method with a connectionString parameter
        /// and sets this property).
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets the name of this database.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets whether CKCore kernel support must be installed in the database.
        /// Defaults to false.
        /// Always true if <see cref="IsDefaultDatabase"/> is true.
        /// </summary>
        public bool InstallCore
        {
            get { return _installCore | IsDefaultDatabase; }
            set { _installCore = value; }
        }

        /// <summary>
        /// Default database name is <see cref="DefaultDatabaseName"/> = "sqlite".
        /// </summary>
        public bool IsDefaultDatabase => Name == DefaultDatabaseName;
    }
}
