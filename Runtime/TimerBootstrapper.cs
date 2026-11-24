using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LOGIYGames.Timers
{
    /// <summary>
    /// Installs the timer update callback into Unity's Update PlayerLoop phase.
    /// </summary>
    internal static class TimerBootstrapper
    {
        private static PlayerLoopSystem timerSystem;

        /// <summary>
        /// Called by Unity after assemblies load. Existing package callbacks are removed first,
        /// making initialization safe when Enter Play Mode options keep the scripting domain alive.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            RemoveTimerManager<Update>(ref currentPlayerLoop);
            if (!InsertTimerManager<Update>(ref currentPlayerLoop, 0))
            {
                Debug.LogWarning("LOGIYGames Timers was not initialized: unable to find the Update PlayerLoop phase.");
                return;
            }

            PlayerLoop.SetPlayerLoop(currentPlayerLoop);

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;
#endif
        }

        private static void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
        {
            if (timerSystem.type == null)
            {
                timerSystem = CreateTimerSystem();
            }

            PlayerLoopUtils.RemoveSystem<T>(ref loop, in timerSystem);
        }

        private static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
        {
            timerSystem = CreateTimerSystem();
            return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
        }

        private static PlayerLoopSystem CreateTimerSystem()
        {
            return new PlayerLoopSystem
            {
                type = typeof(TimerManager),
                updateDelegate = TimerManager.UpdateTimers,
                subSystemList = null
            };
        }

#if UNITY_EDITOR
        private static void OnPlayModeState(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveTimerManager<Update>(ref currentPlayerLoop);
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            TimerManager.Clear();
        }
#endif
    }
}
