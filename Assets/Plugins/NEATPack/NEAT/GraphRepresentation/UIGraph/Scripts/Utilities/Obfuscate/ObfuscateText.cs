using UnityEngine;

public class ObfuscateText : MonoBehaviour
{
	[ContextMenu("Obfuscate")]
	public void Obfuscate()
  {
    UnityEngine.UI.Text t = GetComponent<UnityEngine.UI.Text>();
		if(t)
			t.text = t.text.GetHashCode().ToString();
	}
}