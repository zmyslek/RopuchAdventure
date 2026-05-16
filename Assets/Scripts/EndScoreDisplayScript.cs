using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class EndScoreDisplayScript : MonoBehaviour
{
    [SerializeField]
    Text endScoreText;

    [SerializeField]
    Text statusText;

    string endScoreTemplate;

    void Start()
    {
        if (endScoreText == null)
        {
            endScoreText = GetComponent<Text>();
        }

        if (endScoreText != null)
        {
            endScoreTemplate = endScoreText.text;
            endScoreText.text = FormatScoreTemplate(endScoreTemplate, ScoreState.CoinsCollected, ScoreState.EnemiesKilled);
        }

        if (statusText != null)
        {
            if (ScoreState.FinalLives > 0)
            {
                statusText.text = "you win";
            }
        }
            Debug.Log("EndScoreDisplay: FinalLives=" + ScoreState.FinalLives + "; statusText assigned=" + (statusText != null));

            if (statusText == null)
            {
                GameObject st = GameObject.Find("StatusText") ?? GameObject.Find("Status") ?? GameObject.Find("StatusLabel");
                if (st != null)
                {
                    statusText = st.GetComponent<Text>();
                    Debug.Log("EndScoreDisplay: found statusText by name: " + (statusText != null));
                }
            }

            if (statusText == null)
            {
                // Fallback: search all Text objects and pick the best candidate
                Text[] allTexts = GameObject.FindObjectsOfType<Text>();
                Text candidate = null;
                foreach (var t in allTexts)
                {
                    if (t == endScoreText) continue;
                    string n = t.gameObject.name.ToLower();
                    string txt = (t.text ?? "").ToLower();
                    if (n.Contains("status") || n.Contains("stat") || txt.Contains("lose") || txt.Contains("win") || txt.Contains("you"))
                    {
                        candidate = t;
                        break;
                    }
                    if (candidate == null) candidate = t;
                }

                if (candidate != null)
                {
                    statusText = candidate;
                    Debug.Log("EndScoreDisplay: found statusText by scan: " + candidate.gameObject.name + " / '" + candidate.text + "'");
                }
            }

            if (statusText != null)
            {
                if (ScoreState.FinalLives > 0)
                {
                    statusText.text = "you win";
                }
                // if final lives <= 0, leave the existing canvas text (e.g. 'You Lose') alone
            }
            else
            {
                Debug.LogWarning("EndScoreDisplay: statusText is still null; cannot update end status label.");
            }
    }

    string FormatScoreTemplate(string template, int coins, int enemies)
    {
        if (string.IsNullOrEmpty(template))
        {
            return "Points: " + coins.ToString() + "\nEnemies killed: " + enemies.ToString();
        }

        Match firstMatch = Regex.Match(template, "\\d+");
        if (!firstMatch.Success)
        {
            return template;
        }

        string result = template.Substring(0, firstMatch.Index) + coins.ToString() + template.Substring(firstMatch.Index + firstMatch.Length);

        Match secondMatch = Regex.Match(result, "\\d+");
        if (!secondMatch.Success)
        {
            return result;
        }

        return result.Substring(0, secondMatch.Index) + enemies.ToString() + result.Substring(secondMatch.Index + secondMatch.Length);
    }
}