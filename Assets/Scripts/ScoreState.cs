using System;
using UnityEngine;

public static class ScoreState
{
    static int score;
    static int finalLives = -1;
    static int coinsCollected = 0;
    static int enemiesKilled = 0;

    public static int CurrentScore
    {
        get { return score; }
    }

    public static int FinalLives
    {
        get { return finalLives; }
        set { finalLives = value; }
    }

    public static int CoinsCollected
    {
        get { return coinsCollected; }
    }

    public static int EnemiesKilled
    {
        get { return enemiesKilled; }
    }

    public static event Action<int> OnCoinsChanged;
    public static event Action<int> OnEnemiesChanged;
    public static event Action<int> OnScoreChanged;

    public static void Reset()
    {
        score = 0;
        finalLives = -1;
        coinsCollected = 0;
        enemiesKilled = 0;

        OnScoreChanged?.Invoke(score);
        OnCoinsChanged?.Invoke(coinsCollected);
        OnEnemiesChanged?.Invoke(enemiesKilled);
    }

    public static void AddPoints(int points)
    {
        if (points <= 0)
        {
            return;
        }

        score += points;
        OnScoreChanged?.Invoke(score);
    }

    public static void AddCoin(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        coinsCollected += amount;
        OnCoinsChanged?.Invoke(coinsCollected);
    }

    public static void AddEnemyKill(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        enemiesKilled += amount;
        OnEnemiesChanged?.Invoke(enemiesKilled);
    }
}