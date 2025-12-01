using System;
using HadiS.Systems;
using UnityEngine;

public class GameStateManager : Singleton<GameStateManager>
{
    public static Action OnGameFinished;
    
    private const string HighScoreKey = "HighScore";
    
    public bool IsGamePaused { get; private set; }
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
        IsGamePaused = false;
        ResumeGame();
    }
    
    public void ResetScore() {
        Score = 0;
    }

    private void OnApplicationQuit() {
        PlayerPrefs.SetInt("HighScore", HighScore);
    }
    
    public void PauseGame() {
        IsGamePaused = true;
        Time.timeScale = 0f;
        GameUIManager.Instance?.SetGameplayUIActive(false);
        GameUIManager.Instance?.SetPauseMenuUIActive(true);
    }
    
    public void ResumeGame() {
        IsGamePaused = false;
        Time.timeScale = 1f;
        GameUIManager.Instance?.SetGameplayUIActive(true);
        GameUIManager.Instance?.SetPauseMenuUIActive(false);
    }

    public void WinGame() {
        ResumeGame();
        GameUIManager.Instance?.SetGameplayUIActive(false);
        GameUIManager.Instance?.SetVictoryUIActive(true);
        OnGameFinished?.Invoke();
    }
    
    public void LoseGame() {
        ResumeGame();
        GameUIManager.Instance?.SetGameplayUIActive(false);
        GameUIManager.Instance?.SetGameOverUIActive(true);
        OnGameFinished?.Invoke();
    }
}
