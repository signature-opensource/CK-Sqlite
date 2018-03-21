using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Sqlite.Setup
{
    /// <summary>
    /// Attribute that must decorate a <see cref="SqlTable"/> class.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false )]
    public class SqliteTableAttribute : SqlitePackageAttributeBase
    {
        /// <summary>
        /// Initializes a new <see cref="SqliteTableAttribute"/>.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        public SqliteTableAttribute( string tableName )
            : base( "CK.Sqlite.Setup.SqliteTableAttributeImpl, CK.Sqlite.Setup.Runtime" )
        {
            TableName = tableName;
        }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string TableName { get; set; }

    }
}
