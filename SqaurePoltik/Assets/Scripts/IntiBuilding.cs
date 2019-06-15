using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IntiBuilding : MonoBehaviour
{
	public Transform Ground;

	public BuildingTypeData buildingTypeData;

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

	void Awake()
	{
	}
}
