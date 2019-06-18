using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLeaningUpdate : MonoBehaviour
{
	public const float UPDATE_INTERVAL = 5;

	public Slider slider;

	float _nextUpdate;

	void UpdateBar()
	{
		var peopleInfo = PeopleInfo.Instance;

		Debug.Assert(peopleInfo.TotalLeaning / peopleInfo.MaxTotalLeaning < 1);

		slider.value = -(peopleInfo.TotalLeaning / peopleInfo.MaxTotalLeaning);
	}

	void Start()
	{
		_nextUpdate = 10f;
	}

	// Update is called once per frame
	void Update()
    {
		_nextUpdate -= Time.deltaTime;

        if(_nextUpdate <= 0)
		{
			UpdateBar();
			_nextUpdate = UPDATE_INTERVAL;
		}
    }
}
