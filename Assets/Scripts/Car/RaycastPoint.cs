using UnityEngine;

public class RaycastPoint : MonoBehaviour
{
	[SerializeField] private bool drawGizmos = false;
  [Header("Position & Size")]
	[SerializeField] private Transform rayOrigin = null;
  [SerializeField] private float length = 1f;

  private void LateUpdate()
  {
    if(!drawGizmos || !GizmosControl.Instance || !GizmosControl.Instance.EnableGizmos)
      return;

    if(rayOrigin != null)
    {
      GL_Debug.DrawLine(rayOrigin.position, transform.position, Color.red);
      GL_Debug.DrawCube(transform.position, Quaternion.identity, Vector3.one * length, Color.green);
    }
  }

  /* Old solution:
  private void OnDrawGizmos()
  {
    if(!drawGizmos || !GizmosControl.Instance || !GizmosControl.Instance.EnableGizmos)
      return;

    Gizmos.DrawWireSphere(transform.position, length);
    Gizmos.DrawWireCube(transform.position, Vector3.one * length);

    if(rayOrigin != null)
      Gizmos.DrawLine(rayOrigin.position, transform.position);
  }*/
}