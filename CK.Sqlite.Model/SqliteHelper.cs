using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;
using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace CK.Sqlite
{
    /// <summary>
    /// Offers utility methods to deal with Sqlite objects and data.
    /// </summary>
    public static class SqliteHelper
    {
        /// <summary>
        /// Provides a correct string content by replacing ' with ''.
        /// This does not enclose the result by surrounding quotes: this has to be done at the caller level.
        /// </summary>
        /// <param name="s">The starting string.</param>
        /// <returns>An encoded string.</returns>
        static public string SqliteEncodeStringContent( string? s )
        {
            return s == null ? string.Empty : s.Replace( "'", "''" );
        }

        /// <summary>
        /// Protects pattern meta character of Sql Server: <c>[</c>, <c>_</c> and <c>%</c> by 
        /// appropriates encoding. Then, if <paramref name="expandWildCards"/> is true, 
        /// expands <c>*</c> and <c>?</c> by appropriate pattern markers.
        /// </summary>
        /// <param name="s">The starting string.</param>
        /// <param name="expandWildCards">True if the pattern contains * and ? that must be expanded.. See remarks.</param>
        /// <param name="innerPattern">True to ensure that the pattern starts and ends with a %. See remarks.</param>
        /// <returns>An encoded string.</returns>
        /// <remarks>
        /// When <paramref name="expandWildCards"/> is true, use \* for a real *, \? for a 
        /// real ?. \ can be used directly except when directly followed by *, ? or another \: it must then be duplicated.<br/>
        /// When <paramref name="innerPattern"/> is true, an empty or null string is returned as '%'.
        /// </remarks>
        static public string SqliteEncodePattern( string? s, bool expandWildCards, bool innerPattern )
        {
            if( s == null || s.Length == 0 ) return innerPattern ? "%" : String.Empty;
            StringBuilder b = new StringBuilder( s );
            b.Replace( "'", "''" );
            b.Replace( "[", "[[]" );
            b.Replace( "_", "[_]" );
            b.Replace( "%", "[%]" );
            if( expandWildCards )
            {
                b.Replace( @"\\", "\x0" );
                b.Replace( @"\*", "\x1" );
                b.Replace( @"\?", "\x2" );
                b.Replace( '*', '%' );
                b.Replace( '?', '_' );
                b.Replace( '\x0', '\\' );
                b.Replace( '\x1', '*' );
                b.Replace( '\x2', '?' );
            }
            if( innerPattern )
            {
                if( b[0] != '%' ) b.Insert( 0, '%' );
                if( b.Length > 1 && b[b.Length - 1] != '%' ) b.Append( '%' );
            }
            return b.ToString();
        }

        [return:NotNullIfNotNull( nameof(value))]
        public static DateTime? ReadDateTimeFromSqliteValue( object? value )
        {
            if( value == null || value == DBNull.Value ) return null; 
            if( value is string stringValue ) return DateTime.ParseExact( stringValue, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal );
            if( value is long longValue ) return DateTime.UnixEpoch + TimeSpan.FromSeconds( longValue );
            return Throw.NotSupportedException<DateTime>( $"Could not parse SQLite date with unsupported type {value.GetType().FullName}" );
        }
    }
}
