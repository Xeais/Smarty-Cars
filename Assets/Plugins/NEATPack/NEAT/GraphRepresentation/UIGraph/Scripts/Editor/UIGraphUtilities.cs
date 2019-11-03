using System;
using UnityEngine;
using UnityEngine.UI;

public static class UIGraphUtilities
{
  private static readonly Type[] nodeTypes = new Type[] {typeof(Canvas), typeof(Image), typeof(GraphNode)};
  private static readonly string nodeName = "New Node";
  private static Vector2 nodeSize = new Vector2(400f, 200f);
  private static readonly float nodeScale = 0.005f;

  private static readonly string textName = "Text";
  private static readonly Type[] textTypes = new Type[] {typeof(Text)};
  private static readonly int textSize = 48;

#if UNITY_EDITOR
  [UnityEditor.MenuItem("GameObject/Create Other/Graph Node")]
#endif
  public static void CreateNode()
  {
		GameObject node = new GameObject(nodeName, nodeTypes);

		RectTransform transform = node.GetComponent<RectTransform>();
		transform.localScale = Vector3.one * nodeScale;
		transform.sizeDelta = nodeSize;

		CreateText(transform);
	}

	public static void CreateText(Transform parent)
  {
		GameObject go = new GameObject(textName, textTypes);

		RectTransform transform = go.GetComponent<RectTransform>();
		transform.SetParent(parent, false);
		transform.anchorMin = Vector2.zero;
		transform.anchorMax = Vector2.one;
		transform.sizeDelta = Vector2.zero;

		Text text = go.GetComponent<Text>();
		text.alignment = TextAnchor.MiddleCenter;
		text.color = Color.black;
		text.fontSize = textSize;
		text.text = nodeName;
	}

#if UNITY_EDITOR
  [UnityEditor.MenuItem("GameObject/Create Other/Graph Connection")]
#endif
  public static void CreateConnection() => new GameObject("New Connection", typeof(Connection));
}