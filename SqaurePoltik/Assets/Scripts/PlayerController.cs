using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
	public Camera cam;

	public NavMeshAgent agent;

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

		var d = Input.GetAxis("Mouse ScrollWheel");
		if (d > 0f || d < 0f)
		{
			d = -d * 20;

			if(cam.orthographicSize + d > 0)
				cam.orthographicSize += d;
		}
	}
}
