using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactScript : MonoBehaviour
{
    [SerializeField]
    int sortingOrder = 12;

    void Start()
    {
        SetSortingOrderRecursively(gameObject, sortingOrder);
    }

    public void impactFinished()
    {
        Destroy(gameObject);
    }

    void SetSortingOrderRecursively(GameObject target, int order)
    {
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = order;
        }

        foreach (Transform child in target.transform)
        {
            SetSortingOrderRecursively(child.gameObject, order);
        }
    }
}
