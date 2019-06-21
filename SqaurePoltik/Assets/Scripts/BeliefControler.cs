using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeliefControler : MonoBehaviour
{
	const float MAX_LEFT = -10;
	const float MAX_RIGHT = 10;
	const float MAX_CONVINCING = 100;
	const int FOLLWERS_NEEDED_FOR_RIOT = 5;


	public enum EPassionLevel
	{
		Low,
		Med,
		High,
		Max
	}

	interface IBelief
	{
		// Postive Support Neg Reject
		float Value { get; set; }
	}

	class GenBelief : IBelief
	{
		float _value;

		public float Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (value > MAX_RIGHT)
				{
					value = MAX_RIGHT;
				}
				else if (value < MAX_LEFT)
				{
					value = MAX_LEFT;
				}

				float updateValue = _value - value;

				PeopleInfo.Instance.RegisterChange(updateValue);

				_value = value;
			}
		}
		public string Name { get; set; }

		public GenBelief(string aName, float personalLeaning, float modifer)
		{
			Name = aName;
			Value = personalLeaning + Random.Range(MAX_LEFT * modifer, MAX_RIGHT * modifer);
		}
	}

	public class ExchangeBeleifResult
	{
		public string subject;
		public float changeCD;
	}

	static List<string> _beliefsNames = null;

	static string GetBeleifName()
	{
		if (_beliefsNames == null)
		{
			var nameSet = new HashSet<string>();

			while(nameSet.Count < (int)MAX_CONVINCING)
			{
				nameSet.Add(Util.RandomString(10));
			}

			_beliefsNames = new List<string>(nameSet);
		}

		return Util.RandomElement(_beliefsNames);
	}

	public MeshRenderer meshRenderer;
	public float convincingVal;
	public List<FollowerJob> follwers { get; set; }

	Dictionary<string, IBelief> _beliefList = new Dictionary<string, IBelief>();
	MaterialPropertyBlock _propBlock;
	Color32 _currentColor;
	EPassionLevel _passionLevel;
	string _speechBelief;
	float _personalLeaning;
	float _modifer;
	float _stubones;
	bool _registred;

	IBelief SpeechBelief
	{
		get
		{
			return _beliefList[_speechBelief];
		}
	}

	public float TotalLeaning
	{
		get
		{
			return _beliefList.Values.Sum(i => i.Value);
		}
	}

	public Color32 BeliefColor
	{
		get
		{
			return _currentColor;
		}
	}

	public EPassionLevel PassionLevel
	{
		get
		{
			return _passionLevel;
		}
	}

	void ReactToBeliefChange()
	{
		UpdatePassionLevel();
		UpdateColor(TotalLeaning);
	}

	void UpdatePassionLevel()
	{
		EPassionLevel newState;

		if (convincingVal < MAX_CONVINCING * 0.3)
		{
			newState = EPassionLevel.Low;
		}
		else if (convincingVal < MAX_CONVINCING * 0.5)
		{
			newState = EPassionLevel.Med;
		}
		else if (convincingVal < MAX_CONVINCING * 0.9)
		{
			newState = EPassionLevel.High;
		}
		else
		{
			newState = EPassionLevel.Max;
			if (!_registred)
			{
				PeopleInfo.Instance.RegstierMax(this);
				_registred = true;
			}
		}

		if(newState == EPassionLevel.Max && follwers.Count >= FOLLWERS_NEEDED_FOR_RIOT)
		{

		}

		_passionLevel = newState;

	}

	// Start is called before the first frame update
	void Start()
	{
		follwers = new List<FollowerJob>();
		_propBlock = new MaterialPropertyBlock();

		var extremeCata = Random.Range(0, 10);
		_modifer = 0f;

		if (extremeCata < 5)
		{
			_modifer = 0.25f;
		}
		else if (extremeCata < 7)
		{
			_modifer = 0.5f;
		}
		else if (extremeCata == 8)
		{
			_modifer = 0.75f;
		}
		else
		{
			_modifer = 1f;
		}

		convincingVal = Random.Range(0, MAX_CONVINCING * _modifer);
		_stubones = Random.Range(MAX_CONVINCING / 2, MAX_CONVINCING * _modifer);
		_personalLeaning = Random.Range(MAX_LEFT * _modifer, MAX_RIGHT * _modifer);

		PeopleInfo.Instance.MaxTotalLeaning += MAX_RIGHT;

		while (_beliefList.Count < convincingVal)
		{
			string nextName = GetBeleifName();
			if (!_beliefList.ContainsKey(nextName))
				_beliefList.Add(nextName, new GenBelief(nextName, _personalLeaning, _modifer));
		}

		_registred = false;

		ReactToBeliefChange();
	}

	public void UpdateColor(float totalLeaning)
	{
		var leaning = totalLeaning;

		var maxLeaning = convincingVal * System.Math.Abs(MAX_LEFT);
	
		var leanVal = (System.Math.Abs(leaning) / maxLeaning) * 256;

		byte r = 0;
		byte b = 0;

		if (leaning > 0)
		{
			b = (byte)System.Math.Round(leanVal);
		}
		else if(leaning < 0)
		{
			r = (byte)System.Math.Round(leanVal);
		}

		meshRenderer.GetPropertyBlock(_propBlock);
		_currentColor = new Color32() { r = r, g  = 0, b = b, a = byte.MaxValue };
		_propBlock.SetColor("_Color", _currentColor);
		meshRenderer.SetPropertyBlock(_propBlock);
	}

	public void StartSpeech()
	{
		_speechBelief = Util.RandomElement(_beliefList.Keys);
	}

	public void StopSpeech()
	{
		_speechBelief = null;
	}	

	public string CommonBelief(BeliefControler other)
	{
		var beliefIntersect = _beliefList.Keys.Intersect(other._beliefList.Keys);

		if (beliefIntersect.Count() <= 0)
		{
			return null;
		}

		var subject = Util.RandomElement(beliefIntersect);

		return subject;
	}

	public void ReceiveSpeechBelief(BeliefControler other)
	{
		if (!_beliefList.ContainsKey(other._speechBelief))
		{
			_beliefList.Add(other._speechBelief, new GenBelief(other._speechBelief, _personalLeaning, _modifer));
		}

		var otherValue = other.SpeechBelief.Value;
		otherValue *= other.convincingVal / MAX_CONVINCING;
		otherValue *= _stubones / MAX_CONVINCING;

		_beliefList[other._speechBelief].Value += otherValue;

		UpdatePassionLevel();
	}

	public void TalkTo(BeliefControler other, string subject)
	{
		var speakers = new List<BeliefControler>() { this, other }.OrderByDescending(i => Util.RandomFloat(0, i.convincingVal)).ToArray();

		var otherValue = speakers[1]._beliefList[subject].Value;
		otherValue *= speakers[1].convincingVal / MAX_CONVINCING;
		otherValue *= speakers[0]._stubones / MAX_CONVINCING;

		speakers[0]._beliefList[subject].Value += otherValue;

		speakers[0].ReactToBeliefChange();
	}

	void OnDestroy()
	{
		var toDestory = follwers.ToList();

		foreach(var follower in toDestory)
		{
			follower.Worker.QuitJob();
		}

		follwers.Clear();
	}
}
