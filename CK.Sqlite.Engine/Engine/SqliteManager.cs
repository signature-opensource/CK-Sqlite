using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using CK.Core;
using Microsoft.Data.Sqlite;

namespace CK.Sqlite.Setup;

/// <summary>
/// Offers script execution facility and higher level database management for SQLite databases.
/// </summary>
public class SqliteManager : ISqliteManager
{
    readonly IActivityMonitor _monitor;
    SqliteConnection? _oCon;
    bool _ckCoreInstalled;

    /// <summary>
    /// Initializes a new SqlManager.
    /// </summary>
    public SqliteManager( IActivityMonitor monitor )
    {
        Throw.CheckNotNullArgument( monitor );
        _monitor = monitor;
    }

    /// <summary>
    /// Gets the <see cref="SqliteConnection"/> of this manager.
    /// </summary>
    public SqliteConnection? Connection => _oCon;

    void IDisposable.Dispose() => Close();

    /// <summary>
    /// Close the connection. <see cref="Connection"/> becomes null.
    /// Can be called multiple times.
    /// </summary>
    public void Close()
    {
        if( _oCon != null )
        {
            _oCon.StateChange -= new StateChangeEventHandler( OnConnStateChange );
            _oCon.Dispose();
            _oCon = null;
        }
    }

    void DoOpen( string connectionString )
    {
        Debug.Assert( _oCon == null );
        try
        {
            _oCon = new SqliteConnection( connectionString );
            if( _monitor != null )
            {
                _oCon.StateChange += new StateChangeEventHandler( OnConnStateChange );
            }
            _oCon.Open();
        }
        catch
        {
            Close();
            throw;
        }
    }

    void CheckOpen()
    {
        if( _oCon == null ) throw new InvalidOperationException( "SqliteManager is closed." );
    }

    /// <summary>
    /// Opens a database from a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to the database.</param>
    /// <returns>True on success.</returns>
    public bool OpenFromConnectionString( string connectionString )
    {
        using( _monitor.OpenInfo( $"Connection to {connectionString}." ) )
        {
            try
            {
                Close();
                DoOpen( connectionString );
                return true;
            }
            catch( Exception ex )
            {
                _monitor.Error( ex );
                return false;
            }
        }
    }


    /// <summary>
    /// Small helper that opens or crates a database and returns an opened <see cref="SqliteManager"/>.
    /// </summary>
    /// <param name="connectionString">Connection string to use.</param>
    /// <param name="monitor">Monitor that will be associated to the SqliteManager. Can not be null.</param>
    /// <returns>Opened SqliteManager.</returns>
    static public SqliteManager OpenOrCreate( string connectionString, IActivityMonitor monitor )
    {
        SqliteManager m = new SqliteManager( monitor );
        m.OpenFromConnectionString( connectionString );
        return m;
    }

    /// <summary>
    /// Gets the <see cref="IActivityMonitor"/>.
    /// </summary>
    public IActivityMonitor Monitor => _monitor;

    /// <summary>
    /// Ensures that the CKCore kernel is installed.
    /// </summary>
    /// <param name="monitor">The monitor to use. Can not be null.</param>
    /// <returns>True on success.</returns>
    public bool EnsureCKCoreIsInstalled( IActivityMonitor monitor )
    {
        Throw.CheckNotNullArgument( monitor );
        CheckOpen();
        if( !_ckCoreInstalled )
        {
            _ckCoreInstalled = SqliteCKCoreInstaller.Install( this, monitor );
        }
        return _ckCoreInstalled;
    }

    class SqliteExecutor : ISqliteScriptExecutor
    {
        readonly SqliteManager _manager;
        readonly SqliteCommand _command;
        readonly IActivityMonitor? _monitor;
        readonly string _databaseName;

        /// <summary>
        /// Gets or sets the number of <see cref="Execute"/> that failed.
        /// </summary>
        public int FailCount { get; set; }

        /// <summary>
        /// Gets whether the last <see cref="Execute"/> succeed.
        /// </summary>
        public bool LastSucceed { get; private set; }

        internal SqliteExecutor( SqliteManager m, IActivityMonitor monitor, bool autoRestoreDatabase )
        {
            _manager = m;
            _monitor = monitor;
            _command = new SqliteCommand();
            // 8 minutes timeout... should be enough!
            _command.CommandTimeout = 8 * 60;
            _command.Connection = _manager.Connection;
            _databaseName = autoRestoreDatabase ? _command.Connection.Database : null;
        }

        public bool Execute( string script )
        {
            Throw.CheckNotNullArgument( script );
            LastSucceed = false;
            bool hasBeenTraced = false;
            try
            {
                script = script.Trim();
                if( script.Length > 0 )
                {
                    _command.CommandText = script;
                    if( _monitor != null ) hasBeenTraced = _monitor.Trace( script );
                    _command.ExecuteNonQuery();
                }
                LastSucceed = true;
            }
            catch( Exception e )
            {
                FailCount = FailCount + 1;
                if( _monitor == null ) throw;
                // If the monitor is tracing, the text has already been logged.
                if( hasBeenTraced ) _monitor.Error( e );
                else
                {
                    // If the text is not already logged, then we unconditionally log it below the error.
                    using( _monitor.OpenError( e ) )
                    {
                        _monitor.Info( script );
                    }
                }
            }
            return LastSucceed;
        }

