using JetBrains.Annotations;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private SceneReference _leve1Scene;

    
    public void StartGame() {
        GameStateManager.Instance?.ResetGame();
        GameSceneManager.Instance?.LoadScene(_leve1Scene, isGameScene: true);
        GameUIManager.Instance?.HideAllUI();
    }
    
    public void ShowCredits() {
        // Implement credits display logic here
        Debug.Log("Credits button clicked. Show credits UI.");
    }
    
    public void QuitGame() {
        Application.Quit();
    }
}
