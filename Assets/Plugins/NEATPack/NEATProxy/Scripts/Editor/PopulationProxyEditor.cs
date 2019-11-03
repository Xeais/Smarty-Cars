using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

namespace NEAT
{
	[CustomEditor(typeof(PopulationProxy), true)]
	public class PopulationProxyEditor : Editor
	{
    public PopulationProxy Target {get => (PopulationProxy)target;}

    private readonly EditorSpeciesControlData editorSpeciesCtrlData = new EditorSpeciesControlData();

		public override void OnInspectorGUI()
		{
			if(UnityEngine.Application.isPlaying && Target.gameObject.activeSelf)
			{
				EditorGUILayout.LabelField("Maximum fitness: ", Target.GenomeProxies.Max(x => x.GenomeProperty.Fitness).ToString());
				EditorGUILayout.LabelField("Average fitness: ", Target.GenomeProxies.Average(x => x.GenomeProperty.Fitness).ToString());
				EditorGUILayout.LabelField(Target.Popul.Generation.ToString() + ".", "Generation");
        DrawSpeciesControl(Target.Popul.SpeciesCtrl, editorSpeciesCtrlData);
			}

			base.OnInspectorGUI();
		}

		private void DrawSpeciesControl(SpeciesControl speciesControl, EditorSpeciesControlData editorData)
		{			
			editorData.IsFoldedOut = EditorGUILayout.Foldout(editorData.IsFoldedOut, "Species Control:");
			if(!editorData.IsFoldedOut)
				return;

			EditorGUI.indentLevel++;

			if(speciesControl.SpeciesList == null)
				return;

			editorData.UpdateArraySize(speciesControl.SpeciesList.Count);

			int i = 0;
			foreach(var species in speciesControl.SpeciesList)
			{
				EditorGUI.indentLevel++;
				DrawSpecies(species, editorData.Tab[i]);
				EditorGUI.indentLevel--;
				i++;
			}

			EditorGUI.indentLevel--;
		}

		private void DrawSpecies(Species species, EditorSpeciesData editorSpeciesData)
		{
			editorSpeciesData.IsFoldedOut = EditorGUILayout.Foldout(editorSpeciesData.IsFoldedOut, "Species Count: " + species.Genomes.Count);
			if(!editorSpeciesData.IsFoldedOut)
				return;

			EditorGUI.indentLevel++;

			EditorGUILayout.LabelField("Average Fitness: ", species.AverageFitness.ToString());

			editorSpeciesData.UpdateArraySize(species.Genomes.Count);

			for(int i = 0; i < species.Genomes.Count; i++)
				GenomeProxyEditor.DisplayGenome(species.Genomes[i], editorSpeciesData.Tab[i]);

			EditorGUI.indentLevel--;
		}
	}

	public class EditorSpeciesControlData
	{
    public bool IsFoldedOut {get; set;} = false;
    public EditorSpeciesData[] Tab {get; private set;} = new EditorSpeciesData[0];

    public void UpdateArraySize(int size)
		{
			if(Tab.Length != size)
			{
				Tab = new EditorSpeciesData[size];
				for(int i = 0; i < size; i++)
					Tab[i] = new EditorSpeciesData();
			}
		}
	}

	public class EditorSpeciesData
	{
    public bool IsFoldedOut {get; set;} = false;
    public EditorGenomeData[] Tab {get; private set;} = new EditorGenomeData[0];

    public void UpdateArraySize(int size)
		{
			if(Tab.Length != size)
			{
				Tab = new EditorGenomeData[size];
				for(int i = 0; i < size; i++)
					Tab[i] = new EditorGenomeData();
			}
		}
	}
}
#endif