using System;
using HadiS.Systems;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    private const string HighScoreKey = "HighScore";
    
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    
    
    protected override void Awake() {
        base.Awake();
        ResetScore();
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }
    
    public void AddScore(int amount) {
        Score += amount;
        if (Score <= HighScore) return;
        HighScore = Score;
        PlayerPrefs.SetInt("HighScore", HighScore);
    }
    
    public void ResetGame() {
        ResetScore();
        // Add other game state reset logic here
    }
    
    public void ResetScore() {
        Score = 0;
    }

    private void OnApplicationQuit() {
        PlayerPrefs.SetInt("HighScore", HighScore);
    }
}
