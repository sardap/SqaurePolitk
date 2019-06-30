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

	List<BeliefControler> _allBeliefControlers = new List<BeliefControler>();

	Dictionary<BeliefControler, Faction> _factionLst = new Dictionary<BeliefControler, Faction>();

	public float MaxTotalLeaning
	{
		get
		{
			return (_allBeliefControlers.Count * BeliefControler.BeliefsCount) * BeliefControler.MAX_RIGHT;
		}
	}

	public float TotalLeaning
	{
		get
		{
			return _allBeliefControlers.Sum(i => i.TotalLeaning);
		}
	}

	PeopleInfo()
	{
	}

	public void RegsiterPerson(BeliefControler beliefControler)
	{
		_allBeliefControlers.Add(beliefControler);
	}

	public void RegstierMax(BeliefControler beliefControler)
	{
		if (!_factionLst.ContainsKey(beliefControler))
		{
			var newFact = new Faction(beliefControler.TotalLeaning)
			{
				Leader = beliefControler.GetComponent<FactionCom>()
			};

			_factionLst.Add(beliefControler, newFact);
		}
	}

	public void UnregsterMax(BeliefControler beliefControler)
	{
		Debug.Assert(_factionLst.ContainsKey(beliefControler));
		_factionLst[beliefControler].Destroy();
		_factionLst.Remove(beliefControler);
	}

	public void UnregsterPerson(BeliefControler beliefControler)
	{
		_allBeliefControlers.Remove(beliefControler);
	}

	public GameObject FindClosetToFollow(FollowerJob newJob, BeliefControler beliefControler, FactionCom factionCom)
	{
		if(_factionLst.Values.Count == 0)
		{
			return null;
		}

		SortedList<float, Faction> canadiets = new SortedList<float, Faction>();

		Faction tMin = null;

		foreach (var go in _factionLst.Values)
		{
			float diff = Mathf.Abs(go.Leaning - beliefControler.TotalLeaning);

			canadiets.Add(diff, go);
		}

		int n = (int)Mathf.Max(canadiets.Count * 0.3f, 1f);

		while(canadiets.Count > n)
		{
			canadiets.RemoveAt(canadiets.IndexOfValue(canadiets.Last().Value));
		}

		tMin = Util.Shuffle(canadiets.ToList()).First().Value;

		tMin.Leader.beliefControler.follwers.Add(newJob);
		factionCom.Faction = tMin;
		tMin.AddMember(factionCom);
		return tMin.Leader.beliefControler.gameObject;
	}
}
