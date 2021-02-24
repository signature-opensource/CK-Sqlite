using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Sqlite
{
    /// <summary>
    /// Basic script executor. It is a disposable object.
    /// </summary>
    public interface ISqliteScriptExecutor : IDisposable
    {
        /// <summary>
        /// Executes a single script.
        /// </summary>
        /// <param name="script">Script to execute.</param>
        /// <returns>True on success.</returns>
        bool Execute( string script );

    }

    /// <summary>
    /// Extends <see cref="ISqliteScriptExecutor"/> to support multiple scripts execution at once.
    /// </summary>
    public static class SqliteScriptExecutorExtension
    {
        /// <summary>
        /// Executes multiple scripts.
        /// </summary>
        /// <param name="this">This <see cref="ISqliteScriptExecutor"/>.</param>
        /// <param name="scripts">A set of scripts.</param>
        /// <param name="stopOnError">False to continue execution regardless of a script failure.</param>
        /// <returns>The number of script that failed.</returns>
        public static int Execute( this ISqliteScriptExecutor @this, IEnumerable<string> scripts, bool stopOnError = true )
        {
            if( scripts == null ) throw new ArgumentNullException( "scripts" );
            int failCount = 0;
            foreach( string s in scripts )
            {
                if( s != null && !@this.Execute( s ) )
                {
                    ++failCount;
                    if( !stopOnError ) break;
                }
            }
            return failCount;
        }
    }

}
