using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FoodNeed : INeed
{
	const float MAX_VALUE = 100f;

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
			return 0.5f;
		}
	}

	public bool Fatal
	{
		get
		{
			return true;
		}
	}

	public FoodNeed()
	{
		Value = Util.RandomFloat(MAX_VALUE * 0.5f, MAX_VALUE);
	}
}
