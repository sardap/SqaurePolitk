using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeedSlider : MonoBehaviour
{
	public Slider slider;

	public void UpdateBar(INeed need)
	{
		slider.value = need.Value;
		slider.maxValue = need.MaxValue;
	}
}
