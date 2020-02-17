namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Repository for all supported api versions.
    /// </summary>
    public class ApiVersionRepository : IApiVersionRepository
    {
        private readonly IDictionary<string, ApiVersion> supportedVersions =
            new Dictionary<string, ApiVersion>();

        /// <inheritdoc />
        public void Add( ApiVersion apiVersion )
        {
            if ( apiVersion == null )
            {
                throw new ArgumentNullException( nameof(apiVersion) );
            }

            supportedVersions[apiVersion.ToString()] = apiVersion;
        }

        /// <inheritdoc />
        public bool IsSupportedVersion( ApiVersion apiVersion )
        {
            if ( apiVersion == null )
            {
                throw new ArgumentNullException( nameof( apiVersion ) );
            }

            return supportedVersions.ContainsKey( apiVersion.ToString() );
        }
    }
}