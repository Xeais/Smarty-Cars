using UnityEngine;

public class GizmosControl : Singleton<GizmosControl>
{
	[SerializeField] private bool enableGizmos = false; //Gizmos are used to give visual debugging or setup aids in the Scene-view.
  public bool EnableGizmos {get => enableGizmos;}
  [SerializeField] private bool enableDebugLines = true;
  public bool EnableDebugLines {get => enableDebugLines;}

  protected GizmosControl() {}
}