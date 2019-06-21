using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Needs : MonoBehaviour
{
	const int NEEDS_MAX = 1;
	const int NEEDS_MIN = 1;
	const float UPDATE_INTERVAL = 5f;

	List<INeed> _needLst;
	float _nextUpdate;

	public FoodNeed FoodNeed { get; set; }

    // Start is called before the first frame update
    void Start()
    {
		FoodNeed = new FoodNeed();
		var newLst = new HashSet<INeed>() { FoodNeed };

		var numberOfNeeds = Random.Range(NEEDS_MIN, NEEDS_MAX);

		while (newLst.Count < numberOfNeeds)
		{
			throw new System.NotImplementedException();
		}

		_needLst = newLst.ToList();

		_nextUpdate = UPDATE_INTERVAL;

	}

	public INeed MostDesprateNeed()
	{
		_needLst.OrderBy(i => i.Value);

		return _needLst[0];
	}

	public bool Die
	{
		get
		{
			return _needLst.Any(i => i.Value < 0 && i.Fatal);
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
