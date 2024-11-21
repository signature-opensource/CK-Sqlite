using CK.Core;
using CK.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CK.Sqlite;

/// <summary>
/// Base class for actual packages and <see cref="SqlTable"/>.
/// </summary>
[RealObject( ItemKind = DependentItemKindSpec.Container )]
[StObjProperty( PropertyName = "ResourceLocation", PropertyType = typeof( IResourceLocator ) )]
[CKTypeDefiner]
public class SqlitePackage : ISqliteConnectionStringProvider, IRealObject
{
    /// <summary>
    /// Gets or sets the database to which this package belongs.
    /// Typically initialized by an attribute (like <see cref="SqlitePackageAttribute"/>).
    /// </summary>
    [AmbientProperty, AllowNull]
    public SqliteDatabase Database { get; set; }

    string ISqliteConnectionStringProvider.ConnectionString => Database?.ConnectionString;
}
