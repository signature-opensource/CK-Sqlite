using System;
using System.Collections.Generic;
using CK.Core;

namespace CK.Sqlite.Setup;


/// <summary>
/// Offers script execution facility and higher level database management (such as automatically 
/// creating a database) for SQLite databases.
/// This abstraction hides the actual database and enables fake implementations.
/// </summary>
public interface ISqliteManagerBase : IDisposable
{
    /// <summary>
    /// Gets the <see cref="IActivityMonitor"/>. 
    /// Must never be null.
    /// </summary>
    IActivityMonitor Monitor { get; }

    /// <summary>
    /// Opens a database from a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <returns>True on success, false otherwise.</returns>
    bool OpenFromConnectionString( string connectionString );

    /// <summary>
    /// Ensures that the CKCore kernel is installed.
    /// </summary>
    /// <param name="monitor">The monitor to use. Can not be null.</param>
    /// <returns>True on success.</returns>
    bool EnsureCKCoreIsInstalled( IActivityMonitor monitor );

    /// <summary>
    /// The script is traced (if <paramref name="monitor"/> is not null).
    /// </summary>
    /// <param name="monitor">The monitor to use. Null to not log anything (and throw exception on error).</param>
    /// <param name="autoRestoreDatabase">By default, if the script USE another database, the initial one is automatically restored.</param>
    ISqliteScriptExecutor CreateExecutor( IActivityMonitor monitor, bool autoRestoreDatabase = true );

    /// <summary>
    /// Executes one script. 
    /// The script is traced (if <paramref name="monitor"/> is not null).
    /// </summary>
    /// <param name="monitor">The monitor to use. Null to not log anything (and throw exception on error).</param>
    /// <param name="script">The script to execute.</param>
    /// <returns>
    /// Always true if <paramref name="monitor"/> is null since otherwise an exception
    /// will be thrown in case of failure. 
    /// If a monitor is provided, this method will return true or false to indicate success.
    /// </returns>
    /// <remarks>
    /// At the end of the execution, the current database is checked and if it has changed,
    /// the connection is automatically restored onto the original database.
    /// This behavior enables the use of <code>Use OtherDbName</code> commands from inside 
    /// any script and guaranty that, at the beginning of a script, we always are on the 
    /// same configured database.
    /// </remarks>
    bool ExecuteOneScript( string script, IActivityMonitor monitor = null );

    /// <summary>
    /// Simple helper to call <see cref="ExecuteOneScript"/> for multiple scripts (this uses the same <see cref="ISqliteScriptExecutor"/>).
    /// </summary>
    /// <param name="scripts">Set of scripts to execute.</param>
    /// <param name="monitor">The monitor to use. Null to not log anything (and throw exception on error).</param>
    /// <returns>
    /// Always true if <paramref name="monitor"/> is null since otherwise an exception
    /// will be thrown in case of failure. 
    /// If a monitor is provided, this method will return true or false to indicate success.
    /// </returns>
    bool ExecuteScripts( IEnumerable<string> scripts, IActivityMonitor monitor );

}
