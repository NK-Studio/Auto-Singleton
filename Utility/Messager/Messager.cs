using System;
using System.Collections.Generic;
using UnityEngine;

namespace USingleton
{
    /// <summary>
    /// The Messager class provides functionality to register and send messages.
    /// </summary>
    public static class Messager
    {
        public delegate void Message();

        private static readonly Dictionary<string, Message> RegisteredMessages;

        static Messager()
        {
            RegisteredMessages = new Dictionary<string, Message>();
        }

        /// <summary>
        /// Registers a message with the given message name.
        /// </summary>
        /// <param name="messageName">The name of the message.</param>
        /// <param name="message">The message to register.</param>
        public static void RegisterMessage(string messageName, Message message)
        {
            if (!RegisteredMessages.TryAdd(messageName, message))
                Debug.LogWarning($"Messager: The item {messageName} already contains a reference to the message.");
        }

        /// <summary>
        /// Remove a message from the RegisteredMessages dictionary by its name.
        /// </summary>
        /// <param name="messageName">The name of the message to be removed.</param>
        public static void RemoveMessage(string messageName)
        {
            if (RegisteredMessages.ContainsKey(messageName))
                RegisteredMessages.Remove(messageName);
        }

        /// <summary>
        /// Removes all registered messages.
        /// </summary>
        public static void RemoveAllMessages()
        {
            RegisteredMessages.Clear();
        }

        /// <summary>
        /// Sends a message with the specified event name.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public static void Send(string eventName)
        {
            if (RegisteredMessages.TryGetValue(eventName, out Message message))
            {
                try
                {
                    message?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Messager: An exception of type {e.GetType().Name} was caught while sending the {eventName} message.");
                    Debug.LogException(e);
                }
            }
            else
            {
                Debug.LogWarning($"Messager: The {eventName} event is not registered.");
            }
        }
    }
}