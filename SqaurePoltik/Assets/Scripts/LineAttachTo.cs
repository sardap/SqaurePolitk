using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAttachTo : MonoBehaviour
{
	public Transform targetA;
	public Transform targetB;
	public LineRenderer lineRenderer;
	public float lineHeight;

	Vector3 _tAPos;
	Vector3 _tBPos;

	bool CheckVecEqual(Vector3 a, Vector3 b)
	{
		return a.x == b.x && a.z == b.z;
	}

	public void UpdatePos(int index, Transform trans)
	{
		lineRenderer.SetPosition(index, new Vector3(trans.position.x, lineHeight, trans.position.z));
	}

	// Update is called once per frame
	void Update()
    {
		if(targetA != null && !CheckVecEqual(targetA.position, _tAPos))
		{
			UpdatePos(0, targetA);
			_tAPos = targetA.position;
		}

		if (targetB != null && !CheckVecEqual(targetB.position, _tBPos))
		{
			UpdatePos(1, targetB);
			_tBPos = targetB.position;
		}
	}
}
