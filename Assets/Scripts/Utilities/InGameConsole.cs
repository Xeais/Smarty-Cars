using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

/// <summary>
/// A basic container for log details
/// </summary>
struct Log
{
  public int Count {get; set;}
  public string Message {get; set;}
  public string StackTrace {get; set;}
  public LogType Type {get; set;}

  /// <summary>
  /// The maximal string length supported by "UnityEngine.GUILayout.Label" without triggering this error:
  /// "String too long for TextMeshGenerator. Cutting off characters."
  /// </summary>
  private const int MAX_MESSAGE_LENGTH = 16382;

  public bool Equals(Log log) => Message == log.Message && StackTrace == log.StackTrace && Type == log.Type;

  /// <summary>
  /// Return a truncated message if it exceeds the maximal message length.
  /// </summary>
  public string GetTruncatedMessage()
  {
    if(string.IsNullOrEmpty(Message))
      return Message;

    return Message.Length <= MAX_MESSAGE_LENGTH ? Message : Message.Substring(0, MAX_MESSAGE_LENGTH);
  }
}

/// <summary>
/// A console to display Unity's debug logs in-game.
/// </summary>
class InGameConsole : MonoBehaviour
{
  [Header("Open/close")]
  /// <summary>
  /// Whether to open as soon as the game starts.
  /// </summary>
  [SerializeField, Tooltip("Whether to open as soon as the game starts.")] private bool openOnStart = false;
  /// <summary>
  /// The hotkey to show and hide the console window.
  /// </summary>
  [SerializeField, Tooltip("The hotkey to show and hide the console window.")] private KeyCode toggleKey = KeyCode.Tab;
  /// <summary>
  /// Whether to open the window by shaking the device.
  /// </summary>
#if UNITY_ANDROID || UNITY_IOS
  [SerializeField, Tooltip("Whether to open the window by shaking the device.")] private bool shakeToOpen = true;
  /// <summary>
  /// The (squared) acceleration above which the window should open.
  /// </summary>
  [SerializeField, Tooltip("The (squared) acceleration above which the window should open.")] private float shakeAcceleration = 3f;
#endif
  [Header("Restrictions")]
  /// <summary>
  /// Whether to only keep a certain number of logs, useful if memory usage is a concern.
  /// </summary>
  [SerializeField, Tooltip("Whether to only keep a certain number of logs, useful if memory usage is a concern.")] private bool restrictLogCount = false;
  /// <summary>
  /// Number of logs to keep before removing old ones.
  /// </summary>
  [SerializeField, Tooltip("Number of logs to keep before removing old ones.")] private int maxLogCount = 1000;
  /// <summary>
  /// Can the window be dragged by its title bar?
  /// </summary>
  [SerializeField, Tooltip("Can the window be dragged by its title bar?")] private bool draggingAllowed = true;
  [Header("Appearance")]
  /// <summary>
  /// The title of the window
  /// </summary>
  [SerializeField] private string windowTitle = "Console";
  /// <summary>
  /// Background-color of the entire window
  /// </summary>
  [SerializeField, Tooltip("Background-color of the entire window")] private Color backgroundColor = Color.cyan;
  /// <summary>
  /// The left and right margin of the window to the Unity-application-window
  /// </summary>
  [SerializeField, Range(5, 50), Tooltip("The left and right margin of the console-window to the Unity-application-window")] private int leftRightMargin = 20;
  /// <summary>
  /// The top and bottom margin of the window to the Unity-application-window
  /// </summary>
  [SerializeField, Range(5, 50), Tooltip("The top and bottom margin of the console-window to the Unity-application-window")] private int topBottomMargin = 30;

  public static Version Version {get; private set;} = new Version(1, 0, 1);

  private static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
  private static readonly GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

  private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
  {
    {LogType.Error, Color.red},
    {LogType.Exception, Color.red},
    {LogType.Assert, Color.white},
    {LogType.Warning, Color.yellow},
    {LogType.Log, Color.white},
  };

  private bool isCollapsed, isVisible;
  private List<Log> logs = new List<Log>();
  private ConcurrentQueue<Log> queuedLogs = new ConcurrentQueue<Log>();

  private Vector2 scrollPosition;
  private Rect windowRect, titleBarRect = new Rect(0f, 0f, 10000f, 20f);

  private readonly Dictionary<LogType, bool> logTypeFilters = new Dictionary<LogType, bool>
  {
    {LogType.Error, true},
    {LogType.Exception, true},
    {LogType.Assert, true},        
    {LogType.Warning, true},
    {LogType.Log, true},
  };

  private void OnEnable()
  {
    Application.logMessageReceivedThreaded += HandleLogThreaded;
  }

  private void OnDisable()
  {
    Application.logMessageReceivedThreaded -= HandleLogThreaded;
  }

  private void Start()
  {
    windowRect = new Rect(leftRightMargin, topBottomMargin, Screen.width - (leftRightMargin * 2f), Screen.height - (topBottomMargin * 2f));

    if(openOnStart)
      isVisible = true;
  }

  private void OnGUI()
  {
    if(!isVisible)
      return;

    GUI.backgroundColor = backgroundColor;
    windowRect = GUILayout.Window(123456, windowRect, DrawWindow, windowTitle);
  }

  private void Update()
  {
    UpdateQueuedLogs();

    if(Input.GetKeyDown(toggleKey))
      isVisible = !isVisible;

#if UNITY_ANDROID || UNITY_IOS
    if(shakeToOpen && Input.acceleration.sqrMagnitude > shakeAcceleration)
      isVisible = true;
#endif
  }

