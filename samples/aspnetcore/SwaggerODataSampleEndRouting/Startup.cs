﻿using System.Linq;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Examples.Data;
using Microsoft.Examples.Models;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;

namespace Microsoft.Examples
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.PlatformAbstractions;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.IO;
    using System.Reflection;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
    using static Microsoft.AspNetCore.Mvc.CompatibilityVersion;
    using static Microsoft.OData.ODataUrlKeyDelimiter;

    /// <summary>
    /// Represents the startup process for the application.
    /// </summary>
    public class Startup
    {
        private const string ODataMediaType = "application/prs.odatatestxx-odata";

        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="services">The collection of services to configure the application with.</param>
        public void ConfigureServices( IServiceCollection services )
        {
            var repository = new ResourceRepository<Person>();
            repository.AddRange( new[]
            {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@somewhere.com",
                    Phone = "555-987-1234",
                },
                new Person
                {
                    Id = 2,
                    FirstName = "Bob",
                    LastName = "Smith",
                    Email = "bob.smith@somewhere.com",
                    Phone = "555-654-4321",
                },
                new Person
                {
                    Id = 3,
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@somewhere.com",
                    Phone = "555-789-3456",
                }
            } );
            services.AddSingleton<IResourceRepository<Person>>( sp => repository );

            // the sample application always uses the latest version, but you may want an explicit version such as Version_2_2
            // note: Endpoint Routing is enabled by default; however, it is unsupported by OData and MUST be false
            services.AddMvc( options =>
            {
                options.EnableEndpointRouting = false;
                foreach ( var outputFormatter in options.OutputFormatters.OfType<ODataOutputFormatter>().Where( _ => _.SupportedMediaTypes.Count == 0 ) )
                {
                    outputFormatter.SupportedMediaTypes.Add( new MediaTypeHeaderValue( ODataMediaType ) );
                }
                foreach ( var inputFormatter in options.InputFormatters.OfType<ODataInputFormatter>().Where( _ => _.SupportedMediaTypes.Count == 0 ) )
                {
                    inputFormatter.SupportedMediaTypes.Add( new MediaTypeHeaderValue( ODataMediaType ) );
                }

            } ).SetCompatibilityVersion( Latest );
            services.AddApiVersioning( options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.DefaultApiVersion = ApiVersion.Parse( "3.0" );
            } );
            services.AddOData().EnableApiVersioning();
            services.AddODataApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;

                    options.AssumeDefaultVersionWhenUnspecified = true;

                    options.DefaultApiVersion = ApiVersion.Parse( "3.0" );

                    // configure query options (which cannot otherwise be configured by OData conventions)
                    options.QueryOptions.Controller<V2.PeopleController>()
                                        .Action( c => c.Get( default ) )
                                            .Allow( Skip | Count )
                                            .AllowTop( 100 )
                                            .AllowOrderBy( "firstName", "lastName" );

                    options.QueryOptions.Controller<V3.PeopleController>()
                                        .Action( c => c.Get( default ) )
                                            .Allow( Skip | Count )
                                            .AllowTop( 100 )
                                            .AllowOrderBy( "firstName", "lastName" );
                } );
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(
                options =>
                {
                    // add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    // integrate xml comments
                    options.IncludeXmlComments( XmlCommentsFilePath );
                } );
        }

        /// <summary>
        /// Configures the application using the provided builder, hosting environment, and logging factory.
        /// </summary>
        /// <param name="app">The current application builder.</param>
        /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builder</see> used to create OData entity data models (EDMs).</param>
        /// <param name="provider">The API version descriptor provider used to enumerate defined API versions.</param>
        public void Configure( IApplicationBuilder app, VersionedODataModelBuilder modelBuilder, IApiVersionDescriptionProvider provider )
        {
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    // build a swagger endpoint for each discovered API version
                    foreach ( var description in provider.ApiVersionDescriptions )
                    {
                        options.SwaggerEndpoint( $"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant() );
                    }
                } );
            app.UseRouting();
            app.UseEndpoints( endpoints =>
            {
                // the following will not work as expected
                // BUG: https://github.com/OData/WebApi/issues/1837
                // routeBuilder.SetDefaultODataOptions( new ODataOptions() { UrlKeyDelimiter = Parentheses } );
                endpoints.ServiceProvider.GetRequiredService<ODataOptions>().UrlKeyDelimiter = Parentheses;

                // global odata query options
                endpoints.Count();
                //routeBuilder.MapODataServiceRoute(  )
                endpoints.MapVersionedODataRoutes( "odata", "api", modelBuilder.GetEdmModels() );
            } );
        }

        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var fileName = typeof( Startup ).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine( basePath, fileName );
            }
        }
    }
}