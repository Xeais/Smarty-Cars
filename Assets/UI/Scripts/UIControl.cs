using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NEAT;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
  [Header("Texts")]
  [SerializeField] private Text generationsText = null;
	[SerializeField] private Text aliveText = null;
	[SerializeField] private Text speciesText = null;
  [Header("Input Field & Toggle")]
  [SerializeField] private InputField loadInputField = null;
	[SerializeField] private Toggle fullMapView = null;

	private PopulationCar populationCar;
	private List<GenomeProxy> lastDrawnGenomes = new List<GenomeProxy>();
  private NEATGraph neatGraph;
  private SmoothFollow smoothFollow;

	private void Awake()
	{
		populationCar = FindObjectOfType<PopulationCar>();
		if(populationCar == null)
			Destroy(gameObject);
	}

	private void Start()
	{
    neatGraph = FindObjectOfType<NEATGraph>();
    smoothFollow = FindObjectOfType<SmoothFollow>();

    loadInputField.onEndEdit.AddListener(val =>
    {
      if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        LoadGenome(loadInputField);
    });
  }

  public void UpdateGenerationText() => generationsText.text = (populationCar.Popul.Generation + 1).ToString() + ". Generation";

  public void UpdateSpeciesText() => speciesText.text = "Species: " + populationCar.Popul.SpeciesCtrl.SpeciesList.Count(x => x.Genomes.Count != 0).ToString();

  public void UpdateAliveText() => aliveText.text = "Alive: " + populationCar.GenomeProxies.Count(x => !x.IsDone);

  public void SaveBest()
  {
    Genome best = populationCar.Popul.Genomes.OrderByDescending(x => x.Fitness).ToArray()[0];
    var fileName = GenomeSaver.GenerateSaveFilePath(GenomeSaver.DefaultSaveDir, best.Fitness, populationCar.Popul.Generation);
    GenomeSaver.SaveGenome(best, fileName);
    Debug.Log($"<color=green>Saved best genome as \"{fileName}\" to \"{GenomeSaver.DefaultSaveDir}\".</color>");
	}

	public void LoadGenome(InputField filePathInputField)
	{
    try
    {
      string inputFieldText = filePathInputField.text;
      if(inputFieldText == "")
      {
        Debug.LogWarning("No file path for the genome was provided!");
        return;
      }

      if(inputFieldText[0] == '"')
        inputFieldText = inputFieldText.Remove(0, 1);
      if(inputFieldText.Last() == '"')
        inputFieldText = inputFieldText.Remove(inputFieldText.Length - 1, 1);

      GenomeProxy firstAlive = populationCar.GenomeProxies.FirstOrDefault(x => !x.IsDone);
      if(firstAlive == null)
      {
        Debug.Log("Couldn't find any living genome!");
        return;
      }

      var newGenome = GenomeSaver.LoadGenome(populationCar.Popul.Config, inputFieldText);
      newGenome.Fitness = firstAlive.GenomeProperty.Fitness;
      populationCar.Popul.Genomes[firstAlive.Id] = newGenome;

      //Retrieve generation-number:
      inputFieldText = inputFieldText.Substring(inputFieldText.LastIndexOf('\\') + 1); //Example: "C:\Users\Username\Desktop\85.-Generation-Fitness-167.0961.genome" - remove everything before the last backslash.
      int index = inputFieldText.IndexOf(".-"); //".-" follows always the generation-number.
      bool isInt = false;
      if(index != -1)
      { 
        isInt = int.TryParse(inputFieldText.Substring(0, index), out int generation); //Example now: "85.-Generation-Fitness-167.0961.genome" - only get the leading number.
        if(isInt)
        {
          populationCar.Popul.Generation = generation;
          generationsText.text = generation.ToString() + ". Generation";
        }
      }
      if(index == -1 || !isInt)
        Debug.Log("<color=red>Could not retrieve the generation-number from the genome file path!</color>");

      Debug.Log("<color=green>New genome has been loaded.</color>");
		}
		catch(System.Exception e) {Debug.LogError("<b>Failed to load genome:</b>\n" + e);}
	}

	public void DrawNetwork()
	{
		if(neatGraph == null)
			return;

		if(smoothFollow != null)
		{
			smoothFollow.Follow = false;
			smoothFollow.GotoAllMapView();
		}

		fullMapView.isOn = true;

		var genomeToDraw = populationCar.GenomeProxies.OrderByDescending(x => x.GenomeProperty.Fitness).FirstOrDefault(x => !lastDrawnGenomes.Contains(x));
		if(genomeToDraw == null)
			genomeToDraw = populationCar.GenomeProxies.OrderByDescending(x => x.GenomeProperty.Fitness).First();
		lastDrawnGenomes.Add(genomeToDraw);

		neatGraph.RemoveAllNodes();
		neatGraph.GenomeProxyToDraw = genomeToDraw;
		neatGraph.DrawGivenGenomeProxy();
	}

	public void ResetNetworkDisplay()
	{
		if(neatGraph == null)
			return;

		lastDrawnGenomes.Clear();
		neatGraph.RemoveAllNodes();
	}

	public void OnToggleFullMapView()
	{
		if(smoothFollow == null)
			return;

		if(fullMapView.isOn)
		{
			smoothFollow.Follow = false;
			smoothFollow.GotoAllMapView();
		}
		else
		{
			smoothFollow.Follow = true;
			ResetNetworkDisplay();
      smoothFollow.GotoCarView();
    }
	}

  public void KillAll() => populationCar.KillAll();
}