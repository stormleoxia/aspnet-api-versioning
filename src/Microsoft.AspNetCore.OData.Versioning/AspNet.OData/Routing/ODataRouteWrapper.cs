namespace Microsoft.AspNet.OData.Routing
{
    using System;

    /// <summary>
    /// Adapter for ODataRoute to IODataRoute.
    /// </summary>
    /// <seealso cref="Microsoft.AspNet.OData.Routing.IODataRoute" />
    [CLSCompliant(false)]
    public class ODataRouteWrapper : IODataRoute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteWrapper"/> class.
        /// </summary>
        /// <param name="route">The route.</param>
        public ODataRouteWrapper( ODataRoute route )
        {
            if ( route == null )
            {
                throw new ArgumentNullException(nameof(route));
            }

            RoutePrefix = route.RoutePrefix;
        }

        /// <inheritdoc />
        public string RoutePrefix { get; }
    }
}