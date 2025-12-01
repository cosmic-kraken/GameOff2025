using UnityEngine;

public class GameOverUIManager : MonoBehaviour
{
    
    public void RetryLevel() {
        GameStateManager.Instance?.ResetGame();
        GameUIManager.Instance?.HideAllUI();
        
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++) {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene.name.Contains("Level", System.StringComparison.OrdinalIgnoreCase)) {
                GameSceneManager.Instance?.LoadScene(scene.name, isGameScene: true);
                return;
            }
        }
    }
    
    public void ReturnToMainMenu() {
        GameStateManager.Instance.ResetGame();
        GameSceneManager.Instance.ToMainMenu();
    }
}