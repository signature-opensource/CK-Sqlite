using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using CK.Core;
using CK.Setup;
using CK.Testing.CKSetup;
using CK.Testing.SqliteDBSetup;
using CK.Text;
using CKSetup;

namespace CK.Testing
{
    public class SqliteDBSetupTestHelper : ISqliteDBSetupTestHelperCore
    {
        public static readonly string DefaultConnectionString = "Data Source=CKSetup; Mode=Memory; Cache=Shared";

        readonly ISetupableSetupTestHelper _setupableSetup;
        readonly string _defaultConnectionString;

        internal SqliteDBSetupTestHelper( ITestHelperConfiguration config, ISetupableSetupTestHelper setupableSetup )
        {
            _setupableSetup = setupableSetup;
            _setupableSetup.StObjSetupRunning += OnStObjSetupRunning;
            _defaultConnectionString = config.Get( "Sqllite/DefaultConnectionString", DefaultConnectionString );
        }

        void OnStObjSetupRunning( object sender, StObjSetup.StObjSetupRunningEventArgs e )
        {
            if( !e.StObjEngineConfiguration.Aspects.Any( c => c is SqliteSetupAspectConfiguration ) )
            {
                var conf = new SqliteSetupAspectConfiguration();
                conf.DefaultDatabaseConnectionString = _defaultConnectionString;

                e.ForceSetup |= _defaultConnectionString == DefaultConnectionString;
                e.StObjEngineConfiguration.Aspects.Add( conf );
            }
        }

        string ISqliteDBSetupTestHelperCore.SqliteDefaultConnectionString => _defaultConnectionString;

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

                    stObjConf.ForceSetup |= connectionString == _defaultConnectionString;

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
