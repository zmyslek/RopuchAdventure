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
            EnsureLifeHud();
        }

        private static void EnsureLifeHandler()
        {
            if (Object.FindObjectOfType<LifeHandler>() != null)
            {
                return;
            }

            new GameObject("LifeHandler").AddComponent<LifeHandler>();
        }

        private static void EnsureLifeHud()
        {
            if (Object.FindObjectOfType<LifeUi>() != null)
            {
                return;
            }

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("LifeHudCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();

                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920.0f, 1080.0f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            GameObject root = new GameObject("LifeHud", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            root.transform.SetParent(canvas.transform, false);

            RectTransform rootTransform = root.GetComponent<RectTransform>();
            rootTransform.anchorMin = new Vector2(0.0f, 1.0f);
            rootTransform.anchorMax = new Vector2(0.0f, 1.0f);
            rootTransform.pivot = new Vector2(0.0f, 1.0f);
            rootTransform.anchoredPosition = new Vector2(24.0f, -24.0f);
            rootTransform.sizeDelta = new Vector2(380.0f, 0.0f);

            Image rootImage = root.GetComponent<Image>();
            rootImage.color = new Color(0.05f, 0.05f, 0.08f, 0.78f);

            VerticalLayoutGroup verticalLayout = root.GetComponent<VerticalLayoutGroup>();
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            verticalLayout.spacing = 8.0f;
            verticalLayout.padding = new RectOffset(14, 14, 14, 14);
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = root.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateHeader(root.transform);

            foreach (ProfileHudConfig profile in Profiles)
            {
                CreateProfileRow(root.transform, profile);
            }
        }

        private static void CreateHeader(Transform parent)
        {
            GameObject header = CreateTextObject("Header", parent, "Life System", 22, new Color(0.97f, 0.97f, 1.0f, 1.0f), FontStyle.Bold, TextAnchor.MiddleLeft);
            LayoutElement element = header.AddComponent<LayoutElement>();
            element.preferredHeight = 28.0f;
        }

        private static void CreateProfileRow(Transform parent, ProfileHudConfig profile)
        {
            GameObject row = new GameObject(profile.Label + "Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(LifeUi));
            row.transform.SetParent(parent, false);

            LayoutElement rowElement = row.GetComponent<LayoutElement>();
            rowElement.preferredHeight = 34.0f;

            HorizontalLayoutGroup rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.spacing = 12.0f;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            GameObject label = CreateTextObject(profile.Label + "Label", row.transform, profile.Label, 18, profile.AccentColor, FontStyle.Bold, TextAnchor.MiddleLeft);
            LayoutElement labelElement = label.AddComponent<LayoutElement>();
            labelElement.preferredWidth = 120.0f;

            GameObject count = CreateTextObject(profile.Label + "Count", row.transform, "0", 18, new Color(1.0f, 1.0f, 1.0f, 1.0f), FontStyle.Bold, TextAnchor.MiddleCenter);
            LayoutElement countElement = count.AddComponent<LayoutElement>();
            countElement.preferredWidth = 52.0f;

            GameObject timer = CreateTextObject(profile.Label + "Timer", row.transform, "Full", 18, new Color(0.84f, 0.89f, 1.0f, 1.0f), FontStyle.Normal, TextAnchor.MiddleLeft);
            LayoutElement timerElement = timer.AddComponent<LayoutElement>();
            timerElement.preferredWidth = 120.0f;

            LifeUi lifeUi = row.GetComponent<LifeUi>();
            lifeUi.ProfileId = profile.ProfileId;
            lifeUi.m_LifeCount = count.GetComponent<Text>();
            lifeUi.m_TileRemaining = timer.GetComponent<Text>();
        }

        private static GameObject CreateTextObject(string objectName, Transform parent, string text, int fontSize, Color color, FontStyle fontStyle, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text uiText = textObject.GetComponent<Text>();
            uiText.text = text;
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontSize = fontSize;
            uiText.fontStyle = fontStyle;
            uiText.color = color;
            uiText.alignment = alignment;
            uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;
            uiText.raycastTarget = false;

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0.0f, 28.0f);

            return textObject;
        }
    }
}
