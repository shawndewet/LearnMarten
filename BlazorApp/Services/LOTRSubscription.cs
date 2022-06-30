using Marten;
using Marten.Events;
using Marten.Events.Projections;

namespace BlazorApp.Services
{
    public class LOTRSubscription : IProjection
    {
        private readonly IMartenEventsConsumer consumer;

        public LOTRSubscription(IMartenEventsConsumer consumer)
        {
            this.consumer = consumer;
        }

        public void Apply(
            IDocumentOperations operations,
            IReadOnlyList<StreamAction> streams
        )
        {
            throw new NotSupportedException("Subscription should be only run asynchronously");
        }

        public Task ApplyAsync(
            IDocumentOperations operations,
            IReadOnlyList<StreamAction> streams,
            CancellationToken ct
        )
        {
            return consumer.ConsumeAsync(streams);
        }
    }

}
