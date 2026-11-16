using CK.Core;
using CK.Setup;
using Microsoft.Data.Sqlite;
using System;
using static CK.Testing.MonitorTestHelper;

namespace CK.Testing;

/// <summary>
/// Provides EngineConfiguration.EnsureSqliteConfigurationAspect() helper.
/// </summary>
public static class SqliteEngineTestHelperExtensions
{
    /// <summary>
    /// Adds or configures the <see cref="SetupableAspectConfiguration"/> and <see cref="SqliteAspectConfiguration"/> in the
    /// <see cref="EngineConfiguration.Aspects"/>.
    /// <para>
    /// The database is created if it doesn't exist.
    /// </para>
    /// </summary>
    /// <param name="engineConfiguration">This engine configuration to configure.</param>
    /// <param name="connectionString">Optional connection string.</param>
    /// <param name="revertOrderingName">
    /// By default, the topological sort of the real objects and setupable items graphs randomly sort the
    /// items in the same rank with their ascending or descending names. This helps find missing constraints
    /// in the graphs.
    /// <para>
    /// To disable the random behavior, set this to false or true.
    /// </para>
    /// </param>
    /// <returns>The configured aspect.</returns>
    public static SqliteAspectConfiguration EnsureSqliteConfigurationAspect( this EngineConfiguration engineConfiguration,
                                                                             string? connectionString = null,
                                                                             bool? revertOrderingName = null )
    {
        bool revertOrdering = revertOrderingName ?? (Environment.TickCount % 2) == 0;
        if( revertOrdering )
        {
            TestHelper.Monitor.Info( "Reverting ordering names in both real objects and setupable items graphs." );
        }
        engineConfiguration.RevertOrderingNames = revertOrdering;
        var setupable = engineConfiguration.EnsureAspect<SetupableAspectConfiguration>();
        setupable.RevertOrderingNames = revertOrdering;
        var liteConfig = engineConfiguration.EnsureAspect<SqliteAspectConfiguration>();
        if( connectionString != null )
        {
            liteConfig.DefaultDatabaseConnectionString = connectionString;
        }
        return liteConfig;
    }
}
