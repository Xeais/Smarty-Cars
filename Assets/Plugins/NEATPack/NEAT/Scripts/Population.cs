using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NEAT
{
  [System.Serializable]
  public class Population
  {
    public int GenomeCount {get;} = -1;
    public int InputCount {get;} = -1;
    public int OutputCount {get;} = -1;

    public NEATConfig Config {get;} = null;
    public int Generation {get; set;} = 0;

    public List<Genome> Genomes {get; private set;} = new List<Genome>();
    public SpeciesControl SpeciesCtrl {get;} = null;

    public Population(int genomeCount, int inCount, int outCount, NEATConfig config)
    {
      SpeciesCtrl = new SpeciesControl();
      GenomeCount = genomeCount;
      InputCount = inCount;
      OutputCount = outCount;
      Config = config;

      Genomes = new List<Genome>(GenomeCount);
      Populate();
    }

    public void Evolve()
    {
      MakeGenomesHaveOnlyPositiveFitnesses();
      SpeciesCtrl.Speciate(Config, Genomes);
      DoSelection();

      if(Genomes.Count <= 0)
      {
        Debug.Log("Repopulating ...");
        Populate();
        return;
      }

      Genomes = CreateNextGeneration();
      Generation++;
    }

    /// <summary>
    /// Create a population with random genomes.
    /// </summary>
    private void Populate()
    {
      for(int i = 0; i < GenomeCount; i++)
        Genomes.Add(new Genome(InputCount, OutputCount, Config.WeightInitRandomValue));
    }

    /// <summary>
    /// Remove genomes with no enabled connections.
    /// Remove species if the maximum stagnation is reached, unless it's the last.
    /// </summary>
    private void DoSelection()
    {
      RemoveBrokenGenomes();
      RemoveRetrospectiveSpecies();
    }

    /// <summary>
    /// Remove genomes with no enabled connections.
    /// </summary>
    private void RemoveBrokenGenomes()
    {
      for(int i = 0; i < Genomes.Count; i++)
      {
        if(Genomes[i].NbOfActiveGenes == 0)
          RemoveGenome(Genomes[i--]);
      }
    }

    /// <summary>
    /// Remove species if the maximum stagnation is reached.
    /// If all species are to be eliminated, keep the best.
    /// A species is said to be retrospective, if its maximum average fitness 
    /// didn't improve in the last "maxStagnation" generations.
    /// </summary>
    private void RemoveRetrospectiveSpecies()
    {
      Species speciesToKeepForLastResort = null;

      SpeciesCtrl.SpeciesList.ForEach(x => x.UpdateMaxFitness());
      var speciesToEliminate = SpeciesCtrl.SpeciesList.Where(x => (x.DidntImproveInLastGenerations(Config) && x.AverageFitness < Config.GoodFitness) || x.Genomes.Count <= 0);

      if(speciesToEliminate.Count() == SpeciesCtrl.SpeciesList.Count)
        speciesToKeepForLastResort = speciesToEliminate.OrderByDescending(x => x.AverageFitness).First();

      SpeciesCtrl.SpeciesList = SpeciesCtrl.SpeciesList.Except(speciesToEliminate.Where(x => x != speciesToKeepForLastResort)).ToList();
    }

    /// <summary>
    /// Take genomes from all species, depending on their average fitness, for the next generation. 
    /// If a species is found to recive one or less population units, it won't offer anything for the next generation, 
    /// making this species to disappear.
    /// </summary>
    private List<Genome> CreateNextGeneration()
    {
      float fitnessSum = SpeciesCtrl.TotalAverageFitness;
      var newGeneration = new List<Genome>();
      int availableNumberOfGenomes = Config.GenomeCount;
      var bestSpecies = SpeciesCtrl.SpeciesList.OrderByDescending(x => x.AverageFitness).First();

      if(fitnessSum > 0.1f)
      {
        foreach(var species in SpeciesCtrl.SpeciesList.Where(x => x != bestSpecies))
        {
          //Get children or clones from each species.
          int n = (int)(species.AverageFitness / fitnessSum * Config.GenomeCount - 1);
          if(n > Config.GenomeCount || n < 0)
            break;

          //Skip species with zero or less genomes.
          if(n <= 1)
          {
            species.Genomes.Clear();
            continue;
          }

          availableNumberOfGenomes -= n;
          newGeneration.AddRange(species.GetNGenomesForNextGeneration(Config, n));
        }
      }

      if(availableNumberOfGenomes > Config.GenomeCount)
        availableNumberOfGenomes = Config.GenomeCount - newGeneration.Count;

      if(availableNumberOfGenomes > 0)
        newGeneration.AddRange(bestSpecies.GetNGenomesForNextGeneration(Config, availableNumberOfGenomes));

      if(Config.MinNbOfGenomesToKeep <= 0)
        throw new System.Exception("A species has to remember at least one genome.");

      foreach(var species in SpeciesCtrl.SpeciesList)
      {
        int n = Mathf.Max(Config.MinNbOfGenomesToKeep, Mathf.RoundToInt(species.Genomes.Count * Config.GenomesToRememberForNextGeneration));
        species.RememberTheBestNGenomes(n);
      }

      if(newGeneration.Count != Config.GenomeCount)
        throw new System.Exception(string.Format("Invalid number of genomes in population: {0} given, but {1} expected", newGeneration.Count, Config.GenomeCount));

      return newGeneration;
    }

    /// <summary>
    /// Eliminate the species with its genomes.
    /// </summary>
    private void EliminateSpecies(Species speciesTarget)
    {
      Genomes.RemoveAll(x => speciesTarget.Genomes.Contains(x));
      SpeciesCtrl.SpeciesList.Remove(speciesTarget);
    }

    /// <summary>
    /// Remove a genome from this and "SpeciesCtrl"-storages.
    /// </summary>
    private void RemoveGenome(Genome targetGenome)
    {
      Genomes.Remove(targetGenome);
      SpeciesCtrl.SpeciesList.ForEach(x => x.Genomes.Remove(targetGenome));
    }

    /// <summary>
    /// If the minimum fitness is less than zero, the absolute value of it is added to every genome, 
    /// so that they all have positive fitness.
    /// This may be useful in some computations and it doesn't affect anything.
    /// </summary>
    private void MakeGenomesHaveOnlyPositiveFitnesses()
    {
      float minFitness = Genomes.Min(x => x.Fitness);
      if(minFitness > 0f)
        return;

      float absMinFitness = Mathf.Abs(minFitness);
      Genomes.ForEach(x => x.Fitness += absMinFitness);
    }
  }
}