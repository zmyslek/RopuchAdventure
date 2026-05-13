using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneScript : MonoBehaviour
{
    [SerializeField]
    int healthUnits;
    public GameObject droneExplosion;
    // Start is called before the first frame update
    void Start()
    {
        healthUnits=3;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void decrLifeUnits()
    {
        healthUnits--;
        if(healthUnits<=0)
        {
            Destroy(gameObject);
            Instantiate(droneExplosion, transform.position, transform.rotation);
        }
    }
}
