using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAreaAgg : MonoBehaviour
{
	SpawnArea[] _spawnAreaAry;

	public Vector3 GetRandomPostion()
	{
		return Util.RandomElement(_spawnAreaAry).GetRandomPostion();
	}

	void Awake()
	{
		_spawnAreaAry = GetComponentsInChildren<SpawnArea>();
	}

	void Start()
	{
	}
}
