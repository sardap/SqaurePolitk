using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillSelfInX : MonoBehaviour
{
	public float timeToDie;

	float _cd;

	void OnEnable()
	{
		Start();
	}

	// Start is called before the first frame update
	void Start()
    {
		_cd = 0;
	}

    // Update is called once per frame
    void Update()
    {
		_cd += Time.deltaTime;

		if (_cd >= timeToDie)
		{
			Factory.Instance.ReleaseBusyTalkingText(gameObject);
		}
    }
}
