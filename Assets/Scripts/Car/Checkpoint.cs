using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
  [Header("Type & Configuration")]
  [SerializeField] private bool isFinish = false;
  public bool IsFinish {get => isFinish; set => isFinish = value;}
  [Space]
  [SerializeField] private bool isFitnessPoint = false;
  public bool IsFitnessPoint {get => isFitnessPoint; set => isFitnessPoint = value;}
  [SerializeField, Tooltip("This value is immediately added to the fitness-value (NEAT) of the car touching this checkpoint.")] private float fitnessWhenTouched = 1000f;
  public float FitnessWhenTouched {get => fitnessWhenTouched;}
  [Space]
  [SerializeField] private bool isTeleport = false;
  public bool IsTeleport {get => isTeleport; set => isTeleport = value;}
  [SerializeField] private Transform teleportPosition = null;
  [Header("Miscellaneous")]
  [SerializeField] private LayerMask layerMask;
  [Space]
  [SerializeField, Tooltip("Keeps track of which genome-cars (NEAT) have passed this checkpoint.")] private List<GenomeCar> crossedBy = new List<GenomeCar>();
  public List<GenomeCar> CrossedBy {get => crossedBy;}
  [Space]
  [SerializeField, Tooltip("These fireworks are shot in the air to celebrate the crossing of the, last - in case of NEAT, finish line.")] private GameObject fireworksAll = null;

  private GameObject fireworks;
  private bool instantiated = false;

  public void Teleport(Transform target)
	{
    if(teleportPosition != null)
    {
      target.transform.position = teleportPosition.position;
      target.transform.rotation = teleportPosition.rotation;
      target.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
    else if(isTeleport)
      Debug.LogWarning("A teleport-checkpoint has not been allotted a teleport position!");
	}

  private void OnTriggerEnter(Collider other)
  {
    //Debug.Log("Other tag: " + other.tag);
    if(fireworksAll != null && isFinish && other.CompareTag("AI") && (other.GetComponentInParent<GenomeCar>()?.IsDrivingForward() ?? true /* Not NEAT, so it is safe to assume the car is going forward. */))
    {
      if(!instantiated)
      {
        fireworks = Instantiate(fireworksAll, transform.position, Quaternion.identity);
        instantiated = true;
      }

      fireworks?.GetComponent<ParticleSystem>()?.Play();

      Debug.Log($"<color=green>We have a finisher - bravo!</color>");
    }
  }

  private void OnTriggerExit(Collider other)
  {
    var fireworksParSys = fireworks?.GetComponent<ParticleSystem>();
    if(fireworksParSys?.isPlaying ?? false)
      fireworksParSys.Stop();
  }
}