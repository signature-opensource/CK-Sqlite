using System;
using System.Collections.Generic;
using System.Linq;
using CK.Core;
using CK.Setup;

namespace CK.Sqlite.Setup
{
    public class SqliteSetupAspect : IStObjEngineAspect, ISqliteSetupAspect, IDisposable
    {
        readonly SqliteSetupAspectConfiguration _config;
        readonly ISetupableAspectRunConfiguration _setupConfiguration;
        readonly SqliteManagerProvider _databases;
        ISqliteManagerBase _defaultDatabase;

        class StObjConfiguratorHook : StObjConfigurationLayer
        {
            readonly SqliteSetupAspectConfiguration _config;

            public StObjConfiguratorHook( SqliteSetupAspectConfiguration config )
            {
                _config = config;
            }

            public override void ResolveParameterValue( IActivityMonitor monitor, IStObjFinalParameter parameter )
            {
                base.ResolveParameterValue( monitor, parameter );
                if( parameter.Name == "connectionString"
                    && parameter.Owner.FinalImplementation.Implementation is SqliteDatabase db )
                {
                    parameter.SetParameterValue( _config.FindConnectionStringByName( db.Name ) );
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="SqliteSetupAspect"/>.
        /// This constructor is called by the StObjEngine whenever a <see cref="SqliteSetupAspectConfiguration"/> configuration object
        /// appears in <see cref="StObjEngineConfiguration.Aspects"/> list.
        /// </summary>
        /// <param name="config">Configuration object.</param>
        /// <param name="monitor">Monitor to use.</param>
        /// <param name="setupConfiguration"></param>
        public SqliteSetupAspect( SqliteSetupAspectConfiguration config, IActivityMonitor monitor, ConfigureOnly<ISetupableAspectRunConfiguration> setupConfiguration )
        {
            if( setupConfiguration.Service == null ) throw new ArgumentNullException( nameof( setupConfiguration ) );
            if( config == null ) throw new ArgumentNullException( nameof( config ) );
            _config = config;
            _setupConfiguration = setupConfiguration.Service;
            _databases = new SqliteManagerProvider( monitor );
            _databases.Add( SqliteDatabase.DefaultDatabaseName, _config.DefaultDatabaseConnectionString );
            foreach( var db in _config.Databases )
            {
                _databases.Add( db.LogicalDatabaseName, db.ConnectionString );
            }
        }

        bool IStObjEngineAspect.Configure( IActivityMonitor monitor, IStObjEngineConfigureContext context )
        {
            _defaultDatabase = _databases.FindManagerByName( SqliteDatabase.DefaultDatabaseName );
            ISqliteManager realSqlManager = _defaultDatabase as ISqliteManager;
            if( !context.ServiceContainer.IsAvailable<IVersionedItemReader>() )
            {
                if( realSqlManager != null )
                {
                    monitor.Info( "Registering SqliteVersionedItemReader on the default database as the version reader." );
                    context.ServiceContainer.Add<IVersionedItemReader>( new SqliteVersionedItemReader( realSqlManager ) );
                }
                else
                {
                    monitor.Info( $"Unable to use SqliteVersionedItemReader on the default database as the version writer since the underlying sql manager is not a real manager." );
                }
            }
            if( !context.ServiceContainer.IsAvailable<IVersionedItemWriter>() )
            {
                monitor.Info( "Registering SqliteVersionedItemWriter on the default database as the version writer." );
                context.ServiceContainer.Add<IVersionedItemWriter>( new SqliteVersionedItemWriter( _defaultDatabase ) );
            }
            if( !context.ServiceContainer.IsAvailable<ISetupSessionMemoryProvider>() )
            {
                if( realSqlManager != null )
                {
                    monitor.Info( $"Registering SqliteSetupSessionMemoryProvider on the default database as the memory provider." );
                    context.ServiceContainer.Add<ISetupSessionMemoryProvider>( new SqliteSetupSessionMemoryProvider( realSqlManager ) );
                }
                else
                {
                    monitor.Info( "Unable to use SqliteSetupSessionMemoryProvider on the default database as the memory provider since the underlying sql manager is not a real manager." );
                }
            }
            context.ServiceContainer.Add<ISqliteManagerProvider>( _databases );
            context.Configurator.AddLayer( new StObjConfiguratorHook( _config ) );
            context.AddExplicitRegisteredType( typeof( SqliteDefaultDatabase ) );
            return true;
        }

        bool IStObjEngineAspect.RunPreCode(CK.Core.IActivityMonitor monitor, CK.Setup.IStObjEngineRunContext context)
        {
            return true;
        }

        bool IStObjEngineAspect.RunPostCode( CK.Core.IActivityMonitor monitor, CK.Setup.IStObjEnginePostCodeRunContext context )
        {
            return true;
        }

        bool IStObjEngineAspect.Terminate( IActivityMonitor monitor, IStObjEngineTerminateContext context )
        {
            return true;
        }

        /// <summary>
        /// Gets the configuration object.
        /// </summary>
        public SqliteSetupAspectConfiguration Configuration => _config;

        /// <summary>
        /// Gets the default database as a <see cref="ISqliteManagerBase"/> object.
        /// </summary>
        public ISqliteManagerBase DefaultSqliteDatabase => _defaultDatabase;

        /// <summary>
        /// Gets the available databases (including the <see cref="DefaultSqliteDatabase"/>).
        /// It is initialized with <see cref="SqliteSetupAspectConfiguration.Databases"/> content.
        /// </summary>
        public ISqliteManagerProvider SqliteDatabases => _databases;

        /// <summary>
        /// Releases all database managers.
        /// Can safely be called multiple times.
        /// </summary>
        public virtual void Dispose()
        {
            // Can safely be called multiple times.
            _databases.Dispose();
        }

    }
}
