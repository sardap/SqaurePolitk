using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaningSlider : MonoBehaviour
{
	public class TextValues
	{
		public string Left { get; set; }
		public string Right { get; set; }
	}

	public Slider slider;

	public TextValues UpdateBar(float totalLeaning, float maxLeaning)
	{
		var leaning = (totalLeaning / maxLeaning);
		slider.value = -leaning;

		float leftPercent;
		float rightPercent;

		if (leaning > 0)
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

		return new TextValues()
		{
			Left = (int)System.Math.Round(leftPercent * 100) + "%",
			Right = (int)System.Math.Round(rightPercent * 100) + "%"
		};
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
