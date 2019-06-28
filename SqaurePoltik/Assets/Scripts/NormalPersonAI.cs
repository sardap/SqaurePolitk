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
	const float TALKING_COOLDOWN = 60f;
	const float FOOD_STARVING_THRESHHOLD = 0.4f;

	public enum State
	{
		Idling,
		Wandering,
		TalkingInit,
		Talking,
		FinshedTalking,
		RevSpeach,
		Working,
		FindingFood,
		ConsumingFood,
		Dying
	}

	public class StateMachineVoilation : System.Exception
	{
		public StateMachineVoilation(string currentState, string iligealState, string name, Stack<State> prevousStates) : base(string.Format("Frame:{0} {3} Current State {1} ileagal state {2} Last 5 States {4}", Time.frameCount, currentState, iligealState, name, LastFiveStates(prevousStates)))
		{

		}
	}

	static string LastFiveStates(IEnumerable<State> prevousStates)
	{
		string result = "";

		for(int i  = 0; i < 5 && i < prevousStates.Count(); i++)
		{
			result += prevousStates.ElementAt(i);

			if(i != 4 || i != prevousStates.Count() - 1)
			{
				result += ", ";
			}
		}

		return result;
	}

	static List<NormalPersonAI> personList = new List<NormalPersonAI>();

	static readonly State[] MovableStates = new State[] {State.Wandering, State.RevSpeach, State.FindingFood};

	State _state;
	float _changeCD;
	float _talkingCD;
	float _talkingToOtherCD;
	float _workingCD;
	NormalPersonAI _talkingSubject;
	MaxJob _speaker;
	GameObject _line;
	Stack<NormalPersonAI> _listeners = new Stack<NormalPersonAI>();
	Worker _worker;
	Needs _needs;

	MarketInfo _targetMarket { get; set; }

	Stack<State> _prevousStates;

	public Wander wander;
	public BeliefControler beliefControler;
	public NavMeshAgent agent;
	public new Camera camera;
	public MeshRenderer hat;

	// Start is called before the first frame update
	void Start()
	{
		_prevousStates = new Stack<State>();
		personList.Add(this);
		ChangeState(State.Wandering);
		_talkingToOtherCD = Random.Range(0, 120);
		_worker = GetComponent<Worker>();
		_needs = GetComponent<Needs>();
	}

	void ChangeState(State newState)
	{
		if (_state == State.Talking && newState != State.FinshedTalking)
		{
			throw new StateMachineVoilation(_state.ToString(), newState.ToString(), gameObject.name, _prevousStates);
		}

		if (newState == State.Working && _worker.CurrentState != Worker.State.HasJob)
		{
			throw new StateMachineVoilation(_state.ToString(), newState.ToString(), gameObject.name, _prevousStates);
		}

		_prevousStates.Push(newState);

		_state = newState;
		Debug.LogFormat("Frame:{2} {1} NEW STATE {0} ", newState, gameObject.name, Time.frameCount);

		wander.enabled = _state == State.Wandering;

		agent.isStopped = !(MovableStates.Any(i => _state == i) || _state == State.Working && _worker.Job.Walk);

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

	bool DepsrateForFood()
	{
		var mostDesprateNeed = _needs.MostDesprateNeed();
		return mostDesprateNeed.GetType() == typeof(FoodNeed) && mostDesprateNeed.Value <= mostDesprateNeed.MaxValue * FOOD_STARVING_THRESHHOLD;
	}

	// Update is called once per frame
	void Update()
    {
		_changeCD -= Time.deltaTime;
		_talkingToOtherCD -= Time.deltaTime;
		_workingCD -= Time.deltaTime;

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

				var mostDesprateNeed = _needs.MostDesprateNeed();
				bool eatChance = false;

				if (mostDesprateNeed.GetType() == typeof(FoodNeed))
				{

					if (DepsrateForFood())
					{
						eatChance = true;
					}
					else if (mostDesprateNeed.Value <= mostDesprateNeed.MaxValue * 0.5)
					{
						eatChance = Random.Range(0, 100) >= 50;
					}
					else if (mostDesprateNeed.Value <= mostDesprateNeed.MaxValue * 0.7)
					{
						eatChance = Random.Range(0, 100) >= 80;
					}

				}

				if (_changeCD <= 0 || eatChance)
				{
					if (_needs.Die)
					{
						ChangeState(State.Dying);
						break;
					}

					State newState = State.Idling;

					if (eatChance)
					{
						if (StartFindFood())
						{
							newState = State.FindingFood;
						}
					}
					else if(_workingCD <= 0)
					{
						if (_worker.CurrentState == Worker.State.Jobless)
						{
							FindJob();
						}

						if (_worker.CurrentState == Worker.State.HasJob)
						{
							StartWork();
							newState = State.Working;
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

					_talkingToOtherCD = TALKING_COOLDOWN;

					hat.enabled = false;
					hat.material.color = Color.white;

					_talkingSubject = null;
					ChangeState(State.Wandering);
				}
				break;

			case State.RevSpeach:
				if (_changeCD <= 0)
				{
					beliefControler.ReceiveSpeechBelief(_speaker.beliefControler);
					_changeCD = Random.Range(0f, 2f);
				}

				break;

			case State.Working:
				if (_worker.CurrentState == Worker.State.Jobless)
				{
					ChangeState(State.Wandering);
					break;
				}

				_worker.Job.Step();

				if (_changeCD <= 0)
				{
					StopWork();
					ChangeState(State.Wandering);
				}
				break;

			case State.FindingFood:
				if (ReachedTarget())
				{
					if (_targetMarket.ConsumeFood(_needs.FoodNeed))
					{
						_talkingCD = 5f;
						hat.material.color = new Color32() { a = byte.MaxValue, g = 255, b = 255 };
						ChangeState(State.ConsumingFood);
					}
					else
					{
						if (!StartFindFood())
						{
							hat.enabled = false;
							ChangeState(State.Wandering);
						}
					}
				}
				break;

			case State.ConsumingFood:
				_talkingCD -= Time.deltaTime;

				if (_talkingCD <= 0)
				{
					if (_needs.FoodNeed.Value > _needs.FoodNeed.MaxValue * 0.9)
					{
						hat.enabled = false;
						ChangeState(State.Wandering);
					}
					else
					{
						if (StartFindFood())
						{
							ChangeState(State.FindingFood);
						}
						else
						{
							hat.enabled = false;
							ChangeState(State.Wandering);
						}
					}
				}
				break;

			case State.Dying:
				Die();
				break;
		}
    }

	void Die()
	{
		Debug.LogFormat("Frame:{1} {0} DIED LAST 5 STATES: {2}", gameObject.name, Time.frameCount, LastFiveStates(_prevousStates));
		PeopleInfo.Instance.UnregsterMax(beliefControler);

		if(_worker.CurrentState == Worker.State.HasJob)
		{
			_worker.QuitJob();
		}

		Destroy(gameObject);
	}

	bool StartFindFood()
	{
		_targetMarket = CityInfo.Instance.FindClosetMarket(transform);

		if(_targetMarket == null)
		{
			return false;
		}

		agent.SetDestination(_targetMarket.GetPlaceToStand());

		hat.enabled = true;
		hat.material.color = new Color32() { a = byte.MaxValue, g = 255, b = 100 };

		return true;
	}

	void FindJob()
	{
		if (beliefControler.PassionLevel == BeliefControler.EPassionLevel.Max)
		{

			var newJob = new MaxJob()
			{
				agent = agent,
				beliefControler = beliefControler,
				Camera = camera
			};

			_worker.GiveJob(newJob);
		}
		else if(beliefControler.PassionLevel == BeliefControler.EPassionLevel.High)
		{
			if(!PeopleInfo.Instance.MaxForLeaning(beliefControler))
			{
				return;
			}

			var newJob = new FollowerJob()
			{
				beliefControler = beliefControler,
				agent = agent
			};

			newJob.following = PeopleInfo.Instance.FindClosetToFollow(newJob, beliefControler).transform;

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
		return !agent.pathPending && agent.remainingDistance <= 0.1;
	}

	public void RevSpeech(MaxJob speaker, Vector3 standingPostion)
	{
		_speaker = speaker;

		ChangeState(State.RevSpeach);

		agent.SetDestination(standingPostion);

		transform.LookAt(speaker.agent.transform);

		_line = Factory.Instance.GetTalkingLine();

		var lat = _line.GetComponent<LineAttachTo>();
		lat.targetA = transform;
		lat.targetB = speaker.agent.transform;

		var lr = _line.GetComponent<LineRenderer>();
		lr.material.color = Color.green;

		_changeCD = 0f;
	}

	public void StopRevSpeech()
	{
		_speaker = null;
		Factory.Instance.ReleaseaTalkingLine(ref _line);
		ChangeState(State.Wandering);
	}

	bool CanTalk()
	{
		return _state == State.Wandering && _talkingToOtherCD <= 0 && !DepsrateForFood();
	}

	public bool CanRevSpeech()
	{
		return (_state == State.Idling || _state == State.Wandering) && !DepsrateForFood();
	}


	void StopWork()
	{
		_worker.Job.Stop();
		hat.enabled = false;
		_workingCD = WORKING_COOLDOWN;
	}

	void StartWork()
	{
		_worker.Job.Start();

		_changeCD = _worker.Job.Length;

		hat.enabled = true;
		hat.material.color = _worker.Job.HatColor;
	}

	//Detect collisions between the GameObjects with Colliders attached
	void OnTriggerEnter(Collider other)
	{
		if(CanTalk() && other.gameObject.tag == "Person")
		{
			var otherPerson = other.gameObject.GetComponent<NormalPersonAI>();

			if (!otherPerson.CanTalk())
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

			hat.enabled = true;
			hat.material.color = Color.white;

			otherPerson.hat.enabled = true;
			otherPerson.hat.material.color = Color.white;

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
