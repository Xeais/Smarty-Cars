using UnityEngine;

namespace NEAT
{
	/// <summary>
	/// Saveable gene
	/// </summary>
	[System.Serializable]
	public class PackedGene
	{
    [SerializeField] private bool isEnabled;
    public bool IsEnabled {get => isEnabled;}
    [Space]
    [SerializeField] private float weight;
    public float Weight {get => weight;}
    [Header("Neurons")]
    [SerializeField] private PackedNeuron inNeuron;
    public PackedNeuron InNeuron {get => inNeuron;}
    [SerializeField] private PackedNeuron outNeuron;
    public PackedNeuron OutNeuron {get => outNeuron;}

    public PackedGene(Gene gene)
		{
			weight = gene.Weight;
			isEnabled = gene.IsEnabled;
			inNeuron = new PackedNeuron(gene.StartNode);
			outNeuron = new PackedNeuron(gene.EndNode);
		}
  }
}