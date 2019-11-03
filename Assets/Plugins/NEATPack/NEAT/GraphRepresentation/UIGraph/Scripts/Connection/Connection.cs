using UnityEngine;

[RequireComponent(typeof(LineRenderer)), ExecuteInEditMode]
public class Connection : MonoBehaviour
{
	private const int MIN_RESOLUTION = 2;
  private const int MAX_RESOLUTION = 20;
  private const int AVG_RESOLUTION = 20;

  [SerializeField, Range(MIN_RESOLUTION, MAX_RESOLUTION)] private int resolution = AVG_RESOLUTION;
  [SerializeField] private RectTransform[] targets = new RectTransform[2];
  [SerializeField] private ConnectionPoint[] points = new ConnectionPoint[2]
  {
		new ConnectionPoint(),
		new ConnectionPoint()
	};
  public ConnectionPoint[] Points {get => points;}
	[SerializeField] private LineRenderer line;
	public LineRenderer Line
  {
		get
    {
			if(!line)
        line = GetComponent<LineRenderer>();
			return line;
		}
	}

  public bool IsValid => targets[0] && targets[1];

  public bool Match(RectTransform start, RectTransform end)
  {
		if(!start || !end)
      return false;

		return start.Equals(targets[0]) && end.Equals(targets[1]) || end.Equals(targets[0]) && start.Equals(targets[1]);
	}

	public bool Contains(RectTransform transform)
  {
		if(!transform)
      return false;

		return transform.Equals(targets[0]) || transform.Equals(targets[1]);
	}

	public int GetIndex(RectTransform transform)
  {
		if(transform)
    {
			if(transform.Equals(targets[0]))
        return 0;

			if(transform.Equals(targets[1]))
        return 1;
		}

		return -1;
	}

	public void OnValidate()
  {
		OrganizeTransforms();
		UpdateName();
		UpdateCurve();
	}

  private void Awake()
  {
		ConnectionManager.AddConnection(this);
	}

  private void OnDestroy()
  {
		ConnectionManager.RemoveConnection(this);
	}

  private void Update()
  {
		if(IsValid)
    {
			if(targets[0].hasChanged || targets[1].hasChanged)
				UpdateCurve();
		}
	}

  private void OrganizeTransforms()
  {
		string name1 = targets[0] ? targets[0].name : null;
		string name2 = targets[1] ? targets[1].name : null;
		if(string.Compare(name1, name2) > 0)
    {
			RectTransform swapT = targets[1];
			targets[1] = targets[0];
			targets[0] = swapT;

			ConnectionPoint swapP = points[1];
			points[1] = points[0];
			points[0] = swapP;
		}
	}

  private void UpdateName()
  {
		string name1 = targets[0] ? targets[0].name : "None";
		string name2 = targets[1] ? targets[1].name : "None";
		gameObject.name = string.Format("{0} <> {1}", name1, name2);
	}

  private void UpdateCurve()
  {
		if(!line)
      return;

		if(!IsValid)
    {
			line.enabled = false;
			return;
		}

		bool sActive = targets[0].gameObject.activeInHierarchy;
		bool eActive = targets[1].gameObject.activeInHierarchy;

		if(!sActive && !eActive)
			line.enabled = false;
    else
    {
			line.enabled = true;
			if(sActive && !eActive)
      {
				line.startColor = points[0].Color;
				line.endColor = Color.clear;
			}
      else if(!sActive && eActive) 
			{
				line.startColor = Color.clear;
				line.endColor = points[0].Color;
			}
      else
      {
				line.startColor = points[0].Color;
				line.endColor = points[1].Color;
			}
		}

		points[0].CalculateVectors(targets[0]);
		points[1].CalculateVectors(targets[1]);

		line.positionCount = resolution;
		for(int i = 0; i < resolution; i++)
			line.SetPosition(i, GetBezierPoint(i / (resolution - 1)));

    /* --------------------
     * Handle icons here. *
     * ------------------ */

		transform.position = GetBezierPoint(0.5f);
	}

	public Vector3 GetBezierPoint(float t, int derivative = 0)
  {
		derivative = Mathf.Clamp(derivative, 0, 2);
		float u = 1f - t;
		Vector3 p1 = points[0].P, p2 = points[1].P, c1 = points[0].C, c2 = points[1].C;

    switch(derivative)
    {
      case 0:
        return u * u * u * p1 + 3f * u * u * t * c1 + 3f * u * t * t * c2 + t * t * t * p2;
      case 1:
        return 3f * u * u * (c1 - p1) + 6f * u * t * (c2 - c1) + 3f * t * t * (p2 - c2);
      case 2:
        return 6f * u * (c2 - 2f * c1 + p1) + 6f * t * (p2 - 2f * c2 + c1);
      default:
        return Vector3.zero;
    }
	}

	public void SetTargets(RectTransform rTransform1, RectTransform rTransform2)
  {
		targets[0] = rTransform1;
		targets[1] = rTransform2;
	}
}