using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.Messenger
{
    public static class Messenger
    {
        public static readonly MessageBus Global = new();

        public static void Subscribe<T>(IMessageListener<T> listener) where T : IMessage
        {
            Global.Subscribe(listener);
        }

        public static void Unsubscribe<T>(IMessageListener<T> listener) where T : IMessage
        {
            Global.Unsubscribe(listener);
        }

        public static void Send<T>() where T : IMessage, new()
        {
            Global.Send<T>();
        }

        public static void Send<T>(T message) where T : IMessage
        {
            Global.Send(message);
        }

        public static void Clear()
        {
            Global.Clear();
        }
    }

    public class MessageBus
    {
        private readonly Dictionary<Type, object> _cachedMessages = new();
        private readonly Dictionary<Type, List<object>> _listeners = new();

        public void Subscribe<T>(IMessageListener<T> listener) where T : IMessage
        {
            if (_listeners.TryGetValue(typeof(T), out var listeners))
            {
#if DEBUG
                Assert.IsFalse(listeners.Contains(listener));
#endif
                listeners.Insert(0, listener);
            }
            else
            {
                _listeners.Add(typeof(T), new List<object> { listener });
            }
        }

        public void Unsubscribe<T>(IMessageListener<T> listener) where T : IMessage
        {
            if (!_listeners.TryGetValue(typeof(T), out var listeners))
            {
                return;
            }

            listeners.Remove(listener);
        }

        public void Send<T>() where T : IMessage, new()
        {
            if (!_cachedMessages.TryGetValue(typeof(T), out var message))
            {
                message = new T();
                _cachedMessages[typeof(T)] = message;
            }

            Send((T)message);
        }

        public void Send<T>(T message) where T : IMessage
        {
            if (!_listeners.TryGetValue(typeof(T), out var listeners))
            {
                return;
            }

            try
            {
                for (var i = listeners.Count - 1; i >= 0; i--)
                {
#if DEBUG
                    Assert.IsNotNull(listeners[i]);
                    Assert.IsTrue(listeners[i] is IMessageListener<T>);
#endif
                    ((IMessageListener<T>)listeners[i]).OnMessage(message);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Clear()
        {
            _listeners.Clear();
            _cachedMessages.Clear();
        }
    }
}
