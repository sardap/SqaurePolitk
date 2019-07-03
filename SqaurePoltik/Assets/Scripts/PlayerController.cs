using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
	public Camera cam;
	public TextMeshProUGUI timeScale;

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

		if (Input.GetKeyDown(KeyCode.Equals))
		{
			Time.timeScale++;
		}
		else if (Input.GetKeyDown(KeyCode.Minus) && Time.timeScale > 0)
		{
			Time.timeScale--;
		}

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
