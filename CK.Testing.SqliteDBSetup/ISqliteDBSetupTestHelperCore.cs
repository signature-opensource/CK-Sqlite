
using CKSetup;
using System;

namespace CK.Testing.SqliteDBSetup
{
    /// <summary>
    /// SQLite DBSetup core helper exposes only <see cref="GenerateSourceFiles"/> property and
    /// the <see cref="RunDBSetup"/> method.
    /// This helper heavily relies on <see cref="CKSetup.ICKSetupTestHelperCore"/>.
    /// </summary>
    public interface ISqliteDBSetupTestHelperCore
    {
        /// <summary>
        /// Gets or sets whether source files must be generated alongside the generated assembly.
        /// Defaults to "SqliteDBSetup/GenerateSourceFiles" configuration or true if the configuration does not exist.
        /// </summary>
        bool GenerateSourceFiles { get; set; }

        /// <summary>
        /// Runs the database setup in <see cref="IBasicTestHelper.BinFolder"/> on the default database.
        /// Automatically called by <see cref="StObjMap.IStObjMapTestHelperCore.StObjMap"/>
        /// when the StObjMap is not yet initialized.
        /// This method uses CKSetup.Core (thanks to <see cref="ICKSetupTestHelper"/>).
        /// </summary>
        /// <param name="traceStObjGraphOrdering">True to trace input and output of StObj graph ordering.</param>
        /// <param name="traceSetupGraphOrdering">True to trace input and output of setup graph ordering.</param>
        /// <param name="revertNames">True to revert names in ordering.</param>
        /// <returns>The setup result: succeed, failed or up-to-date.</returns>
        CKSetupRunResult RunDBSetup( string connectionString, bool traceStObjGraphOrdering = false, bool traceSetupGraphOrdering = false, bool revertNames = false);

        DisposableSqliteDatabase CreateDisposableDatabase();
    }
}
