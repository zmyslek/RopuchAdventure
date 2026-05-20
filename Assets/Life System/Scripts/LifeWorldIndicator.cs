using UnityEngine;
using UnityEngine.UI;

namespace TechJuego.LifeSystem
{
    public class LifeWorldIndicator : MonoBehaviour
    {
        [SerializeField] private string ProfileId = "";
        [SerializeField] private float VerticalOffset = 2.0f;
        [SerializeField] private Vector2 PanelSize = new Vector2(180.0f, 72.0f);

        private Transform target;
        private Text lifeCountText;
        private Text remainingText;
        private bool initialized;

        public void Initialize(string profileId, Transform targetTransform, float verticalOffset)
        {
            ProfileId = NormalizeProfileId(profileId);
            target = targetTransform;
            VerticalOffset = verticalOffset;
            BuildIfNeeded();
            SyncFromHandler();
        }

        private void Awake()
        {
            BuildIfNeeded();
        }

        private void OnEnable()
        {
            LifeEvents.OnGetLifeDetail += OnLifeEvent;
            BuildIfNeeded();
            SyncFromHandler();
        }

        private void Start()
        {
            BuildIfNeeded();
            SyncFromHandler();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = target.position + new Vector3(0.0f, VerticalOffset, 0.0f);
        }

        private void OnDisable()
        {
            LifeEvents.OnGetLifeDetail -= OnLifeEvent;
        }

        private void OnLifeEvent(string profileId, int lifecount, string remainingTime)
        {
            if (string.IsNullOrEmpty(ProfileId) || !string.Equals(ProfileId, NormalizeProfileId(profileId), System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            UpdateText(lifecount, remainingTime);
        }

        private void BuildIfNeeded()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            if (target == null)
            {
                target = transform.parent;
            }

            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            rectTransform.sizeDelta = PanelSize;

            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 500;

            if (GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 10.0f;
                scaler.referencePixelsPerUnit = 100.0f;
            }

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            transform.localScale = Vector3.one * 0.01f;
            transform.localRotation = Quaternion.identity;

            Image background = gameObject.AddComponent<Image>();
            background.sprite = Resources.Load<Sprite>("Images/LifeSystem/RoundedRectangle");
            background.color = new Color(0.03f, 0.03f, 0.05f, 0.75f);
            background.type = Image.Type.Sliced;

            VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 2.0f;
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject upperRow = new GameObject("UpperRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            upperRow.transform.SetParent(transform, false);

            HorizontalLayoutGroup rowLayout = upperRow.GetComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = 6.0f;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            GameObject heart = CreateHeartObject(upperRow.transform);
            LayoutElement heartElement = heart.AddComponent<LayoutElement>();
            heartElement.preferredWidth = 28.0f;
            heartElement.preferredHeight = 28.0f;

            GameObject countObject = CreateTextObject("Count", upperRow.transform, "0", 18, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
            LayoutElement countElement = countObject.AddComponent<LayoutElement>();
            countElement.preferredWidth = 42.0f;

            remainingText = CreateTextObject("Time", transform, "Full", 14, new Color(0.88f, 0.9f, 1.0f, 1.0f), FontStyle.Normal, TextAnchor.MiddleCenter).GetComponent<Text>();
            LayoutElement remainingElement = remainingText.gameObject.AddComponent<LayoutElement>();
            remainingElement.preferredHeight = 18.0f;

            lifeCountText = countObject.GetComponent<Text>();
            transform.name = string.IsNullOrEmpty(ProfileId) ? "LifeWorldIndicator" : ProfileId + "LifeIndicator";
        }

        private GameObject CreateHeartObject(Transform parent)
        {
            GameObject heartPrefab = Resources.Load<GameObject>("Prefabs/HeartContainer");
            if (heartPrefab != null)
            {
                GameObject heart = Instantiate(heartPrefab, parent, false);
                heart.name = "Heart";
                RectTransform rect = heart.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(28.0f, 28.0f);
                }

                return heart;
            }

            GameObject fallback = new GameObject("Heart", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fallback.transform.SetParent(parent, false);
            Image image = fallback.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("Images/LifeSystem/red-heart");
            image.preserveAspect = true;
            return fallback;
        }

        private GameObject CreateTextObject(string objectName, Transform parent, string text, int fontSize, Color color, FontStyle fontStyle, TextAnchor alignment)
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
            rect.sizeDelta = new Vector2(0.0f, 24.0f);

            return textObject;
        }

        private void UpdateText(int lifeCount, string remainingTime)
        {
            if (lifeCountText != null)
            {
                lifeCountText.text = lifeCount.ToString();
            }

            if (remainingText != null)
            {
                remainingText.text = remainingTime;
            }
        }

        private void SyncFromHandler()
        {
            if (LifeHandler.Instance == null || string.IsNullOrEmpty(ProfileId))
            {
                return;
            }

            if (LifeHandler.Instance.TryGetLifeState(ProfileId, out int lifeCount, out string remainingTime))
            {
                UpdateText(lifeCount, remainingTime);
            }
        }

        private static string NormalizeProfileId(string profileId)
        {
            if (string.Equals(profileId, "Gryfica", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Gryf";
            }

            return profileId;
        }
    }
}