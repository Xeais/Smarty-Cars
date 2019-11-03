using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(NEATGraph))]
public class NEATGraphEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    NEATGraph myTarget = target as NEATGraph;
    bool drawRandom, reset, drawGivenGenome;

    GUILayout.BeginHorizontal();
    drawRandom = GUILayout.Button("Draw random");
    drawGivenGenome = GUILayout.Button("Draw given");
    reset = GUILayout.Button("Reset");
    GUILayout.EndHorizontal();

    if(drawRandom)
      myTarget.DrawRandomFromParameters();
    else if(drawGivenGenome)
      myTarget.DrawGivenGenomeProxy();
    else if(reset)
      myTarget.RemoveAllNodes();
  }
}
#endif