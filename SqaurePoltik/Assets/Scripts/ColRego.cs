using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColRego : MonoBehaviour
{
	public HashSet<GameObject> inside = new HashSet<GameObject>();

	void OnTriggerEnter(Collider other)
	{
		inside.Add(other.gameObject);
	}
	 
	void OnTriggerExit(Collider other)
	{
		inside.Remove(other.gameObject);
	}
}
