# LOGIYGames Timers

A lightweight timer library for Unity. Timers are updated by a callback installed into Unity's PlayerLoop; no MonoBehaviour timer component or per-timer coroutine is required.

## Quick start

```csharp
using LOGIYGames.Timers;

var countdown = new CountdownTimer(5f);
countdown.OnTimerStop += () => UnityEngine.Debug.Log("Finished");
countdown.Start();
```

Create the timer from runtime code after Unity has initialized its PlayerLoop. Timers use scaled `Time.deltaTime`, so they pause when `Time.timeScale` is zero.

## Timer types

- `CountdownTimer` counts down to zero and stops.
- `IntervalTimer` counts down and invokes a callback at each interval threshold.
- `FrequencyTimer` invokes a callback at a configured number of ticks per second.
- `StopwatchTimer` accumulates elapsed time without completing.

## Documentation

- [API.json](Documentation/API.json) - structured public API contract for AI agents.
- [Manual.md](Documentation/Manual.md) - full setup, lifecycle, examples, and reference.
- [QuickStart.pdf](Documentation/QuickStart.pdf) - printable usage guide.

## Compatibility

The package manifest targets Unity 2022.3. The public namespace and assembly are `LOGIYGames.Timers`. The runtime assembly retains its existing project Extensions assembly reference.
