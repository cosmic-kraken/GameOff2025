using UnityEditor;
using UnityEngine;


public static class UnityEditorBackgroundColor
{
    private static readonly Color defaultColor = new Color(0.7843f, 0.7843f, 0.7843f);
    private static readonly Color defaultDarkColor = new Color(0.2196f, 0.2196f, 0.2196f);
    
    private static readonly Color selectedColor = new Color(0.22745f, 0.447f, 0.6902f);
    private static readonly Color selectedDarkColor = new Color(0.1725f, 0.3647f, 0.5294f);
    
    private static readonly Color selectedUnfocusedColor = new Color(0.68f, 0.68f, 0.68f);
    private static readonly Color selectedUnfocusedDarkColor = new Color(0.3f, 0.3f, 0.3f);
    
    private static readonly Color hoveredColor = new Color(0.698f, 0.698f, 0.698f);
    private static readonly Color hoveredDarkColor = new Color(0.2706f, 0.2706f, 0.2706f);


    public static Color Get(bool isSelected, bool isHovered, bool isWindowFocused) {

        if (isSelected) {
            
            if (isWindowFocused) {
                return EditorGUIUtility.isProSkin ? selectedDarkColor : selectedColor;
            }
            
            return EditorGUIUtility.isProSkin ? selectedUnfocusedDarkColor : selectedUnfocusedColor;
        }
        
        if (isHovered) {
            return EditorGUIUtility.isProSkin ? hoveredDarkColor : hoveredColor;
        }

        return EditorGUIUtility.isProSkin ? defaultDarkColor : defaultColor;
    }
    
}
