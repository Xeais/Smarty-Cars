/*#pragma warning disable 0168 //Variable declared but not used
#pragma warning disable 0219 //Variable assigned but not used
#pragma warning disable 0414 //Private field assigned but not used*/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Connection))]
public class ConnectionEditor : Editor
{
  private SerializedProperty resolution;
  private SerializedProperty t1, t2;
  private SerializedProperty p1, p2;

  private Connection conn;

  private readonly GUIContent tGUI = new GUIContent("goto", "Go to Transform.");
  private GUIStyle arrowStyle = new GUIStyle();

  private void OnEnable()
  {
		try
		{
			conn = target as Connection;

      resolution = serializedObject.FindProperty("resolution");

      SerializedProperty targetTransforms = serializedObject.FindProperty("targets");
			t1 = targetTransforms.GetArrayElementAtIndex(0);
			t2 = targetTransforms.GetArrayElementAtIndex(1);

      SerializedProperty points = serializedObject.FindProperty("points");
      p1 = points.GetArrayElementAtIndex(0);
			p2 = points.GetArrayElementAtIndex(1);

			arrowStyle.alignment = TextAnchor.MiddleCenter;
		}
		catch(System.Exception e) {Debug.LogWarning("<b>\"ConnectionEditor\" encountered a problem:</b>\n" + e);}
	}

	public override void OnInspectorGUI()
  {
		serializedObject.Update();

		EditorGUILayout.PropertyField(resolution);
		EditorGUILayout.Separator();

		DrawTargetInspector(0);
		DrawConnectionPointInspector(0);

		EditorGUILayout.LabelField("↓ ↑", arrowStyle);

		DrawTargetInspector(1);
		DrawConnectionPointInspector(1);

		serializedObject.ApplyModifiedProperties();
	}

	public int GetIndex(RectTransform transform)
  {
		if(t1 == null || t2 == null)
			OnEnable();

		if(transform)
    {
			if(t1.objectReferenceValue != null && t1.objectReferenceValue.Equals(transform))
        return 0;

			if(t2.objectReferenceValue != null && t2.objectReferenceValue.Equals(transform))
        return 1;
		}

		return -1;
	}

	public void DrawTargetInspector(int index)
  {
		EditorGUILayout.BeginHorizontal();
		if(index == 0)
    {
			EditorGUILayout.PropertyField(t1, GUIContent.none);
			if(GUILayout.Button(tGUI, EditorStyles.miniButton, GUILayout.Width(33f)))
      {
				if(t1.objectReferenceValue != null)
          Selection.activeObject = t1.objectReferenceValue;
			}
		}
    else
    {
			EditorGUILayout.PropertyField(t2, GUIContent.none);
			if(GUILayout.Button(tGUI, EditorStyles.miniButton, GUILayout.Width(33f)))
      {
				if(t2.objectReferenceValue != null)
          Selection.activeObject = t2.objectReferenceValue;
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	public void DrawConnectionPointInspector(int index)
  {
		if(index == 0)
			EditorGUILayout.PropertyField(p1);
    else
			EditorGUILayout.PropertyField(p2);
	}
}
#endif