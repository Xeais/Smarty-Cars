using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehaviour
{
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

  public class SmartCar : MonoBehaviour
  {
    [SerializeField] private bool isAI = true;
    [Header("Axles"), Tooltip("Front -> rear")]
    [SerializeField] private List<Car> axles = new List<Car>();
    [Header("Engine & Steering")]
    [SerializeField, Range(10f, 1000f)] private float maxMotorTorque = 200f;
    public float MaxMotorTorque {get => maxMotorTorque; set => maxMotorTorque = value;}
    [SerializeField, Range(10f, 60f)] private float maxSteeringAngle = 45f;
    [SerializeField] private float maxEngineRPM = 3000f;
    [SerializeField] private float minEngineRPM = 1000f;
    [SerializeField] private float[] gearRatio = null; //4.31, 2.71, 1.88, 1.41, 1.13, 0.93
    [Header("Physics & Lights")]
#if UNITY_EDITOR
    [ReadOnly] //No custom "ReadOnly" for building
#endif
    [SerializeField, Tooltip("The center of mass (abbreviation: com) is the point at which the mass of an object is concentrated. This value is quite finicky. That's why it is read-only.")] private Vector3 cOM = new Vector3(0f, -0.5f, -1f);
    [SerializeField] private float downforce = 100f;
    [SerializeField] private GameObject rearLights = null;
    [Header("Steering Behaviour")]
    [SerializeField] private Transform[] waypoints = null;
    [SerializeField, Tooltip("If the car is at least this distance away from a waypoint, it is considered as passed.")] private float wayPointDistance = 20f;
    [Header("Sensors")]
    [SerializeField, Range(5f, 15f)] private float sensorLength = 5f;
    [SerializeField] private float sideSensorLength = 3f;
    [SerializeField] private Vector3 frontSensorPosition = new Vector3(0f, 0.65f, 2.4f);
    [SerializeField, Tooltip("This should be half the width of the car.")] private float frontSideSensorDist = 1.2f;
    [SerializeField] private LayerMask sensorLayerMask = 0;
    [SerializeField, Range(20f, 35f)] private float frontSensorAngle = 25f;
    [SerializeField, Range(5f, 20f)] private float avoidSpeed = 15f;
    [SerializeField, Range(0f, 1f), Tooltip("If the car is heading right on an obstacle, it will be slowed down by this value.")] private float brakingPower = 0.1f;
    [SerializeField, Range(3f, 15f), Tooltip("After this amount of seconds the car counts as stuck.")] private float waitToReverse = 3f;
    [SerializeField, Range(2f, 10f)] private float reverseFor = 4f;
    [SerializeField, Range(3f, 10f)] private float respawnTime = 10f;
    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Text velocity = null; //To avoid the quite slow "Find"-methods.
    [SerializeField, Tooltip("Speed-​​values ​​greater than this are displayed in red via the user interface.")] private float criticalSpeed = 60f;
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
    public float BrakeTorque {get => brakeTorque; set => brakeTorque = value;}

    private Rigidbody rigidb = null;
    public Rigidbody Rigidb {get => rigidb ?? (rigidb = GetComponent<Rigidbody>());}

    private Vector3 relativeWaypointPosition;
    private int currentGear, currentWaypoint = 0;
    private float inputSteer, inputTorque, firstPoweredRPM, summedRPM, engineRPM, reverseCounter, respawnCounter = 0f;
    private readonly float minSignifantSpeed = 2f;
    private bool avoiding = false;
    private bool reversing = false;

    private void Awake()
    {
      rigidb = GetComponent<Rigidbody>();
      rigidb.centerOfMass = cOM;
    }

    private void FixedUpdate()
    {
      rigidb.drag = rigidb.velocity.magnitude / 250f; //Easy way to limit the maximum speed of the car. In reality, this is achieved by air resistance.

      float kmh = rigidb.velocity.magnitude * 3.6f;
      string speedColor = (kmh <= criticalSpeed) ? $"<color=black>{kmh.ToString("F0")}</color>" : $"<color=red>{kmh.ToString("F0")}</color>";
      velocity.text = $"<b>Velocity:</b> {speedColor} km/h";

      if(isAI)
      {
        NavigateTowardsWaypoint();

        int i = 0;
        bool firstPowered = false;
        foreach(Car axle in axles)
        {
          if(axle.Powered)
          {
            if(!firstPowered)
            {
              firstPoweredRPM = axle.LeftWheel.rpm;
              firstPowered = true;
            }

            summedRPM += axle.LeftWheel.rpm + axle.RightWheel.rpm;
            i++;
          }
        }

        engineRPM = summedRPM / i * gearRatio[currentGear];
        ShiftGears();

        Sensors();
        Move(inputTorque, inputSteer);
        Respawn();
      }
      else
        Move(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
    }

    public void Move(float verticalAxis, float horizontalAxis)
    {
      //Debug.Log($"Move({verticalAxis}, {horizontalAxis})");
      float reverse = reversing ? -1f : 1f;
      motorTorque = reverse * maxMotorTorque * 10f / gearRatio[currentGear] * verticalAxis;
      steerAngle = avoiding ? horizontalAxis * avoidSpeed : horizontalAxis * maxSteeringAngle;

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
          brakeTorque /= 5f;
          axle.LeftWheel.brakeTorque = axle.RightWheel.brakeTorque = brakeTorque;
          axle.LeftWheel.motorTorque = axle.RightWheel.motorTorque = maxMotorTorque; //BrakeZone!

          rearLights.SetActive(true);
        }

        RotateWheels(axle);
      }
    }

    private void ShiftGears()
    {
      int appropriateGear = currentGear;
      if(engineRPM >= maxEngineRPM)
      {

        for(int i = 0; i < gearRatio.Length; i++)
        {
          if(firstPoweredRPM * gearRatio[i] < maxEngineRPM)
          {
            appropriateGear = i;
            break;
          }
        }
        currentGear = appropriateGear;
      }

      if(engineRPM <= minEngineRPM)
      {
        appropriateGear = currentGear;
        for(int j = gearRatio.Length - 1; j >= 0; j--)
        {
          if(firstPoweredRPM * gearRatio[j] > minEngineRPM)
          {
            appropriateGear = j;
            break;
          }
        }
        currentGear = appropriateGear;
      }
    }

    private void NavigateTowardsWaypoint()
    {
      //Find the relative position of the waypoint from the car transform, so it can be determined how far to the left or right the waypoint is.
      if(waypoints.Length > 0)
      {
        relativeWaypointPosition = transform.InverseTransformPoint(new Vector3(waypoints[currentWaypoint].position.x,
        transform.position.y, waypoints[currentWaypoint].position.z));
      }
      else
      {
        Debug.LogWarning("There are no waypoints to follow!");
        return;
      }

      if(!avoiding)
        inputSteer = relativeWaypointPosition.x / relativeWaypointPosition.magnitude; //Turn angle in percentage

      //Now, do the same for torque, but ensure that it only applies engine-torque when going around a sharp turn below a certain speed.
      if(rigidb.velocity.magnitude < 10f || Mathf.Abs(inputSteer) < 0.5f)
        inputTorque = relativeWaypointPosition.z / relativeWaypointPosition.magnitude - Mathf.Abs(inputSteer);
      else
        inputTorque = 0f;

      //This just checks if the car's position is near enough a waypoint to count as passed. If it is, the target waypoint will be changed to the next one in the list.
      if(relativeWaypointPosition.magnitude <= wayPointDistance)
      {
        currentWaypoint++;

        if(currentWaypoint >= waypoints.Length)
          currentWaypoint = 0;
      }
    }

    private void Sensors()
    {
      Vector3 sensorStartPos = SetOrResetSensorStartPos();

      Vector3 doorSensor = transform.position;
      doorSensor += transform.up * frontSensorPosition.y;

      float avoidMultiplier = 0f;
      float leftHitDist = 0f;
      float rightHitDist = 0f;
      avoiding = false;

      //Braking sensor:
      if(Physics.Raycast(sensorStartPos, transform.forward, out RaycastHit hit, sensorLength / 3f, sensorLayerMask))
      {
        if(!reversing)
          brakeTorque = brakingPower;
        GL_Debug.DrawLine(sensorStartPos, hit.point, Color.red);
      }
      else
        brakeTorque = 0f;

      //Left side sensor:
      sensorStartPos -= transform.right * frontSideSensorDist;
      if(Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, sensorLayerMask))
      {
        avoiding = true;
        leftHitDist = hit.distance;
        avoidMultiplier += 1f;
        GL_Debug.DrawLine(sensorStartPos, hit.point, Color.blue);
      }
      //Left deviation sensor:
      else if(Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength, sensorLayerMask)) 
      {
        avoiding = true;
        avoidMultiplier += 0.5f;
        GL_Debug.DrawLine(sensorStartPos, hit.point, Color.cyan);
      }

      //Right side sensor:
      sensorStartPos = SetOrResetSensorStartPos();
      sensorStartPos += transform.right * frontSideSensorDist;
      if(Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, sensorLayerMask))
      {
        avoiding = true;
        rightHitDist = hit.distance;
        avoidMultiplier -= 1f;
        if(leftHitDist > 0f && leftHitDist < rightHitDist) //Both side sensors, left and right, register a hit, hence, if possible, steer in the direction in which the hit is farther away.
        {
          avoidMultiplier += 1f;
          Debug.Log("Steering was changed (now: full right-steer), as both side sensors reported a hit!");
        }
        GL_Debug.DrawLine(sensorStartPos, hit.point, Color.green);
      }
      //Right deviation sensor:
      else if(Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength, sensorLayerMask))
      {
        avoiding = true;
        avoidMultiplier -= 0.5f;
        GL_Debug.DrawLine(sensorStartPos, hit.point, Color.yellow);
      }

      //Left door sensor:
      if(Physics.Raycast(doorSensor, -transform.right, out hit, sideSensorLength, sensorLayerMask))
      {   
        avoiding = true;
        avoidMultiplier = 0.5f;
        GL_Debug.DrawLine(doorSensor, hit.point, Color.white);
      }

      //Right door sensor:
      if(Physics.Raycast(doorSensor, transform.right, out hit, sideSensorLength, sensorLayerMask))
      {
        avoiding = true;
        avoidMultiplier = -0.5f;
        GL_Debug.DrawLine(doorSensor, hit.point, Color.magenta);
      }

      //Front middle sensor:
      if(avoidMultiplier == 0f) //This floating-point comparison poses no problems, since "avoidMultiplier" was directly set to zero.  
      {
        sensorStartPos = SetOrResetSensorStartPos();
        if(Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, sensorLayerMask))
        {
          avoiding = true;
          avoidMultiplier = (hit.normal.x < 0f) ? -1f : 1f;
          GL_Debug.DrawLine(sensorStartPos, hit.point, Color.black);
        }
      }

      if(rigidb.velocity.magnitude < minSignifantSpeed && !reversing)
      {
        reverseCounter += Time.deltaTime;
        if(reverseCounter >= waitToReverse)
        {
          reverseCounter = 0f;
          reversing = true;
        }
      }
      else if(!reversing)
        reverseCounter = 0f;

      if(reversing)
      {
        avoidMultiplier *= -1f;
        reverseCounter += Time.deltaTime;
        if(reverseCounter >= reverseFor)
        {
          reverseCounter = 0f;
          reversing = false;
        }
      }

      if(avoiding)
        inputSteer = avoidMultiplier;
    }

    private Vector3 SetOrResetSensorStartPos()
    {
      Vector3 sensorStartPos = transform.position;
      sensorStartPos += transform.forward * frontSensorPosition.z;
      sensorStartPos += transform.up * frontSensorPosition.y;
      return sensorStartPos;
    }

    private void Respawn()
    {
      if(rigidb.velocity.magnitude < minSignifantSpeed)
      {
        respawnCounter += Time.deltaTime;
        if(respawnCounter >= respawnTime)
        {
          Vector3 nextWaypointDir;
          if(currentWaypoint == 0)
          {
            transform.position = waypoints[currentWaypoint].position;
            nextWaypointDir = waypoints[currentWaypoint + 1].position - waypoints[currentWaypoint].position;
          }
          else
          {
            transform.position = waypoints[currentWaypoint - 1].position;
            nextWaypointDir = waypoints[currentWaypoint].position - waypoints[currentWaypoint - 1].position;
          }
          respawnCounter = 0f;

          transform.rotation = Quaternion.LookRotation(nextWaypointDir);

          if(transform.localEulerAngles.z > 0f || transform.localEulerAngles.z < 0f) //Car has been flipped upside-down - change that.
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
        }
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
}