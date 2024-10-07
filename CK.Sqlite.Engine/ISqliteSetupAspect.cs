using CK.Setup;

namespace CK.Sqlite.Setup;

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
    /// It is initialized with <see cref="SqliteAspectConfiguration.Databases"/> content but can be changed.
    /// </summary>
    ISqliteManagerProvider SqliteDatabases { get; }

}
