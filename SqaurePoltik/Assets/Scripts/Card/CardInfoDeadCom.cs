using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfoDeadCom : MonoBehaviour
{
	static Color32 DefualtColor = new Color32() { a = byte.MaxValue, r = byte.MaxValue, g = byte.MaxValue, b = byte.MaxValue };

	CardInfo _cardInfo;

	public GameObject outline;

	public void PopluateValue(string name, Faction faction, Worker worker, BeliefControler beliefControler, Needs needs)
	{
		Color32 factionColor;
		string factionName;
		string jobTitle;
		float leaning;
		float maxLeaning;
		DiedNeed foodNeed;
		DiedNeed socialNeed;
		DiedNeed sleepingNeed;

		factionColor = faction == null ? DefualtColor : faction.FactionColor;
		factionName = faction == null ? "N/A" : faction.Name;
		jobTitle = worker.Job == null ? "N/A" : worker.Job.JobTitle;
		leaning = beliefControler.TotalLeaning;
		maxLeaning = (beliefControler.BeliefCount) * BeliefControler.MAX_RIGHT;
		foodNeed = new DiedNeed(needs.FoodNeed);
		socialNeed = new DiedNeed(needs.SocialNeed);
		sleepingNeed = new DiedNeed(needs.SleepingNeed);

		_cardInfo = new CardInfo()
		{
			FactionColor = factionColor,
			FactionName = factionName,
			Job = jobTitle,
			Leaning = leaning,
			MaxLeaning = maxLeaning,
			Name = name,
			FoodNeed = foodNeed,
			SleepingNeed = sleepingNeed,
			SocialNeed = socialNeed
		};

	}

	void Update()
    {
		if (outline.activeSelf && ((PeopleInfo.Instance.SelectedCard != null && PeopleInfo.Instance.SelectedCard.Name != _cardInfo.Name) || PeopleInfo.Instance.SelectedCard == null))
		{
			outline.SetActive(false);
		}
	}

	void OnMouseDown()
	{
		outline.SetActive(true);

		PeopleInfo.Instance.SetActiveCard(_cardInfo);
	}
}
