namespace LOGIYGames.Timers
{
    /// <summary>
    /// Timer that counts upward from zero without a completion condition.
    /// Useful for measuring elapsed duration.
    /// </summary>
    public class StopwatchTimer : Timer
    {
        /// <summary>Creates a stopwatch timer.</summary>
        public StopwatchTimer() : base(0f)
        {
        }

        /// <summary>Adds delta time while the stopwatch is running.</summary>
        public override void Tick(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);
            if (IsRunning)
            {
                CurrentTime += deltaTime;
            }
        }

        /// <inheritdoc />
        public override bool IsFinished => false;
    }
}
