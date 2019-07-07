using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardPop : MonoBehaviour
{
	bool _active;
	string _cardName;

	public TextMeshProUGUI nameText;
	public TextMeshProUGUI jobText;
	public TextMeshProUGUI factionText;
	public LeaningSlider leaningSlider;
	public GameObject card;
	public NeedSlider foodNeed;
	public NeedSlider socialNeed;
	public NeedSlider sleepingNeed;

	void Start()
	{
		_active = true;
		_cardName = "zzzzzzzzzzzzzzzz";
	}

	// Update is called once per frame
	void Update()
    {
		var peopleCard = PeopleInfo.Instance.SelectedCard;

		if (peopleCard == null && _active)
		{
			_active = false;
			card.SetActive(_active);
		}
		else if (peopleCard != null && (!_active || _cardName != peopleCard.Name))
		{
			_active = true;
			card.SetActive(_active);

			nameText.text = peopleCard.Name;
			jobText.text = peopleCard.Job;
			factionText.text = peopleCard.FactionName;
			factionText.color = peopleCard.FactionColor;
			leaningSlider.UpdateBar(peopleCard.Leaning, peopleCard.MaxLeaning);

			foodNeed.UpdateBar(peopleCard.FoodNeed);
			socialNeed.UpdateBar(peopleCard.SocialNeed);
			sleepingNeed.UpdateBar(peopleCard.SleepingNeed);
		}
    }
}
