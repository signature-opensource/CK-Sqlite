using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using CK.Core;
using CK.Setup;
using CK.Sqlite;
using CK.Testing.CKSetup;
using CK.Testing.SqliteDBSetup;
using CKSetup;

namespace CK.Testing
{
    /// <summary>
    /// Implements <see cref="ISqliteDBSetupTestHelperCore"/> and exposes its <see cref="TestHelper"/>.
    /// </summary>
    public class SqliteDBSetupTestHelper : ISqliteDBSetupTestHelperCore
    {
        readonly ISetupableSetupTestHelper _setupableSetup;
        readonly string _defaultConnectionString;
        readonly TemporarySqliteDatabase _tempDB;

        internal SqliteDBSetupTestHelper( TestHelperConfiguration config, ISetupableSetupTestHelper setupableSetup )
        {
            _setupableSetup = setupableSetup;
            _setupableSetup.StObjSetupRunning += OnStObjSetupRunning;
            var c = config.Declare( "Sqllite/DefaultConnectionString", "Connection string to use. By default a temporary database is created. ", null );
            if( c.ConfiguredValue == null )
            {
                _tempDB = new TemporarySqliteDatabase();
                c.SetDefaultValue( _tempDB.ConnectionString );
                _defaultConnectionString = _tempDB.ConnectionString;
            }
            else
            {
                _defaultConnectionString = c.ConfiguredValue;
            }
        }

        void OnStObjSetupRunning( object sender, StObjSetup.StObjSetupRunningEventArgs e )
        {
            if( !e.EngineConfiguration.Aspects.Any( c => c is SqliteSetupAspectConfiguration ) )
            {
                _setupableSetup.Monitor.Info( $"Adding SqliteSetupAspectConfiguration to StObjEngineConfiguration on connection string {_defaultConnectionString}." );
                var conf = new SqliteSetupAspectConfiguration();
                conf.DefaultDatabaseConnectionString = _defaultConnectionString;

                e.EngineConfiguration.AddAspect( conf );
            }
        }

        string ISqliteDBSetupTestHelperCore.SqliteDefaultConnectionString => _defaultConnectionString;

        bool ISqliteDBSetupTestHelperCore.SqliteDatabaseIsTemporarySqliteDatabase => _tempDB != null;

        CKSetupRunResult ISqliteDBSetupTestHelperCore.RunSqliteSetup( string connectionString, bool traceStObjGraphOrdering, bool traceSetupGraphOrdering, bool revertNames )
        {
            return DoRunSqliteDBSetup( connectionString, traceStObjGraphOrdering, traceSetupGraphOrdering, revertNames );
        }

        CKSetupRunResult DoRunSqliteDBSetup( string connectionString, bool traceStObjGraphOrdering, bool traceSetupGraphOrdering, bool revertNames )
        {
            if( connectionString == null ) connectionString = _defaultConnectionString;
            using( _setupableSetup.Monitor.OpenInfo( $"Running SqliteSetup on {connectionString}." ) )
            {
                try
                {
                    var (Configuration, ForceSetup) = StObjSetupTestHelper.CreateDefaultConfiguration( _setupableSetup.Monitor, _setupableSetup );
                    Debug.Assert( Configuration.BinPaths.Count > 0 && Configuration.BinPaths[0].CompileOption == CompileOption.Compile );

                    Configuration.RevertOrderingNames = revertNames;
                    Configuration.TraceDependencySorterInput = traceStObjGraphOrdering;
                    Configuration.TraceDependencySorterOutput = traceStObjGraphOrdering;

                    var setupable = new SetupableAspectConfiguration();
                    setupable.RevertOrderingNames = revertNames;
                    setupable.TraceDependencySorterInput = traceSetupGraphOrdering;
                    setupable.TraceDependencySorterOutput = traceSetupGraphOrdering;
                    Configuration.AddAspect( setupable );

                    var sqlite = new SqliteSetupAspectConfiguration();
                    sqlite.DefaultDatabaseConnectionString = connectionString;
                    Configuration.AddAspect( sqlite );

                    return _setupableSetup.RunStObjSetup( Configuration, ForceSetup );
                }
                catch( Exception ex )
                {
                    _setupableSetup.Monitor.Error( ex );
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ISqliteDBSetupTestHelper"/> default implementation.
        /// </summary>
        public static ISqliteDBSetupTestHelper TestHelper => TestHelperResolver.Default.Resolve<ISqliteDBSetupTestHelper>();
    }

 }
