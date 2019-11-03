using UnityEngine;

public class SpinCamera : MonoBehaviour
{
  [SerializeField] private Vector3 speeds = Vector3.zero;

	private void Update()
  {
		transform.Rotate(speeds * Time.deltaTime);
	}
}