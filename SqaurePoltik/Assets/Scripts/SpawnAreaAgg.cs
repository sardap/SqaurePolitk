using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAreaAgg : MonoBehaviour
{
	public string tagToFind;

	public bool started = false;

	SpawnArea[] _spawnAreaAry;

	public Vector3 GetRandomPostion()
	{
		return Util.RandomElement(_spawnAreaAry).GetRandomPostion();
	}

	void Start()
	{
		_spawnAreaAry = GetComponentsInChildren<SpawnArea>();
		var spawnAreaList = new List<SpawnArea>();

			foreach (var spawn in _spawnAreaAry)
			{
				if(tagToFind == "" || spawn.gameObject.tag == tagToFind)
				{
					spawnAreaList.Add(spawn);
				}
			}

		_spawnAreaAry = spawnAreaList.ToArray();

		started = true;
	}
}
