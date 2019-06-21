using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MarketInfo : MonoBehaviour
{
	List<Transform> _marketBoxes;
	Stack<float> _foodStack;

	public WorkStation[] workStations;
	public SpawnAreaAgg buyingArea;
	public TextMeshPro foodCount;

	void Awake()
	{
		_marketBoxes = new List<Transform>();
		_foodStack = new Stack<float>();

		UpdateFoodCount();
	}

	public bool HasFood()
	{
		return _foodStack.Count > 0;
	}

	public void MakeFood(IFoodCreateInfo foodCreateInfo)
	{
		_foodStack.Push(foodCreateInfo.FillingValue);
		UpdateFoodCount();
	}

	public bool ConsumeFood(FoodNeed foodNeed)
	{
		if(_foodStack.Count > 0)
		{
			foodNeed.Value += _foodStack.Pop();
			UpdateFoodCount();

			return true;
		}

		return false;
	}

	public Vector3 GetPlaceToStand()
	{
		return buyingArea.GetRandomPostion();
	}

	void UpdateFoodCount()
	{
		var foodConsumeAmount = _foodStack.ToList().Sum(i => i);

		foodCount.text = foodConsumeAmount.ToString();
	}
}
