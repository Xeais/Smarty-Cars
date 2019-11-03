using UnityEngine;

namespace SteeringBehaviour
{
  public class Waypoint : MonoBehaviour
  {
    [SerializeField] private bool isStart = false;
    [SerializeField] private bool isEnd = false;
    [Space]
    [SerializeField] private Waypoint next = null;

    private static Waypoint start, end;
    private Transform tr;

    private void Awake()
    {
      tr = GetComponent<Transform>();

      if(!next)
        Debug.Log("This waypoint is not connected. You need to set the next waypoint!", this);

      if(isStart)
        start = this;

      if(isEnd)
        end = this;
    }

    //Returns where the AI should drive towards. "position" is the current position of the car.
    private Vector3 CalculateTargetPosition(Vector3 position)
    {
      //If a car is getting close to a waypoint, return the next one. This results in better behaviour when a car does not exactly hit a waypoint.
      if(Vector3.Distance(transform.position, position) < 6f)
        return next.transform.position;
      else
        return tr.position;
    }

    private void OnDrawGizmos()
    {
      if(next)
      {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, next.transform.position);
      }
    }
  }
}