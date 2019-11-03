using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReadOnlyAttribute : PropertyAttribute {}

//Warning: If you hate errors, do not put classes like this in a folder called "Editor"!
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, true);

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    GUI.enabled = false;
    EditorGUI.PropertyField(position, property, label, true);
    GUI.enabled = true;
  }
}
#endif