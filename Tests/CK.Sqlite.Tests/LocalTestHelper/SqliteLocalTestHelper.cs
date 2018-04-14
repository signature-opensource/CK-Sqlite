using CK.Core;
using CK.Setup;
using CK.Testing;
using CK.Testing.SqliteLocal;
using CK.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing
{
    public class SqliteLocalTestHelper : ISqliteLocalTestHelperCore
    {
        readonly ISqliteDBSetupTestHelper _dbSetup;

        internal SqliteLocalTestHelper(ITestHelperConfiguration config, ISqliteDBSetupTestHelper ckSetup)
        {
            _dbSetup = ckSetup;
            _dbSetup.CKSetup.InitializeStorePath += OnInitializeStorePath;
        }

        IEnumerable<NormalizedPath> ISqliteLocalTestHelperCore.CKSqliteComponentsPaths => GetSqliteComponentsPaths();
        IEnumerable<NormalizedPath> GetSqliteComponentsPaths()
        {
            yield return _dbSetup.SolutionFolder.Combine($"CK.Sqlite.Setup.Model/bin/{_dbSetup.BuildConfiguration}/netstandard2.0");
            yield return _dbSetup.SolutionFolder.Combine($"CK.Sqlite.Setup.Model/bin/{_dbSetup.BuildConfiguration}/net461");
            yield return _dbSetup.SolutionFolder.Combine($"CK.Sqlite.Setup.Runtime/bin/{_dbSetup.BuildConfiguration}/netcoreapp2.0");
            yield return _dbSetup.SolutionFolder.Combine($"CK.Sqlite.Setup.Runtime/bin/{_dbSetup.BuildConfiguration}/net461");
        }

        IEnumerable<NormalizedPath> ISqliteLocalTestHelperCore.AllLocalComponentsPaths => GetAllLocalComponentsPaths();
        IEnumerable<NormalizedPath> GetAllLocalComponentsPaths()
        {
            foreach (var p in GetSqliteComponentsPaths()) yield return p;
        }

        void OnInitializeStorePath(object sender, CKSetup.StorePathInitializationEventArgs e)
        {
            if (e.StorePath == _dbSetup.SolutionFolder.Combine("Tests/CK.Sqlite.Tests/TestStores/Default"))
            {
                // C:\dev\CK-World\CK-Database-Projects\CK-SQLite\Tests\CK.Sqlite.Tests\TestStores\Default
                using (_dbSetup.Monitor.OpenInfo($"LocalHelper initializing Tests/LocalTestHelper/LocalTestStore."))
                {
                    _dbSetup.CKSetup.RemoveComponentsFromStore(
                                        c => c.Version == CSemVer.SVersion.ZeroVersion,
                                        storePath: e.StorePath);
                    if (!_dbSetup.CKSetup.PublishAndAddComponentFoldersToStore(
                                            GetAllLocalComponentsPaths().Select(p => p.ToString()),
                                            storePath: e.StorePath))
                    {
                        throw new InvalidOperationException("Unable to add CK-Database components to Tests/LocalTestHelper/LocalTestStore.");
                    }
                }
            }
        }

        void ISqliteLocalTestHelperCore.DeleteAllLocalComponentsPublishedFolders()
        {
            using (_dbSetup.Monitor.OpenInfo("Deleting published Setup dependencies"))
            {
                foreach (var p in GetAllLocalComponentsPaths())
                {
                    if (p.LastPart.StartsWith("netcoreapp", StringComparison.OrdinalIgnoreCase))
                    {
                        _dbSetup.CleanupFolder(p.Combine("publish"));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ISqliteLocalTestHelper"/> default implementation.
        /// </summary>
        public static ISqliteLocalTestHelper TestHelper => TestHelperResolver.Default.Resolve<ISqliteLocalTestHelper>();

    }
}
