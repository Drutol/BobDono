using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BobDono.BL
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
                DueTime = TimeSpan.FromMinutes(60 - DateTime.Now.Minute);
                return this;
            }
        }

        private readonly List<TimerRegistration> _registeredTasks = new List<TimerRegistration>();
        private Timer _rootTimer;


        #region Singleton

        private TimerService()
        {

        }

        public static TimerService Instance { get; } = new TimerService();

        #endregion


        public void Register(TimerRegistration registration)
        {
            _registeredTasks.Add(registration);

            if (_rootTimer == null)
            {
                _rootTimer = new Timer(TimerTrigger,null,registration.DueTime,registration.Interval);
            }
        }

        public void Deregister(TimerRegistration registration)
        {
            _registeredTasks.Remove(registration);

            if (!_registeredTasks.Any())
            {
                _rootTimer.Dispose();
                _rootTimer = null;
            }
        }

        private void TimerTrigger(object state)
        {
            Parallel.Invoke(_registeredTasks.Select(registration => registration.Task).ToArray());
        }
    }
}
