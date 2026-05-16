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
        // Prefer calling enemy-specific hit methods so death animations/playback run.
        GryficaScript gryfica = collision.gameObject.GetComponent<GryficaScript>();
        if (gryfica != null)
        {
            gryfica.decrLifeUnits();
            DestroyBullet();
            return;
        }

        DziuniaScript dziunia = collision.gameObject.GetComponent<DziuniaScript>();
        if (dziunia != null)
        {
            dziunia.TakeHit();
            DestroyBullet();
            return;
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
        if (ropuchBulletSound != null && !AudioMuteManager.MuteRopuch)
        {
            AudioSource.PlayClipAtPoint(ropuchBulletSound, transform.position, 0.35f);
        }
    }

    void PlayDestroySound()
    {
        if (ropuchBulletSound != null && !AudioMuteManager.MuteRopuch)
        {
            AudioSource.PlayClipAtPoint(ropuchBulletSound, transform.position, 0.5f);
        }
    }
}