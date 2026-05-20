using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TechJuego.LifeSystem
{
    public class LifeWorldIndicator : MonoBehaviour
    {
        [SerializeField] private string ProfileId = "";
        [SerializeField] private float VerticalOffset = 2.0f;
        [SerializeField] private Vector2 PanelSize = new Vector2(180.0f, 40.0f);
        [SerializeField] private float HeartSize = 22.0f;
        [SerializeField] private float HeartSpacing = 2.0f;

        private Transform target;
        private RectTransform heartsRoot;
        private readonly List<GameObject> heartObjects = new List<GameObject>();
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

            UpdateHearts(lifecount);
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
            layout.spacing = 0.0f;
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject heartsObject = new GameObject("Hearts", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            heartsObject.transform.SetParent(transform, false);

            heartsRoot = heartsObject.GetComponent<RectTransform>();

            HorizontalLayoutGroup rowLayout = heartsObject.GetComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.spacing = HeartSpacing;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            LayoutElement rowElement = heartsObject.GetComponent<LayoutElement>();
            rowElement.preferredHeight = HeartSize;

            transform.name = string.IsNullOrEmpty(ProfileId) ? "LifeWorldIndicator" : ProfileId + "LifeIndicator";
        }

        private GameObject CreateHeartObject(Transform parent, int index)
        {
            GameObject heart = new GameObject("Heart" + index, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            heart.transform.SetParent(parent, false);

            RectTransform rect = heart.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(HeartSize, HeartSize);
            rect.localScale = Vector3.one;

            Image image = heart.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("Images/LifeSystem/red-heart");
            image.preserveAspect = true;
            image.raycastTarget = false;

            if (image.sprite == null)
            {
                image.enabled = false;
            }

            return heart;
        }

        private void UpdateHearts(int lifeCount)
        {
            if (heartsRoot == null)
            {
                return;
            }

            while (heartObjects.Count < lifeCount)
            {
                GameObject heart = CreateHeartObject(heartsRoot, heartObjects.Count);
                heartObjects.Add(heart);
            }

            while (heartObjects.Count > lifeCount)
            {
                GameObject heart = heartObjects[heartObjects.Count - 1];
                heartObjects.RemoveAt(heartObjects.Count - 1);
                if (heart != null)
                {
                    Destroy(heart);
                }
            }

            for (int i = 0; i < heartObjects.Count; i++)
            {
                GameObject heart = heartObjects[i];
                if (heart == null)
                {
                    continue;
                }

                heart.SetActive(i < lifeCount);
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
                UpdateHearts(lifeCount);
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