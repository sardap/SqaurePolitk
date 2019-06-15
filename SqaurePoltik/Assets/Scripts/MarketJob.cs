using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

class MarketJob : IJobAction
{
	static Color32 _hatColor = Color.magenta;

	float _talkingCD;

	public Camera camera;
	public Transform transform;
	public NavMeshAgent agent;

	public Color32 HatColor
	{
		get
		{
			return _hatColor;
		}
	}

	public bool Walk
	{
		get
		{
			return true;
		}
	}

	public WorkStation WorkStation
	{
		get;  set;
	}

	public void Start()
	{
		_talkingCD = 0;
		agent.SetDestination(WorkStation.WorkingPostion());
	}

	public void Step()
	{
		if(agent.remainingDistance < 0.5f)
		{
			_talkingCD -= Time.deltaTime;

			if (_talkingCD < 0)
			{
				Helper.CreateTalkingText(camera, HatColor, transform);
				_talkingCD = 5f;
			}
		}
	}

	public void Stop()
	{
	}
}
