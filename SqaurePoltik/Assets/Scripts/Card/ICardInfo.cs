using UnityEngine;

interface ICardInfo
{
	string Name { get; }

	float Leaning { get; }

	float MaxLeaning { get; }

	string Job { get; }

	string FactionName { get;  }

	Color32 FactionColor { get; }

	INeed FoodNeed { get; }

	INeed SleepingNeed { get; }

	INeed SocialNeed { get; }
}
