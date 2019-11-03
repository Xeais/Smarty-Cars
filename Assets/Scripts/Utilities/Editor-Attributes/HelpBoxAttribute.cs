using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/* The message-icon can be set using the second (optional) parameter in the "HelpBoxAttribute"-constructor and allows the following values:
 * - HelpBoxMessageType.None 
 * - HelpBoxMessageType.Info 
 * - HelpBoxMessageType.Warning
 * - HelpBoxMessageType.Error. 
 * If the second parameter is used, wrap the "HelpBoxAttribute" in an "#if UNITY_EDITOR" to circumvent errors when building to your chosen platform. 
 * Example:
 * [SerializeField]
 * #if UNITY_EDITOR
 * [HelpBox("This is a very important help text!", HelpBoxMessageType.Warning)]
 * #endif
 * private float importantValue = 10f;*/

public enum HelpBoxMessageType : byte
{
  None,
  Info,
  Warning,
  Error
}

public class HelpBoxAttribute : PropertyAttribute
{
  public string Text {get;}
  public HelpBoxMessageType MessageType {get;}

  public HelpBoxAttribute(string text, HelpBoxMessageType messageType = HelpBoxMessageType.Info)
  {
    Text = text;
    MessageType = messageType;
  }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(HelpBoxAttribute))]
public class HelpBoxAttributeDrawer : DecoratorDrawer
{
  public override float GetHeight()
  {
    if(!(attribute is HelpBoxAttribute helpBoxAttribute))
      return base.GetHeight();

    var helpBoxStyle = GUI.skin?.GetStyle("helpbox");
    if(helpBoxStyle == null)
      return base.GetHeight();

    return Mathf.Max(40f, helpBoxStyle.CalcHeight(new GUIContent(helpBoxAttribute.Text), EditorGUIUtility.currentViewWidth) + 4f);
  }

  public override void OnGUI(Rect position)
  {
    if(!(attribute is HelpBoxAttribute helpBoxAttribute))
      return;

    EditorGUI.HelpBox(position, helpBoxAttribute.Text, GetMessageType(helpBoxAttribute.MessageType));
  }

  private MessageType GetMessageType(HelpBoxMessageType helpBoxMessageType)
  {
    switch(helpBoxMessageType)
    {
      case HelpBoxMessageType.None:                             
        return MessageType.None;
      default:
      case HelpBoxMessageType.Info:
        return MessageType.Info;
      case HelpBoxMessageType.Warning:
        return MessageType.Warning;
      case HelpBoxMessageType.Error:
        return MessageType.Error;
    }
  }
}
#endif