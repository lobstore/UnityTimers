using System;
using UnityEngine;

namespace LOGIYGames.Timers
{
    /// <summary>
    /// Base class for timers managed by <see cref="TimerManager"/>.
    /// </summary>
    /// <remarks>
    /// Start resets the timer to its initial value. Pause removes the timer from the update
    /// registry without resetting it; Resume re-registers it with its current time intact.
    /// Stop ends the current run. Dispose permanently releases the timer.
    /// </remarks>
    public abstract class Timer : IDisposable
    {
        private bool isDisposed;

        /// <summary>Current timer value in seconds. Meaning depends on the derived timer type.</summary>
        public float CurrentTime { get; protected set; }

        /// <summary>Whether this timer is currently registered and advancing.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>Initial value restored by Start and parameterless Reset.</summary>
        protected float initialTime;

        /// <summary>Invoked when Start transitions the timer from stopped to running.</summary>
        public Action OnTimerStart = delegate { };

        /// <summary>Invoked when Stop transitions the timer from running to stopped.</summary>
        public Action OnTimerStop = delegate { };

        /// <summary>Creates a timer with an initial value expressed in seconds.</summary>
        /// <param name="value">Initial timer value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown for negative or non-finite values.</exception>
        protected Timer(float value)
        {
            ValidateTime(value, nameof(value));
            initialTime = value;
            CurrentTime = value;
        }

        /// <summary>Resets to the initial value and starts or restarts this timer.</summary>
        /// <remarks>If already running, the timer is reset without being registered a second time or firing OnTimerStart again.</remarks>
        public void Start()
        {
            ThrowIfDisposed();
            CurrentTime = initialTime;

            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            TimerManager.RegisterTimer(this);
            OnTimerStart?.Invoke();
        }

        /// <summary>Stops the timer and removes it from the update registry.</summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                TimerManager.DeregisterTimer(this);
                return;
            }

            IsRunning = false;
            TimerManager.DeregisterTimer(this);
            OnTimerStop?.Invoke();
        }

        /// <summary>Resumes a paused or stopped timer without resetting its current value.</summary>
        public void Resume()
        {
            ThrowIfDisposed();
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            TimerManager.RegisterTimer(this);
        }

        /// <summary>Pauses the timer, preserving its current value and removing it from updates.</summary>
        public void Pause()
        {
            if (!IsRunning)
            {
                return;
            }

            IsRunning = false;
            TimerManager.DeregisterTimer(this);
        }

        /// <summary>Advances this timer by the supplied time delta.</summary>
        /// <param name="deltaTime">Elapsed seconds; must be finite and non-negative.</param>
        public abstract void Tick(float deltaTime);

        /// <summary>Whether this timer has reached its completion condition.</summary>
        public abstract bool IsFinished { get; }

        /// <summary>Restores the timer's initial value without changing its running state.</summary>
        public virtual void Reset()
        {
            ThrowIfDisposed();
            CurrentTime = initialTime;
        }

        /// <summary>Sets a new initial value and restores the timer to it.</summary>
        /// <param name="newTime">New initial value in seconds.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown for negative or non-finite values.</exception>
        public virtual void Reset(float newTime)
        {
            ThrowIfDisposed();
            ValidateTime(newTime, nameof(newTime));
            initialTime = newTime;
            Reset();
        }

        /// <summary>Permanently disposes the timer and deregisters it.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Performs disposal. This type owns no unmanaged resources and has no finalizer.</summary>
        /// <param name="disposing">True when called through the public Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            var wasRunning = IsRunning;
            IsRunning = false;
            TimerManager.DeregisterTimer(this);

            if (disposing && wasRunning)
            {
                OnTimerStop?.Invoke();
            }
        }

        /// <summary>Validates time values used by timer implementations.</summary>
        protected static void ValidateTime(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Time values must be finite and non-negative.");
            }
        }

        /// <summary>Validates a time delta passed to Tick.</summary>
        protected static void ValidateDeltaTime(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Delta time must be finite and non-negative.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
