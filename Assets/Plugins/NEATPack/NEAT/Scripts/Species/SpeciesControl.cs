using System.Collections.Generic;
using System.Linq;

namespace NEAT
{
  public class SpeciesControl
  {
    public List<Species> SpeciesList {get; set;} = new List<Species>();
    public Species RandomSpecies => SpeciesList[UnityEngine.Random.Range(0, SpeciesList.Count)];
    public float TotalAverageFitness => SpeciesList.Count == 0 ? 0f : SpeciesList.Sum(x => x.AverageFitness);

    public SpeciesControl() => SpeciesList = new List<Species>();

    /// <summary>
    /// Groups all the genomes in the species, so that they can develop their own structures, 
    /// without being "distracted" by other local maxima.
    /// </summary>
    public void Speciate(NEATConfig config, List<Genome> genomes)
    {
      foreach(var genome in genomes)
      {
        var compatibleSpecies = GetGenomeSpecies(config, SpeciesList, genome);

        if(compatibleSpecies == null)
        {
          var newSpecies = new Species();
          newSpecies.NewGenomes.Add(genome);
          SpeciesList.Add(newSpecies);
        }
        else
          compatibleSpecies.NewGenomes.Add(genome);
      }

      foreach(var species in SpeciesList)
      {
        species.Genomes = species.NewGenomes;
        species.NewGenomes = new List<Genome>();
        species.Generation++;
      }

      EliminateEmptySpecies();
      AssignSpeciesToGenomes(SpeciesList);
    }

    /// <summary>
    /// Determines the species in which the given genome is located.
    /// Two genomes are in the same species if their genetic distance is less than the given compatibility treshold.
    /// </summary>
    private Species GetGenomeSpecies(NEATConfig config, List<Species> Species, Genome genome)
    {
      if(Species == null)
        return null;

      foreach(var species in Species)
      {
        if(species.Genomes.Count == 0)
          continue;

        float threshold = config.SpeciesCompatibility.Threshold;
        float geneticDist = Genome.GeneticDistance(config, genome, species.Representative);

        if(geneticDist <= threshold)
          return species;
      }

      return null;
    }

    /// <summary>
    /// Tell every genome what species they belong to.
    /// </summary>
    private void AssignSpeciesToGenomes(List<Species> Species)
    {
      foreach(var species in Species)
      {
        foreach(var genome in species.Genomes)
          genome.SpeciesId = species;
      }
    }

    private void EliminateEmptySpecies() => SpeciesList.RemoveAll(x => x.Genomes.Count == 0);
  }
}