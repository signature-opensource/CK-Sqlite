using System;
using CK.Core;
using CK.Setup;

namespace CK.Sqlite.Setup
{
    /// <summary>
    /// Implements <see cref="SqlitePackageAttribute"/> attribute.
    /// </summary>
    public class SqlitePackageAttributeImpl : SqlitePackageAttributeImplBase, IStObjSetupConfigurator
    {
        /// <summary>
        /// Initializes a new <see cref="SqlitePackageAttribute"/>.
        /// </summary>
        /// <param name="a">The attribute.</param>
        public SqlitePackageAttributeImpl( SqlitePackageAttribute a )
            : base( a )
        {
        }

        /// <summary>
        /// Masked to be formally associated to the <see cref="SqlPackageAttribute"/> attribte type.
        /// </summary>
        protected new SqlitePackageAttribute Attribute => (SqlitePackageAttribute)base.Attribute;

        /// <summary>
        /// Transfers <see cref="SqlPackageAttribute.HasModel"/> to "HasModel" stobj property.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="o">The configured object.</param>
        protected override void ConfigureMutableItem( IActivityMonitor monitor, IStObjMutableItem o )
        {
            o.SetStObjPropertyValue( monitor, "HasModel", Attribute.HasModel );
        }

        void IStObjSetupConfigurator.ConfigureDependentItem( IActivityMonitor monitor, IMutableStObjSetupData data )
        {
            if( data.IsDefaultFullNameWithoutContext )
            {
                monitor.Info( $"SqlPackage '{data.FullNameWithoutContext}' uses its own full name as its SetupName." );
            }
            if( data.ItemType == null && data.ItemTypeName == null )
            {
                data.ItemType = typeof( SqlitePackageBaseItem );
            }
            if( data.DriverType == null && data.DriverTypeName == null )
            {
                data.DriverType = typeof( SqlitePackageBaseItemDriver );
            }
        }

    }
}
