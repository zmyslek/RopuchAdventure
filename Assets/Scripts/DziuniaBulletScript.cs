using UnityEngine;

public class DziuniaBulletScript : MonoBehaviour
{
    [SerializeField]
    float speed = 0.12f;

    [SerializeField]
    float lifetime = 1.5f;

    [SerializeField]
    Color bulletColor = new Color(0.72f, 0.32f, 1.0f, 1.0f);

    public GameObject bulletImpact;

    float dirX;
    GameObject impactInstance;

    void Start()
    {
        ApplyTint(gameObject);

        if (bulletImpact != null)
        {
            impactInstance = Instantiate(bulletImpact, transform.position, transform.rotation, transform);
            impactInstance.transform.localPosition = Vector3.zero;
            impactInstance.transform.localRotation = Quaternion.identity;
            ApplyTint(impactInstance);
        }
    }

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0.0f)
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        transform.Translate(new Vector2(dirX, 0.0f) * speed);
    }

    public void setDir(bool flipped)
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = flipped;
        }

        dirX = flipped ? -1.0f : 1.0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Do not affect Gryfica (enemy) with Dziunia's bullets
        if (collision.gameObject.GetComponent<GryficaScript>() != null)
        {
            return;
        }

        // If hit player, destroy player
        RopuchControllerScript rop = collision.gameObject.GetComponent<RopuchControllerScript>();
        if (rop != null)
        {
            rop.LoseLife();
            Destroy(gameObject);
            return;
        }

        // existing behavior for other collisions can remain (e.g., hitting the player)
    }

    void ApplyTint(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        SpriteRenderer[] spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = bulletColor;
        }
    }
}