/*
 * Script: DziuniaScript.cs
 * Purpose: Controls Dziunia enemy AI, movement and hit handling.
 */
using System.Collections;
using UnityEngine;
using TechJuego.LifeSystem;
public class DziuniaScript : MonoBehaviour
{
    [SerializeField]
    int lives = 10;

    [SerializeField]
    GameObject bullet;

    [SerializeField]
    Transform leftGun;

    [SerializeField]
    Transform rightGun;

    [SerializeField]
    float shootCooldown = 1.5f;

    [SerializeField]
    bool useRandomShootCooldown = false;

    [SerializeField]
    float minShootCooldown = 1.0f;

    [SerializeField]
    float maxShootCooldown = 3.0f;

    [Header("Spawn Throttling")]
    [Tooltip("Chance to actually fire when cooldown elapses (0.0 - 1.0). Lower reduces frequency without changing cooldowns.")]
    [SerializeField]
    float shootChance = 1.0f;

    [Tooltip("Maximum consecutive shots before forcing a longer burst cooldown. Set 0 to disable.")]
    [SerializeField]
    int maxConsecutiveShots = 1;

    [Tooltip("Cooldown (seconds) applied after a burst of consecutive shots.")]
    [SerializeField]
    float burstCooldown = 2.0f;

    [SerializeField]
    float shootIntervalMultiplier = 0.9f;

    int consecutiveShots = 0;

    [SerializeField]
    float initialShootDelay = 2.0f;

    [SerializeField]
    float deathDelay = 0.4f;

    [SerializeField]
    GameObject deathExplosion;

    [SerializeField]
    float deathExplosionAnimationSpeed = 0.6f;

    [SerializeField]
    int deathExplosionLayer;

    [SerializeField]
    int deathExplosionSortingOrder;

    [SerializeField]
    AudioClip dziuniaShowAudio;

    AudioSource dziuniaShowAudioSource;

    Animator animator;

    SpriteRenderer sr;
    DziuniaImpactScript impactScript;
    float nextShootTime;
    bool canShoot;
    bool isDying;

    float ApplyShootIntervalMultiplier(float interval)
    {
        return Mathf.Max(0.0f, interval) * Mathf.Clamp(shootIntervalMultiplier, 0.1f, 2.0f);
    }

    void Start()
    {
        LifeSystemBootstrap.EnsureInitialized();

        animator = gameObject.GetComponent<Animator>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        impactScript = gameObject.GetComponent<DziuniaImpactScript>();
        nextShootTime = Time.time + initialShootDelay;
        canShoot = false;
        isDying = false;

        lives = lives <= 0 ? 10 : lives;
        deathDelay = deathDelay <= 0.0f ? 1.4f : deathDelay;
        deathExplosionAnimationSpeed = deathExplosionAnimationSpeed <= 0.0f ? 0.6f : deathExplosionAnimationSpeed;
        deathExplosionLayer = deathExplosionLayer < 0 ? 12 : deathExplosionLayer;
        deathExplosionSortingOrder = deathExplosionSortingOrder <= 0 ? 100 : deathExplosionSortingOrder;

        dziuniaShowAudioSource = gameObject.GetComponent<AudioSource>();
        if (dziuniaShowAudioSource == null)
        {
            dziuniaShowAudioSource = gameObject.AddComponent<AudioSource>();
        }

        dziuniaShowAudioSource.playOnAwake = false;
        dziuniaShowAudioSource.loop = true;
        dziuniaShowAudioSource.spatialBlend = 0.0f;
        dziuniaShowAudioSource.volume = 1.0f;

        if (leftGun == null)
        {
            GameObject leftGunObject = GameObject.FindGameObjectWithTag("DziuniaLeftGun");
            if (leftGunObject != null)
            {
                leftGun = leftGunObject.transform;
            }
        }

        if (rightGun == null)
        {
            GameObject rightGunObject = GameObject.FindGameObjectWithTag("DziuniaRightGun");
            if (rightGunObject != null)
            {
                rightGun = rightGunObject.transform;
            }
        }

        // initialize local life display when LifeHandler isn't present yet
        var disp = GetComponentInChildren<EnemyLifeDisplay>();
        if (disp != null && LifeHandler.Instance == null)
        {
            disp.UpdateLocal(lives);
        }
    }

    void Update()
    {
        if (!isDying && canShoot && Time.time >= nextShootTime)
        {
            ShootBullet();
        }
    }

    private void OnBecameVisible()
    {
        if (isDying)
        {
            return;
        }

        canShoot = true;
        // Delay the first shot when becoming visible to avoid immediate burst
        nextShootTime = Time.time + ApplyShootIntervalMultiplier(initialShootDelay);
        consecutiveShots = 0;

        // Play audio with DziuniaShow tag
        PlayDziuniaShowAudio();
        SetRopuchFairyAudioMuted(true);
        AudioMuteManager.MuteRopuch = true;
    }

    private void OnBecameInvisible()
    {
        canShoot = false;
        StopDziuniaShowAudio();
        SetRopuchFairyAudioMuted(false);
        AudioMuteManager.MuteRopuch = false;
        consecutiveShots = 0;
    }

