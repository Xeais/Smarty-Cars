using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NEAT
{
	/// <summary>
	/// A helper-class for genomes
	/// </summary>
	public class Mutator
	{
		/// <summary>
		/// Mutates the given genome.
		/// First, try to mutate the structure. Only one structural mutation can happen at a time.
		/// Then, mutate the genome's weights, each genome has a chance to be modified.
		/// </summary>
		/// <param name="gradient">The mutation chances are multiplied by this gradient.</param>
		/// <returns>The same genome, but mutated</returns>
		public static Genome Mutate(NEATConfig config, Genome genome, float gradient = 1f)
		{
			float rand = Random.Range(0f, 1f);

			//Mutate structure: It's a weighted random choice.
			if(rand <= config.StructMutation.ConnectionAddProb * gradient)
				MutateAddGene(config, genome);
			else if((rand -= config.StructMutation.ConnectionAddProb) <= config.StructMutation.ConnectionDelProb * gradient)
				MutateDelGene(genome);
			else if((rand -= config.StructMutation.ConnectionDelProb) <= config.StructMutation.NodeAddProb * gradient)
				MutateAddNode(genome);
			else if((rand -= config.StructMutation.NodeAddProb) <= config.StructMutation.NodeDelProb * gradient)
				MutateDelNode(genome);

      //Mutate weight values.
      foreach(var gene in genome.Genes)
      {
        if(Random.Range(0f, 1f) <= config.WeightMutation.Prob * gradient)
          gene.Mutate(config);
      }

			return genome;
		}

		/// <summary>
		///	Add a new node by "spliting" a gene in two parts. The first part recives a weight of one
    ///	and the second part recives the previous weight so that the starting effect of the new node is minimized. 
    ///	The "splited" gene is disabled.
		/// </summary>
		public static Neuron MutateAddNode(Genome genome)
		{
			Gene[] availableGenes = genome.Genes.Where(x => x.IsEnabled).ToArray();
			if(availableGenes == null || availableGenes.Length == 0)
				return null;

			var targetGene = availableGenes[Random.Range(0, availableGenes.Length)];
			targetGene.IsEnabled = false;

			var newNeuron = new Neuron(genome.Neurons.Count);
			var preGene = new Gene(targetGene.StartNode, newNeuron);
			var postGene = new Gene(newNeuron, targetGene.EndNode);

			//Reduce the impact of adding this new node.
			preGene.Weight = 1f;
			postGene.Weight = targetGene.Weight;

			//Establish the connections between nodes and genes.
			preGene.ConnectToNeurons(targetGene.StartNode, newNeuron);
			postGene.ConnectToNeurons(newNeuron, targetGene.EndNode);

			genome.Genes.Add(preGene);
			genome.Genes.Add(postGene);
			genome.Neurons.Add(newNeuron);

			return newNeuron;
		}

		public static Neuron MutateDelNode(Genome genome)
		{
			var hiddenNeurons = genome.HiddenNeurons;
			if(hiddenNeurons.Count() == 0)
				return null;

			return genome.RemoveNode(hiddenNeurons.ElementAt(Random.Range(0, hiddenNeurons.Count())));
		}

		public static Gene MutateAddGene(NEATConfig config, Genome genome)
		{
			List<Neuron> possibleStartingNodes = new List<Neuron>(genome.Neurons);
			while(possibleStartingNodes.Count > 0)
			{
				Neuron startingNode = possibleStartingNodes[Random.Range(0, possibleStartingNodes.Count)];
				var alreadyConnectedNodes = startingNode.OutNeurons;

				Neuron[] possibleEndNodes = genome.Neurons.Where(x => x.Type != NeuronType.Input && x.Type != NeuronType.Bias && !alreadyConnectedNodes.Contains(x)).ToArray();

				if(possibleEndNodes.Length != 0)
				{
					Neuron endNeuron = possibleEndNodes[Random.Range(0, possibleEndNodes.Length)];
					var newConnection = new Gene(startingNode, endNeuron, (config == null) ? 0f : config.NewRandomWeight());
					startingNode.OutGenes.Add(newConnection);
					endNeuron.InGenes.Add(newConnection);
					genome.Genes.Add(newConnection);
					return newConnection;
				}
				else
					possibleStartingNodes.Remove(startingNode);
			}

			return null;
		}

		public static void MutateDelGene(Genome genome)
		{
			if(genome.Genes == null || genome.Genes.Count == 0)
				return;

			genome.RemoveGene(genome.Genes[Random.Range(0, genome.Genes.Count)]);
		}

		public static void MutateConnection(NEATConfig config, Genome genome)
		{
			foreach(var gene in genome.Genes)
				gene.Mutate(config);
		}
	}
}