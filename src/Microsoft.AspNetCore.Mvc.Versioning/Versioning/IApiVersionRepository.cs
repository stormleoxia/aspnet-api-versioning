namespace Microsoft.AspNetCore.Mvc.Versioning
{
    /// <summary>
    /// Abstraction over repository of supported api versions.
    /// </summary>
    public interface IApiVersionRepository
    {
        /// <summary>
        /// Adds the specified API version to supported versions.
        /// </summary>
        /// <param name="apiVersion">The API version.</param>
        void Add( ApiVersion apiVersion );

        /// <summary>
        /// Determines whether the repository supports the specified API version.
        /// </summary>
        /// <param name="apiVersion">The API version.</param>
        /// <returns>
        ///   <c>true</c> if the repository supports the specified API version; otherwise, <c>false</c>.
        /// </returns>
        bool IsSupportedVersion( ApiVersion apiVersion );
    }
}