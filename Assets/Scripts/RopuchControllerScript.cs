using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RopuchControllerScript : MonoBehaviour
{
    const float ouchSoundCooldown = 0.4f;

    Animator ar;
    Rigidbody2D rb;
    SpriteRenderer sr;

    [SerializeField]
    int lives;

    float speed;
    float walkSpeed;
    float jumpForce;
    float jumpHorizontalForce;
    float attackAnimationTime;
    float attackRecoveryTime;
    float moveX;

    bool canJump;
    bool jumpRequested;
    bool isAttacking;

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

    GameObject lGun;
    GameObject rGun;

    void Start()
    {
        ar = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        lGun = GameObject.FindGameObjectWithTag("LeftGun");
        rGun = GameObject.FindGameObjectWithTag("RightGun");

        speed = 7.0f;
        walkSpeed = 3.5f;
        jumpForce = 6.9f;
        jumpHorizontalForce = 1.5f;
        attackAnimationTime = 0.5f;
        attackRecoveryTime = 0.3f;
        maxJumpCount = 2;
        jumpsRemaining = maxJumpCount;

        lives = lives <= 0 ? 3 : lives;

        canJump = false;
        jumpRequested = false;
        nextOuchSoundTime = 0.0f;

        isAttacking = false;
        nextGryfDamageTime = 0.0f;
    }

    void Update()
    {
        moveX = canJump ? Input.GetAxisRaw("Horizontal") : 0.0f;

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
        if (jumpRequested)
        {
            rb.velocity = new Vector2(jumpDirectionX * jumpHorizontalForce, jumpForce);
            jumpsRemaining--;
            jumpRequested = false;
            canJump = false;
        }

        if (canJump && !isAttacking)
        {
            float currentSpeed = Mathf.Abs(moveX) > 0.01f ? walkSpeed : speed;
            Vector2 nextPos = rb.position + new Vector2(moveX * currentSpeed * Time.fixedDeltaTime, 0.0f);
            rb.MovePosition(nextPos);
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }
        else if (canJump && isAttacking)
        {
            rb.velocity = new Vector2(0.0f, 0.0f);
        }
    }

    void ShootBullet()
    {
        if (bullet == null)
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
        if (collision.gameObject.CompareTag("Platform"))
        {
            ScoreState.AddPoints(1);
        }

        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Platform"))
        {
            canJump = true;
            jumpsRemaining = maxJumpCount;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
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
            }
        }

        if (collision.gameObject.CompareTag("Dziunia"))
        {
            DziuniaScript dziunia = collision.GetComponent<DziuniaScript>();
            if (dziunia != null)
            {
                LoseLife();
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
        Bounds rBounds = rb.GetComponent<Collider2D>().bounds;
        Bounds cBounds = collision.bounds;
        float ropuchBottomY = rBounds.min.y;
        float colliderTopY = cBounds.max.y;
        return Mathf.Abs(ropuchBottomY - colliderTopY) < 0.1f;
    }

    public void LoseLife()
    {
        lives--;

        PlayOuchSound();

        if (lives <= 0)
        {
            ScoreState.FinalLives = lives;
            Destroy(gameObject);
            SceneManager.LoadScene(2);
        }
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

        nextOuchSoundTime = Time.time + ouchSoundCooldown;
        AudioSource.PlayClipAtPoint(ouchAudio, transform.position);
    }
}
