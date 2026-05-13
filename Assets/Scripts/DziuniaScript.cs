using UnityEngine;

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
    float shootCooldown = 0.25f;

    [SerializeField]
    float initialShootDelay = 1.0f;

    SpriteRenderer sr;
    float nextShootTime;
    bool canShoot;

    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        nextShootTime = Time.time + initialShootDelay;
        canShoot = false;

        lives = lives <= 0 ? 10 : lives;

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
    }

    void Update()
    {
        if (canShoot && Time.time >= nextShootTime)
        {
            ShootBullet();
        }
    }

    public void ShootBullet()
    {
        if (bullet == null || Time.time < nextShootTime)
        {
            return;
        }

        bool facingLeft = sr != null && sr.flipX;
        Transform gun = facingLeft ? leftGun : rightGun;
        if (gun == null)
        {
            return;
        }

        GameObject newBullet = Instantiate(bullet, gun.position, transform.rotation);

        DziuniaBulletScript bulletScript = newBullet.GetComponent<DziuniaBulletScript>();
        if (bulletScript != null)
        {
            bulletScript.setDir(facingLeft);
        }

        nextShootTime = Time.time + shootCooldown;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("MainCamera"))
        {
            canShoot = true;
            nextShootTime = Time.time;
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
        if (lives <= 0)
        {
            return;
        }

        lives--;

        if (lives <= 0)
        {
            Destroy(gameObject);
        }
    }
}
