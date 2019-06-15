using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wander : MonoBehaviour
{
	public float range;
	public float walkTime;
	public NavMeshAgent agent;

	Vector3 _target;
	float _timeLeft;

	void NewTarget()
	{
		_target = new Vector3
		(
			Random.Range(transform.position.x - range, transform.position.x + range), 
			1, 
			Random.Range(transform.position.z - range, transform.position.z + range)
		);
		_target.y = 1;

		agent.SetDestination(_target);

		_timeLeft = walkTime;
	}

	// Update is called once per frame
	void Update()
    {
		_timeLeft -= Time.deltaTime;

		if (_timeLeft < 0)
		{
			NewTarget();
		}
    }
}
