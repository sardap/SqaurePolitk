using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfoCom : MonoBehaviour
{
	static Color32 DefualtColor = new Color32() { a = byte.MaxValue, r = byte.MaxValue, g = byte.MaxValue, b = byte.MaxValue };

	string _name;
	FactionCom _factionCom;
	Worker _worker;
	BeliefControler _beliefControler;
	Needs _needs;

	public GameObject outline;

	public string PersonName
	{
		get
		{
			return _name;
		}
	}

	void Awake()
	{
		_name = LatainSenteceCreator.Instance.GetName();
	}

	void Start()
	{
		_worker = GetComponent<Worker>();
		_factionCom = GetComponent<FactionCom>();
		_beliefControler = GetComponent<BeliefControler>();
		_needs = GetComponent<Needs>();
	}

	void Update()
    {
		_factionCom = GetComponent<FactionCom>();

		if(outline.activeSelf && ((PeopleInfo.Instance.SelectedCard != null && PeopleInfo.Instance.SelectedCard.Name != _name) || PeopleInfo.Instance.SelectedCard == null) )
		{
			outline.SetActive(false);
		}
	}

	void OnMouseDown()
	{
		outline.SetActive(true);

		var card = new CardInfo()
		{
			FactionColor = _factionCom.Faction == null ? DefualtColor : _factionCom.Faction.FactionColor,
			FactionName = _factionCom.Faction == null ? "N/A" : _factionCom.Faction.Name,
			Job = _worker.Job == null ? "N/A" : _worker.Job.JobTitle,
			Leaning = _beliefControler.TotalLeaning,
			MaxLeaning = (_beliefControler.BeliefCount) * BeliefControler.MAX_RIGHT,
			Name = _name,
			FoodNeed = _needs.FoodNeed,
			SleepingNeed = _needs.SleepingNeed,
			SocialNeed = _needs.SocialNeed
		};

		PeopleInfo.Instance.SetActiveCard(card);
	}
}
