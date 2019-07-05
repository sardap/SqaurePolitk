using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeechAreaInfo : MonoBehaviour
{
	public class GetPeopleWithoutOccpying : System.Exception
	{
	}

	public enum States
	{
		Empty,
		Intrest,
		Occupied
	}

	public Transform speakingArea;
	public ColRego colRego;
	public SpawnArea standingArea;

	MeshRenderer _stageRender;
	HashSet<GameObject> _servredPeople;
	States _state;

	public bool Empty
	{
		get
		{
			return _state == States.Empty;
		}
	}

	void Start()
	{
		_servredPeople = new HashSet<GameObject>();
		_state = States.Empty;
		_stageRender = speakingArea.gameObject.GetComponent<MeshRenderer>();
	}

	public void Intrest()
	{
		_stageRender.material.color = Color.yellow;
		_state = States.Intrest;
	}

	public void Occupy()
	{
		_stageRender.material.color = Color.magenta;
		_state = States.Occupied;
	}

	public void Unoccupy()
	{
		_stageRender.material.color = Color.white;
		_state = States.Empty;
		_servredPeople.Clear();
	}

	public Vector3 GetStandingPostion()
	{
		return standingArea.GetRandomPostion();
	}

	public List<NormalPersonAI> GetPeople()
	{
		Debug.Assert(_state == States.Occupied);

		colRego.inside.RemoveWhere(i => i == null);

		foreach(var fuckyou in _servredPeople)
		{
			if(fuckyou == null)
			{
				_servredPeople.Remove(null);
			}
		}

		var personList = colRego.inside.Except(_servredPeople);
		var result = new Stack<NormalPersonAI>(personList.Count());

		foreach(var person in personList)
		{
			NormalPersonAI toAdd = person.GetComponent<NormalPersonAI>();
			if (toAdd != null)
			{
				_servredPeople.Add(person);
				result.Push(toAdd);
			}
		}

		return result.ToList();
	}
}
