using HadiS.Systems;
using UnityEngine;

public class GameUIManager : Singleton<GameUIManager>
{
    
    [SerializeField] private GameObject _gameplayUI;
    [SerializeField] private GameObject _pauseMenuUI;
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _mainMenuUI;
    
    
    public void SetPauseMenuUIActive(bool isActive) {
       _pauseMenuUI?.SetActive(isActive);
    }
    
    public void SetGameplayUIActive(bool isActive) {
        _gameplayUI?.SetActive(isActive);
    }
    
    public void SetGameOverUIActive(bool isActive) {
        _gameOverUI?.SetActive(isActive);
    }
    
    public void SetMainMenuUIActive(bool isActive) {
        _mainMenuUI?.SetActive(isActive);
    }
    
    public void HideAllUI() {
        _gameplayUI?.SetActive(false);
        _pauseMenuUI?.SetActive(false);
        _gameOverUI?.SetActive(false);
        _mainMenuUI?.SetActive(false);
    }
}
