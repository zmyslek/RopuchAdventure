using System.Collections;
using UnityEngine;

public class DziuniaImpactScript : MonoBehaviour
{
    [SerializeField]
    GameObject dziuniaExplosion;

    [SerializeField]
    float impactDelay = 0.1f;

    [SerializeField]
    int explosionLayer;

    [SerializeField]
    int explosionSortingOrder;

    bool isImpacting;

    void Start()
    {
        explosionLayer = explosionLayer < 0 ? 11 : explosionLayer;
        explosionSortingOrder = explosionSortingOrder <= 0 ? 12 : explosionSortingOrder;

        isImpacting = false;
    }

    public void PlayImpactReaction()
    {
        if (!isImpacting)
        {
            StartCoroutine(ImpactExplosion());
        }
    }

    IEnumerator ImpactExplosion()
    {
        isImpacting = true;

        if (dziuniaExplosion != null)
        {
            Vector3 explosionPos = transform.position;
            GameObject explosion = Instantiate(dziuniaExplosion, explosionPos, transform.rotation);
            SetExplosionRenderSettings(explosion);
        }

        yield return new WaitForSeconds(impactDelay);

        isImpacting = false;
    }

    void SetExplosionRenderSettings(GameObject explosion)
    {
        if (explosion == null)
        {
            return;
        }

        explosion.layer = explosionLayer;

        foreach (Transform child in explosion.transform)
        {
            SetExplosionRenderSettings(child.gameObject);
        }

        SpriteRenderer spriteRenderer = explosion.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = explosionSortingOrder;
        }
    }
}
