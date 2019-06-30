using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class FollowerJob : IJobAction
{
	public BeliefControler beliefControler;
	public Transform following;
	public NavMeshAgent agent;
	public FactionCom FactionCom;

	public GameObject _line;

	public Color32 HatColor
	{
		get
		{
			return FactionCom.Faction.FactionColor;
		}
	}

	public Vector3 WorkStationLocation
	{
		set
		{
			throw new System.NotImplementedException();
		}
	}

	public bool Walk
	{
		get
		{
			return true;
		}
	}

	public Worker Worker
	{
		get; set;
	}

	public float Length
	{
		get
		{
			return 70f;
		}
	}

	public bool JobReady()
	{
		var otherAI = following.gameObject.GetComponent<NormalPersonAI>();

		return otherAI == null || otherAI.ReadyToBeFollwed;
	}

	public void Quit()
	{
		following.gameObject.GetComponent<BeliefControler>().follwers.Remove(this);
	}

	public void Start()
	{
		agent.SetDestination(following.position);

		_line = Factory.Instance.GetTalkingLine();

		var lat = _line.GetComponent<LineAttachTo>();
		lat.targetA = agent.transform;
		lat.targetB = following.transform;

		var lr = _line.GetComponent<LineRenderer>();
		lr.material.color = FactionCom.Faction.FactionColor;

		beliefControler.EmulateBeliefStep(following.GetComponent<BeliefControler>(), 10);
	}

	public void Step()
	{
		float dist = Vector3.Distance(following.position, agent.transform.position);
		if (dist > 5)
		{
			agent.isStopped = false;
			agent.SetDestination(following.position);
		}

		if (agent.remainingDistance < 2)
		{
			agent.isStopped = true;
		}
	}

	public void Stop()
	{
		Factory.Instance.ReleaseaTalkingLine(ref _line);
	}
}
