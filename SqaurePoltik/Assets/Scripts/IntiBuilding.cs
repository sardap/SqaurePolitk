using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class IntiBuilding : MonoBehaviour
{
	public Transform Ground;

	public BuildingTypeData buildingTypeData;

	public bool HasStarted
	{
		get
		{
			var test = GetComponentsInChildren<SpawnAreaAgg>();

			return test.All(i => i.started);
		}
	}

	public Vector3 SpawnMin
	{
		get
		{
			return new Vector3(transform.position.x, 0.1f, transform.position.z);
		}
	}

	public Vector3 SpawnArea
	{
		get
		{
			return Ground.localScale;
		}
	}
}
