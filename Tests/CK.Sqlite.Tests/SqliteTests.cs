using CK.Core;
using CK.Setup;
using CK.Testing;
using Shouldly;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.Sqlite.Tests;

[TestFixture]
public class SqliteTests
{
    [Test]
    public async Task setup_on_default_AppContext_BaseDirectory_database_Async()
    {
        var engineConfiguration = TestHelper.CreateDefaultEngineConfiguration();
        var lite = engineConfiguration.EnsureSqliteConfigurationAspect( connectionString: null );
        lite.IsDefaultDatabaseConnectionStringConfigured.ShouldBeFalse( "Using default AppContext.BaseDirectory 'sqlite' database." );

        using( SqliteConnection conn = new SqliteConnection( lite.DefaultDatabaseConnectionString ) )
        {
            await engineConfiguration.RunSuccessfullyAsync();
            await conn.OpenAsync();
            TestThatLocalPackagesHaveBeenInstalled( conn );
        }
    }

    [Test]
    public async Task setup_on_explicit_database_file_Async()
    {
        using( var db = new TemporarySqliteDatabase() )
        {
            var engineConfiguration = TestHelper.CreateDefaultEngineConfiguration();
            engineConfiguration.EnsureSqliteConfigurationAspect( db.ConnectionString );
            await engineConfiguration.RunSuccessfullyAsync();

            using( SqliteConnection conn = new SqliteConnection( db.ConnectionString ) )
            {
                await conn.OpenAsync();
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
                reader.Read().ShouldBeTrue();
                reader.GetInt32( 0 ).ShouldBeGreaterThanOrEqualTo( 0 );
                reader.GetString( 1 ).ShouldBe( "a" );
                reader.GetString( 2 ).ShouldBe( "b" );
            }
        }
    }

    [Test]
    public async Task a_failed_connection_does_not_log_the_password_of_its_connection_string_Async()
    {
        const string password = "S3cret-Marker-Of-The-Test";
        var engineConfiguration = TestHelper.CreateDefaultEngineConfiguration();
        // A file in a folder that does not exist: the database cannot be opened.
        engineConfiguration.EnsureSqliteConfigurationAspect( $"Data Source={TestHelper.TestProjectFolder}/NoSuchFolder/NoSuch.db;Password={password}" );

        using( TestHelper.Monitor.CollectEntries( out var entries, LogLevelFilter.Debug, 100_000 ) )
        {
            (await engineConfiguration.RunAsync()).Status.ShouldBe( RunStatus.Failed );
            // Counts are compared: a failed assertion must not display the entries.
            var connections = entries.Where( e => e.Text.StartsWith( "Connection to " ) ).ToArray();
            connections.Length.ShouldBeGreaterThan( 0 );
            connections.Count( e => e.Text.Contains( password ) ).ShouldBe( 0 );
            // This error is the exception of the aspect that cannot be created.
            var unable = entries.Where( e => e.Exception != null && e.Exception.ToString().Contains( "Unable to open database for" ) ).ToArray();
            unable.Length.ShouldBeGreaterThan( 0 );
            unable.Count( e => e.Exception!.ToString().Contains( password ) ).ShouldBe( 0 );
        }
    }
}
