using CK.Core;
using CK.Setup;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Sqlite
{
    [StObj( ItemKind = DependentItemKindSpec.Container )]
    [StObjProperty( PropertyName = "ResourceLocation", PropertyType = typeof( IResourceLocator ) )]
    public class SqlitePackageBase : ISqliteConnectionStringProvider
    {
        [AmbientProperty]
        public SqliteDatabase Database { get; set; }

        string ISqliteConnectionStringProvider.ConnectionString => Database?.ConnectionString;
    }
}
