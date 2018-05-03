using CK.Core;

namespace CK.Sqlite
{
    public class SqliteTable : SqlitePackageBase, IAmbientContractDefiner
    {
        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string TableName { get; protected set; }
    }
}
