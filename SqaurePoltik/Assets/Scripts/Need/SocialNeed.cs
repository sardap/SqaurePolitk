using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SocialNeed : INeed
{
	public const float MAX_VALUE = 4f * 60f;

	float _value;

	public float MaxValue
	{
		get
		{
			return MAX_VALUE;
		}
	}

	public float Value
	{
		get
		{
			return _value;
		}

		set
		{
			if(value >= MAX_VALUE)
			{
				value = MAX_VALUE;
			}

			_value = value;
		}
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
			return 1f;
		}
	}

	public SocialNeed()
	{
		Value = Util.RandomFloat(0, MAX_VALUE);
	}
}
