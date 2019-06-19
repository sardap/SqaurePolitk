using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLeaningUpdate : MonoBehaviour
{
	public const float UPDATE_INTERVAL = 5;

	public Slider slider;
	public TextMeshProUGUI leftText;
	public TextMeshProUGUI rightText;

	float _nextUpdate;

	void UpdateBar()
	{
		var peopleInfo = PeopleInfo.Instance;

		Debug.Assert(peopleInfo.TotalLeaning / peopleInfo.MaxTotalLeaning < 1);

		var leaning = (peopleInfo.TotalLeaning / peopleInfo.MaxTotalLeaning);
		slider.value = -leaning;

		float leftPercent;
		float rightPercent;

		if(leaning > 0)
		{
			rightPercent = (leaning + 1f) / 2f;
			leftPercent = (2 - (2 * rightPercent)) / 2;
		}
		else
		{
			leaning = System.Math.Abs(leaning);

			leftPercent = (leaning + 1f) / 2f;
			rightPercent = (2 - (2 * leftPercent)) / 2;
		}

		leftText.text = string.Format(leftText.text, (int)System.Math.Round(leftPercent * 100));
		rightText.text = string.Format(rightText.text, (int)System.Math.Round(rightPercent * 100));

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
