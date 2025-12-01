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


    protected override void Awake() {
        base.Awake();
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
        SetCursorVisibility(isActive);
       _pauseMenuUI?.SetActive(isActive);   
    }
    
    public void SetGameplayUIActive(bool isActive) {
        SetCursorVisibility(!isActive);
        _gameplayUI?.SetActive(isActive);
    }
    
    public void SetGameOverUIActive(bool isActive) {
        SetCursorVisibility(isActive);
        _gameOverUI?.SetActive(isActive);
    }
    
    public void SetVictoryUIActive(bool isActive) {
        SetCursorVisibility(isActive);
        _victoryUI?.SetActive(isActive);
    }
    
    public void SetMainMenuUIActive(bool isActive) {
        SetCursorVisibility(isActive);
        _mainMenuUI?.SetActive(isActive);
    }
    
    private void SetCursorVisibility(bool isVisible) {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
    
    public void HideAllUI() {
        _gameplayUI?.SetActive(false);
        _pauseMenuUI?.SetActive(false);
        _gameOverUI?.SetActive(false);
        _victoryUI?.SetActive(false);
        _mainMenuUI?.SetActive(false);
    }
}
