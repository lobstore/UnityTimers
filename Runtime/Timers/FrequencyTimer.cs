using System;

namespace LOGIYGames.Timers
{
    /// <summary>
    /// Timer that invokes an action a specified number of times per second.
    /// </summary>
    /// <remarks>
    /// Elapsed time is accumulated. If a frame delta crosses multiple thresholds, OnTick
    /// is invoked once for each elapsed interval and the fractional remainder is preserved.
    /// </remarks>
    public class FrequencyTimer : Timer
    {
        /// <summary>Configured tick rate in callbacks per second.</summary>
        public int TicksPerSecond { get; private set; }

        /// <summary>Invoked for each elapsed frequency interval.</summary>
        public Action OnTick = delegate { };

        private float timeThreshold;

        /// <summary>Creates a frequency timer.</summary>
        /// <param name="ticksPerSecond">Number of callbacks per second; must be greater than zero.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the tick rate is not positive.</exception>
        public FrequencyTimer(int ticksPerSecond) : base(0f)
        {
            CalculateTimeThreshold(ticksPerSecond);
        }

        /// <summary>Accumulates time and invokes OnTick for every elapsed threshold.</summary>
        public override void Tick(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);
            if (!IsRunning)
            {
                return;
            }

            CurrentTime += deltaTime;
            while (IsRunning && CurrentTime >= timeThreshold)
            {
                CurrentTime -= timeThreshold;
                OnTick?.Invoke();
            }
        }

        /// <inheritdoc />
        public override bool IsFinished => !IsRunning;

        /// <summary>Clears the elapsed-time accumulator.</summary>
        public override void Reset()
        {
            base.Reset();
        }

        /// <summary>Changes the tick rate and clears the elapsed-time accumulator.</summary>
        /// <param name="newTicksPerSecond">New number of callbacks per second; must be greater than zero.</param>
        public void Reset(int newTicksPerSecond)
        {
            CalculateTimeThreshold(newTicksPerSecond);
            Reset();
        }

        private void CalculateTimeThreshold(int ticksPerSecond)
        {
            if (ticksPerSecond <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ticksPerSecond), ticksPerSecond, "Ticks per second must be greater than zero.");
            }

            TicksPerSecond = ticksPerSecond;
            timeThreshold = 1f / TicksPerSecond;
        }
    }
}
