using System.Collections.Generic;
using System.Linq;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class GL_Debug : Singleton<GL_Debug>
{
#if UNITY_EDITOR
  [HelpBox("This script must be attached to a camera!", HelpBoxMessageType.Warning)] //This requires "HelpAttribute.cs"!
#endif
  [SerializeField] private bool displayLines = true;
#if UNITY_EDITOR
  [SerializeField] private bool displayGizmos = false;
#endif
  [Header("Key- & Color-Value")]
  [SerializeField] private KeyCode toggleKey = KeyCode.G;
  [SerializeField, Tooltip("This color is used in all functions which are called without a color-value.")] private Color defaultColor = Color.white;

  private struct Line
  {
    public Vector3 Start {get;}
    public Vector3 End {get;}
    public Color Color {get;}

    private readonly float startTime, duration;

    public Line(Vector3 start, Vector3 end, Color color, float StartTime, float Duration)
    {
      Start = start;
      End = end;
      Color = color;
      startTime = StartTime;
      duration = Duration;
    }

    public bool DurationElapsed(bool drawLine)
    {
      if(drawLine)
      {
        GL.Color(Color);
        GL.Vertex(Start);
        GL.Vertex(End);
      }

      return Time.time - startTime >= duration;
    }
  }

  private static Material matZOn, matZOff;
  private List<Line> linesZOn, linesZOff;
  //private float ms;

  protected GL_Debug() {}

  private void Awake()
  {
    matZOn = GetMaterial();
    matZOff = GetMaterial(true);

    linesZOn = new List<Line>();
    linesZOff = new List<Line>();
  }

  private Material GetMaterial(bool zTest = false)
  {
    //Unity has a built-in shader that is useful for drawing simple colored things.
    Shader shader = Shader.Find("Hidden/Internal-Colored");
    Material material = new Material(shader) {hideFlags = HideFlags.HideAndDontSave};

    //Turn on alpha blending:
    material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
    material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

    //Turn off backface-culling, depth writes and if "zTest" is true also depth test.
    material.SetInt("_Cull", (int)CullMode.Off);
    if(zTest)
      material.SetInt("_ZTest", (int)CompareFunction.Always);
    material.SetInt("_ZWrite", 0);

    return material;
  }

  private void Update()
  {
    if(Input.GetKeyDown(toggleKey))
      displayLines = !displayLines;

    if(!displayLines)
    {
      //Stopwatch timer = Stopwatch.StartNew();

      linesZOn = linesZOn.Where(l => !l.DurationElapsed(false)).ToList();
      linesZOff = linesZOff.Where(l => !l.DurationElapsed(false)).ToList();

      /*timer.Stop();
      ms = timer.Elapsed.Ticks / 10000f;*/
    }
  }

  private void OnPostRender()
  {
    if(!displayLines)
      return;

    //Stopwatch timer = Stopwatch.StartNew();

    matZOn.SetPass(0);
    GL.Begin(GL.LINES);
    linesZOn = linesZOn.Where(l => !l.DurationElapsed(true)).ToList();
    GL.End();

    matZOff.SetPass(0);
    GL.Begin(GL.LINES);
    linesZOff = linesZOff.Where(l => !l.DurationElapsed(true)).ToList();
    GL.End();

    /*timer.Stop();
    ms = timer.Elapsed.Ticks / 10000f;*/
  }

#if UNITY_EDITOR
  private void OnDrawGizmos()
  {
    if(!displayGizmos || !Application.isPlaying)
      return;

    for(int i = 0; i < linesZOn.Count; i++)
    {
      Gizmos.color = linesZOn[i].Color;
      Gizmos.DrawLine(linesZOn[i].Start, linesZOn[i].End);
    }

    for(int i = 0; i < linesZOff.Count; i++)
    {
      Gizmos.color = linesZOff[i].Color;
      Gizmos.DrawLine(linesZOff[i].Start, linesZOff[i].End);
    }
  }
#endif

  /// <summary>
  /// Draw a line from start to end with color for duration ms and with or without depth testing.
  /// If duration is zero, then the line is rendered for just one frame.
  /// </summary>
  /// <param name="start">Point in world space where the line should start.</param>
  /// <param name="end">Point in world space where the line should end.</param>
  /// <param name="color">Color of the line</param>
  /// <param name="duration">How long the line should be visible (in seconds).</param>
  /// <param name="depthTest">Should the line be obscured by objects closer to the camera?</param>
  public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    if((!Instance.displayLines && duration == 0f) || start == end)
      return;

    if(depthTest)
      Instance.linesZOn.Add(new Line(start, end, color ?? Instance.defaultColor, Time.time, duration));
    else
      Instance.linesZOff.Add(new Line(start, end, color ?? Instance.defaultColor, Time.time, duration));
  }

  /// <summary>
  /// Draw a line from start to start + "dir" with color for duration ms and with or without depth testing.
  /// If duration is zero, then the ray is rendered for just one frame.
  /// </summary>
  /// <param name="start">Point in world space where the ray should start.</param>
  /// <param name="dir">Direction and length of the ray</param>
  /// <param name="color">Color of the ray</param>
  /// <param name="duration">How long the ray should be visible (in seconds).</param>
  /// <param name="depthTest">Should the ray be obscured by objects closer to the camera?</param>
  public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    if(dir == Vector3.zero)
      return;

    DrawLine(start, start + dir, color ?? Instance.defaultColor, duration, depthTest);
    //Debug.Log("A ray has been drawn.");
  }

  /// <summary>
  /// Draw an arrow from start to end with color for duration ms and with or without depth testing.
  /// If duration is zero, then the arrow is rendered for just one frame.
  /// </summary>
  /// <param name="start">Point in world space where the arrow should start.</param>
  /// <param name="end">Point in world space where the arrow should end.</param>
  /// <param name="arrowHeadLength">Length of the two lines of the head</param>
  /// <param name="arrowHeadAngle">Angle between the main line and each of the two smaller lines of the head</param>
  /// <param name="color">Color of the arrow</param>
  /// <param name="duration">How long the arrow should be visible (in seconds).</param>
  /// <param name="depthTest">Should the arrow be obscured by objects closer to the camera?</param>
  public static void DrawLineArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20f, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    DrawArrow(start, end - start, arrowHeadLength, arrowHeadAngle, color ?? Instance.defaultColor, duration, depthTest);
    //Debug.Log("A line-arrow has been drawn.");
  }

  /// <summary>
  /// Draw an arrow from start to start + "dir" with color for duration ms and with or without depth testing.
  /// If duration is zero, then the arrow is rendered for just one frame.
  /// </summary>
  /// <param name="start">Point in world space where the arrow should start.</param>
  /// <param name="dir">Direction and length of the arrow</param>
  /// <param name="arrowHeadLength">Length of the two lines of the head.</param>
  /// <param name="arrowHeadAngle">Angle between the main line and each of the two smaller lines of the head</param>
  /// <param name="color">Color of the arrow</param>
  /// <param name="duration">How long the arrow should be visible (in seconds).</param>
  /// <param name="depthTest">Should the arrow be obscured by objects closer to the camera?</param>
  public static void DrawArrow(Vector3 start, Vector3 dir, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20f, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    if(dir == Vector3.zero)
      return;

    if(color == null)
      color = Instance.defaultColor;

    DrawRay(start, dir, color, duration, depthTest);
    Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * Vector3.forward;
    Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * Vector3.forward;

    DrawRay(start + dir, right * arrowHeadLength, color, duration, depthTest);
    DrawRay(start + dir, left * arrowHeadLength, color, duration, depthTest);
  }

  /// <summary>
  /// Draw a square with color for duration ms and with or without depth testing.
  /// If duration is zero, then the square is rendered for just one frame.
  /// </summary>
  /// <param name="pos">Center of the square in world space</param>
  /// <param name="rot">Rotation of the square in world space euler angles</param>
  /// <param name="scale">Size of the square</param>
  /// <param name="color">Color of the square</param>
  /// <param name="duration">How long the square should be visible (in seconds).</param>
  /// <param name="depthTest">Should the square be obscured by objects closer to the camera?</param>
  public static void DrawSquare(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    DrawSquare(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color ?? Instance.defaultColor, duration, depthTest);
  }

  /// <summary>
  /// Draw a square with color for duration ms and with or without depth testing.
  /// If duration is zero, then the square is rendered for just one frame.
  /// </summary>
  /// <param name="pos">Center of the square in world space</param>
  /// <param name="rot">Rotation of the square in world space</param>
  /// <param name="scale">Size of the square</param>
  /// <param name="color">Color of the square</param>
  /// <param name="duration">How long the square should be visible (in seconds).</param>
  /// <param name="depthTest">Should the square be obscured by objects closer to the camera?</param>
  public static void DrawSquare(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    DrawSquare(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color ?? Instance.defaultColor, duration, depthTest);
  }

  //Parameter "matrix": Transformation matrix which represents the square transform.
  private static void DrawSquare(Matrix4x4 matrix, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    if(color == null)
      color = Instance.defaultColor;

    Vector3 p1 = matrix.MultiplyPoint3x4(new Vector3(0.5f, 0f, 0.5f)),
            p2 = matrix.MultiplyPoint3x4(new Vector3(0.5f, 0f, -0.5f)),
            p3 = matrix.MultiplyPoint3x4(new Vector3(-0.5f, 0f, -0.5f)),
            p4 = matrix.MultiplyPoint3x4(new Vector3(-0.5f, 0f, 0.5f));

    DrawLine(p1, p2, color, duration, depthTest);
    DrawLine(p2, p3, color, duration, depthTest);
    DrawLine(p3, p4, color, duration, depthTest);
    DrawLine(p4, p1, color, duration, depthTest);

    Debug.Log("A square has been drawn.");
  }

  /// <summary>
  /// Draw a cube with color for duration ms and with or without depth testing.
  /// If duration is zero, then the square is rendered for just one frame.
  /// </summary>
  /// <param name="pos">Center of the cube in world space</param>
  /// <param name="rot">Rotation of the cube in world space euler angles</param>
  /// <param name="scale">Size of the cube</param>
  /// <param name="color">Color of the cube</param>
  /// <param name="duration">How long the cube should be visible (in seconds).</param>
  /// <param name="depthTest">Should the cube be obscured by objects closer to the camera?</param>
  public static void DrawCube(Vector3 pos, Vector3? rot = null, Vector3? scale = null, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    DrawCube(Matrix4x4.TRS(pos, Quaternion.Euler(rot ?? Vector3.zero), scale ?? Vector3.one), color ?? Instance.defaultColor, duration, depthTest);
  }

  /// <summary>
  /// Draw a cube with color for duration ms and with or without depth testing.
  /// If duration is zero, then the square is rendered for just one frame.
  /// </summary>
  /// <param name="pos">Center of the cube in world space</param>
  /// <param name="rot">Rotation of the cube in world space</param>
  /// <param name="scale">Size of the cube</param>
  /// <param name="color">Color of the cube</param>
  /// <param name="duration">How long the cube should be visible (in seconds).</param>
  /// <param name="depthTest">Should the cube be obscured by objects closer to the camera?</param>
  public static void DrawCube(Vector3 pos, Quaternion? rot = null, Vector3? scale = null, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    DrawCube(Matrix4x4.TRS(pos, rot ?? Quaternion.identity, scale ?? Vector3.one), color ?? Instance.defaultColor, duration, depthTest);
  }

  //Parameter "matrix": Transformation matrix which represents the cube transform.
  private static void DrawCube(Matrix4x4 matrix, Color? color = null, float duration = 0f, bool depthTest = true)
  {
    if(color == null)
      color = Instance.defaultColor;

    Vector3 down1 = matrix.MultiplyPoint3x4(new Vector3(0.5f, -0.5f, 0.5f)),
            down2 = matrix.MultiplyPoint3x4(new Vector3(0.5f, -0.5f, -0.5f)),
            down3 = matrix.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, -0.5f)),
            down4 = matrix.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, 0.5f)),
            up1 = matrix.MultiplyPoint3x4(new Vector3(0.5f, 0.5f, 0.5f)),
            up2 = matrix.MultiplyPoint3x4(new Vector3(0.5f, 0.5f, -0.5f)),
            up3 = matrix.MultiplyPoint3x4(new Vector3(-0.5f, 0.5f, -0.5f)),
            up4 = matrix.MultiplyPoint3x4(new Vector3(-0.5f, 0.5f, 0.5f));

    DrawLine(down1, down2, color, duration, depthTest);
    DrawLine(down2, down3, color, duration, depthTest);
    DrawLine(down3, down4, color, duration, depthTest);
    DrawLine(down4, down1, color, duration, depthTest);

    DrawLine(down1, up1, color, duration, depthTest);
    DrawLine(down2, up2, color, duration, depthTest);
    DrawLine(down3, up3, color, duration, depthTest);
    DrawLine(down4, up4, color, duration, depthTest);

    DrawLine(up1, up2, color, duration, depthTest);
    DrawLine(up2, up3, color, duration, depthTest);
    DrawLine(up3, up4, color, duration, depthTest);
    DrawLine(up4, up1, color, duration, depthTest);

    Debug.Log("A cube has been drawn.");
  }
}