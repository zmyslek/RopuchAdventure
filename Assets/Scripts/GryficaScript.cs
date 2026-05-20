/*
 * Script: GryficaScript.cs
 * Purpose: Enemy behaviour for Gryfica (attacks, damage and death handling).
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechJuego.LifeSystem;

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

    [SerializeField]
    float deathExplosionAnimationSpeed;

    [SerializeField]
    AudioClip gnildaDeadAudio;

    int maxHits;
    int hitsTaken;

    bool isAttacking;
    bool isDying;
    bool isFacingLeft;

    float attackClipLength;

    public bool IsDying
    {
        get { return isDying; }
    }

    void Start()
    {
        LifeSystemBootstrap.EnsureInitialized();

        ar = gameObject.GetComponent<Animator>();
        if (ar == null)
        {
            ar = GetComponentInChildren<Animator>();
        }

        sr = gameObject.GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

        if (ar == null || sr == null)
        {
            Debug.LogError("GryficaScript: missing required components.");
            enabled = false;
            return;
        }

        maxHits = 3;
        yeeshTime = yeeshTime <= 0.0f ? 0.6f : yeeshTime;
        yeeshAnimationSpeed = yeeshAnimationSpeed <= 0.0f ? 0.65f : yeeshAnimationSpeed;
        deathDelay = deathDelay <= 0.0f ? 0.6f : deathDelay;
        deathExplosionAnimationSpeed = deathExplosionAnimationSpeed <= 0.0f ? 0.6f : deathExplosionAnimationSpeed;
        explosionLayer = explosionLayer < 0 ? 11 : explosionLayer;
        explosionSortingOrder = explosionSortingOrder <= 0 ? 100 : explosionSortingOrder;

        isAttacking = false;
        isDying = false;
        isFacingLeft = transform.localScale.x < 0.0f;
        hitsTaken = 0;

        attackClipLength = GetAnimationClipLength("Gryfica-attack");

        ar.SetInteger("State", 0);
        ar.speed = 1.0f;

        if (sr != null)
        {
            sr.flipX = false;
        }

        // initialize local life display when LifeHandler isn't present
        var disp = GetComponentInChildren<EnemyLifeDisplay>();
        if (disp != null && LifeHandler.Instance == null)
        {
            disp.UpdateLocal(maxHits);
        }
    }

    void Update()
    {
        if (ar == null || sr == null)
        {
            return;
        }

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

        // If LifeHandler present, use per-profile lives for Gryfica
        if (LifeHandler.Instance != null)
        {
            LifeHandler.Instance.LooseLife("Gryf");
            int remainingProfile = LifeHandler.Instance.GetCurrentLifeCount("Gryf");
            if (remainingProfile <= 0)
            {
                ScoreState.AddEnemyKill(1);
                StartCoroutine(PlayYeeshAndDie());
                return;
            }

            StartCoroutine(PlayAttackOnly());
            return;
        }

        // fallback to local hit-count logic
        hitsTaken++;

        int remaining = Mathf.Max(0, maxHits - hitsTaken);
        var dispLocal = GetComponentInChildren<EnemyLifeDisplay>();
        if (dispLocal != null)
            dispLocal.UpdateLocal(remaining);

        if (hitsTaken >= maxHits)
        {
            ScoreState.AddEnemyKill(1);
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
            SetFacingLeft(player.transform.position.x < transform.position.x);
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
            SetExplosionAnimationSpeed(explosion);
        }

        PlayGnildaDeadAudio();

        float yeeshDuration = yeeshTime / Mathf.Max(yeeshAnimationSpeed, 0.01f);
        yield return new WaitForSeconds(yeeshDuration);

        yield return new WaitForSeconds(deathDelay);

        Destroy(transform.root.gameObject);
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

    void SetFacingLeft(bool faceLeft)
    {
        if (isFacingLeft == faceLeft)
        {
            return;
        }

        isFacingLeft = faceLeft;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceLeft ? -1.0f : 1.0f);
        transform.localScale = scale;

        if (sr != null)
        {
            sr.flipX = false;
        }
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
            animator.speed = deathExplosionAnimationSpeed;
        }

        foreach (Transform child in explosion.transform)
        {
            SetExplosionAnimationSpeed(child.gameObject);
        }
    }

    void PlayGnildaDeadAudio()
    {
        if (gnildaDeadAudio == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(gnildaDeadAudio, transform.position);
    }
}
