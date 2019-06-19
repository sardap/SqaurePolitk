using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

class CityInfo
{
	public enum BuildingType
	{
		NormalBuilding,
		SpeechArea,
		Market
	}

	public interface IBuilding
	{
		string Name { get; }
	}

	public class SpeechArea : IBuilding
	{
		public string Name { get { return "SpeechArea"; } }
	}

	public class NormalBuilding : IBuilding
	{
		public string Name { get { return "NormalBuilding"; } }
	}

	public class MarketBuilding : IBuilding
	{
		public string Name { get { return "Market"; } }
	}

	static CityInfo _instance;

	public static CityInfo Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = new CityInfo();
			}

			return _instance;
		}
	}

	public Dictionary<BuildingType, IBuilding> BuildingInfo = new Dictionary<BuildingType, IBuilding>()
	{
		{BuildingType.NormalBuilding, new NormalBuilding() },
		{BuildingType.SpeechArea, new SpeechArea() },
		{BuildingType.Market, new MarketBuilding() }
	};

	Dictionary<IBuilding, IList<GameObject>> _buildings = new Dictionary<IBuilding, IList<GameObject>>();

	List<MarketInfo> _workplaces = new List<MarketInfo>();
	List<WorkStation> _avalabeWorkstations = new List<WorkStation>();
	List<WorkStation> _occupyedWorkstations = new List<WorkStation>();

	CityInfo()
	{
		foreach(var building in BuildingInfo.Values)
		{
			_buildings.Add(building, new List<GameObject>());
		}
	}

	public void RegsiterBuilding(BuildingType building, GameObject instance)
	{
		_buildings[BuildingInfo[building]].Add(instance);

		if(building == BuildingType.Market)
		{
			var workPlace = instance.GetComponent<MarketInfo>();
			_workplaces.Add(workPlace);
			_avalabeWorkstations.AddRange(workPlace.workStations);
		}
	}

	public void FindJobForWorker(Worker worker, NavMeshAgent agent, Camera camera)
	{
		if(_avalabeWorkstations.Count <= 0)
		{
			return;
		}

		var workStation = Util.RandomElement(_avalabeWorkstations);
		_avalabeWorkstations.Remove(workStation);
		_occupyedWorkstations.Add(workStation);

		var marketPlace = new MarketJob()
		{
			WorkStation = workStation,
			camera = camera,
			transform = worker.transform,
			agent = agent,
			Market = workStation.marketInfo
		};

		worker.GiveJob(marketPlace);
	}

	public SpeechAreaInfo FindClosetSpeechArea(Transform finder)
	{
		Transform tMin = null;
		float minDist = Mathf.Infinity;
		Vector3 currentPos = finder.position;
		foreach (var gameObject in _buildings[BuildingInfo[BuildingType.SpeechArea]])
		{
			if (!gameObject.GetComponent<SpeechAreaInfo>().Empty)
			{
				continue;
			}

			float dist = Vector3.Distance(gameObject.transform.position, currentPos);
			if (dist < minDist)
			{
				tMin = gameObject.transform;
				minDist = dist;
			}
		}

		if (tMin == null)
			return null;

		return tMin.gameObject.GetComponent<SpeechAreaInfo>();
	}
}
