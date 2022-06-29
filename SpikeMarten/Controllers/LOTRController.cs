using Marten;
using Microsoft.AspNetCore.Mvc;
using SpikeMarten.Domain;
using SpikeMarten.Models;

namespace SpikeMarten.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LOTRController: BaseController
    {
        [HttpGet("{questId}")]
        public async Task<QuestParty> StartQuest(Guid questId,
            [FromServices] IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                return session.Events.AggregateStream<QuestParty>(questId);
            }
        }

        [HttpPost("StartQuest")]
        public async Task<QuestParty> StartQuest(
            [FromBody]StartQuestModel model,
            [FromServices]IDocumentStore store)
        {
            var id = Guid.NewGuid();
            using (var session = store.OpenSession())
            {
                var started = new QuestStarted { Name = model.QuestName }; // "Destroy the One Ring" };
                var joined1 = new MembersJoined(1, model.InitialMembers.Location, model.InitialMembers.Members); //"Hobbiton", "Frodo", "Sam");

                // Start a brand new stream and commit the new events as
                // part of a transaction
                session.Events.StartStream<QuestParty>(id, started, joined1);

                //// Append more events to the same stream
                //var joined2 = new MembersJoined(3, "Buckland", "Merry", "Pippen");
                //var joined3 = new MembersJoined(10, "Bree", "Aragorn");
                //var arrived = new ArrivedAtLocation { Day = 15, Location = "Rivendell" };
                //session.Events.Append(questId, joined2, joined3, arrived);

                // Save the pending changes to db
                await session.SaveChangesAsync();

                return session.Events.AggregateStream<QuestParty>(id);
            }
        }

        [HttpPost("JoinQuest/{questId}")]
        public async Task<IActionResult> JoinQuest(Guid questId,
            [FromBody] JoinQuestModel model,
            [FromServices] IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                session.Events.AggregateStream<QuestParty>(questId);

                //TODO: logic here to check if member is already joined
                //TODO: other business logic...member limit exceeded, etc.

                session.Events.Append(questId, new MembersJoined()
                {
                    Day = model.Day,
                    Location = model.Location,
                    Members = model.Members
                });
                await session.SaveChangesAsync();
                return new OkResult();
            }
        }
    }
}
