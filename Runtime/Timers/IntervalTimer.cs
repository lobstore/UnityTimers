using System;
using UnityEngine;

namespace LOGIYGames.Timers
{
    /// <summary>
    /// Counts down from a total duration and invokes an event at each configured interval.
    /// </summary>
    /// <remarks>
    /// Interval events are emitted for every threshold crossed, including multiple thresholds
    /// during a single frame. An interval longer than the total duration produces no interval events.
    /// </remarks>
    public class IntervalTimer : CountdownTimer
    {
        private readonly float interval;
        private float nextInterval;

        /// <summary>Invoked each time the remaining duration crosses an interval threshold.</summary>
        public Action OnInterval = delegate { };

        /// <summary>Creates an interval countdown.</summary>
        /// <param name="totalTime">Total duration in seconds.</param>
        /// <param name="intervalSeconds">Interval in seconds; must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when intervalSeconds is non-positive or non-finite.</exception>
        public IntervalTimer(float totalTime, float intervalSeconds) : base(totalTime)
        {
            if (float.IsNaN(intervalSeconds) || float.IsInfinity(intervalSeconds) || intervalSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds), intervalSeconds, "Interval must be finite and greater than zero.");
            }

            interval = intervalSeconds;
            nextInterval = totalTime - interval;
        }

        /// <summary>Advances the countdown and invokes OnInterval for every crossed threshold.</summary>
        public override void Tick(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);
            if (!IsRunning)
            {
                return;
            }

            CurrentTime = Mathf.Max(0f, CurrentTime - deltaTime);

            while (IsRunning && nextInterval >= 0f && CurrentTime <= nextInterval)
            {
                OnInterval?.Invoke();
                nextInterval -= interval;
            }

            if (IsRunning && CurrentTime <= 0f)
            {
                CurrentTime = 0f;
                Stop();
            }
        }

        /// <inheritdoc />
        public override void Reset()
        {
            base.Reset();
            nextInterval = initialTime - interval;
        }

        /// <inheritdoc />
        public override void Reset(float newTime)
        {
            base.Reset(newTime);
            nextInterval = initialTime - interval;
        }
    }
}
