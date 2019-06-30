using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionCom : MonoBehaviour
{
	public Faction Faction { get; set; }

	public BeliefControler beliefControler;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnDestroy()
	{
		Faction.RemoveMember(this);
	}

}
