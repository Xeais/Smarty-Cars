using System.Collections.Generic;
using System.Linq;

namespace NEAT
{
	[System.Serializable]
	public class Gene
	{
    public bool IsEnabled {get; set;} = true;
		public float Weight {get; set;} = 0f;
		public int InnovationNb {get;} = -1;

		public Neuron StartNode {get; private set;} = null;
		public Neuron EndNode {get; private set;} = null;

    //Statically store all kinds of genes to identify the innovation number of a new gene.
    private static List<Gene> kindsOfGenes = new List<Gene>();

    public Gene(Neuron inNode, Neuron outNode, float weight = 0f, bool isEnabled = true)
		{
			GeneInit(inNode, outNode, weight, isEnabled);
			InnovationNb = GetInnovationNumber(this);
		}

		public Gene(Gene other)
		{
			GeneInit(other.StartNode, other.EndNode, other.Weight, other.IsEnabled);
			InnovationNb = other.InnovationNb;
		}

		public Gene(PackedGene packedGene)
		{
			GeneInit(new Neuron(packedGene.InNeuron), new Neuron(packedGene.OutNeuron), packedGene.Weight, packedGene.IsEnabled);
			InnovationNb = GetInnovationNumber(this);
		}

		private void GeneInit(Neuron inNode, Neuron outNode, float weight, bool isEnabled)
		{
			StartNode = inNode;
			EndNode = outNode;
			IsEnabled = isEnabled;
			Weight = weight;
		}

		public override string ToString()
		{
			return  $"Innovation-Number: {InnovationNb}, Weight: {Weight.ToString()} | " +
              $"Start Node: {StartNode.ToString()}, End Node: {EndNode.ToString()}";
		}

		/// <summary>
		/// There is a "config.WeightMutateNewValProb" chance to reinitialize the weight.
		/// </summary>
		public void Mutate(NEATConfig config)
		{
			if(UnityEngine.Random.Range(0f, 1f) < config.WeightMutation.NewValProb)
				Weight = config.NewRandomWeight();
			else
				Weight += config.GetAWeightMutation();
		}

		/// <summary>
		/// Disconnect from its neurons removing the references within.
		/// </summary>
		public void Disconnect()
		{
			StartNode.OutGenes.Remove(this);
			EndNode.InGenes.Remove(this);
		}

		/// <summary>
		/// Connect to the given neurons: Assign the references inside these neurons.
		/// </summary>
		public void ConnectToNeurons(Neuron startNode, Neuron endNode)
		{
			if(!startNode.OutGenes.Contains(this))
				startNode.OutGenes.Add(this);

			if(!endNode.InGenes.Contains(this))
				endNode.InGenes.Add(this);

			StartNode = startNode;
			EndNode = endNode;
		}

		/// <summary>
		/// Get a unique innovation number based on the innovation numbers of the start and end node.
		/// This is used for historical marking.
		/// </summary>
		private static int GetInnovationNumber(Gene target)
		{
			Gene matchingGene = kindsOfGenes.FirstOrDefault(x => (x.StartNode.InnovationNb == target.StartNode.InnovationNb) && (x.EndNode.InnovationNb == target.EndNode.InnovationNb));

			if(matchingGene != null)
				return matchingGene.InnovationNb;

			kindsOfGenes.Add(target);
			return kindsOfGenes.Count - 1;
		}
	}

	class GeneComparer : EqualityComparer<Gene>
	{
		public override bool Equals(Gene g1, Gene g2) => g1.InnovationNb == g2.InnovationNb;
		public override int GetHashCode(Gene gene) => gene.InnovationNb.GetHashCode();
	}
}