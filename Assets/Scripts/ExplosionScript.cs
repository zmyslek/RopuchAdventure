using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    [SerializeField]
    int sortingOrder = 12;

    void Start()
    {
        SetRenderSettingsRecursively(gameObject, sortingOrder);
    }

    void explosionFinished()
    {
        Destroy(gameObject);
    }

    void SetRenderSettingsRecursively(GameObject target, int order)
    {
        target.layer = 12;

        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 12;
        }

        foreach (Transform child in target.transform)
        {
            SetRenderSettingsRecursively(child.gameObject, order);
        }
    }
}
