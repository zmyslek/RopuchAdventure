using UnityEngine;

public class CoinScript : MonoBehaviour
{
    [SerializeField]
    int coinValue = 1;

    bool isCollected;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected)
        {
            return;
        }

        if (collision.GetComponentInParent<RopuchControllerScript>() == null)
        {
            return;
        }

        isCollected = true;
        ScoreState.AddPoints(coinValue);
        Destroy(gameObject);
    }
}