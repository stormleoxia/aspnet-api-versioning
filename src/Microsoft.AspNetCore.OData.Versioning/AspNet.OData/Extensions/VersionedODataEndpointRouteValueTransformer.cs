namespace Microsoft.AspNet.OData.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.OData;

    /// <summary>
    /// Versioned Endpoint route ValueTransformer.
    /// </summary>
    /// <seealso cref="Microsoft.AspNet.OData.Extensions.ODataEndpointRouteValueTransformer" />
    [CLSCompliant(false)]
    public class VersionedODataEndpointRouteValueTransformer : DynamicRouteValueTransformer
    {
        private readonly IActionSelector selector;
        private readonly IApiVersionRepository apiVersionRepository;

        private static readonly ValueTask<RouteValueDictionary> Empty =
            new ValueTask<RouteValueDictionary>( new RouteValueDictionary() );

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataEndpointRouteValueTransformer" /> class.
        /// </summary>
        /// <param name="selector">The injected IActionSelector.</param>
        /// <param name="apiVersionRepository">The API version repository.</param>
        public VersionedODataEndpointRouteValueTransformer(
            IActionSelector selector,
            IApiVersionRepository apiVersionRepository)
        {
            this.selector = selector;
            this.apiVersionRepository = apiVersionRepository;
        }

        /// <inheritdoc />
        public override ValueTask<RouteValueDictionary> TransformAsync( HttpContext httpContext, RouteValueDictionary values )
        {
            if ( httpContext == null )
            {
                throw new ArgumentNullException( nameof(httpContext) );
            }

            if ( values == null )
            {
                throw new ArgumentNullException( nameof( values ) );
            }

            if ( httpContext.ODataFeature().Path != null )
            {
                return Empty;
            }

            var (routeName, routePath) = values.GetODataRouteInfo();
            if ( routeName != null )
            {
                var request = httpContext.Request;
                var apiVersion = GetApiVersion( httpContext );
                if ( apiVersion == null )
                {
                    if ( !TryDefaultApiVersion( httpContext, values, out apiVersion ) )
                    {
                        // BadRequest because client should have specified api version
                        return Empty;
                    }
                }

                var versionedRouteName =
                    GetVersionedRouteName( httpContext, routeName, apiVersion );
                Func<IServiceProvider> providerFunc =
                    () => request.CreateRequestContainer( versionedRouteName );
                var leftPart = new Uri( request.GetEncodedUrl() ).GetLeftPart( UriPartial.Path );
                var queryString = GetQueryString( request );
                var odataPath = ODataPathRouteConstraintHelper.GetODataPath(
                    routePath as string,
                    leftPart,
                    queryString,
                    providerFunc );
                if ( odataPath != null )
                {
                    return RouteToODataController(
                        httpContext,
                        values,
                        odataPath,
                        versionedRouteName,
                        routePath );
                }

                request.DeleteRequestContainer( true );
            }

            return Empty;
        }

        private bool IsSupportedVersion( ApiVersion apiVersion )
        {
            return apiVersionRepository.IsSupportedVersion( apiVersion );
        }

        private string GetVersionedRouteName( HttpContext httpContext, string routeName,  ApiVersion apiVersion )
        {
            var versioningFeature = httpContext.ODataVersioningFeature();

            // if an api version is selected, determine if it corresponds to a route that has been previously matched
            if ( !versioningFeature.MatchingRoutes.TryGetValue( apiVersion, out var versionedRouteName ) )
            {
                versionedRouteName = routeName + "-" + apiVersion;
                versioningFeature.MatchingRoutes[apiVersion] = versionedRouteName;
            }

            var odataFeature = httpContext.Features.Get<IODataFeature>();
            odataFeature.RouteName = versionedRouteName;
            return versionedRouteName;
        }

        private bool TryDefaultApiVersion(
            HttpContext httpContext,
            RouteValueDictionary values,
            out ApiVersion apiVersion )
        {
            var options = httpContext.RequestServices.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

            // is implicitly matching an api version allowed?
            if ( options.AssumeDefaultVersionWhenUnspecified ||
                 IsServiceDocumentOrMetadataRoute( values ) )
            {
                var odata = httpContext.ODataVersioningFeature();
                var model = new ApiVersionModel( odata.MatchingRoutes.Keys, Array.Empty<ApiVersion>() );
                var selector = httpContext.RequestServices.GetRequiredService<IApiVersionSelector>();
                var feature = httpContext.Features.Get<IApiVersioningFeature>();
                apiVersion = feature.RequestedApiVersion = selector.SelectVersion( httpContext.Request, model );
                return true;
            }

            apiVersion = ApiVersion.Default;
            return false;
        }

        static bool IsServiceDocumentOrMetadataRoute( RouteValueDictionary values ) =>
            values.TryGetValue( "odataPath", out var value ) && ( value == null || Equals( value, "$metadata" ) );

        private ApiVersion? GetApiVersion( HttpContext httpContext )
        {
            var feature = httpContext.Features.Get<IApiVersioningFeature>();
            try
            {
                return feature.RequestedApiVersion;
            }
            catch ( AmbiguousApiVersionException )
            {
                return null;
            }
        }

        private ValueTask<RouteValueDictionary> RouteToODataController(
            HttpContext httpContext,
            RouteValueDictionary values,
            ODataPath odataPath,
            string versionedRouteName,
            object? routePath)
        {
            var odataFeature = httpContext.ODataFeature();
            odataFeature.Path = odataPath;
            odataFeature.RouteName = versionedRouteName;
            odataFeature.IsEndpointRouting = true;
            var context = new RouteContext(httpContext);
            var candidates = selector.SelectCandidates(context);
            if (selector.SelectBestCandidate(context, candidates) is
                ControllerActionDescriptor actionDescriptor)
            {
                var result = new RouteValueDictionary();
                foreach (var keyValuePair in values)
                {
                    result.Add(keyValuePair.Key, keyValuePair.Value);
                }

                foreach (var keyValuePair in context.RouteData.Values)
                {
                    result[keyValuePair.Key] = keyValuePair.Value;
                }

                result["controller"] = (object) actionDescriptor.ControllerName;
                result["action"] = (object) actionDescriptor.ActionName;
                result["odataPath"] = routePath;
                odataFeature.ActionDescriptor = (ActionDescriptor) actionDescriptor;
                return new ValueTask<RouteValueDictionary>(result);
            }

            return Empty;
        }

        private static string? GetQueryString(HttpRequest request)
        {
            var queryString = request.QueryString;
            return queryString.HasValue ? queryString.ToString() : null;
        }
    }
}