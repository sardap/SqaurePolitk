using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class CardInfo : ICardInfo
{
	public string Name
	{
		get; set;
	}

	public float Leaning
	{
		get; set;
	}

	public float MaxLeaning
	{
		get; set;
	}

	public string Job
	{
		get; set;
	}

	public string FactionName
	{
		get; set;
	}

	public Color32 FactionColor
	{
		get; set;
	}

	public INeed FoodNeed
	{
		get; set;
	}

	public INeed SleepingNeed
	{
		get; set;
	}

	public INeed SocialNeed
	{
		get; set;
	}
}
