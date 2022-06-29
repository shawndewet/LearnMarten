using Marten;
using Marten.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using SpikeMarten.Domain;
using SpikeMarten.Models;

namespace SpikeMarten.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IQuerySession _session;

        public UserController(IQuerySession session)
        {
            _session = session;
        }

        [HttpPost]
        public async Task<UserModel> CreateUser([FromBody] UserModel model,
            [FromServices] IDocumentStore store)
        {
            // Open a session for querying, loading, and
            // updating documents
            using (var session = store.LightweightSession())
            {
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Internal = model.Internal,
                    Department = model.Department,
                    UserName = model.UserName
                };
                session.Store(user);

                await session.SaveChangesAsync();
                model.Id = user.Id;
                return model;
            }
        }

        [HttpGet()]
        public async Task<List<User>> GetUsers(Guid userId)
        {
            return (List<User>)await _session
                .Query<User>()
                .ToListAsync();

        }

        [HttpGet("load/{userId}")]
        public Task<User> Get(Guid userId) => _session.LoadAsync<User>(userId); //returns 204 if not found


        [HttpGet("fastload/{userId}")]
        public Task GetFast(Guid userId) => _session.Json.WriteById<User>(userId, HttpContext); //returns 404 if not found

        [HttpGet("query/{userId}")]
        public async Task<User> Query(Guid userId)
        {
            return await _session
                .Query<User>()
                .SingleOrDefaultAsync(x => x.Id == userId);

        }
    }
}
