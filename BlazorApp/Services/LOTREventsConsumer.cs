using BlazorApp.Hubs;
using Marten;
using Marten.Events;
using Microsoft.AspNetCore.SignalR;

namespace BlazorApp.Services
{
    public interface IMartenEventsConsumer
    {
        Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions);
    }

    public class LOTREventsConsumer : IMartenEventsConsumer
    {
        private readonly ILogger<LOTREventsConsumer> _logger;
        private readonly IHubContext<QuestHub> _questHub;

        public static List<object> Events { get; } = new();

        public LOTREventsConsumer(ILogger<LOTREventsConsumer> logger, IHubContext<QuestHub> questHub)
        {
            _logger = logger;
            _questHub = questHub;
        }

        public async Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions)
        {
            /*SdW findings: this MUST be in a try...catch block so that exceptions don't kill the process.
                however, there is a whole section here about Error handling: https://martendb.io/events/projections/async-daemon.html#error-handling

            noticed this in the log output on the 2nd run: Marten.Events.Daemon.AsyncProjectionHostedService: Information: Projection agent for 'lotrConsumer:All' has started from sequence 3 and a high water mark of 3
            during the first run, 3 events were handled.
                        
            noticed this in the log output on the 4th run: Marten.Events.Daemon.AsyncProjectionHostedService: Information: Projection agent for 'lotrConsumer:All' has started from sequence 8 and a high water mark of 10
            during the second run, a further 5 events were handled.
            I then made changes here to log the full serialized @event object (using system.text.json)
            during the 3rd run, 2 additional events were created, but their serialization failed here (system.text.json could not serialize them).
            so, I switched to Newtonsoft.json, and that worked...leading to the 4th run,
            which picked up the 2 failures, and now successfully processed them.  So I was able to output their json serialization into the code comments below.

            I then ran the API project without running this project, and created a number of events.  Then, when I subsequently started this project, the log showed the following:
            Marten.Events.Daemon.AsyncProjectionHostedService: Information: Projection agent for 'lotrConsumer:All' has started from sequence 10 and a high water mark of 19
            BlazorApp.Services.LOTREventsConsumer: Information: 11 - quest_started
            BlazorApp.Services.LOTREventsConsumer: Information: {"Data":{"Id":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","Name":"Destroy the 3rd Ring","Location":"Hobbiton"},"StreamId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","StreamKey":null,"Id":"0181b55f-e2b6-420b-8690-db40198d1b28","Version":1,"Sequence":11,"Timestamp":"2022-06-30T16:09:29.160471+00:00","TenantId":"*DEFAULT*","CausationId":null,"CorrelationId":null,"Headers":null,"EventType":"LOTRShared.Domain.QuestStarted, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","EventTypeName":"quest_started","DotNetTypeName":"LOTRShared.Domain.QuestStarted, LOTRShared","IsArchived":false,"AggregateTypeName":"quest"}
            BlazorApp.Services.LOTREventsConsumer: Information: 12 - members_joined
            BlazorApp.Services.LOTREventsConsumer: Information: {"Data":{"QuestId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","Day":1,"Location":"Hobbiton","Members":["Frodo","Sam"]},"StreamId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","StreamKey":null,"Id":"0181b55f-e2b8-4a77-aae0-063014e0364a","Version":2,"Sequence":12,"Timestamp":"2022-06-30T16:09:29.160471+00:00","TenantId":"*DEFAULT*","CausationId":null,"CorrelationId":null,"Headers":null,"EventType":"LOTRShared.Domain.MembersJoined, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","EventTypeName":"members_joined","DotNetTypeName":"LOTRShared.Domain.MembersJoined, LOTRShared","IsArchived":false,"AggregateTypeName":"quest"}
            BlazorApp.Services.LOTREventsConsumer: Information: 13 - arrived_at_location
            BlazorApp.Services.LOTREventsConsumer: Information: {"Data":{"QuestId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","Day":3,"Location":"Bree"},"StreamId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","StreamKey":null,"Id":"0181b563-8004-4461-9854-225ecdc81bb9","Version":3,"Sequence":13,"Timestamp":"2022-06-30T16:13:25.905274+00:00","TenantId":"*DEFAULT*","CausationId":null,"CorrelationId":null,"Headers":null,"EventType":"LOTRShared.Domain.ArrivedAtLocation, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","EventTypeName":"arrived_at_location","DotNetTypeName":"LOTRShared.Domain.ArrivedAtLocation, LOTRShared","IsArchived":false,"AggregateTypeName":"quest"}
            BlazorApp.Services.LOTREventsConsumer: Information: 14 - members_joined
            BlazorApp.Services.LOTREventsConsumer: Information: {"Data":{"QuestId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","Day":5,"Location":"Bree","Members":["Pippin","Merry"]},"StreamId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","StreamKey":null,"Id":"0181b564-95be-4617-9b18-64b98204dc89","Version":4,"Sequence":14,"Timestamp":"2022-06-30T16:14:36.996885+00:00","TenantId":"*DEFAULT*","CausationId":null,"CorrelationId":null,"Headers":null,"EventType":"LOTRShared.Domain.MembersJoined, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","EventTypeName":"members_joined","DotNetTypeName":"LOTRShared.Domain.MembersJoined, LOTRShared","IsArchived":false,"AggregateTypeName":"quest"}
            BlazorApp.Services.LOTREventsConsumer: Information: 15 - arrived_at_location
            BlazorApp.Services.LOTREventsConsumer: Information: {"Data":{"QuestId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","Day":5,"Location":"Aragorn"},"StreamId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","StreamKey":null,"Id":"0181b565-50ab-42bb-bf7b-371bd2e15ec4","Version":5,"Sequence":15,"Timestamp":"2022-06-30T16:15:24.844787+00:00","TenantId":"*DEFAULT*","CausationId":null,"CorrelationId":null,"Headers":null,"EventType":"LOTRShared.Domain.ArrivedAtLocation, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","EventTypeName":"arrived_at_location","DotNetTypeName":"LOTRShared.Domain.ArrivedAtLocation, LOTRShared","IsArchived":false,"AggregateTypeName":"quest"}
            BlazorApp.Services.LOTREventsConsumer: Information: 16 - arrived_at_location
            BlazorApp.Services.LOTREventsConsumer: Information: {"Data":{"QuestId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","Day":7,"Location":"Bree"},"StreamId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","StreamKey":null,"Id":"0181b566-ad94-4215-8ad8-9d2128bd6225","Version":6,"Sequence":16,"Timestamp":"2022-06-30T16:16:54.170332+00:00","TenantId":"*DEFAULT*","CausationId":null,"CorrelationId":null,"Headers":null,"EventType":"LOTRShared.Domain.ArrivedAtLocation, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","EventTypeName":"arrived_at_location","DotNetTypeName":"LOTRShared.Domain.ArrivedAtLocation, LOTRShared","IsArchived":false,"AggregateTypeName":"quest"}
            BlazorApp.Services.LOTREventsConsumer: Information: 17 - members_joined
            BlazorApp.Services.LOTREventsConsumer: Information: {"Data":{"QuestId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","Day":10,"Location":"Bree","Members":["Aragorn"]},"StreamId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","StreamKey":null,"Id":"0181b566-ce5a-4a54-8854-580e9242401a","Version":7,"Sequence":17,"Timestamp":"2022-06-30T16:17:02.557508+00:00","TenantId":"*DEFAULT*","CausationId":null,"CorrelationId":null,"Headers":null,"EventType":"LOTRShared.Domain.MembersJoined, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","EventTypeName":"members_joined","DotNetTypeName":"LOTRShared.Domain.MembersJoined, LOTRShared","IsArchived":false,"AggregateTypeName":"quest"}
            BlazorApp.Services.LOTREventsConsumer: Information: 18 - characters_slayed
            BlazorApp.Services.LOTREventsConsumer: Information: {"Data":{"QuestId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","Day":12,"Location":"Bree","Characters":["Dragon1"]},"StreamId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","StreamKey":null,"Id":"0181b56d-517c-41de-a22d-3e3f75831d39","Version":8,"Sequence":18,"Timestamp":"2022-06-30T16:24:09.345279+00:00","TenantId":"*DEFAULT*","CausationId":null,"CorrelationId":null,"Headers":null,"EventType":"LOTRShared.Domain.CharactersSlayed, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","EventTypeName":"characters_slayed","DotNetTypeName":"LOTRShared.Domain.CharactersSlayed, LOTRShared","IsArchived":false,"AggregateTypeName":"quest"}
            BlazorApp.Services.LOTREventsConsumer: Information: 19 - members_departed
            BlazorApp.Services.LOTREventsConsumer: Information: {"Data":{"QuestId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","Day":13,"Location":"Bree","Members":["Aragorn"]},"StreamId":"701ce422-1a96-4cbe-8cb2-51f5cb4d1d0f","StreamKey":null,"Id":"0181b56e-4abd-423c-952b-4ba9db025464","Version":9,"Sequence":19,"Timestamp":"2022-06-30T16:25:13.152522+00:00","TenantId":"*DEFAULT*","CausationId":null,"CorrelationId":null,"Headers":null,"EventType":"LOTRShared.Domain.MembersDeparted, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null","EventTypeName":"members_departed","DotNetTypeName":"LOTRShared.Domain.MembersDeparted, LOTRShared","IsArchived":false,"AggregateTypeName":"quest"}
            Marten.Events.Daemon.AsyncProjectionHostedService: Information: Shard 'lotrConsumer:All': Executed updates for Event range of 'Identity: lotrConsumer:All', 10 to 19
            
            nice how it catches up with unprocessed events.  Wonder where it is storing the sequence number at which to start.  High water mark is the highest event sequence number not yet processed.
             */

            foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
            {
                Events.Add(@event);
                _logger.LogInformation($"{@event.Sequence} - {@event.EventTypeName}");
                //_logger.LogInformation($"{System.Text.Json.JsonSerializer.Serialize(@event)}"); //this fails due to system.text.json not supporting serialization of system.types?!?!?
                _logger.LogInformation($"{Newtonsoft.Json.JsonConvert.SerializeObject(@event)}"); //this works, and the output is pasted at the end of this class.

                await _questHub.Clients.All.SendAsync("ApplyQuestEvent", @event.StreamId, Newtonsoft.Json.JsonConvert.SerializeObject(@event));
            }

            //return Task.CompletedTask;
        }
    }
}

