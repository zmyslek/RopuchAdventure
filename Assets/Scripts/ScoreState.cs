using UnityEngine;

public static class ScoreState
{
    static int score;
    static int finalLives = -1;

    public static int CurrentScore
    {
        get { return score; }
    }

    public static int FinalLives
    {
        get { return finalLives; }
        set { finalLives = value; }
    }

    public static void Reset()
    {
        score = 0;
        finalLives = -1;
    }

    public static void AddPoints(int points)
    {
        if (points <= 0)
        {
            return;
        }

        score += points;
    }
}