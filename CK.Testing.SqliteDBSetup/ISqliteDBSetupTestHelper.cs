using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing
{
    /// <summary>
    /// Mixin that supports SQLite DBSetup based on <see cref="ICKSetupTestHelper"/>.
    /// </summary>
    public interface ISqliteDBSetupTestHelper : IMixinTestHelper, ICKSetupTestHelper, IStObjMapTestHelper, SqliteDBSetup.ISqliteDBSetupTestHelperCore
    {
    }
}
