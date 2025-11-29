using System;
using UnityEngine;
using Object = UnityEngine.Object;


[Serializable]
public class SceneReference
{
    [SerializeField] private Object _sceneAsset;
    [SerializeField] private string _sceneName = "";

    public string SceneName => _sceneName;

    public static implicit operator string(SceneReference sceneReference) {
        return sceneReference.SceneName;
    }
}
