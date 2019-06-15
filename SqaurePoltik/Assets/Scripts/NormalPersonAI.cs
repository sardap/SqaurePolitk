using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class NormalPersonAI : MonoBehaviour
{
	const float WORKING_COOLDOWN = 60f;
	const float SPEEACH_COOLDOWN = 20f;
	const float TALKING_COOLDOWN = 10f;

	enum State
	{
		Idling,
		Wandering,
		TalkingInit,
		Talking,
		FinshedTalking,
		PreparingSpeech,
		GivingASpeech,
		RevSpeach,
		Working
	}

	public class StateMachineVoilation : System.Exception
	{
		public StateMachineVoilation(string currentState, string iligealState, string name) : base(string.Format("Frame:{0} {3} Current State {1} ileagal state {2}", Time.frameCount, currentState, iligealState, name))
		{

		}
	}

	static List<NormalPersonAI> personList = new List<NormalPersonAI>();

	static readonly State[] MovableStates = new State[] {State.Wandering, State.PreparingSpeech, State.RevSpeach};

	State _state;
	float _changeCD;
	float _talkingCD;
	NormalPersonAI _talkingSubject;
	NormalPersonAI _speaker;
	GameObject _line;
	Stack<NormalPersonAI> _listeners = new Stack<NormalPersonAI>();
	SpeechAreaInfo _speechArea;
	Worker _worker;

	public Wander wander;
	public BeliefControler beliefControler;
	public NavMeshAgent agent;
	public new Camera camera;
	public MeshRenderer speakingHat;

	// Start is called before the first frame update
	void Start()
	{
		personList.Add(this);
		ChangeState(State.Wandering);
		_talkingCD = Random.Range(0, 120);
		_worker = GetComponent<Worker>();
	}

	void ChangeState(State newState)
	{
		if (_state == State.Talking && newState != State.FinshedTalking)
		{
			throw new StateMachineVoilation(_state.ToString(), newState.ToString(), gameObject.name);
		}

		if (newState == State.Working && _worker.CurrentState != Worker.State.HasJob)
		{
			throw new StateMachineVoilation(_state.ToString(), newState.ToString(), gameObject.name);
		}

		_state = newState;
		Debug.LogFormat("Frame:{2} {1} NEW STATE {0} ", newState, gameObject.name, Time.frameCount);

		wander.enabled = _state == State.Wandering;

		agent.isStopped = !(MovableStates.Any(i => _state == i) || _state == State.Working && _worker.job.Walk);

		if (_state == State.Idling || _state == State.Wandering)
		{
			_changeCD = Random.Range(0, 120);
		}
	}

	bool OnCamrea()
	{
		var test = camera.WorldToViewportPoint(transform.position);
		return test.x > 0 && test.x < 1 && test.y > 0 && test.y < 1;
	}

	// Update is called once per frame
	void Update()
    {
		_changeCD -= Time.deltaTime;

		switch (_state)
		{
			case State.Idling:
				if(_changeCD <= 0)
				{
					ChangeState(State.Wandering);
				}
				break;

			case State.Wandering:
				_talkingCD -= Time.deltaTime;

				if (_changeCD <= 0)
				{
					State newState = State.Idling;

					if(beliefControler.PassionLevel == BeliefControler.EPassionLevel.Max && Random.Range(0, 30) >= 9 && PrepareSpeech())
					{
						newState = State.PreparingSpeech;
					}
					else
					{
						if (_worker.CurrentState == Worker.State.Jobless)
						{
							FindJob();
						}

						if (_worker.CurrentState == Worker.State.HasJob)
						{
							if (Util.RandomBool())
							{
								StartWork();
								newState = State.Working;
							}
						}
					}

					ChangeState(newState);
				}
				break;

			case State.Talking:
				_talkingCD -= Time.deltaTime;

				if (_talkingCD <= 0)
				{
					CreateTalkingText();

					_talkingCD = 2f;
				}

				if (_changeCD <= 0)
				{
					ChangeState(State.FinshedTalking);
				}

				break;

			case State.FinshedTalking:
				if(_talkingSubject._state == State.FinshedTalking || _talkingSubject._talkingSubject != this)
				{
					if(_line != null)
					{
						Debug.LogFormat("Frame:{2} {1} RETURNING {0} ", _line.name, gameObject.name, Time.frameCount);
						Factory.Instance.ReleaseaTalkingLine(ref _line);
					}

					_talkingCD = TALKING_COOLDOWN;

					speakingHat.enabled = false;
					speakingHat.material.color = Color.white;

					_talkingSubject = null;
					ChangeState(State.Wandering);
				}
				break;

			case State.PreparingSpeech:
				if(ReachedTarget())
				{
					StartSpeech();
					ChangeState(State.GivingASpeech);
				}
				break;

			case State.GivingASpeech:
				_talkingCD -= Time.deltaTime;

				FindListener();

				if (_talkingCD <= 0)
				{
					CreateTalkingText();
					_talkingCD = 0.5f;
				}

				if (_changeCD <= 0)
				{
					StopSpeech();
					ChangeState(State.Wandering);
				}
				break;

			case State.RevSpeach:

				if(_changeCD <= 0)
				{
					beliefControler.ReceiveSpeechBelief(_speaker.beliefControler);
					_changeCD = Random.Range(0f, 2f);
				}

				break;

			case State.Working:
				_worker.job.Step();

				if (_changeCD <= 0)
				{
					StopWork();
					ChangeState(State.Wandering);
				}
				break;
		}
    }

	void FindJob()
	{
		if(beliefControler.PassionLevel == BeliefControler.EPassionLevel.High)
		{
			if(!PeopleInfo.Instance.MaxForLeaning(beliefControler))
			{
				return;
			}

			var newJob = new FollowerJob()
			{
				beliefControler = beliefControler,
				following = PeopleInfo.Instance.FindClosetToFollow(beliefControler).transform,
				agent = agent
			};

			_worker.GiveJob(newJob);
		}
		else
		{
			CityInfo.Instance.FindJobForWorker(_worker, agent,camera);
		}
	}

	void CreateTalkingText()
	{
		Helper.CreateTalkingText(camera, beliefControler.BeliefColor, transform);
	}

	bool ReachedTarget()
	{
		return !agent.pathPending && agent.remainingDistance <= 0.01;
	}

	bool PrepareSpeech()
	{
		_speechArea = CityInfo.Instance.FindClosetSpeechArea(transform);

		if(_speechArea == null)
		{
			return false;
		}

		_speechArea.Intrest();
		agent.SetDestination(_speechArea.speakingArea.position);

		speakingHat.enabled = true;
		speakingHat.material.color = new Color32() { r = byte.MaxValue, g = byte.MaxValue, b = 0, a = byte.MaxValue };

		return true;
	}

	void StartSpeech()
	{
		speakingHat.enabled = false;
		_changeCD = SPEEACH_COOLDOWN;
		_talkingCD = 0f;
		_speechArea.Occupy();
		beliefControler.StartSpeech();
	}

	void StopSpeech()
	{
		beliefControler.StopSpeech();
		speakingHat.enabled = false;

		while (_listeners.Count > 0)
		{
			_listeners.Pop().StopRevSpeech();
		}

		_speechArea.Unoccupy();
		_speechArea = null;
	}

	void FindListener()
	{
		foreach(var person in _speechArea.GetPeople())
		{
			if ((person._state == State.Idling || person._state == State.Wandering))
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
	void RevSpeech(NormalPersonAI speaker, Vector3 standingPostion)
	{
		_speaker = speaker;

		ChangeState(State.RevSpeach);

		agent.SetDestination(standingPostion);

		transform.LookAt(speaker.transform);

		_line = Factory.Instance.GetTalkingLine();

		var lat = _line.GetComponent<LineAttachTo>();
		lat.targetA = transform;
		lat.targetB = speaker.transform;

		var lr = _line.GetComponent<LineRenderer>();
		lr.material.color = Color.green;

		_changeCD = 0f;
	}

	void StopRevSpeech()
	{
		_speaker = null;
		Factory.Instance.ReleaseaTalkingLine(ref _line);
		ChangeState(State.Wandering);
	}

	bool CanTalk(NormalPersonAI person)
	{
		return person._state == State.Wandering && _talkingCD <= 0;
	}

	void StopWork()
	{
		_worker.job.Stop();
		speakingHat.enabled = false;
	}

	void StartWork()
	{
		_changeCD = WORKING_COOLDOWN;
		ChangeState(State.Working);

		_worker.job.Start();

		speakingHat.enabled = true;
		speakingHat.material.color = _worker.job.HatColor;
	}

	//Detect collisions between the GameObjects with Colliders attached
	void OnTriggerEnter(Collider other)
	{
		if(CanTalk(this) && other.gameObject.tag == "Person")
		{
			var otherPerson = other.gameObject.GetComponent<NormalPersonAI>();

			if (!CanTalk(otherPerson))
			{
				return;
			}

			var subject = beliefControler.CommonBelief(otherPerson.beliefControler);

			if(subject == null)
			{
				return;
			}

			ChangeState(State.Talking);
			otherPerson.ChangeState(State.Talking);

			speakingHat.enabled = true;
			speakingHat.material.color = Color.white;

			otherPerson.speakingHat.enabled = true;
			otherPerson.speakingHat.material.color = Color.white;

			_talkingSubject = otherPerson;
			otherPerson._talkingSubject = this;

			beliefControler.TalkTo(otherPerson.beliefControler, subject);

			_talkingCD = 0f;
			otherPerson._talkingCD = 1f;

			_changeCD = Random.Range(4f, 8f);
			otherPerson._changeCD = _changeCD;

			_line = Factory.Instance.GetTalkingLine();
			Debug.LogFormat("Frame:{2} {1} GETTING {0} ", _line.name, gameObject.name, Time.frameCount);

			var lat = _line.GetComponent<LineAttachTo>();
			lat.targetA = transform;
			lat.targetB = otherPerson.transform;

			var lr = _line.GetComponent<LineRenderer>();
			lr.material.color = Color.white;

			transform.LookAt(otherPerson.transform);
		}

		return;
	}
}
