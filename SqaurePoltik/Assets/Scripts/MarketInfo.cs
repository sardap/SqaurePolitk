using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarketInfo : MonoBehaviour
{
	List<Transform> _marketBoxes;

	public List<WorkStation> workStations;
	public SpawnArea standingArea;

	public Vector3 GetRandomMarketBox()
	{
		var selected = Util.RandomElement(_marketBoxes);
		var result = new Vector3()
		{
			x = Random.Range(selected.position.x, selected.position.x),
			y = 0.1f,
			z = Random.Range(selected.position.z, selected.position.z)
		};

		return result;
	}

	private void Awake()
	{
		_marketBoxes = new List<Transform>();
		workStations = new List<WorkStation>();

		foreach (Transform tr in transform)
		{
			if (tr.tag == "MarketBox")
			{
				_marketBoxes.Add(tr);
			}
			else if (tr.name == "WorkStation")
			{
				workStations.Add(tr.gameObject.GetComponent<WorkStation>());
			}
		}
	}
}
