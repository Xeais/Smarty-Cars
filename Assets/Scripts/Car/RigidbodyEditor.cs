using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Rigidbody))]
public class RigidbodyEditor : Editor
{
  private void OnSceneGUI()
  {
    Rigidbody rigidb = target as Rigidbody;
    Handles.color = Color.red;
    Handles.SphereHandleCap(1, rigidb.transform.TransformPoint(rigidb.centerOfMass), rigidb.rotation, 1f, EventType.Repaint);
  }

  public override void OnInspectorGUI()
  {
    GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
    DrawDefaultInspector();
  }
}
#endif