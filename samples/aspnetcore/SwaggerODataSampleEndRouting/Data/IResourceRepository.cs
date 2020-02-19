using System.Threading.Tasks;

namespace Microsoft.Examples.Data
{
    using System.Linq;

    /// <summary>
    /// Abstraction for repository of TResource.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    public interface IResourceRepository<TResource>
        where TResource : class
    {
        /// <summary>
        /// Gets the resources.
        /// </summary>
        /// <returns>the resources.</returns>
        Task<IQueryable<TResource>> GetResources();
    }
}
