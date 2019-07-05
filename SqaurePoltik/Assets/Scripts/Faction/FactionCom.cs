using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionCom : MonoBehaviour
{
	public Faction Faction { get; set; }

	public BeliefControler beliefControler;
	public GameObject factionBubble;
	public NormalPersonAI normalPersonAI;

	public FollowerJob FollowerJob { get; set; }

	void OnDestroy()
	{
		if(Faction != null)
			Faction.RemoveMember(this);
	}

}
