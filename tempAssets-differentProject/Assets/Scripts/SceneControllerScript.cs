using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControllerScript : MonoBehaviour
{
    int numLives;
    int score;

    [SerializeField]
    Text scoreText;

    [SerializeField]
    GameObject[] lives;

    [SerializeField]
    GameObject[] deaths;

    public Text finalScore;
    // Start is called before the first frame update
    void Start()
    {
        numLives = 3;
        score = 0;
        scoreText.text = score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetScene()
    {
        //Removing all asteroids (clear the scene)
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        foreach (GameObject asteroid in asteroids)
        {
            Destroy(asteroid);
        }

        //Decrese lives by 1
        //numLives--;
    }
    // public void DestroyAsteroid(GameObject asteroid)
    // {
    //     Destroy(asteroid);
    // }

    void OnTriggerExit2D(Collider2D other)
    {
        score++;
        scoreText.text = score.ToString();
    }
    public void DecreaseLives()
    {
        if(numLives > 0)
        {
            numLives--;
            lives[numLives].SetActive(false);
            deaths[numLives].SetActive(true);
        }
        else
        {
            lives[numLives].SetActive(false);
            deaths[numLives].SetActive(true);
            finalScore.text = "Final Score: " + score.ToString() + " points";
            SceneManager.LoadScene(2);
        }
    }
}
