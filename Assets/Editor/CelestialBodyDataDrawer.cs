using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(CelestialBodyData))]
public class CelestialBodyDataDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 3; //bodyName, isSpinOrbital, isSpinItself

        SerializedProperty isSpinOrbitalProp = property.FindPropertyRelative("isSpinOrbital");
        if (isSpinOrbitalProp.boolValue)
        {
            lines += 3; //orbitalCentralObject, orbitalRadius, orbitalCycle
        }

        lines++; //gravitationCoefficient

        float spacing = 6f;

        return lines * (EditorGUIUtility.singleLineHeight + spacing) + 4f;

    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float padding = 6f;
        Rect rect = new Rect(position.x, position.y, position.width, lineHeight);
        
        var bodyName= property.FindPropertyRelative("bodyName");
        var isSpinOrbital = property.FindPropertyRelative("isSpinOrbital");
        var orbitalCentralObject = property.FindPropertyRelative("orbitalCentralObject");
        var orbitalRadius = property.FindPropertyRelative("orbitalRadius");
        var orbitalCycle = property.FindPropertyRelative("orbitalCycle");
        var isSpinItself = property.FindPropertyRelative("isSpinItself");
        var gravitationCoefficient = property.FindPropertyRelative("gravitationCoefficient");

        EditorGUI.PropertyField(rect, bodyName);
        rect.y += lineHeight + padding;
        
        EditorGUI.PropertyField(rect, isSpinOrbital);
        rect.y += lineHeight + padding;

        if (isSpinOrbital.boolValue)
        {
            EditorGUI.PropertyField(rect, orbitalCentralObject);
            rect.y += lineHeight + padding;
            
            EditorGUI.PropertyField(rect, orbitalRadius);
            rect.y += lineHeight + padding;
            
            EditorGUI.PropertyField(rect, orbitalCycle);
            rect.y += lineHeight + padding;
        }

        EditorGUI.PropertyField(rect, isSpinItself);
        rect.y += lineHeight + padding;
        
        EditorGUI.PropertyField(rect, gravitationCoefficient);
    }
}
