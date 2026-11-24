using UnityEngine;

namespace LOGIYGames.Timers
{
    /// <summary>
    /// Timer that counts down from an initial duration to zero.
    /// </summary>
    public class CountdownTimer : Timer
    {
        /// <summary>Creates a countdown timer.</summary>
        /// <param name="value">Initial duration in seconds; defaults to zero.</param>
        public CountdownTimer(float value = 0f) : base(value)
        {
        }

        /// <summary>Subtracts delta time and stops when the countdown reaches zero.</summary>
        public override void Tick(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);
            if (!IsRunning)
            {
                return;
            }

            CurrentTime = Mathf.Max(0f, CurrentTime - deltaTime);
            if (CurrentTime <= 0f)
            {
                Stop();
            }
        }

        /// <inheritdoc />
        public override bool IsFinished => CurrentTime <= 0f;

        /// <summary>
        /// Progress from zero (not elapsed) to one (completed).
        /// A zero-duration countdown reports 1 to avoid division by zero and NaN.
        /// </summary>
        public virtual float Progress => initialTime <= 0f
            ? 1f
            : Mathf.Clamp01((initialTime - CurrentTime) / initialTime);
    }
}
