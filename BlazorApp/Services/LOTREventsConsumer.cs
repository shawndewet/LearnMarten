using Marten;
using Marten.Events;

namespace BlazorApp.Services
{
    public interface IMartenEventsConsumer
    {
        Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions);
    }

    public class LOTREventsConsumer : IMartenEventsConsumer
    {
        private readonly ILogger<LOTREventsConsumer> _logger;
        public static List<object> Events { get; } = new();

        public LOTREventsConsumer(ILogger<LOTREventsConsumer> logger)
        {
            _logger = logger;
        }

        public Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions)
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

            */

            foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
            {
                Events.Add(@event);
                _logger.LogInformation($"{@event.Sequence} - {@event.EventTypeName}");
                //_logger.LogInformation($"{System.Text.Json.JsonSerializer.Serialize(@event)}"); //this fails due to system.text.json not supporting serialization of system.types?!?!?
                _logger.LogInformation($"{Newtonsoft.Json.JsonConvert.SerializeObject(@event)}"); //this works, and the output is pasted at the end of this class.
            }

            return Task.CompletedTask;
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