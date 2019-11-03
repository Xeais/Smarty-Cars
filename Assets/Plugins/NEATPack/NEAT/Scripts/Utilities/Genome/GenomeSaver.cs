using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace NEAT
{
  /// <summary>
  /// A helper-class which can save a given genome in a file.
  /// </summary>
  public static class GenomeSaver
  {
    public static string DefaultSaveDir => System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\";

    public static string GenerateSaveFilePath(string dir, float fitness, int generation = 0) => dir + generation + $".-Generation-Fitness-{fitness}.genome";

    public static void SaveGenome(Genome genomeToSave, string filePath)
    {
      var bf = new BinaryFormatter();
      FileStream file = File.Create(filePath);

      bf.Serialize(file, new PackedGenome(genomeToSave));
      file.Close();
    }

    public static Genome LoadGenome(NEATConfig config, string filePath)
    {
      if(!File.Exists(filePath))
        throw new System.Exception($"\"{filePath}\" doesn't exist.");

      var bf = new BinaryFormatter();
      FileStream file = File.Open(filePath, FileMode.Open);

      var packedGenome = bf.Deserialize(file) as PackedGenome;
      file.Close();

      return new Genome(config, packedGenome);
    }
  }
}