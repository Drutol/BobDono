using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BobDono.Core.BL
{
    public class TimerService
    {
        public class TimerRegistration
        {
            public Action Task { get; set; }
            public TimeSpan Interval { get; set; }
            public TimeSpan DueTime { get; set; }

            public TimerRegistration FireOnNextFullHour()
            {
                DueTime = TimeSpan.FromMinutes(60 - DateTime.UtcNow.Minute);
                return this;
            }
        }

        private readonly List<TimerRegistration> _registeredTasks = new List<TimerRegistration>();
        private readonly Dictionary<TimerRegistration,Timer> _timers = new Dictionary<TimerRegistration, Timer>();


        #region Singleton

        private TimerService()
        {

        }

        public static TimerService Instance { get; } = new TimerService();

        #endregion


        public void Register(TimerRegistration registration)
        {
            _registeredTasks.Add(registration);

            _timers.Add(registration,
                new Timer(TimerTrigger,registration.Task, registration.DueTime, registration.Interval));
        }

        public void Deregister(TimerRegistration registration)
        {
            _registeredTasks.Remove(registration);

            _timers.Remove(registration, out var timer);
            timer.Dispose();
        }

        private async void TimerTrigger(object state)
        {
            await Task.Delay(10000);
            (state as Action)();
        }
    }
}
