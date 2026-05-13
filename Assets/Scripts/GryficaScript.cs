using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GryficaScript : MonoBehaviour
{
    Animator ar;
    Rigidbody2D rb;
    SpriteRenderer sr;

    [SerializeField]
    int healthUnits;

    [SerializeField]
    GameObject gryficaExplosion;

    [SerializeField]
    float yeeshTime;

    [SerializeField]
    float attackTime;

    [SerializeField]
    float attackJumpForce;

    [SerializeField]
    float attackForwardForce;

    bool canJump;
    bool isYeeshing;
    bool isAttacking;
    bool applyAttackImpulse;
    float attackTimer;

    GameObject ropuch;

    void Start()
    {
        ar = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();

        healthUnits = healthUnits <= 0 ? 3 : healthUnits;
        yeeshTime = yeeshTime <= 0.0f ? 0.35f : yeeshTime;
        attackTime = attackTime <= 0.0f ? 0.35f : attackTime;
        attackJumpForce = attackJumpForce <= 0.0f ? 8.0f : attackJumpForce;
        attackForwardForce = attackForwardForce <= 0.0f ? 7.0f : attackForwardForce;

        canJump = false;
        isYeeshing = false;
        isAttacking = false;
        applyAttackImpulse = false;
        attackTimer = 0.0f;

        ar.SetInteger("State", 0);
        FindRopuch();
    }

    void Update()
    {
        if (ropuch == null)
        {
            FindRopuch();
        }

        if (Input.GetKeyDown(KeyCode.Return) && canJump && !isYeeshing && !isAttacking)
        {
            BeginAttack();
        }

        if (isYeeshing)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0.0f)
            {
                isYeeshing = false;
                isAttacking = true;
                applyAttackImpulse = true;
                attackTimer = attackTime;
                ar.SetInteger("State", 1);
            }
        }
        else if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0.0f)
            {
                isAttacking = false;
                attackTimer = 0.0f;
                ar.SetInteger("State", 0);
            }
        }
        else if (canJump)
        {
            ar.SetInteger("State", 0);
        }
    }

    void FixedUpdate()
    {
        if (applyAttackImpulse)
        {
            applyAttackImpulse = false;
            rb.velocity = new Vector2(0.0f, 0.0f);

            float attackDirection = sr.flipX ? -1.0f : 1.0f;
            rb.AddForce(new Vector2(attackDirection * attackForwardForce, attackJumpForce), ForceMode2D.Impulse);
        }
    }

    void BeginAttack()
    {
        if (ropuch != null)
        {
            sr.flipX = ropuch.transform.position.x < transform.position.x;
        }

        isYeeshing = true;
        attackTimer = yeeshTime;
        ar.SetInteger("State", 2);
        rb.velocity = new Vector2(0.0f, 0.0f);
    }

    void FindRopuch()
    {
        GameObject ropuchByTag = GameObject.FindGameObjectWithTag("Ropuch");
        if (ropuchByTag != null)
        {
            ropuch = ropuchByTag;
            return;
        }

        RopuchControllerScript ropuchController = FindObjectOfType<RopuchControllerScript>();
        if (ropuchController != null)
        {
            ropuch = ropuchController.gameObject;
        }
    }

    public void decrLifeUnits()
    {
        healthUnits--;
        if (healthUnits <= 0)
        {
            Destroy(gameObject);

            if (gryficaExplosion != null)
            {
                Instantiate(gryficaExplosion, transform.position, transform.rotation);
            }
        }
    }

    private void HandleRopuchHit(GameObject other)
    {
        if (!isAttacking)
        {
            return;
        }

        RopuchControllerScript ropuchController = other.GetComponent<RopuchControllerScript>();
        if (ropuchController != null)
        {
            ropuchController.LoseLife();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = true;
        }

        HandleRopuchHit(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            canJump = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleRopuchHit(collision.gameObject);
    }
}
