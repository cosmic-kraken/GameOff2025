using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


[InitializeOnLoad]
public static class HierarchyAdvancedIcons
{
    private static bool _hierarchyWindowHasFocus = false;
    private static EditorWindow _hierarchyEditorWindow;
    
    
    static HierarchyAdvancedIcons() {
        
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate() {
        
        if (_hierarchyEditorWindow == null) {
            _hierarchyEditorWindow = EditorWindow.GetWindow(System.Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor"));
        }
        
        _hierarchyWindowHasFocus = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow == _hierarchyEditorWindow;
    }

    private static void DrawActivationIcon(Rect selectionRect, GameObject gameObject) {
        
        Rect toggleRect = new Rect(selectionRect);
        toggleRect.x -= 27f;
        toggleRect.width = 13f;
        bool isActive = EditorGUI.Toggle(toggleRect, gameObject.activeSelf);

        if (isActive != gameObject.activeSelf) {
            Undo.RecordObject(gameObject, "Changing active state of object");
            gameObject.SetActive(isActive);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }

    private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect) {

        GameObject obj = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
        if (obj == null) 
            return;
        
        DrawActivationIcon(selectionRect, obj);

        if (PrefabUtility.GetCorrespondingObjectFromSource(obj) != null) 
            return;    
        
        Component[] components = obj.GetComponents<Component>();
        if (components == null || components.Length == 0)
            return;
        
        Component component = components.Length > 1 ? components[1] : components[0];
        
        Type componentType = component.GetType();

        GUIContent icon = EditorGUIUtility.ObjectContent(component, componentType);
        icon.text = null;
        icon.tooltip = componentType.Name;

        if (icon.image == null) 
            return;
        
        bool isSelected = Selection.instanceIDs.Contains(instanceId);
        bool isHovering = selectionRect.Contains(Event.current.mousePosition);
        
        
        Color color = UnityEditorBackgroundColor.Get(isSelected, isHovering, _hierarchyWindowHasFocus);
        Rect backgroundRect = selectionRect;
        backgroundRect.width = 18.5f;
        EditorGUI.DrawRect(backgroundRect, color);
        EditorGUI.LabelField(selectionRect, icon);
    }
}
