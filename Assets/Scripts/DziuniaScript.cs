using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    float shootCooldown = 0.9f;

    [SerializeField]
    float initialShootDelay = 1.0f;

    [SerializeField]
    float deathDelay = 0.4f;

    [SerializeField]
    AudioClip dziuniaShowAudio;

    SpriteRenderer sr;
    DziuniaImpactScript impactScript;
    float nextShootTime;
    bool canShoot;
    bool isDying;

    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        impactScript = gameObject.GetComponent<DziuniaImpactScript>();
        nextShootTime = Time.time + initialShootDelay;
        canShoot = false;
        isDying = false;

        lives = lives <= 0 ? 10 : lives;
        deathDelay = deathDelay <= 0.0f ? 0.4f : deathDelay;

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
        nextShootTime = Time.time;

        // Play audio with DziuniaShow tag
        PlayDziuniaShowAudio();
    }

    private void OnBecameInvisible()
    {
        canShoot = false;
    }

    public void ShootBullet()
    {
        if (bullet == null || isDying || Time.time < nextShootTime)
        {
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

        nextShootTime = Time.time + shootCooldown;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDying)
        {
            return;
        }

        // Check if hit by Ropuch's bullet
        if (collision.gameObject.GetComponent<RopuchBulletScript>() != null)
        {
            TakeHit();
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
        if (impactScript != null)
        {
            impactScript.PlayImpactReaction();
        }

        yield return new WaitForSeconds(deathDelay);

        // Count this Dziunia as killed before leaving to end scene
        ScoreState.AddEnemyKill(1);
        SceneManager.LoadScene(2);
    }

    void PlayDziuniaShowAudio()
    {
        if (dziuniaShowAudio == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(dziuniaShowAudio, transform.position);
    }
}
