using CK.Core;
using Microsoft.Data.Sqlite;
using System;

namespace CK.Sqlite;

/// <summary>
/// Temporary Sqlite database. A <see cref="TemporaryFile"/> is used with a <see cref="SqliteOpenMode.ReadWriteCreate"/> mode.
/// The database is deleted whenever this object is disposed or (since <see cref="System.IO.FileAttributes.Temporary"/> is used),
/// when a system reboot occurs.
/// </summary>
public class TemporarySqliteDatabase : IDisposable
{
    readonly TemporaryFile _file;
    readonly string _connectionString;

    /// <summary>
    /// Initialize a new <see cref="TemporarySqliteDatabase"/>.
    /// </summary>
    public TemporarySqliteDatabase()
    {
        _file = new TemporaryFile( ".sqlite" );
        _connectionString = new SqliteConnectionStringBuilder()
        {
            DataSource = _file.Path,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
    }

    /// <summary>
    /// Gets the connections string to this temporary database.
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <summary>
    /// Gets the connections string to this temporary database.
    /// </summary>
    public string DatabaseFilePath => _file.Path;

    /// <summary>
    /// Disposes this <see cref="TemporarySqliteDatabase"/> by disposing the inner <see cref="TemporaryFile"/>
    /// that deletes the .sqlite file.
    /// </summary>
    public void Dispose()
    {
        _file.Dispose();
    }
}