/*
    {
        "Data": {
            "Id": "00000000-0000-0000-0000-000000000000",
            "Name": "Destroy the 3rd Ring"
        },
        "StreamId": "fb23136e-13ef-42e0-ab20-3bffbe30d88c",
        "StreamKey": null,
        "Id": "0181b4cd-dddf-42d0-a7f6-72f9be6057f9",
        "Version": 1,
        "Sequence": 9,
        "Timestamp": "2022-06-30T13:30:00.220146+00:00",
        "TenantId": "*DEFAULT*",
        "CausationId": null,
        "CorrelationId": null,
        "Headers": null,
        "EventType": "LOTRShared.Domain.QuestStarted, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
        "EventTypeName": "quest_started",
        "DotNetTypeName": "LOTRShared.Domain.QuestStarted, LOTRShared",
        "IsArchived": false,
        "AggregateTypeName": "quest_party"
    }

    {
        "Data": {
            "QuestId": "00000000-0000-0000-0000-000000000000",
            "Day": 1,
            "Location": "Hobbiton",
            "Members": [
                "Frodo",
                "Sam"
            ]
        },
        "StreamId": "fb23136e-13ef-42e0-ab20-3bffbe30d88c",
        "StreamKey": null,
        "Id": "0181b4cd-dde1-4acd-b6ab-5aa547231789",
        "Version": 2,
        "Sequence": 10,
        "Timestamp": "2022-06-30T13:30:00.220146+00:00",
        "TenantId": "*DEFAULT*",
        "CausationId": null,
        "CorrelationId": null,
        "Headers": null,
        "EventType": "LOTRShared.Domain.MembersJoined, LOTRShared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
        "EventTypeName": "members_joined",
        "DotNetTypeName": "LOTRShared.Domain.MembersJoined, LOTRShared",
        "IsArchived": false,
        "AggregateTypeName": "quest_party"
    }
*/