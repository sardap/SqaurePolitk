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
	HashSet<BeliefControler> _lookingForSocialInteraction = new HashSet<BeliefControler>();
	ICardInfo _activeCard;

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

	public ICardInfo SelectedCard
	{
		get
		{
			return _activeCard;
		}
	}

	PeopleInfo()
	{
	}

	public void SetActiveCard(ICardInfo cardInfo)
	{
		_activeCard = cardInfo;
	}

	public void RegsiterPerson(BeliefControler beliefControler)
	{
		_allBeliefControlers.Add(beliefControler);
	}

	public void RegstierMax(BeliefControler beliefControler, GameObject crown)
	{
		if (!_factionLst.ContainsKey(beliefControler))
		{
			var factionCom = beliefControler.GetComponent<FactionCom>();

			var newFact = new Faction(beliefControler.TotalLeaning)
			{
				Leader = factionCom,
				Name = beliefControler.GetComponent<CardInfoCom>().PersonName + " Party"
			};

			crown.SetActive(true);

			crown.GetComponentsInChildren<Renderer>().ToList().ForEach(i => i.material.color = newFact.FactionColor);

			newFact.Leader.factionBubble.SetActive(true);

			newFact.Leader.Faction = newFact;

			Debug.LogFormat("Frame:{0} {1} LEADER OF FACTION ", Time.frameCount, beliefControler.gameObject.name);

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

	public BeliefControler FindClosestSocialInteraction(BeliefControler beliefControler, float maxDist = 20f)
	{
		var target = Helper.FindClosest(_lookingForSocialInteraction, beliefControler.transform);

		if(target == null || Vector3.Distance(target.transform.position, beliefControler.transform.position) > maxDist)
		{
			_lookingForSocialInteraction.Add(beliefControler);
			target = null;
		}
		else
		{
			_lookingForSocialInteraction.Remove(target);
		}

		return target;
	}

	public void RemoveSocialSeeker(BeliefControler beliefControler)
	{
		_lookingForSocialInteraction.Remove(beliefControler);
	}

	public GameObject FindClosetToFollow(FollowerJob newJob, BeliefControler beliefControler, FactionCom factionCom)
	{
		var applcableFactions = _factionLst.Values.Where(i => i.Leader.beliefControler.LeftLeaning == beliefControler.LeftLeaning && i.Leader.normalPersonAI.HasJob);

		if(applcableFactions.Count() == 0)
		{
			return null;
		}

		SortedList<float, Faction> canadiets = new SortedList<float, Faction>();

		Faction tMin = null;

		foreach (var go in applcableFactions)
		{
			float diff = Mathf.Abs(go.Leaning - beliefControler.TotalLeaning);

			canadiets.Add(diff, go);
		}

		int n = (int)System.Math.Max(canadiets.Count * 0.3f, 1);

		while(canadiets.Count > n)
		{
			canadiets.RemoveAt(canadiets.IndexOfValue(canadiets.Last().Value));
		}

		tMin = Helper.FindClosest(canadiets.Values.Select(i => i.Leader), factionCom.normalPersonAI.Home.door).Faction;

		factionCom.Faction = tMin;
		tMin.AddMember(factionCom);
		return tMin.Leader.beliefControler.gameObject;
	}
}
