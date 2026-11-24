using System.Collections.Generic;
using UnityEngine;

namespace LOGIYGames.Timers
{
    /// <summary>
    /// Central registry that ticks all active timers once per Unity Update phase.
    /// </summary>
    /// <remarks>
    /// The manager is static and does not require a scene component. The package bootstrapper
    /// inserts <see cref="UpdateTimers"/> into Unity's Update PlayerLoop phase.
    /// </remarks>
    public static class TimerManager
    {
        private static readonly List<Timer> timers = new List<Timer>();
        private static readonly List<Timer> sweep = new List<Timer>();
        private static bool isUpdating;

        /// <summary>
        /// Registers a timer for PlayerLoop updates. Registering the same instance more than once is ignored.
        /// </summary>
        /// <param name="timer">Timer to register.</param>
        public static void RegisterTimer(Timer timer)
        {
            if (timer == null || timers.Contains(timer))
            {
                return;
            }

            timers.Add(timer);
        }

        /// <summary>
        /// Removes a timer from the update registry. Removing an unknown or null timer is safe.
        /// </summary>
        /// <param name="timer">Timer to deregister.</param>
        public static void DeregisterTimer(Timer timer)
        {
            if (timer == null)
            {
                return;
            }

            timers.Remove(timer);
        }

        /// <summary>
        /// Advances a snapshot of all registered timers using Unity's scaled delta time.
        /// A snapshot allows callbacks to start, stop, pause, or dispose timers safely while ticking.
        /// </summary>
        public static void UpdateTimers()
        {
            if (isUpdating || timers.Count == 0)
            {
                return;
            }

            isUpdating = true;
            sweep.Clear();
            sweep.AddRange(timers);

            try
            {
                for (var i = 0; i < sweep.Count; i++)
                {
                    var timer = sweep[i];
                    if (timer != null && timer.IsRunning)
                    {
                        timer.Tick(Time.deltaTime);
                    }
                }
            }
            finally
            {
                sweep.Clear();
                isUpdating = false;
            }
        }

        /// <summary>
        /// Disposes every currently registered timer and empties the registry.
        /// Primarily used when exiting Play Mode in the Unity Editor.
        /// </summary>
        public static void Clear()
        {
            var snapshot = timers.ToArray();
            timers.Clear();
            sweep.Clear();

            foreach (var timer in snapshot)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }

            // A stop callback may have registered another timer while the registry was being cleared.
            timers.Clear();
            sweep.Clear();
        }
    }
}
