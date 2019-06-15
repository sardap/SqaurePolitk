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
}
