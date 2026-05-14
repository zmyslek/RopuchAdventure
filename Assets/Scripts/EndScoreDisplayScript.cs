using UnityEngine;
using UnityEngine.UI;

public class EndScoreDisplayScript : MonoBehaviour
{
    [SerializeField]
    Text endScoreText;

    [SerializeField]
    Text statusText;

    void Start()
    {
        if (endScoreText == null)
        {
            endScoreText = GetComponent<Text>();
        }

        if (endScoreText != null)
        {
            endScoreText.text = "Points: " + ScoreState.CurrentScore.ToString("000");
        }

        if (statusText != null)
        {
            if (ScoreState.FinalLives > 0)
            {
                statusText.text = "You Win!";
            }
            else
            {
                statusText.text = "You Lose";
            }
        }
    }
}