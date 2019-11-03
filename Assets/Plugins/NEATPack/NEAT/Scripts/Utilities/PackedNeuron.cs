using UnityEngine;

namespace NEAT
{
	[System.Serializable]
	public class PackedNeuron
	{
    [SerializeField] private NeuronType type;
    public NeuronType Type {get => type;}
    [SerializeField] private int innovationNb;
    public int InnovationNb {get => innovationNb;}

    public PackedNeuron(Neuron neuron)
		{
			innovationNb = neuron.InnovationNb;
			type = neuron.Type;
		}
  }
}