using System.Collections.Generic;
using UnityEngine;
using NEAT;

public class GenomeCar : GenomeProxy
{
  [Header("Life & Display")]
  [SerializeField] private float life = 100f;
  [SerializeField] private UnityEngine.UI.Text fitnessText = null;
  [SerializeField] private SpriteRenderer speciesColor = null;
  public SpriteRenderer SpeciesColor {get => speciesColor;}
  [Header("Genome-Saver-Settings")]
  [SerializeField, Tooltip("When disabled, the genome-saver stores all accumulating genomes in the default location.")] private bool saveGenomeInDir = true;
  [Space] //= [Space(8)], since eight is the default value.
  [SerializeField, Tooltip("Merge all genomes in an additional folder, which resides in the default storage location of the genome-saver.")] private string genomeExtraSaveDir = "SavedGenomes";
  [Header("Raycasting")]
	[SerializeField] private LayerMask obstacleLayer = 0;
	[SerializeField] private Transform raycastOrigin = null;
	[SerializeField] private Transform[] raycastEndPoints = null;

  public CarController CarControl {get; private set;}
  public PopulationCar PopulCar {get; private set;}

	private Vector3 lastPositionMark;
  private int finishCross = 0;
  private float currentMaxFitness, lastMaxFitnessUpdate = 0f;
  
  private List<Checkpoint> checkpointPassed = new List<Checkpoint>();

  private void Start()
  {
    CarControl = GetComponent<CarController>();
    PopulCar = FindObjectOfType<PopulationCar>();
  }

  private void Update()
	{
		fitnessText.text = ((int)GenomeProperty.Fitness).ToString();

		life -= CarSettings.Instance.DieSpeed * Time.deltaTime;
		if(life <= 0f)
			Die();

		DieIfNotImproving();
	}

	private void FixedUpdate()
	{
		ActivateNeuralNet();
		CalculateFitness();
	}

  private void OnCollisionEnter(Collision collision) => Die();

  private void OnTriggerEnter(Collider other)
	{
		var checkpoint = other.GetComponent<Checkpoint>();
		if(checkpoint == null)
			return;

		if(checkpoint.IsFinish)
			ProcessFinishCross();

		if(checkpoint.IsFitnessPoint && !checkpointPassed.Contains(checkpoint))
			ProcessFitnessPointCross(checkpoint);

		if(checkpoint.IsTeleport)
			ProcessTeleportCross(checkpoint);
	}

	public override void Init(int id, Population popul)
	{
		base.Init(id, popul);
		AssignGenomeInputFunctions();
	}

	public override void ProcessNetworkOutput(float[] netOutputs)
	{
		base.ProcessNetworkOutput(netOutputs);
		CarControl.Move(verticalAxis: Mathf.Lerp(-1f, 1f, netOutputs[0]), horizontalAxis: Mathf.Lerp(-1f, 1f, netOutputs[1]));
	}

	public void Reinit(Transform targetPositionRotation)
	{
		transform.SetPositionAndRotation(targetPositionRotation.position, targetPositionRotation.rotation);

		lastPositionMark = transform.position;
		gameObject.SetActive(true);

		IsDone = false;
    GenomeProperty.Fitness = 0f;

		life = CarSettings.Instance.StartingCarLife;
		finishCross = 0;
		checkpointPassed.Clear();

		currentMaxFitness = 0f;
		lastMaxFitnessUpdate = Time.time;
	}

	public void Die()
	{
		IsDone = true;
		gameObject.SetActive(false);
	}

	private void AddFitness(float fitness)
	{
    GenomeProperty.Fitness += fitness;
		life += fitness;
	}

	/// <summary>
	/// Define the car's inputs.
	/// A for loop can't be used here, since lambda-functions are defined.
	/// </summary>
	private void AssignGenomeInputFunctions()
	{
		InputFunctions = new GenomeInputFunction[Popul.Config.InputCount];

		InputFunctions[0] = () => GetNormRaycastHit(raycastEndPoints[0].position);
		InputFunctions[1] = () => GetNormRaycastHit(raycastEndPoints[1].position);
		InputFunctions[2] = () => GetNormRaycastHit(raycastEndPoints[2].position);
		InputFunctions[3] = () => GetNormRaycastHit(raycastEndPoints[3].position);
		InputFunctions[4] = () => GetNormRaycastHit(raycastEndPoints[4].position);
		InputFunctions[5] = () => GetNormRaycastHit(raycastEndPoints[5].position);
		InputFunctions[6] = () => GetNormRaycastHit(raycastEndPoints[6].position);
		InputFunctions[7] = () => GetNormRaycastHit(raycastEndPoints[7].position);
		InputFunctions[8] = () => GetNormRaycastHit(raycastEndPoints[8].position);

		//Velocity:
		InputFunctions[9] = () =>
		{
			float forwardVel = transform.InverseTransformDirection(CarControl.Rigidb.velocity).z;
			return Mathf.InverseLerp(PopulCar.MaxNegativeVelocity, PopulCar.MaxVelocity, forwardVel);
		};
	}

