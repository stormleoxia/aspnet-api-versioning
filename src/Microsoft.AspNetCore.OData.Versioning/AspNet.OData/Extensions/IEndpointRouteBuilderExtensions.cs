namespace Microsoft.AspNet.OData.Extensions
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Microsoft.OData.ServiceLifetime;
    using static System.Reflection.BindingFlags;
    using static System.String;

    /// <summary>
    /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add versioned OData routes.
    /// </summary>
    [CLSCompliant( false )]
    public static class IEndpointRouteBuilderExtensions
    {
        static readonly Func<IEndpointRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>> configureDefaultServicesFunc = ResolveConfigureDefaultServicesFunc();

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IEndpointRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IEndpointRouteBuilder MapVersionedODataRoutes(
            this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder> configureAction ) =>
            MapVersionedODataRoutes( builder, routeName, routePrefix, models, configureAction, default );

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IEndpointRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <param name="configureRoutingConventions">The configuring action to add or update routing conventions.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IEndpointRouteBuilder MapVersionedODataRoutes(
            this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            Action<IContainerBuilder>? configureAction,
            Action<ODataConventionConfigurationContext>? configureRoutingConventions )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IEndpointRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IEndpointRouteBuilder MapVersionedODataRoutes( this IEndpointRouteBuilder builder, string routeName, string routePrefix, IEnumerable<IEdmModel> models ) =>
            MapVersionedODataRoutes(
                builder,
                routeName,
                routePrefix,
                models,
                new DefaultODataPathHandler(),
                VersionedODataRoutingConventions.CreateDefault(),
                default );

        /// <summary>
        /// Maps the specified versioned OData routes. When the <paramref name="newBatchHandler"/> is provided, it will create a '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The extended <see cref="IRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler">OData path handler</see> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IODataRoutingConvention">OData routing conventions</see>
        /// to use for controller and action selection.</param>
        /// <param name="newBatchHandler">The <see cref="Func{TResult}">factory method</see> used to create new <see cref="ODataBatchHandler">OData batch handlers</see>.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IEndpointRouteBuilder MapVersionedODataRoutes(
            this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions,
            Func<ODataBatchHandler>? newBatchHandler )
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            if ( models == null )
            {
                throw new ArgumentNullException( nameof( models ) );
            }

            var serviceProvider = builder.ServiceProvider;
            var options = serviceProvider.GetRequiredService<ODataOptions>();
            var routeCollection = serviceProvider.GetRequiredService<IODataRouteCollectionProvider>();
            var inlineConstraintResolver = serviceProvider.GetRequiredService<IInlineConstraintResolver>();
            var routeConventions = VersionedODataRoutingConventions.AddOrUpdate( routingConventions.ToList() );
            var unversionedConstraints = new List<IRouteConstraint>();

            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }

            var modelList = models.ToList();

            var apiVersionRepository = serviceProvider.GetRequiredService<IApiVersionRepository>();
            foreach ( var model in modelList )
            {
                var annotation = model.GetAnnotationValue<ApiVersionAnnotation>( model ) ??
                                 throw new ArgumentException( typeof(ApiVersionAnnotation).Name );
                var apiVersion = annotation.ApiVersion;
                apiVersionRepository.Add(apiVersion);
            }

            foreach ( var model in modelList )
            {
                var perRouteContainer = serviceProvider.GetRequiredService<IPerRouteContainer>();

                var versionedRouteName = routeName;
                var annotation = model.GetAnnotationValue<ApiVersionAnnotation>( model ) ?? throw new ArgumentException( typeof( ApiVersionAnnotation ).Name );
                var apiVersion = annotation.ApiVersion;
                var routeConstraint = MakeVersionedODataRouteConstraint( apiVersion, ref versionedRouteName );

                IEnumerable<IODataRoutingConvention> NewRouteConventions( IServiceProvider services )
                {
                    var conventions = new IODataRoutingConvention[routeConventions.Count + 1];
                    conventions[0] = new VersionedAttributeRoutingConvention( versionedRouteName, serviceProvider, apiVersion );
                    routeConventions.CopyTo( conventions, 1 );
                    return conventions;
                }

                var edm = model;
                var createdBatchHandler = newBatchHandler?.Invoke();

                var configureAction = builder.ConfigureDefaultServices( container =>
                    container.AddService( Singleton, typeof( IEdmModel ), sp => edm )
                             .AddService( Singleton, typeof( IODataPathHandler ), sp => pathHandler )
                             .AddService( Singleton, typeof( IEnumerable<IODataRoutingConvention> ), NewRouteConventions )
                             .AddService( Singleton, typeof( ODataBatchHandler ), sp => createdBatchHandler )
                             .AddService( Singleton, typeof( ApiVersion ), sp => apiVersion ));

                routePrefix = RemoveTrailingSlash( routePrefix );
                unversionedConstraints.Add( new ODataPathRouteConstraint( versionedRouteName ) );

                // MapOData
                var applicationPartManager = serviceProvider.GetRequiredService<ApplicationPartManager>();
                applicationPartManager.ApplicationParts.Add( new AssemblyPart( typeof( MetadataController ).Assembly ) );
                var odataRootContainer = perRouteContainer.CreateODataRootContainer( versionedRouteName, configureAction );
                SetUrlKeyDelimiter( odataRootContainer, serviceProvider );

                routePrefix = RemoveTrailingSlash( routePrefix );
                var batchHandler = odataRootContainer.GetService<ODataBatchHandler>();
                if ( batchHandler != null )
                {
                    batchHandler.ODataRouteName = versionedRouteName;
                    var routeTemplate = string.IsNullOrEmpty( routePrefix ) ? "/" + ODataRouteConstants.Batch : "/" + routePrefix + "/" + ODataRouteConstants.Batch;
                    builder.ServiceProvider.GetRequiredService<ODataBatchPathMapping>().AddRoute( versionedRouteName, routeTemplate );
                }

                var route = new EndpointODataRoute( versionedRouteName, routePrefix, routeConstraint, inlineConstraintResolver );

                routeCollection.Add( new ODataRouteMapping(route, apiVersion, odataRootContainer ) );
                perRouteContainer.AddRoute( versionedRouteName, routePrefix );
            }

            // Can map only once for a given route prefix
            builder.MapDynamicControllerRoute<VersionedODataEndpointRouteValueTransformer>(
                ODataEndpointPattern.CreateODataEndpointPattern( routeName, routePrefix ) );
            NotifyRoutesMapped();
            return builder;
        }

        private static void SetUrlKeyDelimiter(
            IServiceProvider odataRootContainer,
            IServiceProvider serviceProvider )
        {
            var dataPathHandler = odataRootContainer.GetRequiredService<IODataPathHandler>();
            var dataOptions = serviceProvider.GetRequiredService<ODataOptions>();
            if ( dataPathHandler != null && dataPathHandler.UrlKeyDelimiter == null )
            {
                dataPathHandler.UrlKeyDelimiter = dataOptions.UrlKeyDelimiter;
            }
        }

        /// <summary>
        /// Maps the specified versioned OData routes. When the <paramref name="newBatchHandler"/> is provided, it will create a
        /// '$batch' endpoint to handle the batch requests.
        /// </summary>
        /// <param name="builder">The extended <see cref="IEndpointRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="newBatchHandler">The <see cref="Func{TResult}">factory method</see> used to create new <see cref="ODataBatchHandler">OData batch handlers</see>.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IEndpointRouteBuilder MapVersionedODataRoutes(
        this IEndpointRouteBuilder builder,
        string routeName,
        string routePrefix,
        IEnumerable<IEdmModel> models,
        Func<ODataBatchHandler>? newBatchHandler ) =>
        MapVersionedODataRoutes( builder, routeName, routePrefix, models, new DefaultODataPathHandler(), VersionedODataRoutingConventions.CreateDefault(), newBatchHandler );

        /// <summary>
        /// Maps the specified versioned OData routes.
        /// </summary>
        /// <param name="builder">The extended <see cref="IEndpointRouteBuilder">route builder</see>.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler">OData path handler</see> to use for parsing the OData path.</param>
        /// <param name="routingConventions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IODataRoutingConvention">OData routing conventions</see>
        /// to use for controller and action selection.</param>
        /// <returns>The <see cref="IReadOnlyList{T}">read-only list</see> of added <see cref="ODataRoute">OData routes</see>.</returns>
        /// <remarks>The specified <paramref name="models"/> must contain the <see cref="ApiVersionAnnotation">API version annotation</see>.  This annotation is
        /// automatically applied when you use the <see cref="VersionedODataModelBuilder"/> and call <see cref="VersionedODataModelBuilder.GetEdmModels"/> to
        /// create the <paramref name="models"/>.</remarks>
        public static IEndpointRouteBuilder MapVersionedODataRoutes(
            this IEndpointRouteBuilder builder,
            string routeName,
            string routePrefix,
            IEnumerable<IEdmModel> models,
            IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions )
        {
            var configureAction = (Action<IContainerBuilder>)( c =>
                {
                    c.AddSingleton( s => pathHandler );
                    c.AddSingleton( s => routingConventions );
                });
            return MapVersionedODataRoutes(
                builder,
                routeName,
                routePrefix,
                models,
                configureAction );
        }

        static Action<IContainerBuilder> ConfigureDefaultServices( this IEndpointRouteBuilder builder, Action<IContainerBuilder> configureAction ) => configureDefaultServicesFunc( builder, configureAction );

        static Func<IEndpointRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>> ResolveConfigureDefaultServicesFunc()
        {
            var method = typeof( ODataEndpointRouteBuilderExtensions )
                .GetMethod(
                    "ConfigureDefaultServices",
                    NonPublic | Static,
                    null,
                    new[] { typeof(IEndpointRouteBuilder ), typeof( Action<IContainerBuilder> ) },
                    null )!;
            return (Func<IEndpointRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>>) method.CreateDelegate( typeof( Func<IEndpointRouteBuilder, Action<IContainerBuilder>, Action<IContainerBuilder>> ) );
        }

        static void EnsureMetadataController( this IEndpointRouteBuilder builder )
        {
            var applicationPartManager = builder.ServiceProvider.GetRequiredService<ApplicationPartManager>();
            applicationPartManager.ApplicationParts.Add( new AssemblyPart( typeof( VersionedMetadataController ).Assembly ) );
        }

        static void ConfigurePathHandler( this IEndpointRouteBuilder builder, IServiceProvider rootContainer )
        {
            var options = builder.ServiceProvider.GetRequiredService<ODataOptions>();
            rootContainer.ConfigurePathHandler( options );
        }

        static void ConfigurePathHandler( this IServiceProvider rootContainer, ODataOptions options )
        {
            var pathHandler = rootContainer.GetRequiredService<IODataPathHandler>();

            if ( pathHandler != null && pathHandler.UrlKeyDelimiter == null )
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }
        }

        static void ConfigureBatchHandler( this IEndpointRouteBuilder builder, IServiceProvider rootContainer, ODataRoute route )
        {
            if ( rootContainer.GetService<ODataBatchHandler>() is ODataBatchHandler batchHandler )
            {
                batchHandler.Configure( builder, route );
            }
        }

        static void ConfigureBatchHandler( this IEndpointRouteBuilder builder, ODataBatchHandler? batchHandler, ODataRoute route ) => batchHandler?.Configure( builder, route );

        static void Configure( this ODataBatchHandler batchHandler, IEndpointRouteBuilder builder, ODataRoute route )
        {
            batchHandler.ODataRoute = route;
            batchHandler.ODataRouteName = route.Name;

            var batchPath = '/' + ODataRouteConstants.Batch;

            if ( !IsNullOrEmpty( route.RoutePrefix ) )
            {
                batchPath = '/' + route.RoutePrefix + batchPath;
            }

            var batchMapping = builder.ServiceProvider.GetRequiredService<ODataBatchPathMapping>();

            batchMapping.AddRoute( route.Name, batchPath );
        }

        static IRouteConstraint MakeVersionedODataRouteConstraint( ApiVersion apiVersion, ref string versionedRouteName )
        {
            if ( apiVersion == null )
            {
                return new ODataPathRouteConstraint( versionedRouteName );
            }

            versionedRouteName += "-" + apiVersion.ToString();
            return new VersionedODataPathRouteConstraint( versionedRouteName, apiVersion );
        }

        static string RemoveTrailingSlash( this string @string ) => IsNullOrEmpty( @string ) ? @string : @string.TrimEnd( '/' );

        // note: we don't have the required information necessary to build the odata route information
        // until one or more routes have been mapped. if anyone has subscribed changes, notify them now.
        static void NotifyRoutesMapped() => ODataActionDescriptorChangeProvider.Instance.NotifyChanged();
    }
}