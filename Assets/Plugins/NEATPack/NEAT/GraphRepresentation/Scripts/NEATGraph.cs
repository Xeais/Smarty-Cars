using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NEAT;

public class NEATGraph : MonoBehaviour
{
  [Header("Weight & Connection")]
#pragma warning disable 414
  [SerializeField] private float weightSize = 1f;
#pragma warning restore 414
  [SerializeField] private float maxWeight = 20f;
  [SerializeField] private float connectionWidth = 0.5f;
  [Header("Colors")]
  [SerializeField] private Color positiveWeightColor = Color.green;
	[SerializeField] private Color negativeWeightColor = Color.red;
	[SerializeField] private Color disabledColor = Color.black;
  [Header("Nodes")]
  [SerializeField] private float distBetweenNodes = 2f;
  [SerializeField] private GameObject nodePrefab = null;
	[SerializeField] private Transform parentNodes = null;
  [Header("Positions")]
  [SerializeField] private Transform inputNodesPos = null;
	[SerializeField] private Transform outputNodesPos = null;
	[SerializeField] private Transform bottom = null;
  [Header("Live Draw Parameters")]
  [SerializeField] private GenomeProxy genomeProxyToDraw;
  public GenomeProxy GenomeProxyToDraw {get => genomeProxyToDraw; set => genomeProxyToDraw = value;}
  [SerializeField] private int newGenomeInputCount = 2;
  [SerializeField] private int newGenomeOutputCout = 1;
  [SerializeField] private int hiddenNodes = 10;
  [SerializeField] private int connectionMutations = 5;
  [SerializeField] private float newGenomeRandomWeight = 10f;

	private Dictionary<Neuron, RectTransform> nodes = new Dictionary<Neuron, RectTransform>();

  public void DrawGenome(Genome genome)
	{
		RemoveAllNodes();

		int count = 0;
		foreach(var neuron in genome.Neurons)
		{
			var newNode = Instantiate(nodePrefab, parentNodes).GetComponent<RectTransform>();
			newNode.name = "Node " + count;

			string nodeText = neuron.InnovationNb.ToString();
      switch(neuron.Type)
      {
        case NeuronType.Bias:
          nodeText = "bias" + nodeText;
          break;
        case NeuronType.Input:
          nodeText = "in" + nodeText;
          break;
        case NeuronType.Output:
          nodeText = "out" + nodeText;
          break;
        case NeuronType.Hidden:
          nodeText = "hidden" + nodeText;
          break;
      }
			newNode.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = nodeText;

			nodes.Add(neuron, newNode);
			count++;
		}

		var inputNeurons = genome.InputNeurons.ToArray();
		var outputNeurons = genome.OutputNeurons.ToArray();
		foreach(var node in nodes)
		{
			ConnectNode(node.Value, node.Key, nodes, genome);
			if(genome.InputNeurons.Contains(node.Key))
			{
				int i = System.Array.IndexOf(inputNeurons, node.Key) + 1;
				node.Value.localPosition = inputNodesPos.localPosition + Vector3.down * distBetweenNodes * i;
			}
			else if(genome.OutputNeurons.Contains(node.Key))
			{
				int i = System.Array.IndexOf(outputNeurons, node.Key);
				node.Value.localPosition = outputNodesPos.localPosition + Vector3.down * distBetweenNodes * i;
			}
			else if(genome.BiasNeuron == node.Key)
				node.Value.localPosition = inputNodesPos.localPosition;
			else
				node.Value.localPosition = GetPosOfNewNode();
		}
	}

  public void DrawRandomFromParameters() => DrawGenome(RandomGenome(newGenomeInputCount, newGenomeOutputCout, hiddenNodes, connectionMutations, newGenomeRandomWeight));

  public void DrawGivenGenomeProxy()
	{
		if(genomeProxyToDraw == null)
		{
			Debug.LogError("No genome-proxy was given!");
			return;
		}

		RemoveAllNodes();
		DrawGenome(genomeProxyToDraw.GenomeProperty);
	}

	public void RemoveAllNodes()
	{
		var children = new List<GameObject>();
		foreach(Transform child in parentNodes)
			children.Add(child.gameObject);
		children.ForEach(child =>
		{ 
			if(Application.isEditor)
        DestroyImmediate(child);
			else
				Destroy(child);
		});

		nodes.Clear();
		ConnectionManager.CleanConnections();
		nodes.Clear();
	}

	//Create a new random genome from the given parameters.
	public Genome RandomGenome(int inputs = 2, int outputs = 2, int hiddenNodes = 5, int connectionMutations = 5, float weight = 10f)
	{
		Genome result = new Genome(inputs, outputs, weight);
		for(int i = 0; i < hiddenNodes; i++)
		{
			var neuron = Mutator.MutateAddNode(result);
			if(neuron == null)
				break;

			neuron.InGenes[0].Weight = Random.Range(-weight, weight);
			neuron.OutGenes[0].Weight = Random.Range(-weight, weight);
		}

		var populProxy = FindObjectOfType<PopulationProxy>();
		var config = populProxy == null ? populProxy.NEATConfig : new NEATConfig();

		for(int i = 0; i < connectionMutations; i++)
		{
			Gene gene = Mutator.MutateAddGene(config, result);
			if(gene == null)
				break;

			gene.Weight = Random.Range(-weight, weight);
		}

		return result;
	}

	private void ConnectNode(RectTransform target, Neuron targetNeuron, Dictionary<Neuron, RectTransform> nodes, Genome genome)
	{
		foreach(var neuron in targetNeuron.OutGenes.Select(x => x.EndNode))
		{
			if(neuron == targetNeuron)
				continue;

			var conn = ConnectionManager.CreateConnection(target, nodes[neuron]);
			conn.Points[0].Weight = conn.Points[1].Weight = 0.6f;
			conn.Line.widthMultiplier = connectionWidth;
			conn.Points[0].Direction = ConnectionPoint.ConnectionDirection.East;
			conn.Points[1].Direction = ConnectionPoint.ConnectionDirection.West;
			
			Gene gene = targetNeuron.OutGenes.First(x => x.EndNode == neuron);
			Color color = GetColor(gene);
			conn.Points[0].Color = conn.Points[1].Color = color;

			if(!gene.IsEnabled)
				conn.Line.widthMultiplier *= 0.3f;
		}
	}

	private Color GetColor(Gene gene)
	{
		Color result;
		if(gene.IsEnabled)
		{
			float gradient = (gene.Weight + maxWeight) / (maxWeight * 2f);
			result = Color.Lerp(negativeWeightColor, positiveWeightColor, gradient);
			
			if(gene.Weight >= 0f)
				result.a = gene.Weight / maxWeight;
			else
				result.a = -gene.Weight / maxWeight;
		}
		else
			result = disabledColor;

		return result;
	}

	private Vector3 GetPosOfNewNode(int tries = 5)
	{
		var result = Vector3.zero;
		do
		{
			tries--;
			result.x = Random.Range(inputNodesPos.localPosition.x, outputNodesPos.localPosition.x);
			result.y = Random.Range(inputNodesPos.localPosition.y, bottom.localPosition.y);
		}
    while(PosIsTooNearToAnything(result) && tries > 0);

		return result;
	}

	private bool PosIsTooNearToAnything(Vector3 pos)
	{
		foreach(var node in nodes)
		{
			if(Vector3.Distance(pos, node.Value.localPosition) <= distBetweenNodes)
				return true;
		}

		return false;
	}
}