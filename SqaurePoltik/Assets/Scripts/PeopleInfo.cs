using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class PeopleInfo
{
	class IncorrectPassionLevelRegistered : System.Exception
	{
	}

	static PeopleInfo _instance;

	public static PeopleInfo Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PeopleInfo();
			}

			return _instance;
		}
	}

	List<BeliefControler> _posLeaning = new List<BeliefControler>();
	List<BeliefControler> _negLeaning = new List<BeliefControler>();
	float _totalLeaning = 0;

	public float MaxTotalLeaning
	{
		get; set;
	}

	public float TotalLeaning
	{
		get
		{
			return _totalLeaning;
		}
	}

	PeopleInfo()
	{
	}

	public void RegisterChange(float newValue)
	{
		_totalLeaning += newValue;
	}

	public void RegstierMax(BeliefControler beliefControler)
	{
		if (beliefControler.TotalLeaning > 0)
		{
			_posLeaning.Add(beliefControler);
		}
		else
		{
			_negLeaning.Add(beliefControler);
		}
	}

	public void UnregsterMax(BeliefControler beliefControler)
	{
		if (_posLeaning.Contains(beliefControler))
		{
			_posLeaning.Remove(beliefControler);
		}

		if (_negLeaning.Contains(beliefControler))
		{
			_negLeaning.Remove(beliefControler);
		}
	}

	public bool MaxForLeaning(BeliefControler beliefControler)
	{
		return (beliefControler.TotalLeaning > 0 ? _posLeaning : _negLeaning).Count > 0;
	}

	public GameObject FindClosetToFollow(FollowerJob newJob, BeliefControler beliefControler)
	{
		List<BeliefControler> list = beliefControler.TotalLeaning > 0 ? _posLeaning : _negLeaning;

		float minDist = Mathf.Infinity;
		BeliefControler tMin = null;
		var currentPos = beliefControler.transform.position;

		foreach (var go in list)
		{
			float dist = Vector3.Distance(go.transform.position, currentPos);

			if(dist < minDist)
			{
				minDist = dist;
				tMin = go;
			}
		}

		tMin.follwers.Add(newJob);
		return tMin.gameObject;
	}
}
