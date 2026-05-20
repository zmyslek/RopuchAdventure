/*
 * Script: DziuniaImpactScript.cs
 * Purpose: Plays an explosion/impact effect when Dziunia impacts.
 */
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

    [SerializeField]
    float explosionAnimationSpeed;

    bool isImpacting;

    public bool IsImpacting { get { return isImpacting; } }

    void Start()
    {
        explosionLayer = explosionLayer < 0 ? 12 : explosionLayer;
        explosionSortingOrder = explosionSortingOrder <= 0 ? 100 : explosionSortingOrder;
        impactDelay = impactDelay <= 0.0f ? 0.8f : impactDelay;
        explosionAnimationSpeed = explosionAnimationSpeed <= 0.0f ? 0.35f : explosionAnimationSpeed;

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
            DontDestroyOnLoad(explosion);
            SetExplosionRenderSettings(explosion);
            SetExplosionAnimationSpeed(explosion);
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

        explosion.layer = 12;

        foreach (Transform child in explosion.transform)
        {
            SetExplosionRenderSettings(child.gameObject);
        }

        Renderer[] renderers = explosion.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.sortingOrder = explosionSortingOrder;
            }
        }
    }

    void SetExplosionAnimationSpeed(GameObject explosion)
    {
        if (explosion == null)
        {
            return;
        }

        Animator animator = explosion.GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = explosionAnimationSpeed;
        }

        foreach (Transform child in explosion.transform)
        {
            SetExplosionAnimationSpeed(child.gameObject);
        }
    }
}
