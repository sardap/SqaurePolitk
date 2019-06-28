using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Factory
{
	public class ReturnedInccorectObjectExpection : System.Exception
	{
		public ReturnedInccorectObjectExpection(string name) : base(string.Format("Object {0}", name))
		{
		}
	}

	public interface IResouceCreator
	{
		string name { get; }

		void Init();

		GameObject Create();
	}

	class TalkingLineCreator : IResouceCreator
	{
		Material _material;

		public string name
		{
			get
			{
				return "Line";
			}
		}

		public void Init()
		{

			_material = Resources.Load<Material>("Materials/TalkingLine");
		}

		public GameObject Create()
		{
			var gameObject = new GameObject();

			var lr = gameObject.AddComponent<LineRenderer>();
			lr.startWidth = 0.1f;
			lr.endWidth = 0.1f;
			lr.material = _material;

			var lat = gameObject.AddComponent<LineAttachTo>();
			lat.lineHeight = 1.5f;
			lat.lineRenderer = lr;

			return gameObject;
		}
	}

	class BusyTalkingCreator : IResouceCreator
	{
		GameObject _talkingText;

		public string name
		{
			get
			{
				return "BusyTalk";
			}
		}

		public void Init()
		{
			_talkingText = Resources.Load("Text/TalkingText") as GameObject;
		}

		public GameObject Create()
		{
			var talkingText = Object.Instantiate(_talkingText);
			return talkingText;
		}

	}

	public class ResouceManager
	{
		Stack<GameObject> _holding = new Stack<GameObject>();
		Dictionary<string, GameObject> _leasedObjects = new Dictionary<string, GameObject>();
		int _count = 0;
		IResouceCreator _creator;
		GameObject _holder;

		public int HoldingCount
		{
			get
			{
				return _holding.Count;
			}
		}

		public int LeasedCount
		{
			get
			{
				return _leasedObjects.Count;
			}
		}

		public ResouceManager(IResouceCreator resouceCreator, GameObject holder)
		{
			_creator = resouceCreator;
			_creator.Init();
			_holder = new GameObject();
			_holder.name = _creator.name + " holder";
			_holder.transform.SetParent(holder.transform);
		}

		public void DestroyXHolding(int x)
		{
			Debug.LogFormat("Factroy-{1}: Destyroying {0}", x, _creator.name);
			for(int i = 0; i < x; i++)
			{
				var toKill = _holding.Pop();
				Object.Destroy(toKill);
			}
		}

		public GameObject Get()
		{
			if (_holding.Count == 0)
			{
				int n = 5;// (int)System.Math.Max(System.Math.Round(_leasedObjects.Count * 0.5), 1);

				Debug.LogFormat("Factroy-{1}: Creating {0}", n, _creator.name);

				for (int i = 0; i < n; i++)
				{
					var newCreate = _creator.Create();
					newCreate.name = _creator.name + _count++;
					newCreate.transform.parent = _holder.transform;
					newCreate.SetActive(false);
					_holding.Push(newCreate);
				}
			}

			var result = _holding.Pop();
			_leasedObjects.Add(result.name, result);

			result.SetActive(true);
			return result;
		}

		public void Return(GameObject returned)
		{
			if (!_leasedObjects.ContainsKey(returned.name))
			{
				throw new ReturnedInccorectObjectExpection(returned.name);
			}

			_leasedObjects.Remove(returned.name);

			returned.SetActive(false);
			_holding.Push(returned);
		}
	}

	public class FactoryScript : MonoBehaviour
	{

		IEnumerator AnaylseManager(ResouceManager resouceManager)
		{
			int maxLeased = 0;

			while (true)
			{
				maxLeased = System.Math.Max(maxLeased, resouceManager.LeasedCount);

				if(resouceManager.HoldingCount > maxLeased * 0.75f)
				{
					resouceManager.DestroyXHolding(resouceManager.HoldingCount - (int)(maxLeased * 0.75f));
				}

				yield return new WaitForSeconds(10f);
			}
		}

		void Start()
		{
		}

		public void AddManager(ResouceManager resouceManager)
		{
			StartCoroutine(AnaylseManager(resouceManager));
		}

		void Update()
		{
		}
	}

	static Factory _instance;

	public static Factory Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new Factory();
			}

			return _instance;
		}
	}

	GameObject _holder;
	ResouceManager _talkingLines;
	ResouceManager _busyTalking;

	public Factory()
	{
		_holder = new GameObject();
		_talkingLines = new ResouceManager(new TalkingLineCreator(), _holder);
		_busyTalking = new ResouceManager(new BusyTalkingCreator(), _holder);

		_holder.name = "Factory";

		var fs = _holder.AddComponent<FactoryScript>();
		fs.AddManager(_talkingLines);
		fs.AddManager(_busyTalking);
	}

	public GameObject GetTalkingLine()
	{
		return _talkingLines.Get();
	}

	public void ReleaseaTalkingLine(ref GameObject line)
	{
		var lr = line.transform.GetComponent<LineRenderer>();
		_talkingLines.Return(line);
		line = null;
	}

	public GameObject GetBusyTalkingText(string text, Color32 color, Vector3 position)
	{
		var result = _busyTalking.Get();

		var textMeshPro = result.GetComponent<TextMeshPro>();
		textMeshPro.text = text;
		textMeshPro.faceColor = color;
		result.transform.position = new Vector3(position.x, 20, position.z);

		return result;
	}

	public GameObject GetBusyTalkingText(string text, Color32 color, Transform transform)
	{
		return GetBusyTalkingText(text, color, transform.position);
	}

	public void ReleaseBusyTalkingText(GameObject gameObject)
	{
		_busyTalking.Return(gameObject);
	}

	public int BusyTalkingTextCount()
	{
		return _busyTalking.HoldingCount + _busyTalking.LeasedCount;
	}
}