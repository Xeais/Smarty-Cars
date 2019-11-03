using UnityEngine;

public class SimpleAsyncLoader : MonoBehaviour
{
  public void LoadLevel(int sceneIndex) => StartCoroutine(LoadAsynchronously(sceneIndex));

  System.Collections.IEnumerator LoadAsynchronously(int sceneIndex)
  {
    AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
    while(!asyncOperation.isDone)
    {
      Debug.Log("Load-AsyncOperation-Progress: " + asyncOperation.progress);

      yield return null;
    }
  }

  /* If the NEAT-scene is loaded for the second time, its "OnEable"-function is not called anymore. I strongly suspect a Unity-bug! That's why there is no option 
   * for this scene to return to the menu. Quitting the application should be easy nonetheless. */
  public void Exit()
  {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }
}