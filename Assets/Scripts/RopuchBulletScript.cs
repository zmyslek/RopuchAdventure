using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopuchBulletScript : MonoBehaviour
{
    float speed;
    float lifetime;
    float dirX;
    public GameObject bulletImpact;

    void Start()
    {
        speed = 0.1f;
        lifetime = 3.0f;
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
        gameObject.GetComponent<SpriteRenderer>().flipX = flipped;
        dirX = flipped ? -1.0f : 1.0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Drone"))
        {
            collision.gameObject.GetComponent<DroneScript>().decrLifeUnits();
            Destroy(gameObject);
            Instantiate(bulletImpact, transform.position, transform.rotation);
            return;
        }

        GryficaScript gryficaScript = collision.gameObject.GetComponent<GryficaScript>();
        if (gryficaScript != null)
        {
            gryficaScript.decrLifeUnits();
            Destroy(gameObject);
            Instantiate(bulletImpact, transform.position, transform.rotation);
        }
    }
}