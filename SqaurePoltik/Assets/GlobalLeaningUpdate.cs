using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLeaningUpdate : MonoBehaviour
{
	public const float UPDATE_INTERVAL = 1;

	public TextMeshProUGUI leftText;
	public TextMeshProUGUI rightText;
	public LeaningSlider leaningSlider;

	float _nextUpdate;

	public void UpdateBar()
	{
		var peopleInfo = PeopleInfo.Instance;
		var result = leaningSlider.UpdateBar(peopleInfo.TotalLeaning, peopleInfo.MaxTotalLeaning);

		leftText.text = result.Left;
		rightText.text = result.Right;
	}

	void Start()
	{
		_nextUpdate = 10f;
		leftText.text = "??%";
		rightText.text = "??%";
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
