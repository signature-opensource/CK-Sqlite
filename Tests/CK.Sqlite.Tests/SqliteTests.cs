using CK.Setup;
using CK.Testing;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using System;
using static CK.Testing.MonitorTestHelper;

namespace CK.Sqlite.Tests;

[TestFixture]
public class SqliteTests
{
    [Test]
    public void setup_on_default_AppContext_BaseDirectory_database()
    {
        var engineConfiguration = TestHelper.CreateDefaultEngineConfiguration();
        var lite = engineConfiguration.EnsureSqliteConfigurationAspect( connectionString: null );
        lite.IsDefaultDatabaseConnectionStringConfigured.Should().BeFalse( "Using default AppContext.BaseDirectory 'sqlite' database." );

        using( SqliteConnection conn = new SqliteConnection( lite.DefaultDatabaseConnectionString ) )
        {
            engineConfiguration.RunSuccessfully();
            conn.Open();
            TestThatLocalPackagesHaveBeenInstalled( conn );
        }
    }

    [Test]
    public void setup_on_explicit_database_file()
    {
        using( var db = new TemporarySqliteDatabase() )
        {
            var engineConfiguration = TestHelper.CreateDefaultEngineConfiguration();
            engineConfiguration.EnsureSqliteConfigurationAspect( db.ConnectionString );
            engineConfiguration.RunSuccessfully();

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
