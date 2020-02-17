namespace Microsoft.AspNet.OData.Extensions
{
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Routing;
    using System;

    /// <summary>
    /// OData Route for endpoint routing.
    /// </summary>
    /// <seealso cref="Microsoft.AspNet.OData.Routing.IODataRoute" />
    [CLSCompliant(false)]
    public class EndpointODataRoute : IODataRoute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointODataRoute" /> class.
        /// </summary>
        /// <param name="routeName">Name of the route.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="constraint">The constraint.</param>
        /// <param name="resolver">The resolver.</param>
        public EndpointODataRoute( string routeName, string routePrefix, IRouteConstraint constraint, IInlineConstraintResolver resolver )
        {
            RouteName = routeName;
            RoutePrefix = routePrefix;
            Constraint = constraint;
            Resolver = resolver;
        }

        /// <summary>
        /// Gets the name of the route.
        /// </summary>
        /// <value>
        /// The name of the route.
        /// </value>
        public string RouteName { get; }

        /// <inheritdoc />
        public string RoutePrefix { get; }

        /// <summary>
        /// Gets the constraint.
        /// </summary>
        /// <value>
        /// The constraint.
        /// </value>
        public IRouteConstraint Constraint { get; }

        /// <summary>
        /// Gets the resolver.
        /// </summary>
        /// <value>
        /// The resolver.
        /// </value>
        public IInlineConstraintResolver Resolver { get; }
    }
}