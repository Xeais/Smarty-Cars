using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(ConnectionManager))]
public class ConnectionManagerEditor : Editor
{
	private SerializedProperty prefab;

  private ReorderableList connections;

  private readonly GUIContent connGUI = new GUIContent(">", "Go to Connection.");

  private void OnEnable()
  {
		prefab = serializedObject.FindProperty("connectionPrefab");

		connections = new ReorderableList(serializedObject, serializedObject.FindProperty("connections"), true, true, true, true);
		connections.drawElementCallback += (Rect position, int index, bool show, bool active) => 
    {
			SerializedProperty element = connections.serializedProperty.GetArrayElementAtIndex(index);
			Rect lRect = new Rect(position.x, position.y + 2f, position.width - 20f, EditorGUIUtility.singleLineHeight);
			Rect bRect = new Rect(position.x + lRect.width, position.y + 2f, 18f, EditorGUIUtility.singleLineHeight);

			if(element.objectReferenceValue != null)
      {
				EditorGUI.LabelField(lRect, element.objectReferenceValue.name);
				if(GUI.Button(bRect, connGUI))
					Selection.activeObject = element.objectReferenceValue;
			}
      else
				EditorGUI.LabelField(lRect, "Missing Connection");
		};
		connections.drawHeaderCallback += (Rect position) => 
    {
			Rect lRect = new Rect(position.x, position.y, position.width - 80f, position.height);
			Rect b1Rect = new Rect(position.x + lRect.width, position.y + 1f, 40f, position.height - 2f);
			Rect b2Rect = new Rect(position.x + lRect.width + b1Rect.width, position.y + 1f, 40f, position.height - 2f);

			EditorGUI.LabelField(lRect, "Connections: " + connections.count, EditorStyles.miniLabel);
			if(GUI.Button(b1Rect, "Sort", EditorStyles.miniButton))
      {
				ConnectionManager.SortConnections();
				EditorUtility.SetDirty(target);
			}
			if(GUI.Button(b2Rect, "Clean", EditorStyles.miniButton))
      {
				ConnectionManager.CleanConnections();
				EditorUtility.SetDirty(target);
			}
		};
		connections.onRemoveCallback += (ReorderableList reordList) => 
    {
			Connection conn = reordList.serializedProperty.GetArrayElementAtIndex(reordList.index).objectReferenceValue as Connection;
			if(conn)
        DestroyImmediate(conn.gameObject);

			ReorderableList.defaultBehaviours.DoRemoveButton(reordList);
			ReorderableList.defaultBehaviours.DoRemoveButton(reordList);
			EditorUtility.SetDirty(target);
		};
		connections.onAddCallback += (ReorderableList reordList) => 
    {
			ConnectionManager.CreateConnection(null, null);
			EditorUtility.SetDirty(target);
		};

		connections.onSelectCallback += (ReorderableList reordList) => 
    {
			Connection conn = reordList.serializedProperty.GetArrayElementAtIndex(reordList.index).objectReferenceValue as Connection;
			if(conn)
				EditorGUIUtility.PingObject(conn);
		};
	}

	public override void OnInspectorGUI()
  {
		serializedObject.Update();

		EditorGUILayout.PropertyField(prefab);
		EditorGUILayout.Separator();
		connections.DoLayoutList();

		serializedObject.ApplyModifiedProperties();
	}
}
#endif