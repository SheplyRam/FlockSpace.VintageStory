using System;
using System.Collections.Generic;

namespace Flock.Shared.Events;

/// <summary>
/// Default <see cref="IEventBus"/> implementation.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _broadcastHandlers = [];
    private readonly Dictionary<Type, List<Delegate>> _tempBroadcastHandlers = [];

    public void RaiseEvent<TEvent>(ref TEvent evt) where TEvent : notnull
    {
        // Snapshot before invoking: a handler that (un)subscribes mid-dispatch must not affect the current fan-out or
        // skip a remaining handler.
        var allHandlers = new List<Delegate>();
        if (_broadcastHandlers.TryGetValue(typeof(TEvent), out var handlers))
            allHandlers.AddRange(handlers);
        if (_tempBroadcastHandlers.TryGetValue(typeof(TEvent), out var onceHandlers))
            allHandlers.AddRange(onceHandlers);

        if (allHandlers.Count == 0)
            return;

        foreach (var handler in allHandlers.ToArray())
        {
            try
            {
                ((BroadcastDelegate<TEvent>)handler)(ref evt);
            }
            catch (Exception e)
            {
                var target = handler.Target ?? handler;
                throw new InvalidOperationException(
                    $"Couldn't continue with the event handling for {target}: {e.Message}");
            }
        }

        // Once-handlers ran above; drop them from the temp list regardless of which of them actually executed, so a
        // second raise never re-runs them.
        if (onceHandlers != null)
        {
            foreach (var temphandler in onceHandlers.ToArray())
            {
                UnsubscribeEvent((BroadcastDelegate<TEvent>)temphandler);
            }
        }
    }

    public void SubscribeEvent<TEvent>(BroadcastDelegate<TEvent> handler) where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(handler);

        var handlers = GetOrCreateBroadcastHandlers(typeof(TEvent), _broadcastHandlers);
        if (!handlers.Contains(handler))
            handlers.Add(handler);
    }

    public void SubscribeEventOnce<TEvent>(BroadcastDelegate<TEvent> handler) where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(handler);

        var handlers = GetOrCreateBroadcastHandlers(typeof(TEvent), _tempBroadcastHandlers);
        if (!handlers.Contains(handler))
            handlers.Add(handler);
    }

    public void UnsubscribeEvent<TEvent>(BroadcastDelegate<TEvent> handler) where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (_broadcastHandlers.TryGetValue(typeof(TEvent), out var handlers))
            handlers.Remove(handler);

        if (_tempBroadcastHandlers.TryGetValue(typeof(TEvent), out var onceHandlers))
            onceHandlers.Remove(handler);
    }

    private static List<Delegate> GetOrCreateBroadcastHandlers(Type eventType, Dictionary<Type, List<Delegate>> broadcastHandlers)
    {
        if (!broadcastHandlers.TryGetValue(eventType, out var handlers))
        {
            handlers = [];
            broadcastHandlers.Add(eventType, handlers);
        }

        return handlers;
    }
}
