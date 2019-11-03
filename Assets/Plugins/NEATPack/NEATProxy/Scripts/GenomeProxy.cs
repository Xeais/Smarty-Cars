namespace NEAT
{
	public delegate float GenomeInputFunction();

	/// <summary>
	/// A "connection"-class between the genome logic and the actual problem.
	/// This class calculates the fitness and makes use of the neural network.
	/// </summary>
	public abstract class GenomeProxy : UnityEngine.MonoBehaviour
  {
		public int Id {get; protected set;}
		//An array of input functions for the neural network, whose length has to match "NEATConfig.inputCount"!
		public GenomeInputFunction[] InputFunctions {get; set;}
    public bool IsDone {get; set;} //A general state: Indicates if the this genome is done, dead or anything similar.

    protected Population Popul {get; set;}
    public Genome GenomeProperty //The Genome from the population it represents.
    {
			get => Popul.Genomes[Id];
			set => Popul.Genomes[Id] = value; 
		}

		public virtual void Init(int id, Population popul)
		{
			Id = id;
			Popul = popul;
		}

		/// <summary>
		/// Activate the neural network, after which the output is processed.
		/// </summary>
		public void ActivateNeuralNet()
		{
			var netInputs = new float[InputFunctions.Length];
			for(int i = 0; i < InputFunctions.Length; i++)
				netInputs[i] = InputFunctions[i]();

			float[] netOutputs = GenomeProperty.FeedNeuralNetwork(Popul.Config, netInputs);
			ProcessNetworkOutput(netOutputs);
		}

    /// <summary>
    /// This function must be overriden in order to use the network's outputs.
    /// </summary>
    public virtual void ProcessNetworkOutput(float[] netOutputs) => UnityEngine.Debug.Assert(netOutputs.Length == Popul.Config.OutputCount);
  }
}