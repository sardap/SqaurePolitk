using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Needs : MonoBehaviour
{
	const int NEEDS_MAX = 1;
	const int NEEDS_MIN = 1;
	const float UPDATE_INTERVAL = 1f;

	List<INeed> _needLst;
	float _nextUpdate;

	public FoodNeed FoodNeed { get; set; }
	public SleepingNeed SleepingNeed { get; set; }
	public SocialNeed SocialNeed { get; set; }

    // Start is called before the first frame update
    void Start()
    {
		FoodNeed = new FoodNeed();
		SleepingNeed = new SleepingNeed();
		SocialNeed = new SocialNeed();
		var newLst = new HashSet<INeed>() { FoodNeed, SleepingNeed, SocialNeed };

		_needLst = newLst.ToList();

		_nextUpdate = UPDATE_INTERVAL;

	}

	public INeed MostDesprateNeed()
	{
		return _needLst.OrderBy(i => i.Value).ToArray()[0];
	}

	public bool Die
	{
		get
		{
			return _needLst.Any(i => i.Fatal && i.Value < 0);
		}
	}

    // Update is called once per frame
    void Update()
    {
		_nextUpdate -= Time.deltaTime;

		if(_nextUpdate < 0)
		{
			foreach(var need in _needLst)
			{
				need.Value -= need.Tick;
			}

			_nextUpdate += UPDATE_INTERVAL;
		}
	}
}
