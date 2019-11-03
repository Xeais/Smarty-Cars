using System.Collections.Generic;
using System.Linq;

namespace NEAT
{
	public enum NeuronType : byte
	{
		Bias,
    Input,
    Output,
    Hidden
	}

	public class Neuron
	{
    public NeuronType Type {get; private set;} = NeuronType.Hidden;
    public int InnovationNb {get; private set;} = -1;
		//Genes entering this node.
		public List<Gene> InGenes {get; private set;} = null;
		//Genes leaving this node.
		public List<Gene> OutGenes {get; private set;} = null;
		//The neurons that this neuron leads.
		public IEnumerable<Neuron> OutNeurons => OutGenes.Select(x => x.EndNode);

    private float innerValue = UnityEngine.Mathf.Infinity; //The node value                                                 
    public float InnerValue //The value of the neuron: Always one if it's bias.
    {
      get => (Type == NeuronType.Bias) ? 1f : innerValue; 
      set => innerValue = value;
    }

    public Neuron(int inovationNb, NeuronType type = NeuronType.Hidden) => NeuronInit(inovationNb, type);

    public Neuron(Neuron other)
		{
			NeuronInit(other.InnovationNb, other.Type);
			InnerValue = other.InnerValue;
		}

		public Neuron(PackedNeuron packedNeuron)
		{
			NeuronInit(packedNeuron.InnovationNb, packedNeuron.Type);
			InnerValue = 0f;
		}

		private void NeuronInit(int inovationNb, NeuronType type)
		{
			InnerValue = 0f;
			InnovationNb = inovationNb;
			Type = type;

      InGenes = new List<Gene>();
      OutGenes = new List<Gene>();
    }

    public override string ToString() => $"Type: {Type.ToString()}, Innovation-Number: {InnovationNb}, Node Value: {InnerValue}";
  }
}