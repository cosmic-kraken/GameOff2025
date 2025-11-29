using UnityEngine;
using UnityEngine.SceneManagement;

internal static class SceneBootstrapper
{
    private const string SystemScene = "System";
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeSystemScene() {
        var nrOfScenes = SceneManager.sceneCount;

        for (var i = 0; i < nrOfScenes; i++) {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name == SystemScene) return;
        }

        SceneManager.LoadSceneAsync(SystemScene, LoadSceneMode.Additive);
    }
}
