namespace Flock.Shared.Events;

/// <summary>
/// Receiver of a broadcast event.
/// </summary>
public delegate void BroadcastDelegate<TEvent>(ref TEvent evt) where TEvent : notnull;

/// <summary>
/// Synchronous in-process event bus.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Invokes every subscriber of <typeparamref name="TEvent"/> now, on the calling thread.
    /// </summary>
    void RaiseEvent<TEvent>(ref TEvent evt) where TEvent : notnull;

    /// <summary>
    /// Subscribes until <see cref="UnsubscribeEvent{TEvent}"/> is called.
    /// </summary>
    void SubscribeEvent<TEvent>(BroadcastDelegate<TEvent> handler) where TEvent : notnull;

    /// <summary>
    /// Subscribes for exactly one raise. Unsubscribes the handler after it ran.
    /// </summary>
    void SubscribeEventOnce<TEvent>(BroadcastDelegate<TEvent> handler) where TEvent : notnull;

    /// <summary>
    /// Removes the handler from both persistent and once-only subscriptions.
    /// </summary>
    void UnsubscribeEvent<TEvent>(BroadcastDelegate<TEvent> handler) where TEvent : notnull;
}
