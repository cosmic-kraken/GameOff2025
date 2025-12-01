using UnityEngine;

public class PauseUIManager : MonoBehaviour
{
    
    
    public void ResumeGame() {
        GameStateManager.Instance?.ResumeGame();
    }
    
    public void ReturnToMainMenu() {
        GameStateManager.Instance.ResetGame();
        GameSceneManager.Instance.ToMainMenu();
    }
}
