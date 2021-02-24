
using CKSetup;
using CK.Sqlite;
using System;
using CK.Setup;

namespace CK.Testing.SqliteDBSetup
{
    /// <summary>
    /// SQLite DBSetup core helper exposes the <see cref="SqliteDatabaseIsTemporarySqliteDatabase"/> property and
    /// the <see cref="RunSqliteSetup"/> method.
    /// This helper heavily relies on <see cref="CKSetup.ICKSetupTestHelperCore"/>.
    /// </summary>
    public interface ISqliteDBSetupTestHelperCore
    {
        /// <summary>
        /// Gets the default connection string that will be used from "Sqllite/DefaultConnectionString" configuration
        /// if it exists or defaults to a <see cref="TemporarySqliteDatabase"/>.
        /// </summary>
        string SqliteDefaultConnectionString { get; }

        /// <summary>
        /// Gets whether the <see cref="SqliteDefaultConnectionString"/> is bound to a <see cref="TemporarySqliteDatabase"/>.
        /// </summary>
        bool SqliteDatabaseIsTemporarySqliteDatabase { get; }

        /// <summary>
        /// Runs the database setup in <see cref="IBasicTestHelper.BinFolder"/> on a connection string.
        /// This method calls <see cref="StObjSetup.IStObjSetupTestHelperCore.RunStObjSetup"/>.
        /// </summary>
        /// <param name="connectionString">Connection string. Defaults to <see cref="SqliteDefaultConnectionString"/>.</param>
        /// <param name="traceStObjGraphOrdering">True to trace input and output of StObj graph ordering.</param>
        /// <param name="traceSetupGraphOrdering">True to trace input and output of setup graph ordering.</param>
        /// <param name="revertNames">True to revert names in ordering.</param>
        /// <returns>The setup result: succeed, failed or up-to-date.</returns>
        CKSetupRunResult RunSqliteSetup( string connectionString = null, bool traceStObjGraphOrdering = false, bool traceSetupGraphOrdering = false, bool revertNames = false );

    }
}
