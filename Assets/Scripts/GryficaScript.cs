using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GryficaScript : MonoBehaviour
{
    Animator ar;
    SpriteRenderer sr;

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

    float attackClipLength;

    public bool IsDying
    {
        get { return isDying; }
    }

    void Start()
    {
        ar = gameObject.GetComponent<Animator>();
        sr = gameObject.GetComponentInChildren<SpriteRenderer>();

        maxHits = 3;
        yeeshTime = yeeshTime <= 0.0f ? 0.6f : yeeshTime;
        yeeshAnimationSpeed = yeeshAnimationSpeed <= 0.0f ? 1.0f : yeeshAnimationSpeed;
        deathDelay = deathDelay <= 0.0f ? 0.2f : deathDelay;
        explosionLayer = explosionLayer < 0 ? 11 : explosionLayer;
        explosionSortingOrder = explosionSortingOrder <= 0 ? 12 : explosionSortingOrder;

        isAttacking = false;
        isDying = false;
        hitsTaken = 0;

        attackClipLength = GetAnimationClipLength("Gryfica-attack");

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

        // Face the player before attacking so bullets fire in correct direction
        var player = FindObjectOfType<RopuchControllerScript>();
        if (player != null && sr != null)
        {
            sr.flipX = player.transform.position.x < transform.position.x;
        }

        isAttacking = true;
        ar.SetInteger("State", 1);
        ar.speed = 1.0f;

        float waitTime = Mathf.Max(attackClipLength, 5.25f);
        yield return new WaitForSeconds(waitTime);

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

        if (gryficaExplosion != null)
        {
            Vector3 explosionPos = GetGryfCenterPosition();
            GameObject explosion = Instantiate(gryficaExplosion, explosionPos, transform.rotation);
            SetExplosionRenderSettings(explosion);
        }

        float yeeshDuration = yeeshTime / Mathf.Max(yeeshAnimationSpeed, 0.01f);
        yield return new WaitForSeconds(yeeshDuration);

        yield return new WaitForSeconds(deathDelay);

        Destroy(gameObject);
    }

    float GetAnimationClipLength(string clipName)
    {
        if (ar == null || ar.runtimeAnimatorController == null)
        {
            return 0.0f;
        }

        AnimationClip[] clips = ar.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip != null && clip.name == clipName)
            {
                return clip.length;
            }
        }

        return 0.0f;
    }

    Vector3 GetGryfCenterPosition()
    {
        return transform.position;
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
