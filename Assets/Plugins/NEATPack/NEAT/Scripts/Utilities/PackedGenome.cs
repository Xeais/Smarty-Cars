using System.Linq;

namespace NEAT
{
	/// <summary>
	/// Saveable genome
	/// This class can be saved into a file.
	/// </summary>
	[System.Serializable]
	public class PackedGenome
	{
    [UnityEngine.SerializeField] private PackedGene[] genes;
    public PackedGene[] Genes {get => genes;}

    public PackedGenome(Genome genome) => genes = genome.Genes.Select(x => new PackedGene(x)).ToArray();
  }
}