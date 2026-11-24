using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace LOGIYGames.Timers
{
    /// <summary>
    /// Utilities for safely editing and inspecting Unity's PlayerLoop tree.
    /// </summary>
    public static class PlayerLoopUtils
    {
        /// <summary>
        /// Removes every occurrence of a PlayerLoop system matching the supplied type and update delegate.
        /// </summary>
        /// <typeparam name="T">PlayerLoop parent type used by the corresponding insertion operation.</typeparam>
        /// <param name="loop">PlayerLoop node to modify.</param>
        /// <param name="systemToRemove">System identity (type and update delegate) to remove.</param>
        public static void RemoveSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove)
        {
            RemoveSystemRecursive(ref loop, systemToRemove);
        }

        private static bool RemoveSystemRecursive(ref PlayerLoopSystem loop, PlayerLoopSystem systemToRemove)
        {
            if (loop.subSystemList == null || loop.subSystemList.Length == 0)
            {
                return false;
            }

            var systems = new List<PlayerLoopSystem>(loop.subSystemList.Length);
            var changed = false;

            for (var i = 0; i < loop.subSystemList.Length; i++)
            {
                var child = loop.subSystemList[i];

                if (child.type == systemToRemove.type && child.updateDelegate == systemToRemove.updateDelegate)
                {
                    changed = true;
                    continue;
                }

                if (RemoveSystemRecursive(ref child, systemToRemove))
                {
                    changed = true;
                }

                systems.Add(child);
            }

            if (changed)
            {
                loop.subSystemList = systems.ToArray();
            }

            return changed;
        }

        /// <summary>
        /// Inserts a system into the first PlayerLoop node whose type matches <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Parent PlayerLoop system type, for example <c>UnityEngine.PlayerLoop.Update</c>.</typeparam>
        /// <param name="loop">PlayerLoop tree to modify.</param>
        /// <param name="systemToInsert">System to insert.</param>
        /// <param name="index">Insertion index within the parent's child list. Valid range is 0 through child count.</param>
        /// <returns>True when a matching parent system was found and the child was inserted; otherwise false.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is outside the parent's child-list bounds.</exception>
        public static bool InsertSystem<T>(ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index)
        {
            if (loop.type == typeof(T))
            {
                var systems = loop.subSystemList != null
                    ? new List<PlayerLoopSystem>(loop.subSystemList)
                    : new List<PlayerLoopSystem>();

                if (index < 0 || index > systems.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Insertion index must be within the parent system's child list.");
                }

                systems.Insert(index, systemToInsert);
                loop.subSystemList = systems.ToArray();
                return true;
            }

            if (loop.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < loop.subSystemList.Length; i++)
            {
                var child = loop.subSystemList[i];
                if (!InsertSystem<T>(ref child, in systemToInsert, index))
                {
                    continue;
                }

                loop.subSystemList[i] = child;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes the complete PlayerLoop hierarchy to the Unity Console.
        /// </summary>
        /// <param name="loop">Root PlayerLoop node, usually returned by PlayerLoop.GetCurrentPlayerLoop.</param>
        public static void PrintPlayerLoop(PlayerLoopSystem loop)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Unity Player Loop");
            PrintSubsystem(loop, builder, 0);
            Debug.Log(builder.ToString());
        }

        private static void PrintSubsystem(PlayerLoopSystem system, StringBuilder builder, int level)
        {
            var typeName = system.type != null ? system.type.FullName : "<unknown>";
            builder.Append(' ', level * 2).AppendLine(typeName);

            if (system.subSystemList == null || system.subSystemList.Length == 0)
            {
                return;
            }

            foreach (var child in system.subSystemList)
            {
                PrintSubsystem(child, builder, level + 1);
            }
        }
    }
}
