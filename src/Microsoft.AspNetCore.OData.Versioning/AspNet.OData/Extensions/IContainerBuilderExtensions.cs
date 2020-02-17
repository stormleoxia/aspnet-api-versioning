namespace Microsoft.AspNet.OData.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.OData;

    /// <summary>
    /// Extensions for <see cref="IContainerBuilder"/>.
    /// </summary>
    public static class IContainerBuilderExtensions
    {
        /// <summary>
        /// Adds a singleton registration into the container.
        /// </summary>
        /// <typeparam name="T">type to register.</typeparam>
        /// <param name="builder">The container builder.</param>
        /// <param name="factoryFunc">The factory function.</param>
        /// <returns>the container builder.</returns>
        public static IContainerBuilder AddSingleton<T>(this IContainerBuilder builder, Func<IServiceProvider, T> factoryFunc)
            where T : class
        {
            builder.AddService( ServiceLifetime.Singleton, factoryFunc );
            return builder;
        }
    }
}