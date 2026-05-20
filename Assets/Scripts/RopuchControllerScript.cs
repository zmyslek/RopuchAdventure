using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TechJuego.LifeSystem;

public class RopuchControllerScript : MonoBehaviour
{
    const float ouchSoundCooldown = 0.4f;

    Animator ar;
    Rigidbody2D rb;
    SpriteRenderer sr;

    [SerializeField]
    int lives;

    float walkSpeed;
    float jumpForce;
    float jumpHorizontalForce;
    float attackAnimationTime;
    float attackRecoveryTime;
    float moveX;

    bool canJump;
    bool jumpRequested;
    bool isAttacking;
    bool isDying;

    [SerializeField]
    float gryfDamageCooldown = 0.75f;

    float nextGryfDamageTime;

    int maxJumpCount;
    int jumpsRemaining;
    float jumpDirectionX;
    int attackRestoreState;

    [SerializeField]
    AudioClip ouchAudio;

    float nextOuchSoundTime;

    [SerializeField]
    GameObject bullet;

    [SerializeField]
    GameObject deathExplosion;

    [SerializeField]
    float deathSequenceDelay = 0.8f;

    [SerializeField]
    float deathExplosionAnimationSpeed = 0.6f;

    [SerializeField]
    int deathExplosionLayer;

    [SerializeField]
    int deathExplosionSortingOrder;

    GameObject lGun;
    GameObject rGun;

    AudioSource fairyAudioSource;

    void Start()
    {
        LifeSystemBootstrap.EnsureInitialized();

        ar = gameObject.GetComponent<Animator>();
        if (ar == null)
        {
            ar = GetComponentInChildren<Animator>();
        }

        rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody2D>();
        }

        sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>(true);
        }

        if (ar == null || rb == null || sr == null)
        {
            Debug.LogError("RopuchControllerScript: missing required components.");
            enabled = false;
            return;
        }

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        lGun = GameObject.FindGameObjectWithTag("LeftGun");
        rGun = GameObject.FindGameObjectWithTag("RightGun");

        walkSpeed = 3.5f;
        jumpForce = 6.9f;
        jumpHorizontalForce = 1.5f;
        attackAnimationTime = 0.5f;
        attackRecoveryTime = 0.3f;
        maxJumpCount = 2;
        jumpsRemaining = maxJumpCount;

        lives = lives <= 0 ? 5 : lives;

        canJump = false;
        jumpRequested = false;
        nextOuchSoundTime = 0.0f;

        isAttacking = false;
        isDying = false;
        nextGryfDamageTime = 0.0f;
        deathSequenceDelay = deathSequenceDelay <= 0.0f ? 1.4f : deathSequenceDelay;
        deathExplosionAnimationSpeed = deathExplosionAnimationSpeed <= 0.0f ? 0.6f : deathExplosionAnimationSpeed;
        deathExplosionLayer = deathExplosionLayer < 0 ? 12 : deathExplosionLayer;
        deathExplosionSortingOrder = deathExplosionSortingOrder <= 0 ? 100 : deathExplosionSortingOrder;

        ResolveFairyAudioSource();
    }

    void Update()
    {
        if (isDying || ar == null || rb == null || sr == null)
        {
            return;
        }

        moveX = Input.GetAxisRaw("Horizontal");

        if (canJump)
        {
            bool hasAnyInput = Mathf.Abs(moveX) > 0.01f || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return);

            if (Mathf.Abs(moveX) > 0.01f)
            {
                sr.flipX = moveX < 0.0f;
                ar.SetInteger("State", 1);
            }

            if (Input.GetKeyDown(KeyCode.Space) && !isAttacking && jumpsRemaining > 0)
            {
                ar.SetInteger("State", 1);
                jumpDirectionX = Mathf.Abs(moveX) > 0.01f ? Mathf.Sign(moveX) : (sr.flipX ? -1.0f : 1.0f);
                jumpRequested = true;
            }
            else if (Input.GetKeyDown(KeyCode.Return) && !isAttacking)
            {
                StartCoroutine(PlayAttackSequence());
            }
            else if (!hasAnyInput && !isAttacking)
            {
                ar.SetInteger("State", 0);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return) && !isAttacking)
            {
                StartCoroutine(PlayAttackSequence());
            }

            if (!isAttacking)
            {
                ar.SetInteger("State", 1);
            }
        }

        if (isAttacking)
        {
            rb.velocity = new Vector2(0.0f, 0.0f);
        }

        bool isMovingOrAirborne = !canJump || Mathf.Abs(moveX) > 0.01f || Mathf.Abs(rb.velocity.x) > 0.01f || Mathf.Abs(rb.velocity.y) > 0.01f;
        ar.speed = (ar.GetInteger("State") == 0 && isMovingOrAirborne) ? 0.0f : 1.0f;
    }

    IEnumerator PlayAttackSequence()
    {
        isAttacking = true;
        attackRestoreState = ar.GetInteger("State");
        if (attackRestoreState != 0 && attackRestoreState != 1)
        {
            attackRestoreState = 0;
        }

        bool wasIdle = attackRestoreState == 0;
        bool showAttackAnim = wasIdle && canJump; // only play attack anim if idle and on ground

        if (showAttackAnim)
        {
            rb.velocity = new Vector2(0.0f, 0.0f);
            ar.SetInteger("State", 2);
            ar.speed = 1.0f;

            yield return new WaitForSeconds(attackAnimationTime);

            ShootBullet();

            yield return new WaitForSeconds(attackRecoveryTime);

            isAttacking = false;
            ar.SetInteger("State", attackRestoreState);
            ar.speed = 1.0f;
        }
        else
        {
            // Shooting without changing animation (e.g., while jumping or walking)
            ShootBullet();
            yield return new WaitForSeconds(attackRecoveryTime);
            isAttacking = false;
        }
    }

    void FixedUpdate()
    {
        if (isDying)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            return;
        }

        if (rb == null)
        {
            return;
        }

        if (jumpRequested)
        {
            rb.velocity = new Vector2(jumpDirectionX * jumpHorizontalForce, jumpForce);
            jumpsRemaining--;
            jumpRequested = false;
            canJump = false;
        }

        // Only allow walking while grounded.
        if (canJump && !isAttacking && Mathf.Abs(moveX) > 0.01f)
        {
            float currentSpeed = walkSpeed;
            Vector2 nextPos = rb.position + new Vector2(moveX * currentSpeed * Time.fixedDeltaTime, 0.0f);
            rb.MovePosition(nextPos);
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }
        else if (isAttacking)
        {
            rb.velocity = new Vector2(0.0f, 0.0f);
        }
    }

    void ShootBullet()
    {
        if (bullet == null || sr == null)
        {
            return;
        }

        GameObject gun = sr.flipX ? lGun : rGun;
        if (gun == null)
        {
            return;
        }

        Vector3 iniPos = gun.transform.position;
        GameObject newBullet = Instantiate(bullet, iniPos, transform.rotation) as GameObject;

        RopuchBulletScript ropuchBulletScript = newBullet.GetComponent<RopuchBulletScript>();
        if (ropuchBulletScript != null)
        {
            ropuchBulletScript.setDir(sr.flipX);
            return;
        }

        RopuchBulletScript bulletScript = newBullet.GetComponent<RopuchBulletScript>();
        if (bulletScript != null)
        {
            bulletScript.setDir(sr.flipX);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDying)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Portal"))
        {
            Debug.Log("Portal triggered: setting final lives and loading end scene. FinalLives before=" + ScoreState.FinalLives);
            ScoreState.FinalLives = Mathf.Max(1, ScoreState.FinalLives);
            SceneManager.LoadScene(2);
            return;
        }

        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Platform"))
        {
            canJump = true;
            jumpsRemaining = maxJumpCount;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDying)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Platform"))
        {
            if (IsOnTopOfCollider(collision))
            {
                canJump = true;
                jumpsRemaining = maxJumpCount;
            }
        }

        if (collision.gameObject.CompareTag("Gryf"))
        {
            GryficaScript gryfica = collision.GetComponent<GryficaScript>();
            if (gryfica != null && !gryfica.IsDying && Time.time >= nextGryfDamageTime)
            {
                nextGryfDamageTime = Time.time + Mathf.Max(0.05f, gryfDamageCooldown);
                LoseLife();
                gryfica.decrLifeUnits();
            }
        }

        if (collision.gameObject.CompareTag("Dziunia"))
        {
            DziuniaScript dziunia = collision.GetComponent<DziuniaScript>();
            if (dziunia != null && Time.time >= nextGryfDamageTime)
            {
                nextGryfDamageTime = Time.time + Mathf.Max(0.05f, gryfDamageCooldown);
                LoseLife();
                dziunia.TakeHit();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Platform"))
        {
            canJump = false;
        }
    }

    bool IsOnTopOfCollider(Collider2D collision)
    {
        Collider2D ownCollider = rb != null ? rb.GetComponent<Collider2D>() : null;
        if (ownCollider == null)
        {
            ownCollider = GetComponent<Collider2D>();
        }

        if (ownCollider == null)
        {
            return false;
        }

        Bounds rBounds = ownCollider.bounds;
        Bounds cBounds = collision.bounds;
        float ropuchBottomY = rBounds.min.y;
        float colliderTopY = cBounds.max.y;
        return Mathf.Abs(ropuchBottomY - colliderTopY) < 0.1f;
    }

    public void LoseLife()
    {
        if (isDying)
        {
            return;
        }

        PlayOuchSound();

        if (LifeHandler.Instance != null)
        {
            LifeHandler.Instance.LooseLife("Ropuch");
            if (!LifeHandler.Instance.CanPlay("Ropuch"))
            {
                ScoreState.FinalLives = 0;
                StartCoroutine(PlayDeathAndLoadEndScene());
            }
            return;
        }

        // fallback to local lives if LifeHandler not present
        lives--;
        if (lives <= 0)
        {
            ScoreState.FinalLives = 0;
            StartCoroutine(PlayDeathAndLoadEndScene());
        }
    }

    public void AddLife(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        if (LifeHandler.Instance != null)
        {
            for (int i = 0; i < amount; i++)
            {
                LifeHandler.Instance.AddLife("Ropuch");
            }

            return;
        }

        lives = Mathf.Min(5, lives + amount);
    }

    IEnumerator PlayDeathAndLoadEndScene()
    {
        if (isDying)
        {
            yield break;
        }

        isDying = true;
        isAttacking = false;
        canJump = false;
        jumpRequested = false;

        rb.velocity = Vector2.zero;
        ar.speed = 1.0f;
        ar.SetInteger("State", 0);

        RotateDeathVisual();

        if (deathExplosion != null)
        {
            GameObject explosion = Instantiate(deathExplosion, transform.position, transform.rotation);
            SetExplosionRenderSettings(explosion);
            SetExplosionAnimationSpeed(explosion);
        }

        yield return new WaitForSeconds(deathSequenceDelay);

        Destroy(gameObject);
        SceneManager.LoadScene(2);
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

    void RotateDeathVisual()
    {
        if (sr == null)
        {
            return;
        }

        // Rotate only visual transform so camera/follow objects parented to Ropuch are unaffected.
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

    void PlayOuchSound()
    {
        if (Time.time < nextOuchSoundTime)
        {
            return;
        }

        if (ouchAudio == null)
        {
            return;
        }

        if (AudioMuteManager.MuteRopuch)
        {
            return;
        }

        nextOuchSoundTime = Time.time + ouchSoundCooldown;
        AudioSource.PlayClipAtPoint(ouchAudio, transform.position);
    }

    public void SetFairyAudioMuted(bool muted)
    {
        ResolveFairyAudioSource();

        if (fairyAudioSource != null)
        {
            fairyAudioSource.mute = muted;
        }
    }

    void ResolveFairyAudioSource()
    {
        if (fairyAudioSource != null)
        {
            return;
        }

        AudioSource[] sources = GetComponentsInChildren<AudioSource>(true);
        foreach (AudioSource source in sources)
        {
            if (source == null)
            {
                continue;
            }

            string sourceName = source.gameObject.name.ToLowerInvariant();
            string clipName = source.clip != null ? source.clip.name.ToLowerInvariant() : string.Empty;
            if (sourceName.Contains("fairy") || clipName.Contains("fairy"))
            {
                fairyAudioSource = source;
                break;
            }
        }
    }
}
