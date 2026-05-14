using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class FalconScript : MonoBehaviour
{
    
    
    public float speed;
    float movementDir;
    Rigidbody2D rb2D;
    Vector3 iniPosition;
    public GameObject sc;
    SceneControllerScript scScript;
    // Start is called before the first frame update
    void Start()
    {
        speed = 100.0f;
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        iniPosition = transform.position;
        scScript = sc.GetComponent<SceneControllerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        movementDir = Input.GetAxis("Horizontal");
        rb2D.AddForce(new Vector2(movementDir, 0.0f ) * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Reset position and speed
        transform.position = iniPosition;
        rb2D.velocity = new Vector2(0.0f, 0.0f);

        //Removing all asteroids (clear the scene)
        scScript.ResetScene();
        scScript.DecreaseLives();
    }
}
