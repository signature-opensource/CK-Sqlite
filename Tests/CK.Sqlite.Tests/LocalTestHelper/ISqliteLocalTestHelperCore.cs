using System;
using System.Collections.Generic;
using CK.Testing;
using CK.Text;

namespace CK.Testing.SqliteLocal
{
    /// <summary>
    /// Helper local to Sqlite solution.
    /// </summary>
    public interface ISqliteLocalTestHelperCore
    {
        /// <summary>
        /// Gets the bin paths in net461 and netstandard2.0 (or netcoreapp2.0) for CK.Sqlite.Setup.Model
        /// and CK.Sqlite.Setup.Runtime.
        /// </summary>
        IEnumerable<NormalizedPath> CKSqliteComponentsPaths { get; }

        /// <summary>
        /// Gets <see cref="CKSqliteComponentsPaths"/> plus <see cref="SqlZonePackageComponentsPaths"/>
        /// (with the runtimes of SqlActorPackage and SqlZonePackage).
        /// </summary>
        IEnumerable<NormalizedPath> AllLocalComponentsPaths { get; }

        /// <summary>
        /// Deletes <see cref="AllLocalComponentsPaths"/>/publish folders (only the ones in netcoreapp2.0).
        /// </summary>
        void DeleteAllLocalComponentsPublishedFolders();

    }
}
