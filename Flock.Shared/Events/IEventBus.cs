namespace Flock.Shared.Events;

public delegate void BroadcastDelegate<TEvent>(ref TEvent evt) where TEvent : notnull;

public interface IEventBus
{
    void RaiseEvent<TEvent>(ref TEvent evt) where TEvent : notnull;
    void SubscribeEvent<TEvent>(BroadcastDelegate<TEvent> handler) where TEvent : notnull;
    void SubscribeEventOnce<TEvent>(BroadcastDelegate<TEvent> handler) where TEvent : notnull;
    void UnsubscribeEvent<TEvent>(BroadcastDelegate<TEvent> handler) where TEvent : notnull;
}
