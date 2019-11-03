using UnityEngine;

namespace NEAT
{
	/// <summary>
	/// A "connection"-class between the population logic and the actual problem.
	/// </summary>
	public abstract class PopulationProxy : MonoBehaviour
	{
		[SerializeField] private NEATConfig nEATConfig = null;
    public NEATConfig NEATConfig {get => nEATConfig;}
    [Header("Proxy-Configuration")]
    [SerializeField] private GameObject genomeProxyPrefab = null;
		[SerializeField] private Transform genomeProxyStorage = null;

		public Population Popul {get; protected set;}
		public GenomeProxy[] GenomeProxies {get; protected set;}

		protected virtual void Awake()
		{
			InitPopul();
		}

		protected virtual void Start()
		{
			InstantiateAllGenomes();
		}

    public virtual void InitPopul() => Popul = new Population(genomeCount: NEATConfig.GenomeCount, inCount: NEATConfig.InputCount, outCount: NEATConfig.OutputCount, config: nEATConfig);

    public virtual void Evolve()
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
			Popul.Evolve();
		}

		//Instantiates all genomes from the current population.
		protected void InstantiateAllGenomes()
		{
			GenomeProxies = new GenomeProxy[Popul.Genomes.Count];

			for(int i = 0; i < Popul.Genomes.Count; i++)
			{
				GameObject obj;
				if(genomeProxyStorage == null)
					obj = Instantiate(genomeProxyPrefab).gameObject;
				else
					obj = Instantiate(genomeProxyPrefab, genomeProxyStorage).gameObject;

				var proxyComponent = obj.GetComponent<GenomeProxy>();
				proxyComponent.Init(i, Popul);
				GenomeProxies[i] = proxyComponent;

				InitGenomeProxyObj(obj);
			}
		}

    //Initialize the "physical" atributes of the instantiated "genomeProxy".
    protected virtual void InitGenomeProxyObj(GameObject genomeProxy) => genomeProxy.transform.position = Vector3.zero;
  }
}