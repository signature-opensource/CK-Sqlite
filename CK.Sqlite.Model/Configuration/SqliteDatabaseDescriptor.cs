using System;
using System.Xml.Linq;

namespace CK.Setup;

/// <summary>
/// Associates a name to a connection string.
/// </summary>
public class SqliteDatabaseDescriptor
{
    /// <summary>
    /// Initializes a new <see cref="SqliteDatabaseDescriptor"/>.
    /// </summary>
    /// <param name="name">Logical database name.</param>
    /// <param name="connectionString">Connection string to the database.</param>
    public SqliteDatabaseDescriptor( string name, string connectionString )
    {
        LogicalDatabaseName = name;
        ConnectionString = connectionString;
    }

    static readonly XName xLogicalDatabaseName = XNamespace.None + "LogicalDatabaseName";
    static readonly XName xConnectionString = XNamespace.None + "ConnectionString";
    static readonly XName xAutoCreate = XNamespace.None + "AutoCreate";

    /// <summary>
    /// Initializes a new <see cref="SqliteDatabaseDescriptor"/> from its xml representation.
    /// </summary>
    /// <param name="e">The element.</param>
    public SqliteDatabaseDescriptor( XElement e )
    {
        LogicalDatabaseName = e.Element( xLogicalDatabaseName ).Value;
        ConnectionString = e.Element( xConnectionString ).Value;
    }


    /// <summary>
    /// Serializes its content in the provided <see cref="XElement"/> and returns it.
    /// The <see cref="SqliteDatabaseDescriptor(XElement)"/> constructor will be able to read this element back.
    /// </summary>
    /// <param name="e">The element to populate.</param>
    /// <returns>The <paramref name="e"/> element.</returns>
    public XElement Serialize( XElement e )
    {
        e.Add( new XElement( xLogicalDatabaseName, LogicalDatabaseName ),
               new XElement( xConnectionString, ConnectionString ) );
        return e;
    }

    /// <summary>
    /// Gets or sets the logical name of the database.
    /// It is independent of the actual database name.
    /// </summary>
    public string LogicalDatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string ConnectionString { get; set; }

}
