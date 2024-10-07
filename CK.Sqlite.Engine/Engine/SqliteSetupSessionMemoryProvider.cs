using System;
using CK.Core;
using CK.Setup;
using Microsoft.Data.Sqlite;

namespace CK.Sqlite.Setup;

/// <summary>
/// Sqlite based memory provider for the setup.
/// It is used to skip already executed scripts.
/// </summary>
public class SqliteSetupSessionMemoryProvider : ISetupSessionMemoryProvider, ISetupSessionMemory
{
    readonly ISqliteManager _manager;
    bool _initialized;

    public SqliteSetupSessionMemoryProvider( ISqliteManager manager )
    {
        if( manager == null ) throw new ArgumentNullException( nameof( manager ) );
        _manager = manager;
    }

    /// <summary>
    /// Gets the date and time of the previous start.
    /// </summary>
    public DateTime LastStartDate { get; private set; }

    /// <summary>
    /// Gets the number of non terminated setup attempts.
    /// </summary>
    public int StartCount { get; private set; }

    /// <summary>
    /// Gets a description of the last failure (set by <see cref="StopSetup"/>).
    /// </summary>
    public string LastError { get; private set; }

    /// <summary>
    /// Gets whether <see cref="StartSetup"/> has been called and <see cref="StopSetup"/> has 
    /// not yet been called.
    /// </summary>
    public bool IsStarted { get; private set; }


    void Initialize()
    {
        _manager.EnsureCKCoreIsInstalled( _manager.Monitor );

        _manager.ExecuteNonQuery( _ensureTables );


        using( var cRead = new SqliteCommand( _getMemoryRows ) )
        {
            var existing = _manager.ReadFirstRow( cRead );

            if( existing == null || existing.Length == 0 )
            {
                _manager.ExecuteNonQuery( "insert into CKCore_tSetupMemory(SurrogateId,LastStartDate,TotalStartCount,StartCount,LastError) values( 0, 0, 0, 0, null );" );
            }
        }

        using( var cRead = new SqliteCommand( _getMemoryRows ) )
        {
            var existing = _manager.ReadFirstRow( cRead );

            LastStartDate = SqliteHelper.ReadDateTimeFromSqliteValue( existing[0] ).Value;
            if( LastStartDate == DateTime.UnixEpoch ) LastStartDate = Util.UtcMinValue;

            StartCount = Convert.ToInt32( (long)existing[1] );

            LastError = existing[2] == DBNull.Value ? null : (string)existing[2];
            _initialized = true;
        }
    }

    static string _ensureTables = @"
create table if not exists CKCore_tSetupMemory
(
	SurrogateId integer PRIMARY KEY,
	CreationDate datetime default CURRENT_TIMESTAMP,
	LastStartDate datetime not null,
	TotalStartCount integer not null,
	StartCount integer not null,
	LastError text
);
create table if not exists CKCore_tSetupMemoryItem
(
	ItemKey text not null PRIMARY KEY,
	ItemValue text not null
);
";
    static string _getMemoryRows = @"
select LastStartDate, StartCount, LastError from CKCore_tSetupMemory;
";

    /// <summary>
    /// Starts a setup session. <see cref="IsStarted"/> must be false 
    /// otherwise an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    public ISetupSessionMemory StartSetup()
    {
        if( IsStarted ) throw new InvalidOperationException();
        if( !_initialized ) Initialize();
        _manager.ExecuteNonQuery( "update CKCore_tSetupMemory set LastStartDate = datetime('now'), TotalStartCount = TotalStartCount+1, StartCount = StartCount+1, LastError='Started but not Stopped yet.'" );
        IsStarted = true;
        return this;
    }

    /// <summary>
    /// On success, the whole memory of the setup process must be cleared. 
    /// On failure (when <paramref name="error"/> is not null), the memory must be persisted.
    /// <see cref="IsStarted"/> must be true otherwise an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <param name="error">
    /// Must be not null to indicate an error. Null on success. 
    /// Empty or white space will raise an <see cref="ArgumentException"/>.
    /// </param>
    public void StopSetup( string error )
    {
        if( !IsStarted ) throw new InvalidOperationException();
        if( error == null )
        {
            _manager.ExecuteNonQuery( "update CKCore_tSetupMemory set StartCount = 0, LastError=null" );
            _manager.ExecuteNonQuery( "drop table CKCore_tSetupMemoryItem" );
            StartCount = 0;
            LastError = null;
        }
        else
        {
            if( string.IsNullOrWhiteSpace( error ) ) throw new ArgumentException( "Must be null or not be empty.", "error" );

            using( var c = new SqliteCommand( $"update CKCore_tSetupMemory set LastError=@LastError", _manager.Connection ) )
            {
                c.Parameters.AddWithValue( "@LastError", error );

                c.ExecuteNonQuery();
            }

            using( var c = new SqliteCommand( @"select LastStartDate, StartCount from CKCore_tSetupMemory;" ) )
            {
                var resync = _manager.ReadFirstRow( c );

                LastStartDate = SqliteHelper.ReadDateTimeFromSqliteValue( resync[0] ).Value;

                StartCount = Convert.ToInt32( (long)resync[1] );
            }
        }
        IsStarted = false;
    }

    #region ISetupSessionMemory Auto implementation

    void ISetupSessionMemory.RegisterItem( string itemKey, string itemValue )
    {
        if( itemValue == null ) throw new ArgumentNullException( "itemValue" );
        if( String.IsNullOrWhiteSpace( itemKey ) || itemKey.Length > 255 ) throw new ArgumentException( "Must not be null or empty or longer than 255 characters.", "itemKey" );

        using( var c = new SqliteCommand( @"insert or replace into CKCore_tSetupMemoryItem ( ItemKey, ItemValue ) values(@ItemKey, @ItemValue)" ) )
        {
            c.Connection = _manager.Connection;
            c.Parameters.AddWithValue( "@ItemKey", itemKey );
            c.Parameters.AddWithValue( "@ItemValue", itemValue );
            c.ExecuteNonQuery();
        }
    }

    string ISetupSessionMemory.FindRegisteredItem( string itemKey )
    {
        if( string.IsNullOrWhiteSpace( itemKey ) || itemKey.Length > 255 ) throw new ArgumentException( "Must not be null or empty or longer than 255 characters.", "itemKey" );
        using( var c = new SqliteCommand( @"select ItemValue from CKCore_tSetupMemoryItem where ItemKey=@ItemKey;" ) )
        {
            c.Connection = _manager.Connection;
            c.Parameters.AddWithValue( "@ItemKey", itemKey );
            return (string)c.ExecuteScalar();
        }
    }

    bool ISetupSessionMemory.IsItemRegistered( string itemKey )
    {
        if( String.IsNullOrWhiteSpace( itemKey ) || itemKey.Length > 255 ) throw new ArgumentException( "Must not be null or empty or longer than 255 characters.", "itemKey" );
        using( var c = new SqliteCommand( @"select 'a' from CKCore_tSetupMemoryItem where ItemKey=@ItemKey;" ) )
        {
            c.Connection = _manager.Connection;
            c.Parameters.AddWithValue( "@ItemKey", itemKey );
            return (string)c.ExecuteScalar() != null;
        }
    }
    #endregion

}
