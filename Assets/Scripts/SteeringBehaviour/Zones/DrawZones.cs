using UnityEngine;

namespace SteeringBehaviour
{
  public class DrawZones : MonoBehaviour
  {
    [SerializeField] private Color triggerColor;

    private void OnDrawGizmos()
    {
      Gizmos.color = triggerColor;
      Gizmos.matrix = transform.localToWorldMatrix;
      Gizmos.DrawCube(Vector3.zero, Vector3.one);
      Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    private void OnDrawGizmosSelected()
    {
      Gizmos.color = triggerColor;
      triggerColor.a = 0.25f;
      Gizmos.matrix = transform.localToWorldMatrix;
      Gizmos.DrawCube(Vector3.zero, Vector3.one);
      Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
  }
}