    public void ShootBullet()
    {
        if (bullet == null || isDying || Time.time < nextShootTime)
        {
            return;
        }
        // Chance check: skip firing sometimes to reduce overall frequency
        if (shootChance < 1.0f && Random.value > Mathf.Clamp01(shootChance))
        {
            // schedule next attempt
            if (useRandomShootCooldown)
            {
                float minC = Mathf.Max(0.0f, minShootCooldown);
                float maxC = Mathf.Max(minC, maxShootCooldown);
                nextShootTime = Time.time + ApplyShootIntervalMultiplier(Random.Range(minC, maxC));
            }
            else
            {
                nextShootTime = Time.time + ApplyShootIntervalMultiplier(shootCooldown);
            }
            consecutiveShots = 0;
            return;
        }
        Transform gun = leftGun;
        if (gun == null)
        {
            return;
        }

        GameObject newBullet = Instantiate(bullet, gun.position, transform.rotation);

        DziuniaBulletScript bulletScript = newBullet.GetComponent<DziuniaBulletScript>();
        if (bulletScript != null)
        {
            bulletScript.setDir(true); // Always shoot left
        }

        // Mark a shot and compute standard next cooldown
        consecutiveShots++;
        if (useRandomShootCooldown)
        {
            float minC = Mathf.Max(0.0f, minShootCooldown);
            float maxC = Mathf.Max(minC, maxShootCooldown);
            nextShootTime = Time.time + ApplyShootIntervalMultiplier(Random.Range(minC, maxC));
        }
        else
        {
            nextShootTime = Time.time + ApplyShootIntervalMultiplier(shootCooldown);
        }

        // If we've fired enough consecutive shots, enforce a longer burst cooldown
        if (maxConsecutiveShots > 0 && consecutiveShots >= maxConsecutiveShots)
        {
            nextShootTime = Time.time + ApplyShootIntervalMultiplier(burstCooldown);
            consecutiveShots = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDying)
        {
            return;
        }

        if (!collision.gameObject.CompareTag("Gryf"))
        {
            return;
        }

        TakeHit();
    }

    public void TakeHit()
    {
        if (isDying)
        {
            return;
        }

        // If LifeHandler profile exists, use it as source of truth
        if (LifeHandler.Instance != null)
        {
            LifeHandler.Instance.LooseLife("Dziunia");
            int remaining = LifeHandler.Instance.GetCurrentLifeCount("Dziunia");
            if (remaining <= 0)
            {
                isDying = true;
                canShoot = false;
                StartCoroutine(PlayDeathSequence());
                return;
            }

            if (impactScript != null)
            {
                impactScript.PlayImpactReaction();
            }

            return;
        }

        // fallback to local lives
        if (lives <= 0)
            return;

        lives--;
        var disp2 = GetComponentInChildren<EnemyLifeDisplay>();
        if (disp2 != null)
            disp2.UpdateLocal(lives);

        if (lives <= 0)
        {
            isDying = true;
            canShoot = false;
            StartCoroutine(PlayDeathSequence());
            return;
        }

        if (impactScript != null)
        {
            impactScript.PlayImpactReaction();
        }
    }

    IEnumerator PlayDeathSequence()
    {
        StopDeathAnimation();
        RotateDeathVisual();

        if (deathExplosion != null)
        {
            GameObject explosion = Instantiate(deathExplosion, transform.position, transform.rotation);
            SetExplosionRenderSettings(explosion);
            SetExplosionAnimationSpeed(explosion);
        }
        else if (impactScript != null)
        {
            impactScript.PlayImpactReaction();
        }

        float waitTime = Mathf.Max(deathDelay, 1.4f);
        // wait for the explosion/impact to at least start and be visible
        yield return new WaitForSeconds(waitTime);

        // Count this Dziunia as killed 
        ScoreState.AddEnemyKill(1);
        StopDziuniaShowAudio();
        SetRopuchFairyAudioMuted(false);
        AudioMuteManager.MuteRopuch = false;
        Destroy(gameObject);
    }

    void RotateDeathVisual()
    {
        if (sr == null)
        {
            return;
        }

        Transform visualTransform = sr.transform;
        if (visualTransform != transform)
        {
            Vector3 euler = visualTransform.eulerAngles;
            euler.x += 180.0f;
            visualTransform.eulerAngles = euler;
            return;
        }

        sr.flipY = !sr.flipY;
    }

    void StopDeathAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.speed = 0.0f;
        animator.enabled = false;
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
                renderer.sortingOrder = deathExplosionSortingOrder;
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

    void PlayDziuniaShowAudio()
    {
        if (dziuniaShowAudio == null)
        {
            return;
        }

        if (dziuniaShowAudioSource == null)
        {
            return;
        }

        dziuniaShowAudioSource.clip = dziuniaShowAudio;
        dziuniaShowAudioSource.loop = true;

        if (!dziuniaShowAudioSource.isPlaying)
        {
            dziuniaShowAudioSource.Play();
        }
    }

    void StopDziuniaShowAudio()
    {
        if (dziuniaShowAudioSource != null && dziuniaShowAudioSource.isPlaying)
        {
            dziuniaShowAudioSource.Stop();
        }
    }

    void SetRopuchFairyAudioMuted(bool muted)
    {
        RopuchControllerScript ropuch = FindObjectOfType<RopuchControllerScript>();
        if (ropuch != null)
        {
            ropuch.SetFairyAudioMuted(muted);
        }
    }
}
