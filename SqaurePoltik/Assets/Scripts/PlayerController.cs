using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
	public Camera cam;
	public TextMeshProUGUI timeScale;
	public FollowTrans followTrans;

	public NavMeshAgent agent;

	float _rotationSpeed = 30.0f;
	float _camreaMoveIncrement = 10f;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				agent.SetDestination(hit.point);
			}
		}

		if (Input.GetKeyDown(KeyCode.Equals))
		{
			if(Time.timeScale * 2 < 100)
			{
				Time.timeScale *= 2f;
			}
		}

		if (Input.GetKeyDown(KeyCode.Minus))
		{
			Time.timeScale /= 2f;
		}

		if (Input.GetKeyDown(KeyCode.F))
		{
			followTrans.enabled = !followTrans.enabled;
		}

		var camMoveVec = new Vector3();

		var camreaMoveIncrement = _camreaMoveIncrement;

		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			camreaMoveIncrement *= 2f;
		}

		if (Input.GetKey(KeyCode.W))
		{
			camMoveVec.z = camreaMoveIncrement * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.A))
		{
			camMoveVec.x = -camreaMoveIncrement * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.S))
		{
			camMoveVec.z = -camreaMoveIncrement * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.D))
		{
			camMoveVec.x = camreaMoveIncrement * Time.deltaTime;
		}

		cam.transform.position += camMoveVec;

		timeScale.text = Time.timeScale + "x";

		var d = Input.GetAxis("Mouse ScrollWheel");
		if (d > 0f || d < 0f)
		{
			d = -d * 20;

			if(cam.orthographicSize + d > 0)
				cam.orthographicSize += d;
		}
	}
}
