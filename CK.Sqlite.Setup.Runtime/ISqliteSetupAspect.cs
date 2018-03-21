using CK.Setup;

namespace CK.Sqlite.Setup
{
    /// <summary>
    /// Sql setup aspect. Provides <see cref="SqliteDatabases"/>.
    /// </summary>
    public interface ISqliteSetupAspect
    {
        /// <summary>
        /// Gets the default database as a <see cref="ISqliteManagerBase"/> object.
        /// </summary>
        ISqliteManagerBase DefaultSqliteDatabase { get; }

        /// <summary>
        /// Gets the available databases (including the <see cref="DefaultSqliteDatabase"/>).
        /// It is initialized with <see cref="SqliteSetupAspectConfiguration.Databases"/> content but can be changed.
        /// </summary>
        ISqliteManagerProvider SqliteDatabases { get; }

        /// <summary>
        /// Gets whether the resolution of objects must be done globally.
        /// This is a temporary property: this should eventually be the only mode...
        /// </summary>
        bool GlobalResolution { get; }

    }
}
