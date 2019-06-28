using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

class MarketJob : IJobAction
{
	static Color32 _hatColor = Color.magenta;
	static IFoodCreateInfo[] _foodCreateInfoAry = new IFoodCreateInfo[] { new BunCreateInfo() };

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

	public MarketInfo Market
	{
		get; set;
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

	public void Start()
	{
		_talkingCD = 0;
		agent.SetDestination(WorkStation.transform.position);
	}

	public void Step()
	{
		if(!agent.pathPending && agent.remainingDistance < 0.5f)
		{
			_talkingCD -= Time.deltaTime;

			if (_talkingCD < 0)
			{
				Helper.CreateTalkingText(camera, HatColor, transform);

				var createInfo = Util.RandomElement(_foodCreateInfoAry);
				Market.MakeFood(createInfo);
				_talkingCD = createInfo.TimeToCreate;
			}
		}
	}

	public void Stop()
	{
	}

	public void Quit()
	{
		CityInfo.Instance.ReturnWorkstation(WorkStation);
	}
}
