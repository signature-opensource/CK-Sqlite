using CK.Core;
using CK.Setup;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Sqlite
{
    /// <summary>
    /// Base class for actual packages and <see cref="SqlTable"/>.
    /// </summary>
    [StObj( ItemKind = DependentItemKindSpec.Container )]
    [StObjProperty( PropertyName = "ResourceLocation", PropertyType = typeof( IResourceLocator ) )]
    [CKTypeDefiner]
    public class SqlitePackage : ISqliteConnectionStringProvider, IRealObject
    {
        /// <summary>
        /// Gets or sets the database to which this package belongs.
        /// Typically initialized by an attribute (like <see cref="SqlPackageAttribute"/>).
        /// </summary>
        [AmbientProperty]
        public SqliteDatabase Database { get; set; }

        string ISqliteConnectionStringProvider.ConnectionString => Database?.ConnectionString;
    }

}
