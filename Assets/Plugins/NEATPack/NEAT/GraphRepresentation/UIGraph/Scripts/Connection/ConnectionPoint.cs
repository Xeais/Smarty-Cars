using UnityEngine;

[System.Serializable]
public class ConnectionPoint
{
	public enum ConnectionDirection : byte
  {
		North,
		East,
		South,
		West,
		Polar
	}

  [SerializeField] private ConnectionDirection direction = ConnectionDirection.North;
  public ConnectionDirection Direction {get => direction; set => direction = value;}
  [SerializeField] private Color color = Color.white;
  public Color Color {get => color; set => color = value;}
  [Space]
  [SerializeField] private float weight = 1f;
  public float Weight {get => weight; set => weight = value;}
  [SerializeField, Range(-1f, 1f)] private float position = 0f;
  
	public Vector3 P {get; private set;}
	public Vector3 C {get; private set;}

  public void Reset()
  {
		color = Color.white;
		direction = ConnectionDirection.North;
		position = 0f;
		weight = 1f;
	}

	public void CalculateVectors(RectTransform transform)
  {
		if(!transform)
      return;

		switch(direction)
    {
			case ConnectionDirection.North:
				P = transform.TransformPoint(transform.rect.width / 2f * position, transform.rect.height /2f, 0f);
				C = P + transform.up * weight;
			break;
			case ConnectionDirection.South:
				P = transform.TransformPoint(transform.sizeDelta.x / 2f * position, -transform.sizeDelta.y / 2f, 0f);
				C = P - transform.up * weight;
			break;
			case ConnectionDirection.East:
				P = transform.TransformPoint(transform.sizeDelta.x / 2f, transform.sizeDelta.y / 2f * position, 0f);
				C = P + transform.right * weight;
			break;
			case ConnectionDirection.West:
				P = transform.TransformPoint(-transform.sizeDelta.x / 2f, transform.sizeDelta.y / 2f * position, 0f);
				C = P - transform.right * weight;
			break;
			default:
				float angle = Mathf.PI / 2f - position * Mathf.PI;
				P = transform.TransformPoint(transform.sizeDelta.x / 2f * Mathf.Cos(angle), transform.sizeDelta.y / 2f * Mathf.Sin(angle), 0f);
				C = P + transform.TransformDirection(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * weight;
			break;
		}
	}
}