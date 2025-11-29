using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferencePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, GUIContent.none, property);
        var sceneAsset = property.FindPropertyRelative("_sceneAsset");
        var sceneName = property.FindPropertyRelative("_sceneName");
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        if (sceneAsset != null) {
            sceneAsset.objectReferenceValue = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);

            if (sceneAsset.objectReferenceValue != null) sceneName.stringValue = ((SceneAsset)sceneAsset.objectReferenceValue).name;
        }

        EditorGUI.EndProperty();
    }
}
#endif