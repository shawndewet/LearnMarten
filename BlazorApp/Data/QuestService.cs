using LOTRShared.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Data
{
    public class QuestService
    {
        private readonly IQuerySession _session;

        public QuestService(IQuerySession session)
        {
            _session = session;
        }

        public async Task<List<Quest>> GetQuestsAsync()
        {
                return (List<Quest>)await _session
                    .Query<Quest>()
                    .ToListAsync();
        }

        public async Task<Quest> GetQuestAsync(Guid questId)
        {
            return await _session.LoadAsync<Quest>(questId);
        }
    }
}