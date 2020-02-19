using System.Collections.Generic;

namespace Microsoft.Examples.Data
{
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Resource repository.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    /// <seealso cref="Microsoft.Examples.Data.IResourceRepository{TResource}" />
    public class ResourceRepository<TResource> : IResourceRepository<TResource> where TResource : class
    {
        private readonly List<TResource> resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRepository{TResource}"/> class.
        /// </summary>
        public ResourceRepository()
        {
            resources = new List<TResource>();
        }

        /// <summary>
        /// Adds the specified resource in the repository.
        /// </summary>
        /// <param name="resource">The resource.</param>
        public void Add( TResource resource )
        {
            resources.Add( resource );
        }

        /// <summary>
        /// Adds the resources in the repository.
        /// </summary>
        /// <param name="newResources">The new resources.</param>
        public void AddRange( IEnumerable<TResource> newResources )
        {
            resources.AddRange( newResources );
        }

        /// <inheritdoc />
        public Task<IQueryable<TResource>> GetResources()
        {
            return Task.FromResult( resources.AsQueryable() );
        }
    }
}