	/// <summary>
	/// Make a raycast in the given direction and return the distance to the first object it hit within the "obstacleLayer"-mask. 
  /// The distance is normalized between from zero to one. In case it didn't hit anything, one is returned.
	/// </summary>
	/// <returns>Normalized distance to the first object detected</returns>
	private float GetNormRaycastHit(Vector3 endPoint)
	{
    bool aHit = Physics.Linecast(raycastOrigin.position, endPoint, out RaycastHit hit, obstacleLayer);
    if(GizmosControl.Instance.EnableDebugLines && aHit) 
      GL_Debug.DrawLine(raycastOrigin.position, hit.point, Color.red);
		
		float dist = (raycastOrigin.position - endPoint).magnitude;
		return aHit ? (hit.point - raycastOrigin.position).magnitude / dist : 1f;
	}

  public bool IsDrivingForward() => Mathf.Sign(transform.InverseTransformDirection(CarControl.Rigidb.velocity).z) > 0f;

  /// <summary>
  /// The fitness is calculated by saving the last position and calculating the traveled distance from the last position.
  /// If the car moves forward, the fitness is added, otherwise, it's substracted.
  /// </summary>
  private void CalculateFitness()
	{
		Vector3 localVel = transform.InverseTransformDirection(CarControl.Rigidb.velocity);
		float travelDir = Mathf.Sign(localVel.z);

		float fitness = (transform.position - lastPositionMark).magnitude;
		fitness *= CarSettings.Instance.FitnessPerUnit * Time.fixedDeltaTime * travelDir;
		AddFitness(fitness);

		lastPositionMark = transform.position;
	}

	private void DieIfNotImproving()
	{
		if(GenomeProperty.Fitness > currentMaxFitness)
		{
			currentMaxFitness = GenomeProperty.Fitness;
			lastMaxFitnessUpdate = Time.time;
		}

		if(Time.time > lastMaxFitnessUpdate + CarSettings.Instance.MaxTimeWithNoFitnessImprovement)
			Die();
	}

	private void ProcessFinishCross()
	{
		if(!IsDrivingForward())
		{
			Die();
			return;
		}

		finishCross++;
		if(finishCross >= CarSettings.Instance.TargetFinishCrossTimes)
		{
			float fitnessMult = CarSettings.Instance.FinishFitnessMultiplier;
			fitnessMult *= Mathf.Lerp(1f, CarSettings.Instance.FitnessMultiplierForBeingFirst, 1f - (float)PopulCar.TheRealFinish.CrossedBy.Count / PopulCar.NEATConfig.GenomeCount);
			PopulCar.TheRealFinish.CrossedBy.Add(this);
			AddFitness(GenomeProperty.Fitness * fitnessMult);

			Die();
			SaveGenome();
		}
	}

	private void ProcessFitnessPointCross(Checkpoint checkpoint)
	{
		checkpointPassed.Add(checkpoint);
		if(IsDrivingForward())
			AddFitness(checkpoint.FitnessWhenTouched);
	}

	private void ProcessTeleportCross(Checkpoint checkpoint)
	{
		if(!IsDrivingForward())
		{
			Die();
			return;
		}

		checkpoint.Teleport(transform);
	}

	private void SaveGenome()
	{
		string dir = (saveGenomeInDir && genomeExtraSaveDir != "") ? GenomeSaver.DefaultSaveDir + genomeExtraSaveDir + "\\" : GenomeSaver.DefaultSaveDir;
		if(!System.IO.Directory.Exists(dir))
			System.IO.Directory.CreateDirectory(dir);

		string filePath = GenomeSaver.GenerateSaveFilePath(dir, GenomeProperty.Fitness, PopulCar.Popul.Generation);
		GenomeSaver.SaveGenome(GenomeProperty, filePath);
	}
}