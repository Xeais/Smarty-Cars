using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Be aware, this will not prevent a non singleton constructor such as "T myT = new T();".
/// To prevent that, add "protected T() {}" to your inheriting singleton-class!
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
  private static Dictionary<Type, Singleton<T>> instances;
  public static T Instance
  {
    get
    {
      if(Quitting)
      {
        Debug.LogWarning($"Instance of \"Singleton<{typeof(T)}>\" already destroyed on application quit. " +
                         "Won't create again - returning null.");

        return null;
      }

      lock(locked)
      {
        if(instances == null)
          instances = new Dictionary<Type, Singleton<T>>();

        if(instances.ContainsKey(typeof(T)))
        {
          T test = instances[typeof(T)] as T;

          /* Debug.Log($"Using instance already created: {test.name}!");
           * The ??-operator is called the null-coalescing operator. 
           * It returns the left-hand operand if the operand is not null; 
           * otherwise it returns the right hand operand. */
          return test ?? null;
        }
        else
          return null;
      }
    }
  }

  protected static bool Quitting {get; private set;}

  [RuntimeInitializeOnLoadMethod] private static void RunOnStart() => Application.quitting += Quit;
  private static readonly object locked = new object(); //-> https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/lock-statement

  private void OnEnable()
  {
    if(!Quitting)
    {
      bool iAmSingleton = false;

      lock(locked)
      {
        if(instances == null)
          instances = new Dictionary<Type, Singleton<T>>();

        if(instances.ContainsKey(GetType()))
        {
          Debug.Log($"<color=red>There is already an instance of \"Singleton<{typeof(T)}>\". \"{gameObject.name}\" will be deleted!</color>");
          Destroy(gameObject);
        }
        else
        {
          iAmSingleton = true;

          instances.Add(GetType(), this);

          Debug.Log($"<color=green>An instance of \"Singleton<{typeof(T)}>\" was created.</color>");

          /* Not used for this project ...
          if(gameObject.transform == transform.root) //"DontDestroyOnLoad()" only works for root "GameObjects" and its components.
          { 
            DontDestroyOnLoad(gameObject);
            Debug.Log($"<color=green>An instance of \"Singleton<{typeof(T)}>\" was created with \"DontDestroyOnLoad()\".</color>");
          }*/
        }
      }

      if(iAmSingleton)
        OnEnableCallback();
    }
  }

  private void OnDisable() //-> https://docs.unity3d.com/Manual/ExecutionOrder.html)
  {
    if(instances.ContainsKey(GetType()))
      instances = null;

    OnDisableCallback();
  }

  private static void Quit() => Quitting = true;

  protected virtual void OnEnableCallback() => Debug.Log($"The virtual function \"OnEnableCallback()\" of \"Singleton<{typeof(T)}>\" has been called.");

  protected virtual void OnDisableCallback() => Debug.Log($"The virtual function \"OnDisableCallback()\" of \"Singleton<{typeof(T)}>\" has been called. <color=orange>\"instances\" have been cleared!</color>");
}