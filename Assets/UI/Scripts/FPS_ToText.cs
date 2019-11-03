using UnityEngine;

/// <summary>
/// Writes the FPS-value to an Unity-UI-Text-component.
/// </summary>
public class FPS_ToText : MonoBehaviour
{
  [HelpBox("This reports the frames per second for the current environment, so the build is accurate, but in the editor time spent rendering the entire editor is included.\n" +
           "Therefore, you will see lower fps in the editor, though the statistic-window will show a higher framerate, since it apparently does not incorporate editor-rendering-time.")] //This requires "HelpAttribute.cs"!
  //[Header("Sample Groups of Data")]
  [SerializeField, Tooltip("If you just want the last frame's data, turn off group sampling.")] private bool groupSampling = false;
  [Space]
  [SerializeField, Range(5, 50), Tooltip("The sample size indicates how many frames should be averaged. You will not see a value until this many frames have passed since valid data must first be collected.")] private int sampleSize = 20;
  [Header("Configuration")]
  [SerializeField] private UnityEngine.UI.Text targetText = null;
  [SerializeField, Range(1, 10), Tooltip("Update the frame-rate-text every umpteenth cycle - this value determines the cycle.")] private int updateTextEvery = 1;
  [SerializeField, Range(2, 4), Tooltip("Limit the length of the Unity-UI-Text-component \"targetText\".")] private int maxTextLength = 3;
  [Space(4)]
  [SerializeField, Tooltip("Force the fps-value to be an integer.")] private bool forceIntResult = true;
  [SerializeField, Tooltip("Bypass allocations by using a string map for the fps-values.")] private bool zeroAllocOnly = false;
  [Header("System FPS (updates once per second)")]
  [SerializeField, Tooltip("Use \"System.Environment.TickCount\" to compute the frames per second.")] private bool useSystemTick = false;
  [Header("Color-Configuration")]
  [SerializeField] private bool useColors = true;
  [Space]
  [SerializeField] private int okayBelow = 60;
  [SerializeField] private int badBelow = 30;
  [Space(4)]
  [SerializeField] private Color good = Color.green;
  [SerializeField] private Color okay = Color.yellow;
  [SerializeField] private Color bad = Color.red;

  public float FPS {get; private set;}

  protected float[] fpsSamples;
  protected int sampleIndex, textUpdateIndex;

  private int sysLastSysTick, sysLastFrameRate, sysFrameRate;
  private string localFps;
  private static readonly string[] FPS_StringMap = 
  {
    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
    "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
    "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
    "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80",
    "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100",
    "101", "102", "103", "104", "105", "106", "107", "108", "109", "110", "111", "122", "113", "114", "115", "116", "117", "118", "119", "120",
    "121", "132", "123", "124", "125", "126", "127", "128", "129", "130", "131", "142", "133", "134", "135", "136", "137", "138", "139", "140",
    "141", "152", "143", "144", "145", "146", "147", "148", "149", "150", "151", "162", "153", "154", "155", "156", "157", "158", "159", "160",
    "161", "172", "163", "164", "165", "166", "167", "168", "169", "170", "171", "182", "173", "174", "175", "176", "177", "178", "179", "180",
    "181", "192", "183", "184", "185", "186", "187", "188", "189", "190", "191", "192", "193", "194", "195", "196", "197", "198", "199", "200",
    "201", "202", "203", "204", "205", "206", "207", "208", "209", "210", "211", "222", "213", "214", "215", "216", "217", "218", "219", "220",
    "221", "222", "223", "224", "225", "226", "227", "228", "229", "230", "231", "232", "233", "234", "235", "236", "237", "238", "239", "240",
    "241", "242", "243", "244", "245", "246", "247", "248", "249", "250", "251", "252", "253", "254", "255", "256", "257", "258", "259", "260",
    "261", "262", "263", "264", "265", "266", "267", "268", "269", "270", "271", "272", "273", "274", "275", "276", "277", "278", "279", "280",
    "281", "282", "283", "284", "285", "286", "287", "288", "289", "290", "291", "292", "293", "294", "295", "296", "297", "298", "299+"
  };

  protected virtual void Reset()
  {
    sampleSize = 20;
    updateTextEvery = 1;
    maxTextLength = 5;
    forceIntResult = useColors = true;
    useSystemTick = false;
    good = Color.green;
    okay = Color.yellow;
    bad = Color.red;
    okayBelow = 60;
    badBelow = 30;
  }

  protected virtual void Start()
  {
    fpsSamples = new float[sampleSize];
    for(int i = 0; i < fpsSamples.Length; i++)
      fpsSamples[i] = 0.001f;

    if(!targetText)
      enabled = false;
  }

  protected virtual void Update()
  {
    if(groupSampling)
      Group();
    else
      SingleFrame();

    localFps = zeroAllocOnly ? FPS_StringMap[Mathf.Clamp((int)FPS, 0, 299)] : FPS.ToString(System.Globalization.CultureInfo.CurrentCulture);

    sampleIndex = (sampleIndex < sampleSize - 1) ? sampleIndex + 1 : 0;
    textUpdateIndex = (textUpdateIndex > updateTextEvery) ? 0 : textUpdateIndex + 1;
    if(textUpdateIndex == updateTextEvery)
      targetText.text = localFps.Substring(0, (localFps.Length < maxTextLength) ? localFps.Length : maxTextLength);

    if(!useColors)
      return;

    if(FPS < badBelow)
    {
      targetText.color = bad;
      return;
    }

    targetText.color = (FPS < okayBelow) ? okay : good;
  }

  protected virtual void SingleFrame()
  {
    FPS = useSystemTick ? GetSystemFrameRate() : 1f / Time.unscaledDeltaTime;

    if(forceIntResult)
      FPS = (int)FPS;
  }

  protected virtual void Group()
  {
    fpsSamples[sampleIndex] = useSystemTick ? GetSystemFrameRate() : 1f / Time.unscaledDeltaTime;

    FPS = 0f;
    bool loop = true;
    int i = 0;
    while(loop)
    {
      if(i == sampleSize - 1)
        loop = false;
      FPS += fpsSamples[i];
      i++;
    }
    FPS /= fpsSamples.Length;
    if(forceIntResult)
      FPS = (int)FPS;
  }

  protected virtual int GetSystemFrameRate()
  {
    if(System.Environment.TickCount - sysLastSysTick >= 1000)
    {
      sysLastFrameRate = sysFrameRate;
      sysFrameRate = 0;
      sysLastSysTick = System.Environment.TickCount;
    }

    sysFrameRate++;
    return sysLastFrameRate;
  }
}