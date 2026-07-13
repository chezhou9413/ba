using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    public sealed class SandWormVisualStateManager : GameComponent
    {
        private sealed class VisualEntry
        {
            public GameObject Root;
            public Map Map;
            public bool Visible;
        }

        private static readonly List<VisualEntry> RegisteredObjects = new List<VisualEntry>();
        private static bool lastShouldShow = true;

        public SandWormVisualStateManager(Game game)
        {
        }

        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
            RefreshVisibility();
        }

        public static void Register(GameObject gameObject, Map map)
        {
            if (gameObject == null)
            {
                return;
            }

            for (int i = 0; i < RegisteredObjects.Count; i++)
            {
                if (RegisteredObjects[i].Root == gameObject)
                {
                    return;
                }
            }

            VisualEntry entry = new VisualEntry
            {
                Root = gameObject,
                Map = map,
                Visible = true
            };
            RegisteredObjects.Add(entry);
            ApplyVisibleState(entry, ShouldShowEntry(entry), force: true);
        }

        public static void Unregister(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            for (int i = RegisteredObjects.Count - 1; i >= 0; i--)
            {
                if (RegisteredObjects[i].Root == gameObject)
                {
                    RegisteredObjects.RemoveAt(i);
                    return;
                }
            }
        }

        public static void RefreshVisibility()
        {
            lastShouldShow = Current.ProgramState == ProgramState.Playing && !WorldRendererUtility.WorldSelected;
            for (int i = RegisteredObjects.Count - 1; i >= 0; i--)
            {
                VisualEntry entry = RegisteredObjects[i];
                if (entry.Root == null)
                {
                    RegisteredObjects.RemoveAt(i);
                    continue;
                }

                ApplyVisibleState(entry, ShouldShowEntry(entry), force: false);
            }
        }

        public static void ApplyVisibleState(GameObject gameObject, bool visible)
        {
            if (gameObject == null)
            {
                return;
            }

            for (int i = 0; i < RegisteredObjects.Count; i++)
            {
                if (RegisteredObjects[i].Root == gameObject)
                {
                    ApplyVisibleState(RegisteredObjects[i], visible, force: false);
                    return;
                }
            }

            VisualEntry tempEntry = new VisualEntry
            {
                Root = gameObject,
                Map = null,
                Visible = !visible
            };
            ApplyVisibleState(tempEntry, visible, force: true);
        }

        private static bool ShouldShowEntry(VisualEntry entry)
        {
            if (!lastShouldShow || entry == null || entry.Root == null)
            {
                return false;
            }

            return entry.Map == null || Find.CurrentMap == entry.Map;
        }

        private static void ApplyVisibleState(VisualEntry entry, bool visible, bool force)
        {
            if (entry == null || entry.Root == null)
            {
                return;
            }

            if (!force && entry.Visible == visible)
            {
                return;
            }

            entry.Root.SetActive(visible);
            entry.Visible = visible;
        }
    }
}
