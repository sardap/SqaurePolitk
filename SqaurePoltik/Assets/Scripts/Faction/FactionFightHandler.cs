using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionFightHandler : MonoBehaviour
{
	public FactionCom factionCom;

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag != "FactionBubble" || factionCom.Faction.Fighting)
		{
			return;
		}

		var fuck = other.GetComponent<FactionFightHandler>();
		var shit = fuck.factionCom;

		if (shit.Faction.Fighting || factionCom.Faction.Fighting)
		{
			return;
		}

		factionCom.Faction.StartFight(shit.Faction);
	}

}
