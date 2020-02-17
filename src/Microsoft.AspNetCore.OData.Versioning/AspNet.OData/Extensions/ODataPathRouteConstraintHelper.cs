namespace Microsoft.AspNet.OData.Extensions
{
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using System;
    using System.Globalization;

    /// <summary>
    /// Local copy of AspNetCore ODataPathRouteConstraint.
    /// </summary>
    [CLSCompliant(false)]
    public static class ODataPathRouteConstraintHelper
    {
        // "%2F"
        private static readonly string escapedSlash = Uri.EscapeDataString( "/" );

        /// <summary>
        /// Get the OData path from the url and query string.
        /// </summary>
        /// <param name="oDataPathString">The ODataPath from the route values.</param>
        /// <param name="pathString">The Uri from start to end of path, i.e. the left portion.</param>
        /// <param name="queryString">The Uri from the query string to the end, i.e. the right portion.</param>
        /// <param name="requestContainerFactory">The request container factory.</param>
        /// <returns>The OData path.</returns>
        public static ODataPath? GetODataPath( string? oDataPathString, string pathString, string? queryString, Func<IServiceProvider> requestContainerFactory )
        {
            if ( pathString == null )
            {
                throw new ArgumentNullException( nameof(pathString) );
            }

            if ( requestContainerFactory == null )
            {
                throw new ArgumentNullException( nameof(requestContainerFactory) );
            }

            ODataPath? path;

            try
            {
                // Service root is the current RequestUri, less the query string and the ODataPath (always the
                // last portion of the absolute path).  ODL expects an escaped service root and other service
                // root calculations are calculated using AbsoluteUri (also escaped).  But routing exclusively
                // uses unescaped strings, determined using
                //    address.GetComponents(UriComponents.Path, UriFormat.Unescaped)
                //
                // For example if the AbsoluteUri is
                // <http://localhost/odata/FunctionCall(p0='Chinese%E8%A5%BF%E9%9B%85%E5%9B%BEChars')>, the
                // oDataPathString will contain "FunctionCall(p0='Chinese西雅图Chars')".
                //
                // Due to this decoding and the possibility of unnecessarily-escaped characters, there's no
                // reliable way to determine the original string from which oDataPathString was derived.
                // Therefore a straightforward string comparison won't always work.  See RemoveODataPath() for
                // details of chosen approach.
                string serviceRoot = pathString;

                if ( !string.IsNullOrEmpty( oDataPathString ) )
                {
                    serviceRoot = RemoveODataPath( serviceRoot, oDataPathString );
                }

                // As mentioned above, we also need escaped ODataPath.
                // The requestLeftPart and request.QueryString are both escaped.
                // The ODataPath for service documents is empty.
                string oDataPathAndQuery = pathString.Substring( serviceRoot.Length );

                if ( !string.IsNullOrEmpty( queryString ) )
                {
                    // Ensure path handler receives the query string as well as the path.
                    oDataPathAndQuery += queryString;
                }

                // Leave an escaped '/' out of the service route because DefaultODataPathHandler will add a
                // literal '/' to the end of this string if not already present. That would double the slash
                // in response links and potentially lead to later 404s.
                if ( serviceRoot.EndsWith( escapedSlash, StringComparison.OrdinalIgnoreCase ) )
                {
                    serviceRoot = serviceRoot.Substring( 0, serviceRoot.Length - escapedSlash.Length );
                }

                var requestContainer = requestContainerFactory();
                var pathHandler = requestContainer.GetRequiredService<IODataPathHandler>();
                path = pathHandler.Parse( serviceRoot, oDataPathAndQuery, requestContainer );
            }
            catch ( ODataException )
            {
                path = null;
            }

            return path;
        }

        // Find the substring of the given URI string before the given ODataPath.  Tests rely on the following:
        // 1. ODataPath comes at the end of the processed Path
        // 2. Virtual path root, if any, comes at the beginning of the Path and a '/' separates it from the rest
        // 3. OData prefix, if any, comes between the virtual path root and the ODataPath and '/' characters separate
        //    it from the rest
        // 4. Even in the case of Unicode character corrections, the only differences between the escaped Path and the
        //    unescaped string used for routing are %-escape sequences which may be present in the Path
        //
        // Therefore, look for the '/' character at which to lop off the ODataPath.  Can't just unescape the given
        // uriString because subsequent comparisons would only help to check whether a match is _possible_, not where
        // to do the lopping.
        private static string RemoveODataPath( string uriString, string oDataPathString )
        {
            // Potential index of oDataPathString within uriString.
            var endIndex = uriString.Length - oDataPathString.Length - 1;
            if ( endIndex <= 0 )
            {
                // Bizarre: oDataPathString is longer than uriString.  Likely the values collection passed to Match()
                // is corrupt.
                throw InvalidOperation( LocalSR.RequestUriTooShortForODataPath, uriString, oDataPathString );
            }

            string startString = uriString.Substring( 0, endIndex + 1 );  // Potential return value.
            string endString = uriString.Substring( endIndex + 1 );       // Potential oDataPathString match.
            if ( string.Equals( endString, oDataPathString, StringComparison.Ordinal ) )
            {
                // Simple case, no escaping in the ODataPathString portion of the Path.  In this case, don't do extra
                // work to look for trailing '/' in startString.
                return startString;
            }

            while ( true )
            {
                // Escaped '/' is a derivative case but certainly possible.
                int slashIndex = startString.LastIndexOf( '/', endIndex - 1 );
                int escapedSlashIndex =
                    startString.LastIndexOf( escapedSlash, endIndex - 1, StringComparison.OrdinalIgnoreCase );
                if ( slashIndex > escapedSlashIndex )
                {
                    endIndex = slashIndex;
                }
                else if ( escapedSlashIndex >= 0 )
                {
                    // Include the escaped '/' (three characters) in the potential return value.
                    endIndex = escapedSlashIndex + 2;
                }
                else
                {
                    // Failure, unable to find the expected '/' or escaped '/' separator.
                    throw InvalidOperation( LocalSR.ODataPathNotFound, uriString, oDataPathString );
                }

                startString = uriString.Substring( 0, endIndex + 1 );
                endString = uriString.Substring( endIndex + 1 );

                // Compare unescaped strings to avoid both arbitrary escaping and use of lowercase 'a' through 'f' in
                // %-escape sequences.
                endString = Uri.UnescapeDataString( endString );
                if ( string.Equals( endString, oDataPathString, StringComparison.Ordinal ) )
                {
                    return startString;
                }

                if ( endIndex == 0 )
                {
                    // Failure, could not match oDataPathString after an initial '/' or escaped '/'.
                    throw InvalidOperation( LocalSR.ODataPathNotFound, uriString, oDataPathString );
                }
            }
        }

        private static Exception InvalidOperation( string format, params object[] parameters )
        {
            return new InvalidOperationException(string.Format( CultureInfo.CurrentCulture, format, parameters ));
        }
    }
}