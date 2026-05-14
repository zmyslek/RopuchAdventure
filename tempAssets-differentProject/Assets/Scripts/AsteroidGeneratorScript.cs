using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidGeneratorScript : MonoBehaviour
{
    public GameObject[] asteroids;
    float countdown;
    float maxSeconds;
    // Start is called before the first frame update
    void Start()
    {
        maxSeconds = 1.0f;
        countdown = maxSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0.0f)
        {   
            int range = Random.Range(0, 3);
            float randomX = transform.position.x + Random.Range(-6.0f, 6.0f);
            Vector3 randomPosition = new Vector3(randomX, transform.position.y, transform.position.z);
            Instantiate(asteroids[range], randomPosition, transform.rotation);
            countdown = maxSeconds;
        }
    }
}
