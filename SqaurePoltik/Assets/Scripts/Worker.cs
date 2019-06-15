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

	public IJobAction job;

	public State CurrentState
	{
		get; set;
	}

	public void GiveJob(IJobAction aJob)
	{
		job = aJob;
		CurrentState = State.HasJob;
	}

	void Start()
	{
		CurrentState = State.Jobless;
	}
}
