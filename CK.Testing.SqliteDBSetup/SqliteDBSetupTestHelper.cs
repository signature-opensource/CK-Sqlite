using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using CK.Core;
using CK.Testing.CKSetup;
using CK.Testing.SqliteDBSetup;
using CK.Text;
using CKSetup;
using Microsoft.Data.Sqlite;

namespace CK.Testing
{
    public class SqliteDBSetupTestHelper : ISqliteDBSetupTestHelperCore
    {
        public static readonly string DefaultConnectionString = "Data Source=CKSetup; Mode=Memory; Cache=Shared";

        readonly ICKSetupTestHelper _ckSetup;
        readonly IStObjMapTestHelper _stObjMap;

        bool _generateSourceFiles;

        internal SqliteDBSetupTestHelper(ITestHelperConfiguration config, ICKSetupTestHelper ckSetup, IStObjMapTestHelper stObjMap)
        {
            _ckSetup = ckSetup;
            _stObjMap = stObjMap;
            stObjMap.StObjMapLoading += OnStObjMapLoading;
            _generateSourceFiles = config.GetBoolean("SqliteDBSetup/GenerateSourceFiles") ?? true;
        }

        void OnStObjMapLoading(object sender, EventArgs e)
        {
            var file = _stObjMap.BinFolder.AppendPart(_stObjMap.GeneratedAssemblyName + ".dll");
            if (!System.IO.File.Exists(file))
            {
                _stObjMap.Monitor.Info($"File '{file}' does not exist. Running DBSetup to create it.");
                DoRunDBSetup(DefaultConnectionString, false, false, false);
            }
        }

        bool ISqliteDBSetupTestHelperCore.GenerateSourceFiles { get => _generateSourceFiles; set => _generateSourceFiles = value; }

        CKSetupRunResult ISqliteDBSetupTestHelperCore.RunDBSetup(string connectionString, bool traceStObjGraphOrdering, bool traceSetupGraphOrdering, bool revertNames)
        {
            return DoRunDBSetup(connectionString, traceStObjGraphOrdering, traceSetupGraphOrdering, revertNames);
        }

        CKSetupRunResult DoRunDBSetup(string connectionString, bool traceStObjGraphOrdering, bool traceSetupGraphOrdering, bool revertNames)
        {
            using (_ckSetup.Monitor.OpenInfo($"Running DBSetup on {connectionString}."))
            {
                try
                {
                    bool forceSetup = _ckSetup.CKSetup.DefaultForceSetup
                                        || _ckSetup.CKSetup.FinalDefaultBinPaths
                                             .Select(p => p.AppendPart(_stObjMap.GeneratedAssemblyName + ".dll"))
                                             .Any(p => !File.Exists(p));

                    var conf = new SetupConfiguration();
                    conf.EngineAssemblyQualifiedName = "CK.Setup.StObjEngine, CK.StObj.Engine";
                    conf.Configuration = XElement.Parse($@"
                        <StObjEngineConfiguration>
                            <TraceDependencySorterInput>{traceStObjGraphOrdering}</TraceDependencySorterInput>
                            <TraceDependencySorterOutput>{traceStObjGraphOrdering}</TraceDependencySorterOutput>
                            <RevertOrderingNames>{revertNames}</RevertOrderingNames>
                            <GenerateSourceFiles>{_generateSourceFiles}</GenerateSourceFiles>
                            <GeneratedAssemblyName>{_stObjMap.GeneratedAssemblyName}</GeneratedAssemblyName>
                            <Aspect Type=""CK.Setup.SetupableAspectConfiguration, CK.Setupable.Model"" >
                                <TraceDependencySorterInput>{traceSetupGraphOrdering}</TraceDependencySorterInput>
                                <TraceDependencySorterOutput>{traceSetupGraphOrdering}</TraceDependencySorterOutput>
                                <RevertOrderingNames>{revertNames}</RevertOrderingNames>
                            </Aspect>
                            <Aspect Type=""CK.Setup.SqliteSetupAspectConfiguration, CK.Sqlite.Setup.Model"" >
                                <DefaultDatabaseConnectionString>{connectionString}</DefaultDatabaseConnectionString>
                            </Aspect>
                        </StObjEngineConfiguration>");
                    var result = _ckSetup.CKSetup.Run(conf, forceSetup: forceSetup);
                    if (result != CKSetupRunResult.Failed)
                    {
                        string genDllName = _stObjMap.GeneratedAssemblyName + ".dll";
                        var firstGen = new NormalizedPath(conf.BinPaths[0]).AppendPart(genDllName);
                        if (firstGen != _stObjMap.BinFolder.AppendPart(genDllName) && File.Exists(firstGen))
                        {
                            _stObjMap.Monitor.Info($"Copying generated '{genDllName}' from first BinPath ({conf.BinPaths[0]}) to bin folder.");
                            File.Copy(firstGen, Path.Combine(AppContext.BaseDirectory, genDllName), true);
                        }
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    _ckSetup.Monitor.Error(ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ISqliteDBSetupTestHelper"/> default implementation.
        /// </summary>
        public static ISqliteDBSetupTestHelper TestHelper => TestHelperResolver.Default.Resolve<ISqliteDBSetupTestHelper>();

        public DisposableSqliteDatabase CreateDisposableDatabase()
        {
            string fileName = Path.GetTempFileName();
            return new DisposableSqliteDatabase(fileName);
        }
    }

    public class DisposableSqliteDatabase : IDisposable
    {
        private readonly SqliteConnectionStringBuilder _csb;

        public DisposableSqliteDatabase(string dataSource)
        {
            _csb = new SqliteConnectionStringBuilder
            {
                DataSource = dataSource,
                Cache = SqliteCacheMode.Default,
                Mode = SqliteOpenMode.ReadWriteCreate
            };
        }

        public SqliteCacheMode CacheMode
        {
            get => _csb.Cache;
            set { _csb.Cache = value; }
        }
        public SqliteOpenMode OpenMode
        {
            get => _csb.Mode;
            set { _csb.Mode = value; }
        }

        public string DataSource => _csb.DataSource;

        public string GetConnectionString() => _csb.ToString();

        public CKSetupRunResult RunDBSetup(bool traceStObjGraphOrdering = false, bool traceSetupGraphOrdering = false, bool revertNames = false)
        {
            return SqliteDBSetupTestHelper.TestHelper.RunDBSetup(GetConnectionString(), traceStObjGraphOrdering, traceSetupGraphOrdering, revertNames);
        }

        public SqliteConnection CreateAndOpenConnection()
        {
            SqliteConnection c = new SqliteConnection(GetConnectionString());
            c.Open();
            return c;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DisposableSqliteDatabase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
