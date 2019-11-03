using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Car
{
  [SerializeField] private WheelCollider leftWheel = null;
  public WheelCollider LeftWheel {get => leftWheel;}
  [SerializeField] private GameObject leftWheelMesh = null;
  public GameObject LeftWheelMesh {get => leftWheelMesh;}
  [SerializeField] private WheelCollider rightWheel = null;
  public WheelCollider RightWheel {get => rightWheel;}
  [SerializeField] private GameObject rightWheelMesh = null;
  public GameObject RightWheelMesh {get => rightWheelMesh;}
  [SerializeField] private bool powered = false;
  public bool Powered {get => powered;}
  [SerializeField] private bool steered = false;
  public bool Steered {get => steered;}
  [SerializeField] private bool steeringReversed = false;
  public bool SteeringReversed {get => steeringReversed;}
}

public class CarController : MonoBehaviour
{
  [SerializeField] private bool isAI = true;
  [Header("Axles"), Tooltip("Front -> rear")]
  [SerializeField] private List<Car> axles = new List<Car>();
  [Header("Engine & Steering")]
  [SerializeField, Range(10f, 1000f)] private float maxMotorTorque = 150f;
  [SerializeField, Range(10f, 60f)] private float maxSteeringAngle = 45f;
  [Header("Physics & Lights")]
#if UNITY_EDITOR
  [ReadOnly] //No custom "ReadOnly" for building
#endif
  [SerializeField, Tooltip("The center of mass (abbreviation: com) is the point at which the mass of an object is concentrated. This value is really finicky and has a huge impact on the result! That's why it is read-only.")] private Vector3 cOM = new Vector3(0f, 0.3f, -0.7f);
  [SerializeField] private float downforce = 200f;
  [SerializeField] private GameObject rearLights = null;
  [Header("Inputs (Read-only):")]
#if UNITY_EDITOR
  [SerializeField, ReadOnly] private float motorTorque = 0f;
  [SerializeField, ReadOnly] private float steerAngle = 0f;
  [SerializeField, ReadOnly] private float brakeTorque = 0f;
#else
  [SerializeField] private float motorTorque = 0f;
  [SerializeField] private float steerAngle = 0f;
  [SerializeField] private float brakeTorque = 0f;
#endif

  private Rigidbody rigidb = null;
  public Rigidbody Rigidb {get => rigidb ?? (rigidb = GetComponent<Rigidbody>());}

  private void Awake()
  {
    rigidb = GetComponent<Rigidbody>();
    rigidb.centerOfMass = cOM;
  }

  private void FixedUpdate()
  {
    rigidb.drag = rigidb.velocity.magnitude / 250f; //Easy way to limit the maximum speed of the car. In reality, this is achieved by air resistance.

    if(!isAI)
      Move(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
  }

  public void Move(float verticalAxis, float horizontalAxis, float jumpAxis = 0f)
  {
    motorTorque = verticalAxis * maxMotorTorque * Time.fixedDeltaTime * 720f;
    steerAngle = horizontalAxis * maxSteeringAngle;

    Vector3 localVel = transform.InverseTransformDirection(rigidb.velocity);
    if((motorTorque < 0f && localVel.z > 1f) || (motorTorque > 0f && localVel.z < -1f))
      brakeTorque = rigidb.mass * 3f;
    else if(motorTorque == 0f)
      brakeTorque = 60f;
    else
      brakeTorque = 0f;

    foreach(Car axle in axles)
    {
      if(axle.Steered)
        axle.LeftWheel.steerAngle = axle.RightWheel.steerAngle = (axle.SteeringReversed ? -1f : 1f) * steerAngle;

      if(axle.Powered && brakeTorque <= 0f)
      {
        axle.LeftWheel.brakeTorque = axle.RightWheel.brakeTorque = 0f;
        axle.LeftWheel.motorTorque = axle.RightWheel.motorTorque = motorTorque;

        if(rearLights.activeSelf)
          rearLights.SetActive(false);
      }
      else if(brakeTorque > 0f)
      {
        axle.LeftWheel.brakeTorque = axle.RightWheel.brakeTorque = brakeTorque;
        axle.LeftWheel.motorTorque = axle.RightWheel.motorTorque = 0f;

        if(brakeTorque > 100f)
          rearLights.SetActive(true);
      }

      RotateWheels(axle);
    }
  }

  //Add more grip in relation to speed.
  private void AddDownForce() => rigidb.AddForce(-transform.up * downforce * rigidb.velocity.magnitude);

  private void RotateWheels(Car wheelPair)
  {
    wheelPair.LeftWheel.GetWorldPose(out Vector3 pos, out Quaternion rot);
    wheelPair.LeftWheelMesh.transform.position = pos;
    wheelPair.LeftWheelMesh.transform.rotation = rot;

    wheelPair.RightWheel.GetWorldPose(out pos, out rot);
    wheelPair.RightWheelMesh.transform.position = pos;
    wheelPair.RightWheelMesh.transform.rotation = rot;
  }
}