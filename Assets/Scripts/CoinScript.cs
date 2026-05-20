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
        ScoreState.AddCoin(coinValue);

        RopuchControllerScript ropuch = collision.GetComponentInParent<RopuchControllerScript>();
        if (ropuch != null)
        {
            ropuch.AddLife(1);
        }

        Destroy(gameObject);
    }
}