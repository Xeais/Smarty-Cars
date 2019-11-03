using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace NEAT
{
	[CustomEditor(typeof(GenomeProxy), true)]
	public class GenomeProxyEditor : Editor
	{
    public GenomeProxy Target {get => (GenomeProxy)target;}

    private readonly EditorGenomeData editorGenomeData = new EditorGenomeData();

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if(Application.isPlaying && Target.gameObject.activeSelf)
			{
				RepresentMeOnGraph();
				if(GUILayout.Button("Kill"))
					Target.IsDone = true;

				EditorGUILayout.LabelField("Fitness: ", Target.GenomeProperty.Fitness.ToString());
				EditorGUILayout.LabelField("Hidden Nodes: ", Target.GenomeProperty.HiddenNeurons.Count().ToString());
				EditorGUILayout.LabelField("Genes: ", Target.GenomeProperty.Genes.Count.ToString());

				DisplayGenome(Target.GenomeProperty, editorGenomeData);
			}
		}

		private void RepresentMeOnGraph()
		{
			if(GUILayout.Button("Draw on Graph"))
			{
				var graphDrawer = FindObjectOfType<NEATGraph>();
				graphDrawer.GenomeProxyToDraw = Target;
				graphDrawer.DrawGivenGenomeProxy();
			}
		}

		public static void DisplayGenome(Genome genome, EditorGenomeData editoGenomeData)
		{
			editoGenomeData.IsFoldedOut = EditorGUILayout.Foldout(editoGenomeData.IsFoldedOut, "Genome");
			if(!editoGenomeData.IsFoldedOut)
				return;

			EditorGUI.indentLevel++;

			editoGenomeData.UpdateArraySize(genome.Neurons.Count);
			for(int i = 0; i < genome.Neurons.Count; i++)
				DisplayNode(genome.Neurons[i], editoGenomeData.Tab[i]);
			EditorGUI.indentLevel--;
		}

		public static void DisplayNode(Neuron neuron, EditorNeuronData editorNeuronData)
		{
			editorNeuronData.IsFoldedOut = EditorGUILayout.Foldout(editorNeuronData.IsFoldedOut, "Neuron " + neuron.InnovationNb);
			if(!editorNeuronData.IsFoldedOut)
				return;

			EditorGUI.indentLevel++;
			EditorGUILayout.EnumPopup("Type:", neuron.Type);
			EditorGUILayout.LabelField("Input Genes:");

			editorNeuronData.UpdateArraySize(neuron.InGenes.Count + neuron.OutGenes.Count);

			int counter = 0;
			foreach(var gene in neuron.InGenes)
			{
				EditorGUI.indentLevel++;
				DisplayGene(gene, editorNeuronData.Tab[counter]);
				EditorGUI.indentLevel--;
				counter++;
			}

			EditorGUILayout.LabelField("Output Genes:");

			foreach(var gene in neuron.OutGenes)
			{
				EditorGUI.indentLevel++;
				DisplayGene(gene, editorNeuronData.Tab[counter]);
				EditorGUI.indentLevel--;
				counter++;
			}

			EditorGUI.indentLevel--;
		}

		public static void DisplayGene(Gene gene, EditorGeneData editorGeneData)
		{
			editorGeneData.IsFoldedOut = EditorGUILayout.Foldout(editorGeneData.IsFoldedOut, "Gene: " + gene.InnovationNb);
			if(!editorGeneData.IsFoldedOut)
				return;

			EditorGUI.indentLevel++;

			EditorGUILayout.Toggle("Is enabled: ", gene.IsEnabled);
			EditorGUILayout.LabelField("Start Node: " + gene.StartNode.InnovationNb);
			EditorGUILayout.LabelField("End Node: " + gene.EndNode.InnovationNb);

			EditorGUI.indentLevel--;
		}
	}

	public class EditorGenomeData
	{
    public bool IsFoldedOut {get; set;} = false;
    public EditorNeuronData[] Tab {get; private set;} = new EditorNeuronData[0];

    public void UpdateArraySize(int size)
		{
			if(Tab.Length != size)
			{
				Tab = new EditorNeuronData[size];
				for(int i = 0; i < size; i++)
					Tab[i] = new EditorNeuronData();
			}
		}
	}

	public class EditorNeuronData
	{
    public bool IsFoldedOut {get; set;} = false;
    public EditorGeneData[] Tab {get; private set;} = new EditorGeneData[0];

    public void UpdateArraySize(int size)
		{
			if(Tab.Length != size)
			{
				Tab = new EditorGeneData[size];
				for(int i = 0; i < size; i++)
					Tab[i] = new EditorGeneData();
			}
		}
	}

	public class EditorGeneData
	{
    public bool IsFoldedOut {get; set;} = false;
  }
}
#endif