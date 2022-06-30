using Marten;
using Microsoft.AspNetCore.Mvc;
using LOTRShared.Domain;
using LOTRShared.Models;

namespace SpikeMarten.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LOTRController : BaseController
    {
        [HttpGet]
        public async Task<List<Quest>> ReadQuests(
            [FromServices] IDocumentStore store)
        {
            using (var session = store.QuerySession())
            {
                return (List<Quest>)await session
                    .Query<Quest>()
                    .ToListAsync();
            }
        }

        [HttpGet("{questId}")]
        public async Task<Quest> ReadQuest(Guid questId,
            [FromServices] IDocumentStore store)
        {
            using (var session = store.QuerySession())
            {
                return session.Events.AggregateStream<Quest>(questId);
            }
        }

        [HttpPost("StartQuest")]
        public async Task<Quest> StartQuest(
            [FromBody] StartQuestModel model,
            [FromServices] IDocumentStore store)
        {
            var id = Guid.NewGuid();
            using (var session = store.OpenSession())
            {
                var started = new QuestStarted { Name = model.QuestName, Location = model.Location, Id = id };
                var joined1 = new MembersJoined(id, 1, model.Location, model.InitialMembers.Members);

                session.Events.StartStream<Quest>(id, started, joined1);

                await session.SaveChangesAsync();

                //return session.Events.AggregateStream<Quest>(id); //not sure if this is correct...feels inefficient to be querying what was just sent to the db
                return await session.LoadAsync<Quest>(id); //still not sure if this is correct...feels inefficient to be querying what was just sent to the db
            }
        }

        [HttpPost("JoinQuest/{questId}")]
        public async Task<IActionResult> JoinQuest(Guid questId,
            [FromBody] JoinQuestModel model,
            [FromServices] IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                var quest = await session.LoadAsync<Quest>(questId);// await session.Events.AggregateStreamAsync<Quest>(questId);

                //logic here to ensure member is not already joined
                foreach (var member in model.Members)
                    if (quest.Members.Contains(member))
                        return new BadRequestObjectResult($"Member {member} already joined.");

                if (model.Day < quest.DaysIn)
                    return new BadRequestObjectResult($"Cannot join earlier than DaysIn ({quest.DaysIn})");

                //TODO: other business logic...member limit exceeded, etc.

                session.Events.Append(questId, new MembersJoined(
                    questId,
                    model.Day,
                    quest.Location,
                    model.Members
                ));
                await session.SaveChangesAsync();
                return new OkResult();
            }
        }

        [HttpPost("LeaveQuest/{questId}")]
        public async Task<IActionResult> LeaveQuest(Guid questId,
            [FromBody] LeaveQuestModel model,
            [FromServices] IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                var quest = await session.LoadAsync<Quest>(questId);// await session.Events.AggregateStreamAsync<Quest>(questId);

                foreach (var member in model.Members)
                    if (!quest.Members.Contains(member))
                        return new BadRequestObjectResult($"Member {member} who never joined cannot leave.");

                if (model.Day < quest.DaysIn)
                    return new BadRequestObjectResult($"Cannot leave earlier than DaysIn ({quest.DaysIn})");

                session.Events.Append(questId, new MembersDeparted
                {
                    QuestId = questId,
                    Day = model.Day,
                    Location = quest.Location,
                    Members = model.Members
                });

                await session.SaveChangesAsync();
                return new OkResult();
            }
        }

        [HttpPost("Arrive/{questId}")]
        public async Task<IActionResult> Arrive(Guid questId,
            [FromBody] ArriveAtLocationModel model,
            [FromServices] IDocumentStore store)
        {
            if (string.IsNullOrEmpty(model.Location))
                return new BadRequestObjectResult("Location must be specified");

            using (var session = store.OpenSession())
            {
                var quest = await session.LoadAsync<Quest>(questId);// await session.Events.AggregateStreamAsync<Quest>(questId);

                if (quest.Location == model.Location)
                    return new BadRequestObjectResult($"Quest is already at {model.Location}");

                if (model.Day < quest.DaysIn)
                    return new BadRequestObjectResult($"Cannot arrive earlier than DaysIn ({quest.DaysIn})");

                //TODO: call other service to calculate distance travelled

                session.Events.Append(questId, new ArrivedAtLocation
                {
                    QuestId = questId,
                    Day = model.Day,
                    Location = model.Location
                });

                await session.SaveChangesAsync();
                return new OkResult();
            }
        }

        [HttpPost("Slay/{questId}")]
        public async Task<IActionResult> Slay(Guid questId,
            [FromBody] SlayCharactersModel model,
            [FromServices] IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                var quest = await session.LoadAsync<Quest>(questId);// await session.Events.AggregateStreamAsync<Quest>(questId);

                foreach (var character in model.Characters)
                    if (quest.Slayed.Contains(character))
                        return new BadRequestObjectResult($"{character} has already been slayed.");

                if (model.Day < quest.DaysIn)
                    return new BadRequestObjectResult($"Cannot slay earlier than DaysIn ({quest.DaysIn})");

                session.Events.Append(questId, new CharactersSlayed
                {
                    QuestId = questId,
                    Day = model.Day,
                    Location = quest.Location,
                    Characters = model.Characters
                });

                await session.SaveChangesAsync();
                return new OkResult();
            }
        }
    }
}
