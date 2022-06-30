using Microsoft.AspNetCore.SignalR;

namespace BlazorApp.Hubs
{
    public class QuestHub : Hub
    {
        public async Task ApplyQuestEvent(Guid questId, string eventContent)
        {
            await Clients.All.SendAsync("ApplyQuestEvent", questId, eventContent);
        }
    }
}