        public void Dispose()
        {
            _command.Dispose();
            try
            {
                if( _databaseName != null && _databaseName != _manager.Connection.Database )
                {
                    if( _monitor != null ) _monitor.Info( $"Current database automatically restored from {_manager.Connection.Database} to {_databaseName}." );
                    _command.Connection.ChangeDatabase( _databaseName );
                }
            }
            catch( Exception ex )
            {
                if( _monitor != null ) _monitor.OpenWarn( ex );
                else
                {
                    if( LastSucceed ) throw;
                    // When an error already occurred, we do not rethrow the internal exception.
                }
            }
        }
    }

    /// <summary>
    /// The script is traced (if <paramref name="monitor"/> is not null).
    /// </summary>
    /// <param name="monitor">The monitor to use. Null to not log anything (and throw exception on error).</param>
    /// <param name="autoRestoreDatabase">By default, if the script USE another database, the initial one is automatically restored.</param>
    public ISqliteScriptExecutor CreateExecutor( IActivityMonitor monitor, bool autoRestoreDatabase = true )
    {
        CheckOpen();
        return new SqliteExecutor( this, monitor, autoRestoreDatabase );
    }

    /// <summary>
    /// Simple helper to call <see cref="ExecuteOneScript"/> for multiple scripts (this uses the same <see cref="ISqlScriptExecutor"/>).
    /// </summary>
    /// <param name="scripts">Set of scripts to execute.</param>
    /// <param name="monitor">The monitor to use. Null to not log anything (and throw exception on error).</param>
    /// <returns>
    /// Always true if <paramref name="monitor"/> is null since otherwise an exception
    /// will be thrown in case of failure. 
    /// If a monitor is set, this method will return true or false to indicate success.
    /// </returns>
    public bool ExecuteScripts( IEnumerable<string> scripts, IActivityMonitor? monitor )
    {
        using( var e = CreateExecutor( monitor ) )
        {
            return e.Execute( scripts ) == 0;
        }
    }

    /// <summary>
    /// Executes one script (no GO separator must exist inside). 
    /// The script is traced (if <paramref name="monitor"/> is not null).
    /// </summary>
    /// <param name="monitor">The monitor to use. Null to not log anything (and throw exception on error).</param>
    /// <param name="script">The script to execute.</param>
    /// <returns>
    /// Always true if <paramref name="monitor"/> is null since otherwise an exception
    /// will be thrown in case of failure. 
    /// If a monitor is set, this method will return true or false to indicate success.
    /// </returns>
    /// <remarks>
    /// At the end of the execution, the current database is checked and if it has changed,
    /// the connection is automatically restored onto the original database.
    /// This behavior enables the use of <code>Use OtherDbName</code> commands from inside 
    /// any script and guaranty that, at the beginning of a script, we always are on the 
    /// same configured database.
    /// </remarks>
    public bool ExecuteOneScript( string script, IActivityMonitor? monitor = null )
    {
        using( var e = CreateExecutor( monitor ) )
        {
            return e.Execute( script );
        }
    }

    /// <summary>
    /// Simple execute scalar helper.
    /// The connection must be opened.
    /// </summary>
    /// <param name="select">Select clause.</param>
    /// <returns>The scalar (may be DBNull.Value) or null if no result has been returned.</returns>
    public object ExecuteScalar( string select )
    {
        CheckOpen();
        using( var cmd = new SqliteCommand( select ) { Connection = _oCon } )
        {
            return cmd.ExecuteScalar();
        }
    }

    /// <summary>
    /// Simple execute helper.
    /// The connection must be opened.
    /// </summary>
    /// <param name="cmd">The command text.</param>
    /// <returns>The number of rows.</returns>
    public int ExecuteNonQuery( string cmd, int timeoutSecond = -1 )
    {
        CheckOpen();
        using( var c = new SqliteCommand( cmd ) { Connection = _oCon } )
        {
            if( timeoutSecond >= 0 ) c.CommandTimeout = timeoutSecond;
            return c.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Executes the command and returns the first row as an array of object values.
    /// </summary>
    /// <param name="cmd">The <see cref="SqliteCommand"/> to execute.</param>
    /// <returns>An array of objects or null if nothing has been returned from database.</returns>
    public object[] ReadFirstRow( SqliteCommand cmd )
    {
        CheckOpen();
        cmd.Connection = _oCon;
        using( SqliteDataReader r = cmd.ExecuteReader( CommandBehavior.SingleRow ) )
        {
            if( !r.Read() ) return null;
            object[] res = new object[r.FieldCount];
            r.GetValues( res );
            return res;
        }
    }


    #region Private

    void OnConnStateChange( object sender, StateChangeEventArgs args )
    {
        Debug.Assert( _monitor != null );
        if( args.CurrentState == ConnectionState.Open )
            _monitor.Info( "Connected to database." );
        else _monitor.Info( "Disconnected from database." );
    }

    #endregion
}
