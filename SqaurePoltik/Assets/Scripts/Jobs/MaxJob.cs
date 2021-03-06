﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class MaxJob : IJobAction
{
	const float SPEEACH_COOLDOWN = 20f;

	enum State
	{
		None,
		WalkingToSpeechArea,
		GivingSpeech,
		LookingForFight,
		FinshedSpeech,
		FactionWar
	}

	SpeechAreaInfo _speechArea;
	State _state = State.None;
	Color32 _currentHatColor = new Color32() { r = byte.MaxValue, g = byte.MaxValue, b = 0, a = byte.MaxValue };
	float _changeCD;
	float _talkingCD;
	Stack<NormalPersonAI> _listeners = new Stack<NormalPersonAI>();

	public BeliefControler beliefControler;
	public NavMeshAgent agent;

	public Camera Camera { get; set; }

	public FactionCom FactionCom { get; set; }

	public Color32 HatColor
	{
		get
		{
			return _currentHatColor;
		}
	}

	public float Length
	{
		get
		{
			if(_state == State.FactionWar)
			{
				return float.MaxValue;
			}

			if(_state == State.None)
			{
				return 0;
			}

			return _state == State.FinshedSpeech ? 0 : 120f;
		}
	}

	public Worker Worker
	{
		get; set;
	}

	public bool Walk
	{
		get
		{
			return true;
		}
	}

	public string JobTitle
	{
		get
		{
			return "Poltion";
		}
	}

	public void Quit()
	{
		Stop();
	}

	public void Start()
	{
		_speechArea = CityInfo.Instance.FindClosetSpeechArea(agent.transform);

		if (_speechArea != null)
		{
			_state = State.WalkingToSpeechArea;
			_speechArea.Intrest();

			agent.SetDestination(_speechArea.speakingArea.position);
		}
		else
		{
			_state = State.None;
		}
	}

	public void Step()
	{
		switch (_state)
		{
			case State.WalkingToSpeechArea:
				if (!agent.pathPending && agent.remainingDistance <= 0.1)
				{
					_changeCD = SPEEACH_COOLDOWN;
					_talkingCD = 0f;
					_speechArea.Occupy();
					beliefControler.StartSpeech();
					_state = State.GivingSpeech;
				}
				break;

			case State.GivingSpeech:
				_talkingCD -= Time.deltaTime;

				FindListener();

				if (_talkingCD <= 0)
				{
					Helper.CreateTalkingText(Camera, beliefControler.BeliefColor, agent.transform);
					beliefControler.StartSpeech();
					_talkingCD = 0.5f;
				}

				if (_changeCD <= 0)
				{
					_state = State.FinshedSpeech;
				}
				break;

			case State.FinshedSpeech:
				Stop();
				_state = State.None;
				break;

			case State.FactionWar:
				_talkingCD -= Time.deltaTime;

				if (_talkingCD <= 0)
				{
					Helper.CreateTalkingText(Camera, beliefControler.BeliefColor, agent.transform, "Non gradum retro");
					_talkingCD = 0.5f;
				}

				if (!FactionCom.Faction.Fighting)
				{
					_state = State.None;
				}

				break;
		}
	}

	public void StartFactionFight()
	{
		if(_state == State.GivingSpeech || _state == State.WalkingToSpeechArea)
		{
			Stop();
		}

		_state = State.FactionWar;
	}

	void FindListener()
	{
		Debug.Assert(_speechArea != null);

		var people = _speechArea.GetPeople();

		Debug.Assert(people != null);

		foreach (var person in people)
		{
			if (person.CanRevSpeech())
			{
				var listen = Util.RandomFloat(0, beliefControler.convincingVal) * FactionCom.Faction.MemberMultipler;

				if (listen >= person.beliefControler.convincingVal)
				{
					person.RevSpeech(this, _speechArea.GetStandingPostion());
					_listeners.Push(person);
				}
			}
		}
	}

	public void Stop()
	{
		beliefControler.StopSpeech();

		while (_listeners.Count > 0)
		{
			_listeners.Pop().StopRevSpeech();
		}

		if(_speechArea != null)
		{
			_speechArea.Unoccupy();
			_speechArea = null;
		}
	}

	public bool JobReady()
	{
		return true;
	}
}
