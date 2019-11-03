using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(GraphNode))]
public class GraphNodeEditor : Editor
{
	private GraphNode graphNode;
  private List<Connection> connections;
  private Editor editor;
  private ConnectionEditor connEditor;

  private RectTransform recTrandform1;
  private int index;

  private readonly GUIContent delGUI = new GUIContent("Delete", "Remove Connection.");
  private readonly GUIContent selGUI = new GUIContent("Select", "Select Connection.");
  private readonly GUILayoutOption[] delLayout = new GUILayoutOption[] {GUILayout.Width(40f)};
  private readonly GUILayoutOption[] selLayout = new GUILayoutOption[] {GUILayout.Width(40f)};
  private GUIStyle arrowStyle = new GUIStyle();
  private readonly Color boxColor = new Color(0.625f, 0.625f, 0.625f);

  private void OnEnable()
  {
    graphNode = target as GraphNode;
    recTrandform1 = graphNode.transform as RectTransform;
		GetConnections();

		arrowStyle.alignment = TextAnchor.MiddleCenter;
	}

	public override void OnInspectorGUI()
  {
		if(connections != null)
    {
			foreach(Connection conn in connections)
      {
				if(conn == null || conn.Equals(null))
          continue;

        CreateCachedEditor(conn, typeof(ConnectionEditor), ref editor);
				connEditor = editor as ConnectionEditor;
				index = connEditor.GetIndex(graphNode.transform as RectTransform);
        connEditor.serializedObject.Update();

				EditorGUILayout.Separator();
				Rect box = EditorGUILayout.BeginVertical();
				box.y -= 4f; box.height += 8f; box.x -= 4f; box.width += 5f;
				EditorGUI.DrawRect(box, boxColor);

				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button(delGUI, EditorStyles.miniButton, delLayout))
        {
					DestroyImmediate(conn.gameObject);
					EditorUtility.SetDirty(ConnectionManager.Instance);
					continue;
				}

				if(GUILayout.Button(selGUI, EditorStyles.miniButton, selLayout))
					Selection.activeObject = conn;

				EditorGUILayout.EndHorizontal();
        connEditor.DrawConnectionPointInspector(index);
				EditorGUILayout.LabelField("↓ ↓", arrowStyle);
        connEditor.DrawTargetInspector((index == 0) ? 1 : 0);

				EditorGUILayout.EndVertical();
				EditorGUILayout.Separator();

        connEditor.serializedObject.ApplyModifiedProperties();
			}
		}

		if(GUILayout.Button("Add new Connection", EditorStyles.miniButton))
    {
			ConnectionManager.CreateConnection(recTrandform1, null);
			EditorUtility.SetDirty(ConnectionManager.Instance);
			GetConnections();
		}
	}

  private void GetConnections() => connections = ConnectionManager.FindConnections(recTrandform1);
}
#endif