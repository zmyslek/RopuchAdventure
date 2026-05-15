using UnityEngine;

public class RopuchBulletScript : MonoBehaviour
{
    float speed;
    float lifetime;
    float dirX;
    GameObject impactInstance;
    public GameObject bulletImpact;
    bool isDespawning;

    [SerializeField]
    AudioClip ropuchBulletSound;

    void Start()
    {
        speed = 0.1f;
        lifetime = 3.0f;
        isDespawning = false;

        PlayLaunchSound();

        if (bulletImpact != null)
        {
            impactInstance = Instantiate(bulletImpact, transform.position, transform.rotation, transform);
            impactInstance.transform.localPosition = Vector3.zero;
            impactInstance.transform.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0.0f)
        {
            DestroyBullet();
        }
    }

    void FixedUpdate()
    {
        transform.Translate(new Vector2(dirX, 0.0f) * speed);
    }

    public void setDir(bool flipped)
    {
        gameObject.GetComponent<SpriteRenderer>().flipX = flipped;
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

            DestroyBullet();
            return;
        }

        GryficaScript gryficaScriptOnCollision = collision.gameObject.GetComponent<GryficaScript>();
        if (gryficaScriptOnCollision != null)
        {
            gryficaScriptOnCollision.decrLifeUnits();
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        if (isDespawning)
        {
            return;
        }

        isDespawning = true;
        PlayDestroySound();
        Destroy(gameObject);
    }

    void PlayLaunchSound()
    {
        if (ropuchBulletSound != null)
        {
            AudioSource.PlayClipAtPoint(ropuchBulletSound, transform.position, 0.35f);
        }
    }

    void PlayDestroySound()
    {
        if (ropuchBulletSound != null)
        {
            AudioSource.PlayClipAtPoint(ropuchBulletSound, transform.position, 0.5f);
        }
    }
}