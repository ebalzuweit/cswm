using System.Reactive.Subjects;

namespace cswm.Events;

public class MessageBus
{
    public readonly Subject<Event> Events = new();

    public void Publish<T>(T @event) where T : Event
    {
        Events.OnNext(@event);
    }
}