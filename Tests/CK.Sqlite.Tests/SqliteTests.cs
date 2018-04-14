using CKSetup;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using System;
using static CK.Testing.SqliteLocalTestHelper;

namespace CK.Sqlite.Tests
{
    [TestFixture]
    public class SqliteTests
    {


        [Test]
        public void standard_dbsetup_on_temp_file()
        {
            using( SqliteConnection conn = new SqliteConnection( TestHelper.SqliteDefaultConnectionString ) )
            {
                TestHelper.RunSqliteSetup().Should().Be( CKSetupRunResult.Succeed );

                conn.Open();
                TestThatLocalPackagesHaveBeenInstalled( conn );
            }
        }

        [Test]
        public void standard_dbsetup_on_fixed_file()
        {
            using( var db = new TemporarySqliteDatabase() )
            {
                TestHelper.RunSqliteSetup( db.ConnectionString )
                    .Should().Be( CKSetupRunResult.Succeed );

                using( SqliteConnection conn = new SqliteConnection( db.ConnectionString ) )
                {
                    conn.Open();
                    TestThatLocalPackagesHaveBeenInstalled( conn );
                }
            }
        }

        static void TestThatLocalPackagesHaveBeenInstalled( SqliteConnection conn )
        {
            using( var cmd = new SqliteCommand( $"INSERT INTO tTests(TestColumn1, TestColumn2) VALUES ('a', 'b')", conn ) )
            {
                cmd.ExecuteNonQuery();
            }
            using( var cmd = new SqliteCommand( $"SELECT Id, TestColumn1, TestColumn2 FROM tTests", conn ) )
            {
                using( var reader = cmd.ExecuteReader() )
                {
                    reader.Read().Should().BeTrue();
                    reader.GetInt32( 0 ).Should().BeGreaterOrEqualTo( 0 );
                    reader.GetString( 1 ).Should().Be( "a" );
                    reader.GetString( 2 ).Should().Be( "b" );
                }
            }
        }


    }
}
