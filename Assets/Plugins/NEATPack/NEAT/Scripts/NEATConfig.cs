using UnityEngine;

namespace NEAT
{
	[System.Serializable]
	public class NEATConfig
	{
    [Header("Amounts")]
    [SerializeField] private int genomeCount = 50;
    public int GenomeCount {get => genomeCount;}
    [SerializeField] private int inputCount = 1;
    public int InputCount {get => inputCount;}
    [SerializeField] private int outputCount = 1;
    public int OutputCount {get => outputCount;}
    [Header("Weight & Switch-off")]
    [SerializeField, Tooltip("The gene's weight is initialized with a value between -this value and +this value.")] private float weightInitRandomValue = 1f;
    public float WeightInitRandomValue {get => weightInitRandomValue;}
    //[SerializeField] private float chanceToDisableGeneIfDisabledInEitherParent = 0.75f; Not used at the moment
		[Header("Evolution")]
    [SerializeField, Tooltip("Maximal number of generations allowed for a generation to not improve.")] private int maxStagnation = 20;
    public int MaxStagnation {get => maxStagnation;}
    [SerializeField, Tooltip("If a species has at least this much fitness, it won't be killed, even if doesn't improve in \"maxStagnation\" generations.")] private float goodFitness = 9999999f;
    public float GoodFitness {get => goodFitness;}
    [SerializeField, Tooltip("The percentage of genomes to be remembered for the next generation in a species.")] private float genomesToRememberForNextGeneration = 0.25f;
    public float GenomesToRememberForNextGeneration {get => genomesToRememberForNextGeneration;}
    [SerializeField, Tooltip("The minimum number of genomes a species should keep - if possible.")] private int minNbOfGenomesToKeep = 2;
    public int MinNbOfGenomesToKeep {get => minNbOfGenomesToKeep;}
    [SerializeField, Tooltip("The gradient that decides how much priority those with big fitness have.")] private float weightedRandomGradient = 1f;
    public float WeightedRandomGradient {get => weightedRandomGradient;}
    [SerializeField, Tooltip("The percentage of genomes to be soft copied for the next generation. Soft, because they will be only slightly mutated.")] private float partOfGenomesToCopyForNextGenerations = 0.2f;
    public float PartOfGenomesToCopyForNextGenerations {get => partOfGenomesToCopyForNextGenerations;}

    [SerializeField] private StructMutation structMutation = new StructMutation();
    public StructMutation StructMutation {get => structMutation;}
    [SerializeField] private WeightMutation weightMutation = new WeightMutation();
    public WeightMutation WeightMutation {get => weightMutation;}
    [SerializeField] private SpeciesCompatibility speciesCompatibility = new SpeciesCompatibility();
    public SpeciesCompatibility SpeciesCompatibility {get => speciesCompatibility;}

    public System.Func<float, float> ActivationFunction {get;} = Sigmoid;

    /// <summary>
    /// A random weight	whereby a gene starts.
    /// </summary>
    public float NewRandomWeight() => Random.Range(-weightInitRandomValue, weightInitRandomValue);

		/// <summary>
		///  The value with witch a gene weight is mutated.
		/// </summary>
		public float WeightMutationValue() => WeightMutation.Scale * weightInitRandomValue;

		public float GetAWeightMutation() => Random.Range(-WeightMutationValue(), WeightMutationValue());

		private static float Sigmoid(float x) => 1f / (1f + Mathf.Exp(-x));
	}

	[System.Serializable]
	public class StructMutation
	{
    [Header("Node")]
    [SerializeField, Tooltip("Probability to add a new node.")] private float nodeAddProb = 0.2f;
    public float NodeAddProb {get => nodeAddProb;}
    [SerializeField, Tooltip("Probability to delete a node all its connections.")] private float nodeDelProb = 0.01f;
    public float NodeDelProb {get => nodeDelProb;}
    [Header("Gene")]
    [SerializeField, Tooltip("Probability to add a new gene with a random weight.")] private float connectionAddProb = 0.7f;
    public float ConnectionAddProb {get => connectionAddProb;}
    [SerializeField, Tooltip(" Probability to delete a gene.")] private float connectionDelProb = 0.1f;
    public float ConnectionDelProb {get => connectionDelProb;}
  }

	[System.Serializable]
	public class WeightMutation
	{
    [SerializeField, Tooltip("Chance that the weight will change (in genereal).")] private float prob = 0.8f;
    public float Prob {get => prob;}
    [SerializeField, Tooltip("Chance that the gene's weight will be modified.")] private float changeValProb = 0.9f;
    public float NewValProb => (1f - changeValProb); //Chance that the gene will receive a total new weight.
    [SerializeField, Tooltip("Weight mutation scale: A mutation value is calucated through \"startingMutationValue * scale\".")] private float scale = 0.1f;
    public float Scale {get => scale;}
  }

	/// <summary>
	/// Configurations for population speciation
	/// The calculated genetic distance (genetic compatibility) should be normalized.
	/// </summary>
	[System.Serializable]
	public class SpeciesCompatibility
	{
    [Header("Impacts")]
    [SerializeField] private float excessImpact = 1f;
    public float ExcessImpact {get => excessImpact;}
    [SerializeField] private float disjointImpact = 1f;
    public float DisjointImpact {get => disjointImpact;}
    [Header("Coefficient & Threshold")]
    [SerializeField] private float weightMeanDifCoef = 3f;
    public float WeightMeanDifCoef {get => weightMeanDifCoef;}
    /* If the genetic distance between two genomes is less than this treshold, then they are of the same species.
     * Setting this to one will ensure there is only one species. */
    [SerializeField] private float threshold = 0.2f;
    public float Threshold {get => threshold;}

    public float TotalImpactSum => excessImpact + disjointImpact + weightMeanDifCoef;
  }
}