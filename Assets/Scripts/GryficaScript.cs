using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GryficaScript : MonoBehaviour
{
    Animator ar;

    [SerializeField]
    GameObject gryficaExplosion;

    [SerializeField]
    float yeeshTime;

    [SerializeField]
    float yeeshAnimationSpeed;

    [SerializeField]
    float deathDelay;

    [SerializeField]
    int explosionLayer;

    [SerializeField]
    int explosionSortingOrder;

    int maxHits;
    int hitsTaken;

    bool isAttacking;
    bool isDying;

    void Start()
    {
        ar = gameObject.GetComponent<Animator>();

        maxHits = 3;
        yeeshTime = yeeshTime <= 0.0f ? 0.6f : yeeshTime;
        yeeshAnimationSpeed = yeeshAnimationSpeed <= 0.0f ? 1.0f : yeeshAnimationSpeed;
        deathDelay = deathDelay <= 0.0f ? 0.2f : deathDelay;
        explosionLayer = explosionLayer < 0 ? 11 : explosionLayer;
        explosionSortingOrder = explosionSortingOrder <= 0 ? 12 : explosionSortingOrder;

        isAttacking = false;
        isDying = false;
        hitsTaken = 0;

        ar.SetInteger("State", 0);
        ar.speed = 1.0f;
    }

    void Update()
    {
        if (!isAttacking && !isDying)
        {
            ar.SetInteger("State", 0);
            ar.speed = 1.0f;
        }
    }

    public void decrLifeUnits()
    {
        if (isDying)
        {
            return;
        }

        hitsTaken++;

        if (hitsTaken >= maxHits)
        {
            StartCoroutine(PlayYeeshAndDie());
            return;
        }

        StartCoroutine(PlayAttackOnly());
    }

    IEnumerator PlayAttackOnly()
    {
        if (isAttacking || isDying)
        {
            yield break;
        }

        isAttacking = true;
        ar.SetInteger("State", 1);
        ar.speed = 1.0f;

        // Wait for the full attack state to complete so long clips are fully visible.
        yield return null;
        while (!ar.GetCurrentAnimatorStateInfo(0).IsName("Gryfica-attack"))
        {
            if (isDying)
            {
                isAttacking = false;
                yield break;
            }

            yield return null;
        }

        while (ar.GetCurrentAnimatorStateInfo(0).IsName("Gryfica-attack") && ar.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            if (isDying)
            {
                isAttacking = false;
                yield break;
            }

            yield return null;
        }

        isAttacking = false;

        if (!isDying)
        {
            ar.SetInteger("State", 0);
            ar.speed = 1.0f;
        }
    }

    IEnumerator PlayYeeshAndDie()
    {
        isDying = true;
        isAttacking = false;
        ar.SetInteger("State", 2);
        ar.speed = yeeshAnimationSpeed;

        float yeeshDuration = yeeshTime / Mathf.Max(yeeshAnimationSpeed, 0.01f);
        yield return new WaitForSeconds(yeeshDuration);

        yield return new WaitForSeconds(deathDelay);

        if (gryficaExplosion != null)
        {
            GameObject explosion = Instantiate(gryficaExplosion, transform.position, transform.rotation);
            SetExplosionRenderSettings(explosion);
        }

        Destroy(gameObject);
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
