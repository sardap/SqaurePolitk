using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HouseBuilding : MonoBehaviour
{
	int _sleepingCount;

	public SpawnArea sleepTalkingArea;
	public Transform door;
	public TextMeshPro sleepingCountText;

	public int SleepingCount
	{
		get
		{
			return _sleepingCount;
		}
		set
		{
			_sleepingCount = value;
			sleepingCountText.text = _sleepingCount.ToString();
		}
	}

	public Vector3 SleepTalkingPostion()
	{
		return sleepTalkingArea.GetRandomPostion();
	}
}
