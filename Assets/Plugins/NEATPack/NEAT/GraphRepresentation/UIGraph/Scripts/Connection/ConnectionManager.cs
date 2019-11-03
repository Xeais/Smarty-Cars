using System.Collections.Generic;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
  [SerializeField] private Connection connectionPrefab = null;
  [SerializeField] private List<Connection> connections = new List<Connection>();

  private static ConnectionManager instance;
	public static ConnectionManager Instance
  {
		get
    {
			if(!instance)
      {
				instance = FindObjectOfType<ConnectionManager>();

				if(!instance)
        {
					GameObject go = new GameObject("ConnectionManager");
					instance = go.AddComponent<ConnectionManager>();

					if(!instance)
						Debug.LogError("Fatal Error: Could not create \"ConnectionManager\"!");
				}
			}

			return instance;
		}
	}

	public static Connection FindConnection(RectTransform rTransform1, RectTransform rTransform2)
  {
		if(!Instance)
      return null;

		foreach(Connection connection in Instance.connections)
    {
			if(connection == null)
        continue;

			if(connection.Match(rTransform1, rTransform2))
				return connection;
		}

		return null;
	}

	public static List<Connection> FindConnections(RectTransform transform)
  {
		List<Connection> found = new List<Connection>();
		if(!Instance)
      return found;

		foreach(Connection connection in Instance.connections)
    {
			if(connection == null)
        continue;

			if(connection.Contains(transform))
				found.Add(connection);
		}

		return found;
	}

	public static void AddConnection(Connection conn)
  {
		if(conn == null || !Instance)
      return;

		if(!Instance.connections.Contains(conn))
    {
      conn.transform.SetParent(Instance.transform);
      Instance.connections.Add(conn);
		}
	}

	public static void RemoveConnection(Connection conn)
  {
    //Don't use the property here, as an instance should not be created when the scene loads.
    if(conn != null && instance != null)
		{
			instance.connections.Remove(conn);
      if(conn.gameObject != null)
      {
        if(Application.isPlaying)
          Destroy(conn.gameObject);
#if UNITY_EDITOR
        else
          UnityEditor.EditorApplication.delayCall += () => {DestroyImmediate(conn.gameObject);};
#endif
      }
    }
	}

	public static void SortConnections()
  {
		if(!Instance)
      return;

    Instance.connections.Sort((Connection conn1, Connection conn2) => {return string.Compare(conn1.name, conn2.name);});
		for(int i = 0; i < Instance.connections.Count; i++)
      Instance.connections[i].transform.SetSiblingIndex(i);
	}

	public static void CleanConnections()
  {
		if(!Instance)
      return;

    //First, clean any null entries:
    Instance.connections.RemoveAll((Connection conn) => {return conn == null;});

    var children = new List<GameObject>();
    foreach(Transform child in Instance.transform)
      children.Add(child.gameObject);
    children.ForEach(child =>
    {
      if(Application.isPlaying)
        Destroy(child.gameObject);
      else
        DestroyImmediate(child.gameObject);
    });
	}

	public static Connection CreateConnection(RectTransform rTransform1, RectTransform rTransform2 = null)
  {
		if(!Instance)
      return null;
		
		Connection conn;
		if(Instance.connectionPrefab)
			conn = Instantiate(Instance.connectionPrefab);
    else
			conn = new GameObject("New Connection").AddComponent<Connection>();

		conn.SetTargets(rTransform1, rTransform2);
		return conn;
	}
}