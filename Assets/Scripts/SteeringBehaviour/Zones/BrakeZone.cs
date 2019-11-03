using UnityEngine;

namespace SteeringBehaviour
{
  //Just for demonstration purposes as it is not necessary for this track (and car - stable enough).
  public class BrakeZone : MonoBehaviour
  {
    [SerializeField] private SmartCar smartCar = null;
    [SerializeField, Range(0f, 1f)] private float brakingPower = 0.1f;
    [SerializeField, Range(0, 100), Tooltip("The maximum motor torque is decreased by as many percents while staying in the brake zone.")] int reduceMaxMotorTorque = 5;

    private float startTorque = 0f;

    private void Start()
    {
      if(!smartCar)
        Debug.LogWarning("A brake zone has no associated \"SmartCar\"!");

      //Grab a reference to the user-set motor-torque for resetting to later.
      startTorque = smartCar.MaxMotorTorque;
    }

    private void OnTriggerEnter(Collider other)
    {
      smartCar.BrakeTorque = brakingPower;
      smartCar.MaxMotorTorque -= startTorque / 100 * reduceMaxMotorTorque;
    }

    private void OnTriggerExit(Collider other)
    {
      smartCar.BrakeTorque = 0f;
      smartCar.MaxMotorTorque = startTorque;
    }
  }
}