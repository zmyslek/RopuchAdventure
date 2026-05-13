using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GryficaScript : MonoBehaviour
{
    Animator ar;

    [SerializeField]
    int healthUnits;

    [SerializeField]
    GameObject gryficaExplosion;

    [SerializeField]
    float yeeshTime;

    [SerializeField]
    float yeeshAnimationSpeed;

    bool isYeeshing;
    bool hasPlayedInitialHitYeesh;
    bool isDying;

    void Start()
    {
        ar = gameObject.GetComponent<Animator>();

        healthUnits = healthUnits <= 0 ? 1 : healthUnits;
        yeeshTime = yeeshTime <= 0.0f ? 0.35f : yeeshTime;
        yeeshAnimationSpeed = yeeshAnimationSpeed <= 0.0f ? 0.6f : yeeshAnimationSpeed;

        isYeeshing = false;
        hasPlayedInitialHitYeesh = false;
        isDying = false;

        ar.SetInteger("State", 0);
        ar.speed = 1.0f;
    }

    void Update()
    {
        if (!isYeeshing && !isDying)
        {
            ar.SetInteger("State", 0);
            ar.speed = 1.0f;
        }
    }

    public void decrLifeUnits()
    {
        if (isDying)
        {
            return;
        }

        healthUnits--;

        if (healthUnits <= 0)
        {
            StartCoroutine(PlayYeeshAndDie());
            return;
        }

        if (!hasPlayedInitialHitYeesh)
        {
            hasPlayedInitialHitYeesh = true;
            StartCoroutine(PlayYeeshOnly());
        }
    }

    IEnumerator PlayYeeshOnly()
    {
        isYeeshing = true;
        ar.SetInteger("State", 2);
        ar.speed = yeeshAnimationSpeed;

        float yeeshDuration = yeeshTime / Mathf.Max(yeeshAnimationSpeed, 0.01f);
        yield return new WaitForSeconds(yeeshDuration);

        isYeeshing = false;
        ar.speed = 1.0f;
        ar.SetInteger("State", 0);
    }

    IEnumerator PlayYeeshAndDie()
    {
        isDying = true;
        isYeeshing = true;
        ar.SetInteger("State", 2);
        ar.speed = yeeshAnimationSpeed;

        float yeeshDuration = yeeshTime / Mathf.Max(yeeshAnimationSpeed, 0.01f);
        yield return new WaitForSeconds(yeeshDuration);

        if (gryficaExplosion != null)
        {
            Instantiate(gryficaExplosion, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}
