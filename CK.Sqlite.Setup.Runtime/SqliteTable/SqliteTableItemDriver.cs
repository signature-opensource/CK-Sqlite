using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using CK.Setup;

namespace CK.Sqlite.Setup
{
    /// <summary>
    /// Driver for <see cref="SqliteTableItem"/>.
    /// </summary>
    public class SqliteTableItemDriver : SqlitePackageBaseItemDriver
    {
        /// <summary>
        /// Initializes a new <see cref="SqliteTableItemDriver"/>.
        /// </summary>
        /// <param name="info">Driver build information (required by base SetupItemDriver).</param>
        public SqliteTableItemDriver( BuildInfo info )
            : base( info )
        {
        }

        /// <summary>
        /// Masked to formally associates a <see cref="SqlTableItem"/> type.
        /// </summary>
        public new SqliteTableItem Item => (SqliteTableItem)base.Item;

    }
}
