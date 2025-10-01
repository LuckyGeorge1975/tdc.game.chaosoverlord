using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ChaosOverlords.Core.Services.Messaging;

/// <summary>
/// Default in-memory implementation of <see cref="IMessageHub"/> backed by thread-safe subscriber lists.
/// </summary>
public sealed class MessageHub : IMessageHub
{
    private readonly ConcurrentDictionary<Type, List<ISubscription>> _subscriptions = new();

    public IDisposable Subscribe<TMessage>(Action<TMessage> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var subscription = new Subscription<TMessage>(this, handler);
        var subscribers = _subscriptions.GetOrAdd(typeof(TMessage), _ => new List<ISubscription>());

        lock (subscribers)
        {
            subscribers.Add(subscription);
        }

        return subscription;
    }

    public void Publish<TMessage>(TMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (!_subscriptions.TryGetValue(typeof(TMessage), out var subscribers))
        {
            return;
        }

        ISubscription[] snapshot;
        lock (subscribers)
        {
            snapshot = subscribers.ToArray();
        }

        foreach (var subscription in snapshot)
        {
            if (subscription is Subscription<TMessage> typed)
            {
                typed.Invoke(message);
            }
        }
    }

    private void Unsubscribe(ISubscription subscription)
    {
        if (!_subscriptions.TryGetValue(subscription.MessageType, out var subscribers))
        {
            return;
        }

        lock (subscribers)
        {
            subscribers.Remove(subscription);
        }
    }

    private interface ISubscription
    {
        Type MessageType { get; }
        bool IsDisposed { get; }
    }

    private sealed class Subscription<TMessage> : ISubscription, IDisposable
    {
        private readonly MessageHub _owner;
        private readonly Action<TMessage> _handler;
        private bool _isDisposed;

        public Subscription(MessageHub owner, Action<TMessage> handler)
        {
            _owner = owner;
            _handler = handler;
        }

        public Type MessageType => typeof(TMessage);

        public bool IsDisposed => _isDisposed;

        public void Invoke(TMessage message)
        {
            if (_isDisposed)
            {
                return;
            }

            _handler(message);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _owner.Unsubscribe(this);
        }
    }
}
