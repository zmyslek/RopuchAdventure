/*
 * Script: LifeSystemBootstrap.cs
 * Purpose: Ensures life system components are present in the scene and creates
 * world indicators for known entities.
 */
using UnityEngine;
using UnityEngine.UI;

namespace TechJuego.LifeSystem
{
    public static class LifeSystemBootstrap
    {
        private struct ProfileHudConfig
        {
            public string ProfileId;
            public string Label;
            public Color AccentColor;
        }

        private static readonly ProfileHudConfig[] Profiles = new[]
        {
            new ProfileHudConfig { ProfileId = "Ropuch", Label = "Ropuch", AccentColor = new Color(0.52f, 0.84f, 0.39f, 1.0f) },
            new ProfileHudConfig { ProfileId = "Dziunia", Label = "Dziunia", AccentColor = new Color(0.96f, 0.57f, 0.78f, 1.0f) },
            new ProfileHudConfig { ProfileId = "Gryf", Label = "Gryf", AccentColor = new Color(0.95f, 0.82f, 0.29f, 1.0f) },
        };

        public static void EnsureInitialized()
        {
            EnsureLifeHandler();
            EnsureWorldIndicators();
        }

        private static void EnsureLifeHandler()
        {
            if (Object.FindObjectOfType<LifeHandler>() != null)
            {
                return;
            }

            new GameObject("LifeHandler").AddComponent<LifeHandler>();
        }

        private static void EnsureWorldIndicators()
        {
            EnsureIndicatorForTarget(Object.FindObjectOfType<RopuchControllerScript>(), "Ropuch", 2.0f);
            EnsureIndicatorForTarget(Object.FindObjectOfType<DziuniaScript>(), "Dziunia", 1.8f);
            EnsureIndicatorForTarget(Object.FindObjectOfType<GryficaScript>(), "Gryf", 1.8f);
        }

        private static void EnsureIndicatorForTarget<T>(T targetComponent, string profileId, float verticalOffset) where T : Component
        {
            if (targetComponent == null)
            {
                return;
            }

            if (targetComponent.GetComponentInChildren<LifeWorldIndicator>(true) != null)
            {
                return;
            }

            GameObject indicatorObject = new GameObject(profileId + "LifeIndicator", typeof(RectTransform), typeof(LifeWorldIndicator));
            indicatorObject.transform.SetParent(targetComponent.transform, false);

            LifeWorldIndicator indicator = indicatorObject.GetComponent<LifeWorldIndicator>();
            indicator.Initialize(profileId, targetComponent.transform, verticalOffset);
        }
    }
}
