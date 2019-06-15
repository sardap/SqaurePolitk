using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomHeight : MonoBehaviour
{
	public float Min;
	public float Max;

	// Start is called before the first frame update
	void Start()
    {
		transform.localScale = new Vector3(transform.localScale.x, Random.Range(Min, Max), transform.localScale.z);
    }
}
