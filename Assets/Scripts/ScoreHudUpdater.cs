using UnityEngine;
using UnityEngine.UI;

public class ScoreHudUpdater : MonoBehaviour
{
    [SerializeField]
    Text coinsText;

    [SerializeField]
    Text enemiesText;

    [SerializeField]
    bool includeLabels = true;

    void Start()
    {
        if (coinsText == null)
        {
            var go = GameObject.Find("CoinsText");
            if (go != null) coinsText = go.GetComponent<Text>();
        }

        if (enemiesText == null)
        {
            var go = GameObject.Find("EnemiesText");
            if (go != null) enemiesText = go.GetComponent<Text>();
        }
        // initialize values
        if (coinsText != null)
        {
            coinsText.text = includeLabels ? ("Points: " + ScoreState.CoinsCollected.ToString()) : ScoreState.CoinsCollected.ToString();
        }

        if (enemiesText != null)
        {
            enemiesText.text = includeLabels ? ("Enemies killed: " + ScoreState.EnemiesKilled.ToString()) : ScoreState.EnemiesKilled.ToString();
        }
    }
    void OnEnable()
    {
        ScoreState.OnCoinsChanged += HandleCoinsChanged;
        ScoreState.OnEnemiesChanged += HandleEnemiesChanged;
    }

    void OnDisable()
    {
        ScoreState.OnCoinsChanged -= HandleCoinsChanged;
        ScoreState.OnEnemiesChanged -= HandleEnemiesChanged;
    }

    void HandleCoinsChanged(int newCount)
    {
        if (coinsText == null) return;
        coinsText.text = includeLabels ? ("Points: " + newCount.ToString()) : newCount.ToString();
    }

    void HandleEnemiesChanged(int newCount)
    {
        if (enemiesText == null) return;
        enemiesText.text = includeLabels ? ("Enemies killed: " + newCount.ToString()) : newCount.ToString();
    }
}
