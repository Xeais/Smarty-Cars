using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NEAT
{
	public class Species
	{
		public int Generation {get; set;} = 0;
		public List<Genome> Genomes {get; set;} = new List<Genome>();
		public List<Genome> NewGenomes {get; set;} = null;
		
		public float AverageFitness => Genomes.Count == 0 ? 0f : Genomes.Average(x => x.Fitness);
		public float MaxFitness {get; private set;} = Mathf.NegativeInfinity;
		public float CurrentMaxFitness => (Genomes.Count == 0) ? 0f : Genomes.Max(x => x.Fitness);
		public int GenerationsSinceFitnesImprovement {get; private set;} = 0;
		
		//A random representative is choosen from the current genomes.
		public Genome Representative
		{
			get
			{
				if(Genomes == null || Genomes.Count == 0)
				{
					if(NewGenomes == null || NewGenomes.Count == 0)
						throw new System.Exception("A species can't have zero genomes!");

					return NewGenomes[Random.Range(0, NewGenomes.Count)];
				}
				else
					return Genomes[Random.Range(0, Genomes.Count)];
			}
		}

    public Species(int generation = 0)
    {
      Genomes = new List<Genome>();
      NewGenomes = new List<Genome>();
      Generation = generation;
    }

		/// <summary>
		/// Update some internal values: "maxFitness" and generations since last fitness improvement.
		/// </summary>
		public void UpdateMaxFitness()
		{
			float currentMaxFitness = CurrentMaxFitness;
			if(currentMaxFitness > MaxFitness)
			{
				MaxFitness = currentMaxFitness;
				GenerationsSinceFitnesImprovement = 0;
			}
			else
				GenerationsSinceFitnesImprovement++;
		}

    public bool DidntImproveInLastGenerations(NEATConfig config) => GenerationsSinceFitnesImprovement >= config.MaxStagnation;

    /// <summary>
    /// Get N genomes from this species.
    /// The best genome is simply copied.
    /// A part of the genomes are soft copied. Soft, means that the mutation is less than normal.
    /// The rest, are a result of a crossover between weighted random chosen genomes. The random is weighted, 
    /// based on the fitness. The parents can't be the same.
    /// </summary>
    public Genome[] GetNGenomesForNextGeneration(NEATConfig config, int n)
    {
      var result = new Genome[n];
      Genomes = Genomes.OrderByDescending(x => x.Fitness).ToList();

      for(int i = 0; i < n; i++)
      {
        if(i == 0)
          result[i] = new Genome(Genomes[0]);
        else if(i <= config.PartOfGenomesToCopyForNextGenerations * n || Genomes.Count == 1)
          result[i] = new Genome(FitnessWeightedRandomChoice(Genomes)).Mutate(config, 0.8f);
        else
        {
          var parent1 = FitnessWeightedRandomChoice(Genomes, config.WeightedRandomGradient);
          var parent2 = FitnessWeightedRandomChoice(Genomes.Where(x => x != parent1), config.WeightedRandomGradient);
          result[i] = Genome.Crossover(config, parent1, parent2).Mutate(config);
        }
      }

      return result;
    }

    /// <summary>
    /// For the next speciations, keep only the best N genomes to represent the species.
    /// </summary>
    public void RememberTheBestNGenomes(int n) => Genomes = Genomes.OrderByDescending(x => x.Fitness).Take(n).ToList();

    /// <summary>
    /// Random choice prioritizing the fittest.
    /// If gradient is zero, then a random genome is chosen.
    /// If gradient is one, the genomes with the highest fitness are more likely to be chosen.
    /// For this to work, the fitness shouldn't be less than zero.
    /// </summary>
    private static Genome FitnessWeightedRandomChoice(IEnumerable<Genome> genomes, float gradient = 1f)
		{
			float totalFitness = genomes.Sum(x => x.Fitness);
			float averageFitness = totalFitness / genomes.Count();
			float targetFitness = Random.Range(0f, totalFitness);

			foreach(var genome in genomes)
			{
				float genomeFitness = Mathf.Lerp(averageFitness, genome.Fitness, gradient);
				if(targetFitness <= genomeFitness)
					return genome;
				else
					targetFitness -= genomeFitness;
			}

      //In case of float operation imprecisions
      if(targetFitness <= 0.001f)
				return genomes.Last();

			throw new System.Exception("The function \"FitnessWeightedRandomChoice()\" in \"Species.cs\" failed.");
		}
  }
}