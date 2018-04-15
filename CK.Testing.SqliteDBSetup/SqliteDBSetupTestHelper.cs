using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using CK.Core;
using CK.Setup;
using CK.Sqlite;
using CK.Testing.CKSetup;
using CK.Testing.SqliteDBSetup;
using CK.Text;
using CKSetup;

namespace CK.Testing
{
    public class SqliteDBSetupTestHelper : ISqliteDBSetupTestHelperCore
    {
        readonly ISetupableSetupTestHelper _setupableSetup;
        readonly string _defaultConnectionString;
        readonly TemporarySqliteDatabase _tempDB;

        internal SqliteDBSetupTestHelper( ITestHelperConfiguration config, ISetupableSetupTestHelper setupableSetup )
        {
            _setupableSetup = setupableSetup;
            _setupableSetup.StObjSetupRunning += OnStObjSetupRunning;
            var c = config.Get( "Sqllite/DefaultConnectionString", null );
            if( c == null )
            {
                _tempDB = new TemporarySqliteDatabase();
                c = _tempDB.ConnectionString;
            }
            _defaultConnectionString = c;
        }

        void OnStObjSetupRunning( object sender, StObjSetup.StObjSetupRunningEventArgs e )
        {
            if( !e.StObjEngineConfiguration.Aspects.Any( c => c is SqliteSetupAspectConfiguration ) )
            {
                _setupableSetup.Monitor.Info( $"Adding SqliteSetupAspectConfiguration to StObjEngineConfiguration on connection string {_defaultConnectionString}." );
                var conf = new SqliteSetupAspectConfiguration();
                conf.DefaultDatabaseConnectionString = _defaultConnectionString;

                e.StObjEngineConfiguration.Aspects.Add( conf );
            }
        }

        string ISqliteDBSetupTestHelperCore.SqliteDefaultConnectionString => _defaultConnectionString;

        bool ISqliteDBSetupTestHelperCore.SqliteDatabaseIsTemporarySqliteDatabase => _tempDB != null;

        CKSetupRunResult ISqliteDBSetupTestHelperCore.RunSqliteSetup(string connectionString, bool traceStObjGraphOrdering, bool traceSetupGraphOrdering, bool revertNames)
        {
            return DoRunSqliteDBSetup(connectionString, traceStObjGraphOrdering, traceSetupGraphOrdering, revertNames);
        }

        CKSetupRunResult DoRunSqliteDBSetup(string connectionString, bool traceStObjGraphOrdering, bool traceSetupGraphOrdering, bool revertNames)
        {
            if( connectionString == null ) connectionString = _defaultConnectionString;
            using (_setupableSetup.Monitor.OpenInfo($"Running SqliteSetup on {connectionString}."))
            {
                try
                {
                    var stObjConf = StObjSetupTestHelper.CreateDefaultConfiguration( _setupableSetup );

                    var setupable = new SetupableAspectConfiguration();
                    setupable.RevertOrderingNames = revertNames;
                    setupable.TraceDependencySorterInput = traceSetupGraphOrdering;
                    setupable.TraceDependencySorterInput = traceSetupGraphOrdering;
                    stObjConf.Configuration.Aspects.Add( setupable );

                    var sqlite = new SqliteSetupAspectConfiguration();
                    sqlite.DefaultDatabaseConnectionString = connectionString;
                    stObjConf.Configuration.Aspects.Add( sqlite );

                    return _setupableSetup.RunStObjSetup( stObjConf.Configuration, stObjConf.ForceSetup );
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
