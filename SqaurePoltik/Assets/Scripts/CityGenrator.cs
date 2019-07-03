using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using System.Threading;
using System.Collections.Concurrent;
using Unity.Jobs;
using Unity.Collections;

public class CityGenrator : MonoBehaviour
{
	public Camera mainCamera;
	public int numberOfBuildings;
	public Transform buildingTransform;
	public Transform peopleTransform;
	public GlobalLeaningUpdate globalLeaningUpdate;
	public GameObject player;

	int _personCount = 0;
	ConcurrentQueue<Vector3> _edges = new ConcurrentQueue<Vector3>();

	Semaphore _edgeMutex = new Semaphore(1, 1);
	Semaphore _wakeup = new Semaphore(1, int.MaxValue);
	volatile bool _createEdges = true;

	void CreateEdges()
	{
		var toCreate = new HashSet<Vector3>();
		var edges = new List<Vector3>() { new Vector3(0, 0, 0) };

		void PopluateEdgesForEdge(Vector3 edge)
		{
			var newValue = new Vector3(edge.x, edge.y, edge.z);
			newValue.x += 10;
			newValue.z += 0;
			if (!toCreate.Contains(newValue))
				edges.Add(newValue);

			newValue = new Vector3(edge.x, edge.y, edge.z);
			newValue.x += 0;
			newValue.z += 10;
			if (!toCreate.Contains(newValue))
				edges.Add(newValue);

			newValue = new Vector3(edge.x, edge.y, edge.z);
			newValue.x -= 0;
			newValue.z -= 10;
			if (!toCreate.Contains(newValue))
				edges.Add(newValue);

			newValue = new Vector3(edge.x, edge.y, edge.z);
			newValue.x -= 10;
			newValue.z -= 0;
			if (!toCreate.Contains(newValue))
				edges.Add(newValue);
		}

		do
		{
			_wakeup.WaitOne();

			var edge = edges[Util.SharpRandom.Next(0, edges.Count)];
			while (toCreate.Contains(edge))
			{
				edges.Remove(edge);
				if (edges.Count <= 0)
				{
					PopluateEdgesForEdge(edge);
				}

				edge = edges[Util.SharpRandom.Next(0, edges.Count)];
			}
			edges.Remove(edge);
			_edges.Enqueue(edge);
			toCreate.Add(edge);

			PopluateEdgesForEdge(edge);

			_edgeMutex.WaitOne();
			if (!_createEdges)
			{
				break;
			}
			_edgeMutex.Release();

		} while (true);

		_edges = null;
	}

	IEnumerator CreateBuilding()
	{
		DirectoryInfo levelDirectoryPath = new DirectoryInfo(Application.dataPath + "/Resources/Buildings");
		var files = levelDirectoryPath.GetFiles("*.prefab", SearchOption.AllDirectories);

		var buildings = new Dictionary<CityInfo.BuildingType, GameObject>
		{
			{
				CityInfo.BuildingType.NormalBuilding, Resources.Load("Buildings/" + Path.GetFileNameWithoutExtension(CityInfo.Instance.BuildingInfo[CityInfo.BuildingType.NormalBuilding].Name)) as GameObject
			},

			{
				CityInfo.BuildingType.SpeechArea, Resources.Load("Buildings/" + Path.GetFileNameWithoutExtension(CityInfo.Instance.BuildingInfo[CityInfo.BuildingType.SpeechArea].Name)) as GameObject
			},

			{
				CityInfo.BuildingType.Market, Resources.Load("Buildings/" + Path.GetFileNameWithoutExtension(CityInfo.Instance.BuildingInfo[CityInfo.BuildingType.Market].Name)) as GameObject
			}
		};

		var peopleTypes = new List<GameObject>
		{
			Resources.Load("People/NormalPerson") as GameObject
		};

		var buildingsCreated = new Queue<GameObject>();
		var toRegstier = new Queue<System.Tuple<CityInfo.BuildingType, IntiBuilding>>();

		int count = 0;

		_wakeup.Release(numberOfBuildings);
		for (int i = 0; i < numberOfBuildings; i++)
		{
			Vector3 edge;

			while (!_edges.TryDequeue(out edge))
				yield return null;

			var type = Random.Range(0, 10);

			CityInfo.BuildingType buildingType;

			if (type <= 6)
			{
				buildingType = CityInfo.BuildingType.NormalBuilding;
			}
			else if (type <= 8)
			{
				buildingType = CityInfo.BuildingType.Market;
			}
			else
			{
				buildingType = CityInfo.BuildingType.SpeechArea;
			}

			var building = Instantiate(buildings[buildingType], edge, Quaternion.identity);
			toRegstier.Enqueue(new System.Tuple<CityInfo.BuildingType, IntiBuilding>(buildingType, building.GetComponent<IntiBuilding>()));
			CityInfo.Instance.RegsiterBuilding(buildingType, building);
			building.transform.parent = buildingTransform;
			building.name += count++;
			buildingsCreated.Enqueue(building);

		}

		yield return null;

		_edgeMutex.WaitOne();
		_createEdges = false;
		_edgeMutex.Release();
		_wakeup.Release();

		yield return null;

		while(toRegstier.Count > 0)
		{
			var current = toRegstier.Dequeue();

			while (!current.Item2.HasStarted)
				yield return null;

			CityInfo.Instance.RegsiterBuilding(current.Item1, current.Item2.gameObject);
		}

		GetComponent<NavMeshSurface>().BuildNavMesh();

		while(buildingsCreated.Count > 0)
		{
			var building = buildingsCreated.Dequeue();

			var buildingData = building.GetComponentInChildren<IntiBuilding>();

			var numberLiving = Random.Range(0, buildingData.buildingTypeData.MaxCapaticy);

			var spawnAreaAgg = buildingData.GetComponentInChildren<SpawnAreaAgg>();

			for (int j = 0; j < numberLiving; j++)
			{
				var postion = spawnAreaAgg.GetRandomPostion();

				var person = Instantiate(peopleTypes[Random.Range(0, peopleTypes.Count)], postion, Quaternion.identity);
				person.name += _personCount++;
				person.transform.parent = peopleTransform;
				person.GetComponent<NormalPersonAI>().camera = mainCamera;
				person.GetComponent<NormalPersonAI>().Home = building.GetComponent<HouseBuilding>();

			}

			yield return null;
		}

		globalLeaningUpdate.UpdateBar();
	}

	private void Awake()
	{
		var work = Factory.Instance;
	}

	// Start is called before the first frame update
	void Start()
	{
		Thread thread = new Thread(new ThreadStart(CreateEdges));
		thread.Start();

		/*
		var player = Resources.Load("People/Player") as GameObject;
		player = Instantiate(player, new Vector3(0, 0.1f, 0), Quaternion.identity);
		player.GetComponent<PlayerController>().cam = mainCamera;
		*/
		player.transform.position = new Vector3(0, 0.1f, 0);

		mainCamera.GetComponent<FollowTrans>().target = player.transform;

		StartCoroutine(CreateBuilding());
	}

	// Update is called once per frame
	void Update()
    {
    }
}
