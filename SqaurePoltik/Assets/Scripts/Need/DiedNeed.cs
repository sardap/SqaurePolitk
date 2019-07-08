using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class DiedNeed : INeed
{
	float _maxValue;
	float _value;

	public float MaxValue
	{
		get
		{
			return _maxValue;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public float MoodModifer
	{
		get
		{
			throw new NotSupportedException();
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
			throw new NotSupportedException();
		}
	}

	public float Tick
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public bool Fatal
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public DiedNeed(INeed need)
	{
		_value = need.Value;
		_maxValue = need.MaxValue;
	}
}
