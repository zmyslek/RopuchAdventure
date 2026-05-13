using UnityEngine;

public class DziuniaBulletScript : MonoBehaviour
{
    [SerializeField]
    float speed = 0.12f;

    [SerializeField]
    float lifetime = 3.0f;

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
        if (collision.gameObject.CompareTag("Gryf"))
        {
            GryficaScript gryficaScript = collision.gameObject.GetComponent<GryficaScript>();
            if (gryficaScript != null)
            {
                gryficaScript.decrLifeUnits();
            }

            Destroy(gameObject);
            return;
        }

        GryficaScript otherGryficaScript = collision.gameObject.GetComponent<GryficaScript>();
        if (otherGryficaScript != null)
        {
            otherGryficaScript.decrLifeUnits();
            Destroy(gameObject);
        }
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