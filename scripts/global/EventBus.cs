using Godot;
using System;
using System.Collections.Generic;

namespace desktoppet.scripts.global;


public partial class EventBus : Node
{
    private static EventBus _instance;
    private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();
    private readonly object _lock = new();

    public static EventBus Instance => _instance ?? throw new InvalidOperationException("EventBus not initialized");

    public override void _Ready()
    {
        if (_instance != null && _instance != this)
            QueueFree();
        else
            _instance = this;
    }

    public static void Subscribe<T>(Action<T> handler)
    {
        lock (Instance._lock)
        {
            var eventType = typeof(T);
            if (!Instance._eventHandlers.ContainsKey(eventType))
                Instance._eventHandlers[eventType] = new List<Delegate>();

            Instance._eventHandlers[eventType].Add(handler);
        }
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        lock (Instance._lock)
        {
            var eventType = typeof(T);
            if (Instance._eventHandlers.TryGetValue(eventType, out var handlers))
                handlers.Remove(handler);
        }
    }

    public static void Publish<T>(T eventData)
    {
        List<Delegate> handlersToInvoke;
        lock (Instance._lock)
        {
            var eventType = typeof(T);
            if (!Instance._eventHandlers.TryGetValue(eventType, out var handlers))
                return;

            handlersToInvoke = new List<Delegate>(handlers);
        }

        foreach (var handler in handlersToInvoke)
        {
            try
            {
                ((Action<T>)handler)?.Invoke(eventData);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"EventBus handler error: {ex}");
            }
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
            ClearAllSubscriptions();
    }

    private void ClearAllSubscriptions()
    {
        lock (_lock)
        {
            _eventHandlers.Clear();
        }
    }
}