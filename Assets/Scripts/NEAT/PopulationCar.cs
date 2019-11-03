using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NEAT;

public class PopulationCar : PopulationProxy
{
  [Header("Velocities")]
  [SerializeField] private float maxVelocity = 55f;
  public float MaxVelocity {get => maxVelocity;}
  [SerializeField] private float maxNegativeVelocity = -5f;
  public float MaxNegativeVelocity {get => maxNegativeVelocity;}
  [Header("UI")]
	[SerializeField] private UnityEngine.Events.UnityEvent onGenerationChange = null;
	[SerializeField] private UnityEngine.Events.UnityEvent onGenomeStatusChange = null;
  [Space]
  [SerializeField] private UnityEngine.UI.Text velocity = null; //To avoid the quite slow "Find"-methods.
  [SerializeField, Tooltip("Speed-​​values ​​greater than this are displayed in red via the user interface.")] private float criticalSpeed = 80f;
  [Header("Start & Finish")]
  [SerializeField] private Transform realStart = null;
  [SerializeField] private Checkpoint realFinish = null;
  public Checkpoint TheRealFinish {get => realFinish;}

  public bool EveryoneIsDead => cars.FirstOrDefault(x => !x.IsDone) == null;

  private List<GenomeCar> cars = null;
	private GenomeColorControl genomeColorCtrl = new GenomeColorControl();
  private SmoothFollow cameraFollow = null;
  
	protected override void Awake()
	{
		base.Awake();

		if(realStart == null)
		{
			Debug.LogError("Car spawn point must not be null!");
			Debug.Break();
		}

    if(realFinish != null)
    {
      if(!realFinish.IsFinish || realFinish.IsFitnessPoint || realFinish.IsTeleport) //Ensure the correct settings if a mismatch was found.
      {
        realFinish.IsFinish = true;
        realFinish.IsFitnessPoint = realFinish.IsTeleport = false;

        Debug.Log("The settings of the final destination have been corrected.");
      }
    }
    else
      Debug.Log("<color=red>The final destination has not been set!</color>");

    cameraFollow = FindObjectOfType<SmoothFollow>();
	}

	private void FixedUpdate()
	{
		GenomeCar cameraTarget = cars.OrderByDescending(x => x.GenomeProperty.Fitness).FirstOrDefault(x => x.gameObject.activeSelf);
		cameraFollow.Target = cameraTarget?.transform;

		if(EveryoneIsDead)
			Evolve();

		onGenomeStatusChange.Invoke();

    float kmh = cameraTarget?.CarControl?.Rigidb.velocity.magnitude * 3.6f ?? 0f;
    string speedColor = (kmh <= criticalSpeed) ? $"<color=white>{kmh.ToString("F0")}</color>" : $"<color=red>{kmh.ToString("F0")}</color>";
    velocity.text = $"Velocity of Leader: {speedColor} km/h";
  }

	public void KillAll()
	{
		foreach(var car in cars)
			car.Die();

    ResetCameraRotation();
  }

	public void ReinitCars()
	{
		foreach(var car in cars)
			car.Reinit(realStart);

	  realFinish?.CrossedBy.Clear();

    ResetCameraRotation();
  }

	public override void Evolve()
	{
		base.Evolve();
		ReinitCars();

		genomeColorCtrl.UpdateSpeciesColor(Popul.SpeciesCtrl);
		foreach(var car in cars)
			car.SpeciesColor.color = genomeColorCtrl.GetSpeciesColor(car.GenomeProperty.SpeciesId);
		onGenerationChange.Invoke();
	}

	public override void InitPopul()
	{
		base.InitPopul();
		cars = new List<GenomeCar>(NEATConfig.GenomeCount);
	}

	protected override void InitGenomeProxyObj(GameObject genomeProxy)
	{
		var genomeCar = genomeProxy.GetComponent<GenomeCar>();
		if(cars.FirstOrDefault(x => x.Id == genomeCar.Id) == null)
		{
			cars.Add(genomeCar);
			genomeCar.Reinit(realStart);
		}
	}

  private void ResetCameraRotation() => cameraFollow.GotoCarView();
}