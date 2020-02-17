namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Interface for replacement of ODataRoute in case of Endpoint routing.
    /// </summary>
    public interface IODataRoute
    {
        /// <summary>
        /// Gets the route prefix.
        /// </summary>
        /// <value>
        /// The route prefix.
        /// </value>
        string RoutePrefix { get; }
    }
}