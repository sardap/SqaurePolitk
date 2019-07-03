﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class NormalPersonAI : MonoBehaviour
{
	const float WORKING_COOLDOWN = 60f;
	const float SPEEACH_COOLDOWN = 20f;
	const float TALKING_COOLDOWN = 5f;
	const float FOOD_STARVING_THRESHHOLD = 0.4f;
	const float SLEEP_STARVING_THRESHHOLD = 0.1f;
	const float SPEECH_SOCIAL_BENFIT = SocialNeed.MAX_VALUE * 0.25f;
	const float SPEAKING_SOCIAL_BENFIT = SocialNeed.MAX_VALUE * 0.5f;
	const float SOCIAL_NEEDING_COOLDOWN = 20f;

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
		GoingToBed,
		Sleeping,
		Dying,
		SeekingSocial,
		Fighting
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

	static readonly State[] MovableStates = new State[] {State.Wandering, State.RevSpeach, State.FindingFood, State.GoingToBed, State.SeekingSocial};

	State _state;
	float _changeCD;
	float _talkingCD;
	float _talkingToOtherCD;
	float _workingCD;
	float _wanderingLeftCD;
	NormalPersonAI _talkingSubject;
	MaxJob _speaker;
	GameObject _line;
	Stack<NormalPersonAI> _listeners = new Stack<NormalPersonAI>();
	Worker _worker;
	Needs _needs;
	FactionCom _factionCom;
	Transform _socialSeekingTarget;
	bool _inCharge;

	MarketInfo _targetMarket { get; set; }

	Stack<State> _prevousStates;

	public Wander wander;
	public BeliefControler beliefControler;
	public NavMeshAgent agent;
	public new Camera camera;
	public MeshRenderer hat;

	public HouseBuilding Home { get; set; }

	public bool ReadyToBeFollwed
	{
		get
		{
			return _state != State.Sleeping && _state != State.GoingToBed;
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		_prevousStates = new Stack<State>();
		personList.Add(this);
		ChangeState(State.Wandering);
		_talkingToOtherCD = Random.Range(0, 120);
		_worker = GetComponent<Worker>();
		_needs = GetComponent<Needs>();
		_factionCom = GetComponent<FactionCom>();
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

		if (agent.enabled)
		{
			agent.isStopped = !(MovableStates.Any(i => _state == i) || _state == State.Working && _worker.Job.Walk);
		}

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

	bool DesperateForSocial(SocialNeed socialNeed)
	{
		return socialNeed.Value <= 0;
	}

	bool DesperateForSleep(SleepingNeed sleepingNeed)
	{
		return sleepingNeed.Value <= sleepingNeed.MaxValue * SLEEP_STARVING_THRESHHOLD;
	}

	bool DesperateForFood(FoodNeed foodNeed)
	{
		return foodNeed.Value <= foodNeed.MaxValue * FOOD_STARVING_THRESHHOLD;
	}

	bool WantsFood(FoodNeed foodNeed)
	{
		var num = 100;

		if (foodNeed.Value <= foodNeed.MaxValue * 0.5)
		{
			num = 50;
		}
		else if (foodNeed.Value <= foodNeed.MaxValue * 0.7)
		{
			num = 80;
		}

		return Random.Range(0, 100) >= num;
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

				bool eatChance = false;
				bool sleepAddres = false;
				bool socialAddress = false;

				if (DesperateForFood(_needs.FoodNeed))
				{
					eatChance = true;
				}
				else if(DesperateForSleep(_needs.SleepingNeed))
				{
					sleepAddres = true;
				}
				else if(DesperateForSocial(_needs.SocialNeed))
				{
					socialAddress = true;
				}

				if (_changeCD <= 0 || eatChance || sleepAddres || socialAddress)
				{
					if (_needs.Die)
					{
						ChangeState(State.Dying);
						break;
					}

					State newState = State.Idling;

					if (eatChance || WantsFood(_needs.FoodNeed))
					{
						if (StartFindFood())
						{
							newState = State.FindingFood;
						}
					}
					else if (sleepAddres)
					{
						GoToBed();
						newState = State.GoingToBed;
					}
					else if (socialAddress)
					{
						FindSocialInteraction();
						_changeCD = SOCIAL_NEEDING_COOLDOWN;
						newState = State.SeekingSocial;
					}
					else if(_workingCD <= 0)
					{
						CheckKeepJob();

						if (_worker.CurrentState == Worker.State.Jobless)
						{
							FindJob();
						}

						if (_worker.CurrentState == Worker.State.HasJob)
						{
							if (_worker.Job.JobReady())
							{
								StartWork();
								newState = State.Working;
							}
							else
							{
								newState = State.Wandering;
							}
						}
					}

					ChangeState(newState);
				}
				break;

			case State.SeekingSocial:

				if (ReachedTarget(0.2f))
				{
					FindSocialInteraction();
				}

				if (_changeCD < 0 && _worker.CurrentState == Worker.State.Jobless)
				{
					GiveFollowerJob();
					_changeCD = float.MaxValue;
					StartWork();
					ChangeState(State.Working);
				}
				else if (_needs.SocialNeed.Value >= SocialNeed.MAX_VALUE * 0.8f)
				{
					StopSocialSeeking();
					ChangeState(State.Wandering);
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
					_needs.SocialNeed.Value += SPEECH_SOCIAL_BENFIT;
					ChangeState(State.FinshedTalking);
				}

				break;

			case State.FinshedTalking:
				if(_talkingSubject._state == State.FinshedTalking || _talkingSubject._talkingSubject != this)
				{
					if(_line != null)
					{
						Debug.LogFormat("Frame:{2} {1} RETURNING {0} ", _line.name, gameObject.name, Time.frameCount);
						Factory.Instance.ReleaseTalkingLine(ref _line);
					}

					_talkingToOtherCD = TALKING_COOLDOWN;

					hat.enabled = false;
					hat.material.color = Color.white;

					_talkingSubject = null;
					ChangeState(State.Wandering);
					_changeCD = _wanderingLeftCD;
				}
				break;

			case State.RevSpeach:
				if (_changeCD <= 0)
				{
					beliefControler.ReceiveSpeechBelief(_speaker.beliefControler, _speaker.FactionCom.Faction);
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
				if (ReachedTarget(0.5f))
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

			case State.GoingToBed:
				if (!agent.pathPending && agent.remainingDistance <= 0.5)
				{
					StartSleeping();
					ChangeState(State.Sleeping);
				}
				break;

			case State.Sleeping:
				_talkingCD -= Time.deltaTime;

				_needs.SleepingNeed.Value += 6 * Time.deltaTime;

				if(_talkingCD <= 0)
				{
					Helper.CreateTalkingText(camera, new Color32 { a = 50 }, Home.SleepTalkingPostion(), "zzzzz".Substring(Random.Range(1, 5)));
					_talkingCD = 5f;
				}

				if (_needs.SleepingNeed.Value >= _needs.SleepingNeed.MaxValue)
				{
					WakeUp();
					ChangeState(State.Wandering);
				}
				break;

			case State.Dying:
				Die();
				break;

			case State.Fighting:

				if(_changeCD < 0)
				{
					if(Random.Range(0, 100) > 80)
					{
						_talkingSubject.ChangeState(State.Dying);
						_talkingSubject.StopFighting();
						StopFighting();
						ChangeState(State.Wandering);
					}
					else
					{
						var talkingText = beliefControler.LeftLeaning ? "optimatium caput stercore" : "purgamentum init populist";

						Helper.CreateTalkingText(camera, Color.black, transform, talkingText);
					}

					_changeCD = Random.Range(1f, 4f);
				}

				break;
		}
    }

	void StopFighting()
	{
		if(_line != null)
		{
			Factory.Instance.ReleaseTalkingLine(ref _line);
		}

		hat.enabled = false;

		_talkingSubject = null;
	}

	void CheckKeepJob()
	{
		if (
			_worker.Job != null && 
			(
				(beliefControler.PassionLevel == BeliefControler.EPassionLevel.High && typeof(FollowerJob) != _worker.Job.GetType()) ||
				(beliefControler.PassionLevel == BeliefControler.EPassionLevel.Max && typeof(MaxJob) != _worker.Job.GetType())
			)
		)
		{
			_worker.QuitJob();
		}
	}

	void FindSocialInteraction()
	{
		hat.enabled = true;
		hat.material.color = new Color32() { r = 255, g = 69, b = 69, a = byte.MaxValue };

		var result = PeopleInfo.Instance.FindClosestSocialInteraction(beliefControler);

		Vector3 target;

		if (result != null)
		{
			target = result.transform.position;
		}
		else
		{
			target = new Vector3
			(
				Random.Range(transform.position.x - 5, transform.position.x + 5),
				1,
				Random.Range(transform.position.z - 5, transform.position.z + 5)
			);
			target.y = 1;

			target = beliefControler.transform.position;
		}

		agent.SetDestination(target);
	}

	void Die()
	{
		Debug.LogFormat("Frame:{1} {0} DIED LAST 5 STATES: {2}", gameObject.name, Time.frameCount, LastFiveStates(_prevousStates));

		if(_worker.CurrentState == Worker.State.HasJob)
		{
			_worker.QuitJob();
		}

		var bloodStain = Factory.Instance.GetBloodStain();

		bloodStain.transform.position = new Vector3() { x = transform.position.x, y = 1, z = transform.position.z };

		Destroy(gameObject);
	}
	void GoToBed()
	{
		agent.SetDestination(Home.door.position);
		hat.enabled = true;
		hat.material.color = new Color32() { a = byte.MaxValue, r = 25, g = 25, b = 25 };
	}

	void StartSleeping()
	{
		GetComponentsInChildren<MeshRenderer>().ToList().ForEach(i => i.enabled = false);
		GetComponentsInChildren<Collider>().ToList().ForEach(i => i.enabled = false);
		GetComponentsInChildren<Rigidbody>().ToList().ForEach(i => i.detectCollisions = false);
		GetComponentsInChildren<NavMeshAgent>().ToList().ForEach(i => i.enabled = false);
		_talkingCD = 5f;
		Home.SleepingCount++;
	}

	void WakeUp()
	{
		GetComponentsInChildren<MeshRenderer>().ToList().ForEach(i => i.enabled = true);
		GetComponentsInChildren<Collider>().ToList().ForEach(i => i.enabled = true);
		GetComponentsInChildren<Rigidbody>().ToList().ForEach(i => i.detectCollisions = true);
		GetComponentsInChildren<NavMeshAgent>().ToList().ForEach(i => i.enabled = true);
		hat.enabled = false;
		Home.SleepingCount--;
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

	void GiveFollowerJob()
	{
		var newJob = new FollowerJob()
		{
			beliefControler = beliefControler,
			agent = agent,
			FactionCom = _factionCom
		};

		var cloest = PeopleInfo.Instance.FindClosetToFollow(newJob, beliefControler, _factionCom);

		if (cloest == null)
		{
			return;
		}

		newJob.following = cloest.transform;

		_worker.GiveJob(newJob);

	}

	void FindJob()
	{
		if (beliefControler.PassionLevel == BeliefControler.EPassionLevel.Max)
		{

			var newJob = new MaxJob()
			{
				agent = agent,
				beliefControler = beliefControler,
				Camera = camera,
				FactionCom = _factionCom
			};

			_worker.GiveJob(newJob);
		}
		else if(beliefControler.PassionLevel == BeliefControler.EPassionLevel.High)
		{
			GiveFollowerJob();
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

	bool ReachedTarget(float leanacy = 0.1f)
	{
		return !agent.pathPending && agent.remainingDistance <= leanacy;
	}

	void StopSocialSeeking()
	{
		hat.enabled = false;
		PeopleInfo.Instance.RemoveSocialSeeker(beliefControler);
	}

	public void RevSpeech(MaxJob speaker, Vector3 standingPostion)
	{
		_speaker = speaker;

		if(_state == State.SeekingSocial)
		{
			StopSocialSeeking();
		}

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
		_needs.SocialNeed.Value += SPEECH_SOCIAL_BENFIT;
		Factory.Instance.ReleaseTalkingLine(ref _line);
		ChangeState(State.Wandering);
	}

	bool WantsToFight()
	{
		if(_state != State.Wandering && _state != State.Idling)
		{
			return false;
		}

		var num = Random.Range(0, 100);

		if (_needs.MoodValue == Needs.MoodLevel.Low && num >= 50)
		{
			return true;
		}
		else if(_needs.MoodValue == Needs.MoodLevel.Bottom)
		{
			return true;
		}

		return false;
	}

	bool CanTalk()
	{
		return _state == State.SeekingSocial || ((_state == State.Wandering || _state == State.Idling) && _talkingToOtherCD <= 0 && !DesperateForFood(_needs.FoodNeed));
	}

	public bool CanRevSpeech()
	{
		return _state == State.SeekingSocial || ((_state == State.Idling || _state == State.Wandering) && !DesperateForFood(_needs.FoodNeed));
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
		if(other.gameObject.tag != "Person")
		{
			return;
		}

		var otherPerson = other.gameObject.GetComponent<NormalPersonAI>();

		if (WantsToFight() && otherPerson.WantsToFight())
		{
			if(beliefControler.LeftLeaning == otherPerson.beliefControler.LeftLeaning)
			{
				return;
			}

			hat.enabled = true;
			hat.material.color = Color.black;

			otherPerson.hat.enabled = true;
			otherPerson.hat.material.color = Color.black;

			_wanderingLeftCD = _changeCD;
			otherPerson._wanderingLeftCD = otherPerson._changeCD;

			_talkingSubject = otherPerson;
			otherPerson._talkingSubject = this;

			otherPerson._changeCD = Random.Range(1f, 4f);
			_changeCD = Random.Range(1f, 4f);

			_line = Factory.Instance.GetTalkingLine();

			var lat = _line.GetComponent<LineAttachTo>();
			lat.targetA = transform;
			lat.targetB = otherPerson.transform;

			var lr = _line.GetComponent<LineRenderer>();
			lr.material.color = Color.black;

			otherPerson.ChangeState(State.Fighting);
			ChangeState(State.Fighting);
		}
		else if(CanTalk())
		{
			if (!otherPerson.CanTalk())
			{
				return;
			}

			var subject = beliefControler.CommonBelief(otherPerson.beliefControler);

			if(subject == null)
			{
				return;
			}

			if(_state == State.SeekingSocial)
			{
				StopSocialSeeking();
			}

			if (otherPerson._state == State.SeekingSocial)
			{
				otherPerson.StopSocialSeeking();
			}


			_wanderingLeftCD = _changeCD;
			otherPerson._wanderingLeftCD = otherPerson._changeCD;

			ChangeState(State.Talking);
			otherPerson.ChangeState(State.Talking);

			hat.enabled = true;
			hat.material.color = Color.white;

			otherPerson.hat.enabled = true;
			otherPerson.hat.material.color = Color.white;

			_talkingSubject = otherPerson;
			otherPerson._talkingSubject = this;

			for(int i = 0; i < 5; i++)
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
	}
}
