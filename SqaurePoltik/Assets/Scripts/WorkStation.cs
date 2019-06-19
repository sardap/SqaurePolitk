using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkStation : MonoBehaviour
{
	public MarketInfo marketInfo;

	public Vector3 WorkingPostion()
	{
		return transform.position;
	}
}
