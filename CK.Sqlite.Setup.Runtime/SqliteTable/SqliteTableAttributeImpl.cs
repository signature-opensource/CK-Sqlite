using System;
using CK.Core;
using CK.Setup;

namespace CK.Sqlite.Setup
{
    /// <summary>
    /// Implementation of <see cref="SqliteTableAttribute"/>.
    /// </summary>
    public class SqliteTableAttributeImpl : SqlitePackageAttributeImplBase, IStObjSetupConfigurator
    {
        /// <summary>
        /// Initializes a new <see cref="SqliteTableAttributeImpl"/>.
        /// </summary>
        /// <param name="a">The attribute.</param>
        public SqliteTableAttributeImpl( SqliteTableAttribute a )
            : base( a )
        {
        }

        /// <summary>
        /// Masked to formally associates a <see cref="SqlTableAttribute"/> attribute.
        /// </summary>
        protected new SqliteTableAttribute Attribute => (SqliteTableAttribute)base.Attribute;

        /// <summary>
        /// Transfers <see cref="SqliteTableAttribute.TableName" /> as a direct property "TableName" of the StObj item.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="o">The configured object.</param>
        protected override void ConfigureMutableItem( IActivityMonitor monitor, IStObjMutableItem o )
        {
            if( Attribute.TableName != null ) o.SetDirectPropertyValue( monitor, "TableName", Attribute.TableName );
        }

        void IStObjSetupConfigurator.ConfigureDependentItem( IActivityMonitor monitor, IMutableStObjSetupData data )
        {
            SetAutomaticSetupFullNameWithoutContext( monitor, data, "SqlTable" );
            data.ItemType = typeof( SqliteTableItem );
            data.DriverType = typeof( SqliteTableItemDriver );
        }

    }
}
