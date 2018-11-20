using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.SqliteDBSetupTestHelper;

namespace CK.Sqlite.Tests
{
    [TestFixture]
    public class DBSetup
    {
        [Explicit]
        [Test]
        public void delete_netcore_published_folders()
        {
            TestHelper.LogToConsole = true;
            TestHelper.CleanupFolder( TestHelper.SolutionFolder.Combine( $"CK.Sqlite.Setup.Runtime/bin/{TestHelper.BuildConfiguration}/netcoreapp2.1/publish" ) );
        }
    }
}
