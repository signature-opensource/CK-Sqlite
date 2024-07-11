using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Sqlite.Setup
{
    internal class SqliteCKCoreInstaller
    {
        public readonly static int CurrentVersion = 1;

        /// <summary>
        /// Installs the kernel.
        /// </summary>
        /// <param name="manager">The manager that will be used.</param>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="forceInstall">True to force the installation even if Ver column of CKCore_tSystem where Id = 1 is the same as <see cref="CurrentVersion"/>.</param>
        /// <returns>True on success.</returns>
        public static bool Install( SqliteManager manager, IActivityMonitor monitor, bool forceInstall = false )
        {
            Throw.CheckNotNullArgument( monitor );

            using( monitor.OpenTrace( "Installing CKCore kernel." ) )
            {
                bool ckcoreExists = false;
                int ver = 0;
                if( !forceInstall )
                {
                    long exists = (long)manager.ExecuteScalar("SELECT CASE WHEN EXISTS( SELECT 1 FROM sqlite_master WHERE type='table' AND name='CKCore_tSystem') THEN 1 ELSE 0 END;" );
                    ckcoreExists = (exists == 1);
                    if( ckcoreExists )
                    {
                        ver = Convert.ToInt32((long)manager.ExecuteScalar("select Ver from CKCore_tSystem where Id=1"));
                    }
                }

                if( ckcoreExists && ver == CurrentVersion )
                {
                    monitor.CloseGroup( $"Already installed in version {CurrentVersion}." );
                }
                else
                {
                    monitor.MinimalFilter = LogFilter.Terse;
                    SimpleScriptTagHandler s = new SimpleScriptTagHandler( _script.Replace( "$Ver$", CurrentVersion.ToString() ) );
                    if( !s.Expand( monitor, false ) ) return false;
                    if( !manager.ExecuteScripts( s.SplitScript().Select( one => one.Body ), monitor ) ) return false;
                    if( ver == 0 ) monitor.CloseGroup( String.Format( "Installed in version {0}.", CurrentVersion ) );
                    else monitor.CloseGroup( String.Format( "Installed in version {0} (was {1}).", CurrentVersion, ver ) );
                }
            }
            return true;
        }

        static readonly string _script = @"
create table if not exists CKCore_tSystem
(
	Id int not null PRIMARY KEY,
    CreationDate datetime default CURRENT_TIMESTAMP,
    Ver smallint not null
);
insert or replace into CKCore_tSystem(Id,Ver) VALUES(1,$Ver$);
";
    }
}
