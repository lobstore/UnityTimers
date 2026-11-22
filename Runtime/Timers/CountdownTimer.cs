using UnityEngine;

namespace LOGIYGames.Timers
{
    /// <summary>
    /// Timer that counts down from a specific value to zero.
    /// </summary>
    public class CountdownTimer : Timer {
        public CountdownTimer(float value = 0) : base(value) { }

        public override void Tick(float deltaTime) {
            if (IsRunning && CurrentTime > 0) {
                CurrentTime -= deltaTime;
            }

            if (IsRunning && CurrentTime <= 0) {
                Stop();
            }
        }

        public override bool IsFinished => CurrentTime <= 0;

        public virtual float Progress => Mathf.Clamp((initialTime - CurrentTime) / initialTime, 0, 1);
    }
}