  private void DrawCollapsedLog(Log log)
  {
    GUILayout.BeginHorizontal();

    GUILayout.Label(log.GetTruncatedMessage());
    GUILayout.FlexibleSpace();
    GUILayout.Label(log.Count.ToString(), GUI.skin.box);

    GUILayout.EndHorizontal();
  }

  private void DrawExpandedLog(Log log)
  {
    for(int i = 0; i < log.Count; i++)
      GUILayout.Label(log.GetTruncatedMessage());
  }

  private void DrawLog(Log log)
  {
    GUI.contentColor = logTypeColors[log.Type];

    if(isCollapsed)
      DrawCollapsedLog(log);
    else
      DrawExpandedLog(log);
  }

  private void DrawLogList()
  {
    scrollPosition = GUILayout.BeginScrollView(scrollPosition);

    //Used to determine the height of the accumulated log-labels.
    GUILayout.BeginVertical();

    var visibleLogs = logs.Where(IsLogVisible);
    foreach(Log log in visibleLogs)
      DrawLog(log);

    GUILayout.EndVertical();

    Rect innerScrollRect = GUILayoutUtility.GetLastRect();
    GUILayout.EndScrollView();
    var outerScrollRect = GUILayoutUtility.GetLastRect();

    //If scrolled to bottom, guarantee that it continues to be in next cycle.
    if(Event.current.type == EventType.Repaint && IsScrolledToBottom(innerScrollRect, outerScrollRect))
      ScrollToBottom();

    //Ensure GUI-colour is reset before drawing other components.
    GUI.contentColor = Color.white;
  }

  private void DrawToolbar()
  {
    GUILayout.BeginHorizontal();

    if(GUILayout.Button(clearLabel))
      logs.Clear();

    foreach(LogType logType in Enum.GetValues(typeof(LogType)))
    {
      bool currentState = logTypeFilters[logType];
      string label = logType.ToString();
      logTypeFilters[logType] = GUILayout.Toggle(currentState, label, GUILayout.ExpandWidth(false));
      GUILayout.Space(20f);
    }

    isCollapsed = GUILayout.Toggle(isCollapsed, collapseLabel, GUILayout.ExpandWidth(false));

    GUILayout.EndHorizontal();
  }

  private void DrawWindow(int windowId)
  {
    DrawLogList();
    DrawToolbar();

    if(draggingAllowed) 
      GUI.DragWindow(titleBarRect); //Allow the window to be dragged by its title bar.
  }

  private Log? GetLastLog()
  {
    if(logs.Count == 0)
      return null;

    return logs.Last();
  }

  private void UpdateQueuedLogs()
  {
    while(queuedLogs.TryDequeue(out Log log))
      ProcessLogItem(log);
  }

  private void HandleLogThreaded(string message, string stackTrace, LogType type)
  {
    var log = new Log
    {
      Count = 1,
      Message = message,
      StackTrace = stackTrace,
      Type = type,
    };

    /* Queue the log into a "ConcurrentQueue" to be processed later in the Unity main thread,
     * so there won't be any GUI-related errors for logs coming from other threads. */
    queuedLogs.Enqueue(log);
  }

  private void ProcessLogItem(Log log)
  {
    var lastLog = GetLastLog();
    bool isDuplicateOfLastLog = lastLog.HasValue && log.Equals(lastLog.Value);

    if(isDuplicateOfLastLog)
    {
      //Replace previous log with incremented count instead of adding a new one.
      log.Count = lastLog.Value.Count + 1;
      logs[logs.Count - 1] = log;
    }
    else
    {
      logs.Add(log);
      TrimExcessLogs();
    }
  }

  private bool IsLogVisible(Log log) => logTypeFilters[log.Type];

  private bool IsScrolledToBottom(Rect innerScrollRect, Rect outerScrollRect)
  {
    float innerScrollHeight = innerScrollRect.height;

    //Take extra padding added to the scroll container into account.
    float outerScrollHeight = outerScrollRect.height - GUI.skin.box.padding.vertical;

    //If contents of scroll view haven't exceeded outer container, treat it as scrolled to bottom.
    if(outerScrollHeight > innerScrollHeight)
      return true;

    //Scrolled to bottom (with error margin for float-inaccuracies)
    return Mathf.Approximately(innerScrollHeight, scrollPosition.y + outerScrollHeight);
  }

  private void ScrollToBottom() => scrollPosition = new Vector2(0f, float.MaxValue);

  private void TrimExcessLogs()
  {
    if(!restrictLogCount)
      return;

    int amountToRemove = logs.Count - maxLogCount;

    if(amountToRemove <= 0)
      return;

    logs.RemoveRange(0, amountToRemove);
  }
}

/*/// <summary>
/// Alternative to "System.Collections.Concurrent.ConcurrentQueue"
/// (only available in .NET 4.0 and greater)
/// </summary>
/// <remarks>
/// It's a bit slow (as it uses locks) and only provides a small subset of the interface.
/// Overall, the implementation is intended to be simple and robust.
/// </remarks>
public class ConcurrentQueue<T>
{
  private readonly object queueLock = new object();
  private Queue<T> queue = new Queue<T>();

  public void Enqueue(T item)
  {
    lock(queueLock)
      queue.Enqueue(item);
  }

  public bool TryDequeue(out T result)
  {
    lock(queueLock)
    {
      if(queue.Count == 0)
      {
        result = default;

        return false;
      }

      result = queue.Dequeue();
      return true;
    }
  }
}*/