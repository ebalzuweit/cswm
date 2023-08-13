using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace cswm.Events;

public class MessageBus
{
    public readonly Subject<Event> Events = new(); // TODO dispose properly

    public void Publish<T>(T @event) where T : Event
    {
        Events.OnNext(@event);
    }

    public IDisposable Subscribe(Func<Event, bool> predicate, Action<Event> action)
        => Events.Where(predicate).Subscribe(action);

    public IDisposable Subscribe<T>(Action<T> action) where T : Event
        => Events.Where(@event => @event is T).Subscribe(@event => action.Invoke((T)@event));
}