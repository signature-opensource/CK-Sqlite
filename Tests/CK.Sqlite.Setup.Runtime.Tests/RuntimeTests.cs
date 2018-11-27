using CK.Core;
using CK.Setup;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CK.Sqlite.Setup.Runtime.Tests
{
    public class RuntimeTests
    {
        [Test]
        public void sqlite_aspect_should_read_connectionstring_from_env_var()
        {
            SqliteSetupAspectConfiguration config = new SqliteSetupAspectConfiguration()
            {
                DefaultDatabaseConnectionString = "$ENV::SqliteSetupAspect:DefaultDatabaseConnectionString",
            };
            Environment.SetEnvironmentVariable( "SqliteSetupAspect:DefaultDatabaseConnectionString", "Data Source=:memory:" );
            ConfigureOnly<ISetupableAspectConfiguration> co = new ConfigureOnly<ISetupableAspectConfiguration>( new TestSetupableAspectConfiguration() );


            SqliteSetupAspect aspect = new SqliteSetupAspect( config, new ActivityMonitor(), co );


            aspect.SqliteDatabases.FindManagerByName( SqliteDatabase.DefaultDatabaseName ).Should().NotBeNull();
            aspect.SqliteDatabases.FindManagerByConnectionString( "Data Source=:memory:" ).Should().NotBeNull();
        }

        [Test]
        public void sqlite_aspect_should_fail_on_unknown_from_env_var()
        {
            SqliteSetupAspectConfiguration config = new SqliteSetupAspectConfiguration()
            {
                DefaultDatabaseConnectionString = "$ENV::SqliteSetupAspect:DefaultDatabaseConnectionString",
            };
            Environment.SetEnvironmentVariable( "SqliteSetupAspect:DefaultDatabaseConnectionString", null );
            ConfigureOnly<ISetupableAspectConfiguration> co = new ConfigureOnly<ISetupableAspectConfiguration>( new TestSetupableAspectConfiguration() );


            Action act = () =>
            {
                SqliteSetupAspect aspect = new SqliteSetupAspect( config, new ActivityMonitor(), co );
            };


            act.Should().Throw<InvalidOperationException>();
        }
    }

    class TestSetupableAspectConfiguration : ISetupableAspectConfiguration
    {
        public SetupableAspectConfiguration ExternalConfiguration { get; set; }
        public SetupAspectConfigurator Configurator { get; set; }
        public IList<object> ExternalItems { get; set; }
        public Action<IEnumerable<IDependentItem>> DependencySorterHookInput { get; set; }
        public Action<IEnumerable<ISortedItem>> DependencySorterHookOutput { get; set; }
    }
}
