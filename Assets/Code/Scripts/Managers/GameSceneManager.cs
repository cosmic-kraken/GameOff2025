using System.Collections;
using HadiS.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneManager : Singleton<GameSceneManager>
{
    // Nieuwe note van Hadi:
    // Dit is misschien niet de beste manier om scenes + UI te regelen. Het is spaghetti. Maar het heeft gewoon recht om te bestaan in een GameJam.
    // Thank you for coming to my TedTalk
    
    // Note van Hadi:
    // Roep LoadScene aan met de naam van de scene die je wilt laden. Je kan SceneReference gebruiken om ze in de inspector te zetten.
    // De oude scene wordt automatisch ontladen. Maar de System scene blijft geladen omdat die persistent is. Voor alle managers etc.
    
    [Header("Scenes")]
    [SerializeField] private SceneReference _mainMenuScene;
    
    [Header("Loading Screen")]
    [SerializeField] private GameObject _temporaryCamera;
    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private float _loadingScreenDelay;
    
    private Coroutine _currentSceneLoadingCoroutine;
    

    public void LoadScene(string sceneToLoad, bool isGameScene = false) 
    {
        if (_currentSceneLoadingCoroutine != null)
        {
            StopCoroutine(_currentSceneLoadingCoroutine);
        }
        _currentSceneLoadingCoroutine = StartCoroutine(LoadSceneCoroutine(sceneToLoad, isGameScene));
    }
    
    public void ToMainMenu() 
    {
        if (_currentSceneLoadingCoroutine != null)
        {
            StopCoroutine(_currentSceneLoadingCoroutine);
        }
        GameUIManager.Instance?.HideAllUI();
        GameStateManager.Instance?.ResetGame();
        _currentSceneLoadingCoroutine = StartCoroutine(LoadSceneCoroutine(_mainMenuScene));
    }
    
    private IEnumerator LoadSceneCoroutine(string newScene, bool isGameScene = false)
    {
        // Determine the currently active scene (excluding System)
        var oldScene = string.Empty;
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name != "System" && scene.isLoaded) {
                oldScene = scene.name;
                break;
            }
        }
        
        
        // Hide gameplay UI during scene transition
        GameUIManager.Instance.SetGameplayUIActive(false);
        
        // If no active scene found, abort loading. This should not happen.
        if (string.IsNullOrEmpty(oldScene)) {
            Debug.LogError("No active scene found to unload. Aborting scene load.");
            yield break;
        }
        
        // Fade in loading screen
        _loadingScreen.SetActive(true);
        var loadImage = _loadingScreen.GetComponentInChildren<Image>();
        
        var targetColor = new Color(loadImage.color.r, loadImage.color.g, loadImage.color.b, 1f);
        var initialColor = new Color(loadImage.color.r, loadImage.color.g, loadImage.color.b, 0f);
        loadImage.color = initialColor;
        var fadeDuration = 1f;
        var elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration) {
            elapsedTime += Time.deltaTime;
            loadImage.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
            yield return null;
        }
        
        // Unload old scene and toggle a new temporary MainCamera for loading screen
        var unloadOperation = SceneManager.UnloadSceneAsync(oldScene);
        unloadOperation!.completed += _ => _temporaryCamera.SetActive(true);
        while (!unloadOperation.isDone) {
            yield return null;
        }
        
        // Wait for a brief moment to ensure loading screen is visible
        yield return new WaitForSeconds(_loadingScreenDelay);
        
        // Activate gameplay UI if we loaded a game scene
        if (isGameScene) {
            GameUIManager.Instance.SetGameplayUIActive(true);
        }
        else {
            if (newScene.Contains("MainMenu", System.StringComparison.OrdinalIgnoreCase)) {
                GameUIManager.Instance.SetMainMenuUIActive(true);
            }
        }
        
        // Load new scene and turn off temporary camera
        // TODO: Remove this ugly hack if we continue with this project. We want to fade loading screen out AFTER trash has spawned (heaviest start operation)
        var trashSpawned = true;
        if (newScene.Contains("Level", System.StringComparison.OrdinalIgnoreCase)) {
            trashSpawned = false;
            TrashSpawner.OnTrashSpawned += _ =>  trashSpawned = true;
        }
        
        var loadOperation = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        loadOperation!.completed += _ => _temporaryCamera.SetActive(false);
        while (!loadOperation.isDone && !trashSpawned) {
            yield return null;
        }
        
        // Fade out loading screen
        targetColor = new Color(loadImage.color.r, loadImage.color.g, loadImage.color.b, 0f);
        initialColor = new Color(loadImage.color.r, loadImage.color.g, loadImage.color.b, 1f);
        loadImage.color = initialColor;
        fadeDuration = 0.5f;
        elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration) {
            elapsedTime += Time.deltaTime;
            loadImage.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
            yield return null;
        }
        
        // Ensure loading screen elements are turned off
        _temporaryCamera.SetActive(false);
        _loadingScreen.SetActive(false);
    }
}

