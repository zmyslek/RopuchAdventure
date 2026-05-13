using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopuchControllerScript : MonoBehaviour
{
    Animator ar;
    Rigidbody2D rb;
    SpriteRenderer sr;

    [SerializeField]
    int lives;

    float speed;
    float jumpForce;
    float jumpHorizontalForce;
    float moveX;

    bool canJump;
    bool jumpRequested;
    bool isAttacking;

    int maxJumpCount;
    int jumpsRemaining;
    float jumpDirectionX;

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
        jumpForce = 8.5f;
        jumpHorizontalForce = 3.0f;
        maxJumpCount = 2;
        jumpsRemaining = maxJumpCount;

        lives = lives <= 0 ? 3 : lives;

        canJump = false;
        jumpRequested = false;

        isAttacking = false;
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
            }

            if (Input.GetKeyDown(KeyCode.Space) && !isAttacking && jumpsRemaining > 0)
            {
                ar.SetInteger("State", 1);
                jumpDirectionX = Mathf.Abs(moveX) > 0.01f ? Mathf.Sign(moveX) : (sr.flipX ? -1.0f : 1.0f);
                jumpRequested = true;
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                if (isAttacking)
                {
                    isAttacking = false;
                    ar.SetInteger("State", 0);
                }
                else if (Mathf.Abs(moveX) < 0.01f)
                {
                    ar.SetInteger("State", 2);
                    isAttacking = true;
                    ShootBullet();
                    rb.velocity = new Vector2(0.0f, 0.0f);
                }
            }
            else if (!hasAnyInput && !isAttacking)
            {
                ar.SetInteger("State", 0);
            }
        }
        else
        {
            ar.SetInteger("State", 1);

            if (Input.GetKeyDown(KeyCode.Return) && !isAttacking)
            {
                ShootBullet();
            }
        }

        if (isAttacking)
        {
            rb.velocity = new Vector2(0.0f, 0.0f);
        }

        bool isMovingOrAirborne = !canJump || Mathf.Abs(moveX) > 0.01f || Mathf.Abs(rb.velocity.x) > 0.01f || Mathf.Abs(rb.velocity.y) > 0.01f;
        ar.speed = (ar.GetInteger("State") == 0 && isMovingOrAirborne) ? 0.0f : 1.0f;
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
            Vector2 nextPos = rb.position + new Vector2(moveX * speed * Time.fixedDeltaTime, 0.0f);
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
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = true;
            jumpsRemaining = maxJumpCount;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = true;
            jumpsRemaining = maxJumpCount;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = false;
        }
    }

    public void LoseLife()
    {
        lives--;
        if (lives <= 0)
        {
            Destroy(gameObject);
        }
    }
}
