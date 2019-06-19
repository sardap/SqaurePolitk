using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarketInfo : MonoBehaviour
{
	List<Transform> _marketBoxes;
	Stack<float> _foodStack;

	public List<WorkStation> workStations;

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

	public void MakeFood(IFoodCreateInfo foodCreateInfo)
	{
		_foodStack.Push(foodCreateInfo.FillingValue);
	}

	public float ConsumeFood()
	{
		if(_foodStack.Count > 0)
		{
			return _foodStack.Pop();
		}

		return 0;
	}

	void Awake()
	{
		_marketBoxes = new List<Transform>();
		workStations = new List<WorkStation>();
		_foodStack = new Stack<float>();

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
