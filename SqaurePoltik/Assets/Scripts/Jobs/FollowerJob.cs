using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class FollowerJob : IJobAction
{
	enum State
	{
		Following,
		Fighting
	}

	bool _active;
	State _state;
	FactionCom _fightingTarget;
	GameObject _fightingLine;
	float _changeCD;
	bool _quit = false;

	public BeliefControler beliefControler;
	public Transform following;
	public NavMeshAgent agent;

	public FactionCom FactionCom { get; set; }

	public Camera Camera { get; set; }

	public GameObject _line;

	public Color32 HatColor
	{
		get
		{
			return FactionCom.Faction.FactionColor;
		}
	}

	public Vector3 WorkStationLocation
	{
		set
		{
			throw new System.NotImplementedException();
		}
	}

	public bool Walk
	{
		get
		{
			return true;
		}
	}

	public Worker Worker
	{
		get; set;
	}

	public float Length
	{
		get
		{
			if(!FactionCom.Faction.Leader.normalPersonAI.ReadyToBeFollwed)
			{
				return 0f;
			}

			return _state == State.Fighting ? float.MaxValue : 70f;
		}
	}

	public bool JobReady()
	{
		if (following == null || FactionCom.Faction.Fighting)
			return false;

		var otherAI = following.gameObject.GetComponent<NormalPersonAI>();

		return otherAI == null || otherAI.ReadyToBeFollwed;
	}

	public void Quit()
	{
		Stop();
		_quit = true;
	}

	public void Start()
	{
		agent.SetDestination(following.position);

		Debug.Assert(_line == null);

		_line = Factory.Instance.GetTalkingLine();

		var lat = _line.GetComponent<LineAttachTo>();
		lat.targetA = agent.transform;
		lat.targetB = following.transform;

		var lr = _line.GetComponent<LineRenderer>();
		lr.material.color = FactionCom.Faction.FactionColor;

		beliefControler.EmulateBeliefStep(following.GetComponent<BeliefControler>(), 10);

		_state = FactionCom.Faction.Fighting ? State.Fighting : State.Following;
	}

	public void Step()
	{
		Debug.Assert(!_quit);

		switch (_state)
		{
			case State.Following:
				float dist = Vector3.Distance(following.position, agent.transform.position);
				if (dist > 5)
				{
					agent.isStopped = false;
					agent.SetDestination(following.position);
				}

				if (agent.remainingDistance < 2)
				{
					agent.isStopped = true;
				}
				break;

			case State.Fighting:
				if(_fightingTarget == null || !_fightingTarget.normalPersonAI.Working)
				{
					ClearFightingLine();

					if (!FactionCom.Faction.Fighting)
					{
						StopFightMode();
						break;
					}

					Debug.Assert(FactionCom.normalPersonAI.Working);
					_fightingTarget = FactionCom.Faction.GetNextTarget(FactionCom);

					if (_fightingTarget == null)
					{
						StopFightMode();
						break;
					}

					var newLine = Factory.Instance.GetTalkingLine();

					var lat = newLine.GetComponent<LineAttachTo>();
					lat.targetA = FactionCom.transform;
					lat.targetB = _fightingTarget.transform;

					var lr = newLine.GetComponent<LineRenderer>();
					lr.material.color = Color.black;

					_fightingLine = newLine;

					_changeCD = Random.Range(1f, 4f);
				}

				_changeCD -= Time.deltaTime;

				if (Vector3.Distance(_fightingTarget.transform.position, FactionCom.transform.position) > 1.5f)
				{
					agent.SetDestination(_fightingTarget.transform.position);
				}
				else if (_changeCD < 0)
				{
					if (Random.Range(0, 100) > 80)
					{
						_fightingTarget.normalPersonAI.DieWhileWorking();

						ClearFightingLine();

						_fightingTarget = null;
					}
					else
					{
						var talkingText = beliefControler.LeftLeaning ? "optimatium caput stercore" : "purgamentum init populist";

						Helper.CreateTalkingText(Camera, HatColor, FactionCom.transform, talkingText);

						_changeCD = Random.Range(1f, 4f);
					}
				}

				break;
		}
	}

	public void StopFightMode()
	{
		ClearFightingLine();
		_state = State.Following;
	}

	public void FightMode()
	{
		_state = State.Fighting;
	}

	void ClearFightingLine()
	{
		if (_fightingLine != null)
		{
			Factory.Instance.ReleaseTalkingLine(ref _fightingLine);
		}
	}

	void ClearLines()
	{
		if (_fightingLine != null)
		{
			Factory.Instance.ReleaseTalkingLine(ref _fightingLine);
		}

		if(_line != null)
		{
			Factory.Instance.ReleaseTalkingLine(ref _line);
		}
	}

	public void Stop()
	{
		ClearLines();
	}
}
