using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkStation : MonoBehaviour
{
	int _workers;

	public MarketInfo marketInfo;

	public int Workers
	{
		get
		{
			return _workers;
		}
		set
		{
			_workers = value;
		}
	}

	void Start()
	{
		_workers = 0;
	}
}
