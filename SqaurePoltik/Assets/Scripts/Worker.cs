using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : MonoBehaviour
{
	public enum State
	{
		HasJob,
		Jobless,
		DoesntWantJob
	}

	private IJobAction _job;

	public IJobAction Job { get { return _job; } }

	public State CurrentState
	{
		get; set;
	}

	public void GiveJob(IJobAction aJob)
	{
		Debug.Assert(aJob != null);

		_job = aJob;
		Job.Worker = this;
		CurrentState = State.HasJob;
	}

	public void QuitJob()
	{
		Job.Quit();
		_job = null;
		CurrentState = State.Jobless;
	}

	void Start()
	{
		CurrentState = State.Jobless;
	}
}
