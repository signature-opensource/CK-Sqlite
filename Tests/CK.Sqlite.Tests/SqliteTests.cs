using CKSetup;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using System;
using static CK.Testing.CKDatabaseLocalTestHelper;

namespace CK.Sqlite.Tests
{
    [TestFixture]
    public class SqliteTests
    {
        [Explicit]
        [Test]
        public void delete_netcore_published_folders()
        {
            TestHelper.LogToConsole = true;
            TestHelper.DeleteAllLocalComponentsPublishedFolders();
        }

        [Test]
        public void standard_dbsetup_on_temp_file()
        {
            using (var db = TestHelper.CreateDisposableDatabase())
            {
                var result = db.RunDBSetup();
                result.Should().NotBe(CKSetupRunResult.Failed);

                using (SqliteConnection conn = db.CreateAndOpenConnection())
                {
                    using (var cmd = new SqliteCommand($"INSERT INTO tTests(TestColumn1, TestColumn2) VALUES ('a', 'b')", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqliteCommand($"SELECT Id, TestColumn1, TestColumn2 FROM tTests", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read().Should().BeTrue();
                            reader.GetInt32(0).Should().BeGreaterOrEqualTo(0);
                            reader.GetString(1).Should().Be("a");
                            reader.GetString(2).Should().Be("b");
                        }
                    }
                }
            }
        }

        [Test]
        [Explicit]
        public void standard_dbsetup_on_fixed_file()
        {
            // Used to test error recovery and resume through manual modification of the test Package.
            string filePath = Environment.ExpandEnvironmentVariables(@"%TEMP%\CK.Sqlite.Tests.sqlite");
            string connectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = filePath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();
            TestHelper.CKSetup.DefaultLaunchDebug = true;
            var result = TestHelper.RunDBSetup(connectionString);
            result.Should().NotBe(CKSetupRunResult.Failed);

        }
    }
}
