using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Sqlite
{
    /// <summary>
    /// General and basic interface for objects that know their connection string.
    /// </summary>
    public interface ISqliteConnectionStringProvider
    {
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        string ConnectionString { get; }
    }
}
