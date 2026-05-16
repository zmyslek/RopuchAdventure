using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHudUpdater : MonoBehaviour
{
    [SerializeField]
    Text coinsText;

    [SerializeField]
    Text enemiesText;

    string coinsTemplate;
    string enemiesTemplate;

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
            coinsTemplate = coinsText.text;
            coinsText.text = FormatCount(coinsTemplate, ScoreState.CoinsCollected);
        }

        if (enemiesText != null)
        {
            enemiesTemplate = enemiesText.text;
            enemiesText.text = FormatCount(enemiesTemplate, ScoreState.EnemiesKilled);
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
        coinsText.text = FormatCount(coinsTemplate, newCount);
    }

    void HandleEnemiesChanged(int newCount)
    {
        if (enemiesText == null) return;
        enemiesText.text = FormatCount(enemiesTemplate, newCount);
    }

    string FormatCount(string template, int value)
    {
        if (string.IsNullOrEmpty(template))
        {
            return value.ToString();
        }

        Match match = Regex.Match(template, "\\d+");
        if (!match.Success)
        {
            return template;
        }

        return template.Substring(0, match.Index) + value.ToString() + template.Substring(match.Index + match.Length);
    }
}
