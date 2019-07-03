using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SleepingNeed : INeed
{
	const float MAX_VALUE = 180f;

	public float MaxValue
	{
		get
		{
			return MAX_VALUE;
		}
	}

	public float Value
	{
		get; set;
	}

	public float Tick
	{
		get
		{
			return 1f;
		}
	}

	public bool Fatal
	{
		get
		{
			return false;
		}
	}

	public float MoodModifer
	{
		get
		{
			return 0.5f;
		}
	}

	public SleepingNeed()
	{
		Value = Util.RandomFloat(0, MAX_VALUE);
	}
}
