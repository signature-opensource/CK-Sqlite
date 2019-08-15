using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Sqlite
{
    /// <summary>
    /// Base attribute for <see cref="SqliteTableAttribute"/> and <see cref="SqlitePackageAttribute"/>.
    /// </summary>
    public abstract class SqlitePackageAttributeBase : Setup.AmbientContextBoundDelegationAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="SqlitePackageAttributeBase"/>.
        /// </summary>
        /// <param name="actualAttributeTypeAssemblyQualifiedName">Assembly Qualified Name of the object that will replace this attribute during setup.</param>
        protected SqlitePackageAttributeBase( string actualAttributeTypeAssemblyQualifiedName )
            : base( actualAttributeTypeAssemblyQualifiedName )
        {
        }

        /// <summary>
        /// Gets or sets the package to which this package belongs.
        /// </summary>
        public Type Package { get; set; }

        /// <summary>
        /// Gets or sets the Resource path to use for the <see cref="IResourceLocator"/>. 
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the Resource Type to use for the <see cref="IResourceLocator"/>.
        /// When null (the default that should rarely be changed), it is the decorated type itself that is 
        /// used to locate the resources.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SqliteDatabase"/> type targeted by the package. Let it to null to use the ambient one.
        /// The <see cref="SqlitePackage.Database"/> property is automatically set (see remarks).
        /// </summary>
        /// <remarks>
        /// The type must be a specialization of <see cref="SqliteDatabase"/>. 
        /// If it supports <see cref="IAmbientContract"/>, the property is bound to the corresponding ambient contract instance. 
        /// </remarks>
        public Type Database { get; set; }
    }
}
