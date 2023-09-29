using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader
{
    /// <summary>
    /// Simple subscription message bus.
    /// </summary>
    /// <typeparam name="T">Type of message.</typeparam>
    public class MessageBus<T>
    {
        private static int _existCount = 1;

        private Dictionary<Guid, BusSubscription> _subscribers = new Dictionary<Guid, BusSubscription>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBus{T}"/> class.
        /// </summary>
        public MessageBus()
        {
        }

        /// <summary>
        /// Adds a subscription to the message bus.
        /// </summary>
        /// <param name="callback">Callback to execute when a message is received.</param>
        /// <param name="listenCount">Number of times to execute callback. Subscription is automatically dropped after this many matching events. A value of zero will listen forever.</param>
        /// <param name="filter">Optional filter. If set, only messages matching the filter will notify the subscriber.</param>
        /// <returns>Subscription id.</returns>
        public Guid Subscribe(Action<T> callback, int listenCount = 0, Func<T, bool>? filter = null)
        {
            if (object.ReferenceEquals(null, callback))
            {
                throw new NullReferenceException(nameof(callback));
            }

            var subscription = new BusSubscription(callback)
            {
                Id = Guid.NewGuid(),
                Index = _existCount,
                ListenCount = listenCount,
                Filter = filter,
            };

            _existCount++;

            _subscribers.Add(subscription.Id, subscription);

            return subscription.Id;
        }

        /// <summary>
        /// Stops executing callbacks for the given id.
        /// </summary>
        /// <param name="id">Id to unsubscribe.</param>
        public void Unsubscribe(Guid id)
        {
            if (_subscribers.ContainsKey(id))
            {
                _subscribers.Remove(id);
            }
        }

        /// <summary>
        /// Message bus publication method. All known subscribers will be notified based on filter.
        /// </summary>
        /// <param name="msg">Message to publish.</param>
        public void Publish(T msg)
        {
            foreach (var kvp in _subscribers)
            {
                var subscription = kvp.Value;

                bool doCallback = false;

                // If there is no filter, always perform callback.
                if (object.ReferenceEquals(null, subscription.Filter))
                {
                    doCallback = true;
                }
                else if (subscription.Filter(msg))
                {
                    doCallback = true;
                }

                if (doCallback)
                {
                    Task.Run(() => { subscription.Callback(msg); });

                    subscription.CallbackCount++;

                    if (subscription.ListenCount > 0 && subscription.ListenCount == subscription.CallbackCount)
                    {
                        _subscribers.Remove(subscription.Id);
                    }
                }
            }
        }

        private class BusSubscription
        {
            public BusSubscription(Action<T> callback)
            {
                Callback = callback;
            }

            public int Index { get; set; }

            public Guid Id { get; set; }

            public int ListenCount { get; set; }

            public Func<T, bool>? Filter { get; set; }

            public Action<T> Callback { get; init; }

            public int CallbackCount { get; set; }
        }
    }
}
