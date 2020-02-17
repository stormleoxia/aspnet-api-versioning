namespace Microsoft.AspNet.OData.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.AspNetCore.Routing;

    internal static class ODataEndpointPattern
    {
        /// <summary>
        /// Wildcard route template for the OData Endpoint route pattern.
        /// </summary>
        public const string ODataEndpointPath = "ODataEndpointPath_";

        /// <summary>
        /// Wildcard route template for the OData path route variable.
        /// </summary>
        public const string ODataEndpointTemplate = "{{**" + ODataEndpointPath + "{0}}}";

        /// <summary>
        /// Create an OData Endpoint route pattern.
        /// The route pattern is in this format: "routePrefix/{*ODataEndpointPath_routeName}".
        /// </summary>
        /// <param name="routeName">The route name. It can not be null and verify upper layer.</param>
        /// <param name="routePrefix">The route prefix. It could be null or empty.</param>
        /// <returns>The OData route endpoint pattern.</returns>
        public static string CreateODataEndpointPattern( string routeName, string routePrefix )
        {
            return !string.IsNullOrEmpty( routePrefix ) ?
                routePrefix + "/" +
                string.Format( CultureInfo.InvariantCulture,  ODataEndpointTemplate, routeName ) :
                string.Format( CultureInfo.InvariantCulture, ODataEndpointTemplate, routeName );
        }

        /// <summary>Get the OData route name and path value.</summary>
        /// <param name="values">The dictionary contains route value.</param>
        /// <returns>A tuple contains the route name and path value.</returns>
        public static (string Path, object? Value) GetODataRouteInfo( this RouteValueDictionary values )
        {
            var str = string.Empty;
            object? obj = null;
            foreach ( var keyValuePair in values )
            {
                var key = keyValuePair.Key;
                if ( key.StartsWith( ODataEndpointPath, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    str = key.Substring( ODataEndpointPath.Length );
                    obj = keyValuePair.Value;
                    break;
                }
            }

            return (str, obj);
        }
    }
}