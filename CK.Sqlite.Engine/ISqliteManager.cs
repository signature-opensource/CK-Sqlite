using System;
using System.Collections.Generic;
using CK.Core;
using Microsoft.Data.Sqlite;

namespace CK.Sqlite.Setup
{

    /// <summary>
    /// Extends <see cref="ISqliteManagerBase"/> to offer minimal helpers
    /// to query the database.
    /// </summary>
    public interface ISqliteManager : ISqliteManagerBase
    {
        /// <summary>
        /// Gets the <see cref="SqliteConnection"/> of this <see cref="ISqliteManager"/>.
        /// </summary>
        SqliteConnection? Connection { get; }

        /// <summary>
        /// Simple execute scalar helper.
        /// The connection must be opened.
        /// </summary>
        /// <param name="select">Select clause.</param>
        /// <returns>The scalar (may be DBNull.Value) or null if no result has been returned.</returns>
        object? ExecuteScalar( string select );

        /// <summary>
        /// Simple execute helper.
        /// The connection must be opened.
        /// </summary>
        /// <param name="cmd">The command text.</param>
        /// <param name="timeoutSecond">Timeout of the execution in seconds.</param>
        /// <returns>The number of rows.</returns>
        int ExecuteNonQuery( string cmd, int timeoutSecond = -1 );

        /// <summary>
        /// Executes the command and returns the first row as an array of object values.
        /// </summary>
        /// <param name="cmd">The <see cref="SqlCommand"/> to execute.</param>
        /// <returns>An array of objects or null if nothing has been returned from database.</returns>
        object[]? ReadFirstRow( SqliteCommand cmd );

    }
}
