using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Helper
{
	public static void CreateTalkingText(Color32 color, Transform transform)
	{
		Factory.Instance.GetBusyTalkingText(LatainSenteceCreator.Instance.GetSentence(), color, transform);
	}


	public static void CreateTalkingText(Camera camera, Color32 color, Transform transform)
	{
		if (OnCamrea(camera, transform))
		{
			CreateTalkingText(color, transform);
		}
	}

	public static bool OnCamrea(Camera camera, Transform transform)
	{
		var postion = camera.WorldToViewportPoint(transform.position);
		return postion.x > 0 && postion.x < 1 && postion.y > 0 && postion.y < 1;
	}

	public static int FindClosestIndex<T>(IList<T> toSerach, Transform finder) where T : Component
	{
		int min = -1;
		float minDist = Mathf.Infinity;
		Vector3 currentPos = finder.position;
		for (int i = 0; i < toSerach.Count; i++)
		{
			float dist = Vector3.Distance(toSerach[i].transform.position, currentPos);
			if (dist < minDist)
			{
				min = i;
				minDist = dist;
			}
		}

		return min;
	}


	public static T FindClosest<T>(IEnumerable<T> toSerach, Transform finder) where T : Component
	{
		Transform tMin = null;
		float minDist = Mathf.Infinity;
		Vector3 currentPos = finder.position;
		foreach (var com in toSerach)
		{
			if (!com.GetComponent<T>())
			{
				continue;
			}

			float dist = Vector3.Distance(com.transform.position, currentPos);
			if (dist < minDist)
			{
				tMin = com.transform;
				minDist = dist;
			}
		}

		if (tMin == null)
			return null;

		return tMin.GetComponent<T>();
	}

}
