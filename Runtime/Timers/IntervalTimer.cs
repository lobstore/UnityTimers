using System;
using UnityEngine;

namespace LOGIYGames.Timers
{
    /// <summary>
    /// Countdown timer that fires an event every interval until completion.
    /// </summary>
    public class IntervalTimer : CountdownTimer {
        readonly float interval;
        float nextInterval;

        public Action OnInterval = delegate { };

        public IntervalTimer(float totalTime, float intervalSeconds) : base(totalTime) {
            interval = intervalSeconds;
            nextInterval = totalTime - interval;
        }

        public override void Tick(float deltaTime) {
            if (IsRunning && CurrentTime > 0) {
                CurrentTime -= Time.deltaTime;

                // Fire interval events as long as thresholds are crossed
                while (CurrentTime <= nextInterval && nextInterval >= 0) {
                    OnInterval.Invoke();
                    nextInterval -= interval;
                }
            }

            if (IsRunning && CurrentTime <= 0) {
                CurrentTime = 0;
                Stop();
            }
        }

        public override void Reset() {
            base.Reset();
            nextInterval = initialTime - interval;
        }

        public override void Reset(float newTime) {
            base.Reset(newTime);
            nextInterval = initialTime - interval;
        }
    }
}