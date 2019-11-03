using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AsyncSceneLoader : MonoBehaviour
{
  [Header("Loading Screen")]
  [SerializeField] private GameObject loadingScreen = null;
  [SerializeField] private Slider loadingBar = null;
  [Header("Buttons")]
  [SerializeField] private Button toNeat = null;
  [SerializeField] private Button toSteering = null;
  [SerializeField] private Button exit = null;
  [Header("Scene Names (to load)")]
  [SerializeField] private string steering = "SteeringBehaviour";
  [SerializeField] private string neat = "NEAT";

  public static List<string> ScenesInBuild {get; private set;}

  protected AsyncSceneLoader() {}

  private Text version = null;

  //-> Call-succession: https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html
  private void Awake()
  {
    ScenesInBuild = new List<string>();
    int sceneCount = SceneManager.sceneCountInBuildSettings;
    for(int i = 1; i < sceneCount; i++)
    {
      string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
      int lastSlash = scenePath.LastIndexOf('/');
      ScenesInBuild.Add(scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf('.') - lastSlash - 1));
    }

    version = GameObject.FindGameObjectWithTag("Version").GetComponent<Text>();
    version.text = "Version: " + Application.version;
  }

  //Tell the "OnSceneLoaded"-function to start listening for a scene change as soon as this script is enabled.
  private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    /* Info concerning the "?": https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-, "-" is also associated - Visual Studio 2019 does not recognize this!
     * Button listeners: */
    toSteering?.onClick.AddListener(() => LoadSceneAsync(steering, false, loadingScreen, loadingBar));
    toNeat?.onClick.AddListener(() => LoadSceneAsync(neat, false, loadingScreen, loadingBar));
    exit?.onClick.AddListener(() => Exit());
  }

  /* Tell the "OnSceneLoaded"-function to stop listening for a scene change as soon as this script is disabled. 
   * Remember to always have an unsubscription for every delegate you subscribe to! */
  private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

  public void LoadSceneAsync(string name, bool additive = false, GameObject loadingScreen = null, Slider loadingSlider = null, Text progressText = null)
  {
    StartCoroutine(LoadAsynchronously(name, additive, loadingScreen, loadingSlider, progressText));
  }

  private System.Collections.IEnumerator LoadAsynchronously(string name, bool additive = false, GameObject loadingScreen = null, Slider loadingSlider = null, Text progressText = null)
  {
    if(ScenesInBuild.Contains(name))
    {
      AsyncOperation asyncOperation = (!additive) ? SceneManager.LoadSceneAsync(name) : SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);

      loadingScreen?.SetActive(true);

      //Wait until the asynchronous scene has fully loaded:
      while(!asyncOperation.isDone)
      {
        float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

        if(loadingSlider != null)
          loadingSlider.value = progress;

        if(progressText != null)
          progressText.text = progress * 100f + " %";

        yield return null;
      }

      loadingScreen?.SetActive(false);

      if(!additive)
      {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
        Debug.Log($"<color=blue>Active scene: {SceneManager.GetActiveScene().name}</color>");
      }
    }
    else
      Debug.LogError($"\"{name}\" is not a valid scene-name!");
  }

  private void Exit()
  {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }
}