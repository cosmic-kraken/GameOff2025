using System;
using HadiS.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : Singleton<GameUIManager>
{
    
    [SerializeField] private GameObject _gameplayUI;
    [SerializeField] private GameObject _pauseMenuUI;
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _victoryUI;
    [SerializeField] private GameObject _mainMenuUI;


    private void Start() {
        InitializeUIFromLoadedScenes();
    }
    
    private void InitializeUIFromLoadedScenes() {
        var loadedScenes = SceneManager.loadedSceneCount;
        
        for (int i = 0; i < loadedScenes; i++) {
            if (SceneManager.GetSceneAt(i).name.Contains("MainMenu", StringComparison.OrdinalIgnoreCase)) {
                HideAllUI();
                SetMainMenuUIActive(true);
                break;
            }
            
            if (SceneManager.GetSceneAt(i).name.Contains("Level", StringComparison.OrdinalIgnoreCase)) {
                HideAllUI();
                SetGameplayUIActive(true);
                break;
            }
        }
    }

    public void SetPauseMenuUIActive(bool isActive) {
       _pauseMenuUI?.SetActive(isActive);   
    }
    
    public void SetGameplayUIActive(bool isActive) {
        _gameplayUI?.SetActive(isActive);
    }
    
    public void SetGameOverUIActive(bool isActive) {
        _gameOverUI?.SetActive(isActive);
    }
    
    public void SetVictoryUIActive(bool isActive) {
        _victoryUI?.SetActive(isActive);
    }
    
    public void SetMainMenuUIActive(bool isActive) {
        _mainMenuUI?.SetActive(isActive);
    }
    
    public void HideAllUI() {
        _gameplayUI?.SetActive(false);
        _pauseMenuUI?.SetActive(false);
        _gameOverUI?.SetActive(false);
        _victoryUI?.SetActive(false);
        _mainMenuUI?.SetActive(false);
    }
}
