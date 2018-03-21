using CK.Core;

namespace CK.Sqlite.Setup
{
    public class SqliteTable : SqlitePackageBase, IAmbientContractDefiner
    {
        public SqliteTable()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="SqliteTable"/> with a <see cref="TableName"/>.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public SqliteTable(string tableName)
        {
            TableName = tableName;
        }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string TableName { get; protected set; }
    }
}
