namespace Microsoft.Examples.V3
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Examples.Data;
    using Microsoft.Examples.Models;

    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
    using static Microsoft.AspNetCore.Http.StatusCodes;


    /// <summary>
    /// Represents RESTful service providing users
    /// </summary>
    /// <seealso cref="Microsoft.AspNet.OData.ODataController" />
    [ApiVersion("3.0")]
    [ODataRoutePrefix( "Users" )]
    [Produces( "application/json" )]
    public class UsersController : ODataController
    {
        private readonly IResourceRepository<Person> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public UsersController(IResourceRepository<Person> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>All available users.</returns>
        /// <response code="200">Users successfully retrieved.</response>
        /// <response code="400">The order is invalid.</response>
        [ProducesResponseType( typeof( ODataValue<IEnumerable<Person>> ), Status200OK )]
        [EnableQuery( MaxTop = 100, AllowedQueryOptions = Select | Top | Skip | Count )]
        public IQueryable<Person> Get()
        {
            return _repository.GetResources().Result;
        }

        /// <summary>
        /// Gets a single user.
        /// </summary>
        /// <param name="key">The requested user identifier.</param>
        /// <returns>The requested user.</returns>
        /// <response code="200">The user was successfully retrieved.</response>
        /// <response code="404">The user does not exist.</response>
        [ODataRoute( "({key})" )]
        [ProducesResponseType( typeof( Person ), Status200OK )]
        [ProducesResponseType( Status404NotFound )]
        [EnableQuery( AllowedQueryOptions = Select )]
        public SingleResult<Person> Get( [Required]long key )
        {
            return SingleResult.Create( _repository.GetResources().Result.Where( x => x.Id == key ) );
        }
    }
}
