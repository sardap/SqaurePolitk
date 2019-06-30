using System.Collections.Generic;
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
		FinshedSpeech
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
			return _state == State.FinshedSpeech ? 0 : 60f;
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

	public void AddFollower(FollowerJob follower)
	{
		beliefControler.follwers.Add(follower);
	}

	public void Quit()
	{
		beliefControler.follwers.ToList().ForEach(i => i.Worker.QuitJob());
		beliefControler.follwers.Clear();
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
					_talkingCD = 0.5f;
				}

				if (_changeCD <= 0)
				{
					_state = State.FinshedSpeech;
				}
				break;

			case State.FinshedSpeech:
				Stop();
				break;
		}
	}

	void FindListener()
	{
		foreach (var person in _speechArea.GetPeople())
		{
			if (person.CanRevSpeech())
			{
				var listen = Util.RandomFloat(0, beliefControler.convincingVal);

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
