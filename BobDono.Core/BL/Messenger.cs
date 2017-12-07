using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BobDono.Core.BL
{
    public class Messenger
    {
        #region Singleton

        private Messenger()
        {

        }

        public static Messenger Instance { get; } = new Messenger();

        #endregion

        private Dictionary<Type, Action<object>> _listeners = new Dictionary<Type, Action<object>>();
        private Dictionary<Type, Func<object, Task>> _taskListeners = new Dictionary<Type, Func<object, Task>>();

        public void Register<T>(Action<T> handler)
        {
            _listeners.Add(typeof(T), obj =>
            {
                handler.Invoke((T) obj);
            });
        }

        public void Register<T>(Func<T,Task> handler)
        {
            _taskListeners.Add(typeof(T), obj =>
            {
                return handler.Invoke((T) obj);
            });
        }

        public void Send<T>(T message)
        {
            foreach (var listener in _listeners)
            {
                if (listener.Key == typeof(T))
                {
                    listener.Value.Invoke(message);
                }
            }
        }

        public async Task SendAsync<T>(T message)
        {
            foreach (var listener in _taskListeners)
            {
                if (listener.Key == typeof(T))
                {
                    await listener.Value.Invoke(message);
                }
            }
        }
    }
}
