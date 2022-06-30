﻿using Microsoft.AspNetCore.SignalR;

namespace BlazorApp.Hubs
{
    public class QuestHub : Hub
    {
        public async Task ApplyQuestEvent(Guid questId, string eventType, string eventPayload)
        {
            await Clients.All.SendAsync("ApplyQuestEvent", questId, eventType, eventPayload);
        }
    }
}
