using System;
using System.Collections.Generic;

namespace cswm.Events;

public interface IEvent { }

public interface IEventSubscription<T> where T : IEvent
{
    Action<T> EventHandler { get; }
}

public class EventSubscription<T> : IEventSubscription<T> where T : IEvent
{
    public Action<T> EventHandler { get; init; }

    public EventSubscription(Action<T> handler)
    {
        EventHandler = handler;
    }
}

public interface IEventBus
{
    void Publish<T>(T @event) where T : IEvent;

    IEventSubscription<T> Subscribe<T>(Action<T> handler) where T : IEvent;
}

public class EventBus : IEventBus
{
    private readonly IDictionary<Type, object> _subscriptions = new Dictionary<Type, object>();

    public void Publish<T>(T @event) where T : IEvent
    {
        var eventType = typeof(T);
        var subscription = _subscriptions[eventType];
        (subscription as IEventSubscription<T>)!.EventHandler(@event);
    }

    public IEventSubscription<T> Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var eventType = typeof(T);
        var subscription = new EventSubscription<T>(handler);
        _subscriptions.Add(eventType, subscription);
        return subscription;
    }
}