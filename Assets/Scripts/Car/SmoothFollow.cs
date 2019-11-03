using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
  [SerializeField] private bool follow = true;
  public bool Follow {get => follow; set => follow = value;}
  [Header("Transforms")]
  [SerializeField] private Transform target;
  public Transform Target {get => target; set => target = value;}
  [SerializeField] private Transform allMapViewPosition = null;
  [Header("Settings")]
  [SerializeField] private float height = 20f;
  [SerializeField] private float smoothDampTimePosition = 0.5f;
  [Space]
  [SerializeField, Range(35f, 50f), Tooltip("Angle (in degrees) to view the AI-cars - camera follows the leader - from behind.")] private float tiltAngle = 50f;
  [SerializeField, Tooltip("Show the rear-view while this key is held down.")] private KeyCode rearView = KeyCode.R;

  private Vector3 smoothDampVel;
  private Quaternion cameraOrientation;

  private void Awake()
  {
    GotoCarView();
  }

  private void LateUpdate()
	{
		if(!follow || !target)
			return;

		SmoothDampToTarget();
    TurnCameraAroundYAxis();
  }

  private void SmoothDampToTarget()
	{
		Vector3 targetPosition = target.position + Vector3.up * height;
		transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref smoothDampVel, smoothDampTimePosition);
  }

  private void TurnCameraAroundYAxis()
  {
    float invert = 0f;
    if(Input.GetKey(rearView))
      invert = (target.rotation.eulerAngles.y > 0f) ? 180f : -180f; 

    transform.rotation = Quaternion.Euler(tiltAngle, target.rotation.eulerAngles.y + invert, 0f);
  }

  public void GotoCarView()
  {
    if(!follow)
      return;

    float yRotation = Input.GetKey(rearView) ? 0f : 180f; //Rotate 180 degrees around the y-axis to get a straight-on behind-view.
    cameraOrientation = Quaternion.Euler(tiltAngle, yRotation, 0f);
    transform.rotation = cameraOrientation;
  }

  public void GotoAllMapView()
	{
		if(allMapViewPosition == null)
			return;

    transform.position = allMapViewPosition.position;

    cameraOrientation = Quaternion.Euler(90f, 90f, 50f);
    transform.rotation = cameraOrientation;
	}
}