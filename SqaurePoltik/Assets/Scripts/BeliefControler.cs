using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeliefControler : MonoBehaviour
{
	public const float MAX_RIGHT = 10;
	const float MAX_LEFT = -MAX_RIGHT;
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
				if(value > MAX_RIGHT)
				{
					value = MAX_RIGHT;
				}
				else if(value < MAX_LEFT)
				{
					value = MAX_LEFT;
				}

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

	public static int BeliefsCount
	{
		get
		{
			return _beliefsNames.Count;
		}
	}


	public MeshRenderer meshRenderer;
	public float convincingVal;

	Dictionary<string, IBelief> _beliefs = new Dictionary<string, IBelief>();
	MaterialPropertyBlock _propBlock;
	Color32 _currentColor;
	EPassionLevel _passionLevel;
	string _speechBelief { get; set; }
	float _personalLeaning;
	float _modifer;
	float _stubones;
	bool _registred;

	IBelief SpeechBelief
	{
		get
		{
			return _beliefs[_speechBelief];
		}
	}

	public float TotalLeaning
	{
		get
		{
			return _beliefs.Values.Sum(i => i.Value);
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

	public bool LeftLeaning
	{
		get
		{
			return TotalLeaning < 0;
		}
	}

	void ReactToBeliefChange()
	{
		var totalLeaning = TotalLeaning;
		UpdatePassionLevel(totalLeaning);
		UpdateColor(totalLeaning);
	}

	void UpdatePassionLevel(float totalLeaning)
	{
		if (_passionLevel == EPassionLevel.Max)
			return;

		var totalLeaningAbs = System.Math.Abs(totalLeaning);

		var maxBelief = MAX_RIGHT * _beliefs.Count;

		EPassionLevel newState;

		if (totalLeaningAbs < maxBelief * 0.2)
		{
			newState = EPassionLevel.Low;
		}
		else if (totalLeaningAbs < maxBelief * 0.4)
		{
			newState = EPassionLevel.Med;
		}
		else if (totalLeaningAbs < maxBelief * 0.6 || convincingVal < MAX_CONVINCING * 0.6)
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

		_passionLevel = newState;

	}

	// Start is called before the first frame update
	void Start()
	{
		_propBlock = new MaterialPropertyBlock();

		var extremeCata = Random.Range(0, 10);
		_modifer = 0f;

		if (extremeCata < 5)
		{
			_modifer = 0.25f;
		}
		else if (extremeCata < 7)
		{
			_modifer = 0.75f;
		}
		else if (extremeCata == 8)
		{
			_modifer = 0.9f;
		}
		else
		{
			_modifer = 1f;
		}

		convincingVal = Random.Range(0, MAX_CONVINCING * _modifer);
		_stubones = Random.Range(0.5f * (1f - _modifer), 0.5f);

		if (convincingVal < MAX_CONVINCING * 0.5)
		{
			_personalLeaning = Random.Range(MAX_LEFT * 0.25f, MAX_RIGHT * 0.25f);
		}
		else if(convincingVal < MAX_CONVINCING * 0.7)
		{
			_personalLeaning = Random.Range(MAX_LEFT * 0.5f, MAX_RIGHT * 0.5f);
		}
		else if (convincingVal < MAX_CONVINCING * 0.95)
		{
			var leaning = Random.Range(0, MAX_RIGHT);

			_personalLeaning = Util.RandomBool() ? -leaning : leaning;
		}
		else
		{
			var leaning = Random.Range(MAX_RIGHT * 0.25f, MAX_RIGHT);

			_personalLeaning = Util.RandomBool() ? -leaning : leaning;
		}

		PeopleInfo.Instance.RegsiterPerson(this);

		while (_beliefs.Count < convincingVal)
		{
			string nextName = GetBeleifName();
			if (!_beliefs.ContainsKey(nextName))
				_beliefs.Add(nextName, new GenBelief(nextName, _personalLeaning, _modifer));
		}

		_registred = false;

		ReactToBeliefChange();
	}

	public void UpdateColor(float totalLeaning)
	{
		var leaning = totalLeaning;

		var maxLeaning = _beliefs.Count * System.Math.Abs(MAX_LEFT);
	
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
		_speechBelief = Util.RandomElement(_beliefs.Keys);
	}

	public void StopSpeech()
	{
		_speechBelief = null;
	}	

	public string CommonBelief(BeliefControler other)
	{
		var beliefIntersect = _beliefs.Keys.Intersect(other._beliefs.Keys);

		if (beliefIntersect.Count() <= 0)
		{
			return null;
		}

		beliefIntersect = beliefIntersect.OrderByDescending(i => System.Math.Max(_beliefs[i].Value, other._beliefs[i].Value)).ToList();

		var subject = beliefIntersect.First();

		return subject;
	}

	public void ReceiveSpeechBelief(BeliefControler other, Faction otherFaction)
	{
		if (!_beliefs.ContainsKey(other._speechBelief))
		{
			_beliefs.Add(other._speechBelief, new GenBelief(other._speechBelief, _personalLeaning, _modifer));
		}

		var otherValue = other.SpeechBelief.Value;
		otherValue *= other.convincingVal / MAX_CONVINCING;
		otherValue *= otherFaction.MemberMultipler;
		otherValue *= _stubones;

		_beliefs[other._speechBelief].Value += otherValue;

		ReactToBeliefChange();
	}

	public void TalkTo(BeliefControler other, string subject)
	{
		var speakers = new List<BeliefControler>() { this, other }.OrderByDescending(i => Util.RandomFloat(0, i.convincingVal)).ToArray();

		var otherValue = speakers[1]._beliefs[subject].Value;
		otherValue *= speakers[1].convincingVal / MAX_CONVINCING;

		speakers[0]._beliefs[subject].Value += otherValue;

		speakers[0].ReactToBeliefChange();
	}

	public void EmulateBeliefStep(BeliefControler other, int n)
	{
		for(int i = 0; i < n; i++)
		{
			var key = Util.RandomElement(other._beliefs).Key;

			if (!_beliefs.ContainsKey(key))
			{
				var newBelief = new GenBelief(key, 0, 0);
				// DO NOT COMIBNE
				newBelief.Value = other._beliefs[key].Value * Util.RandomFloat(0.3f, 0.5f);
				_beliefs.Add(newBelief.Name, newBelief);
			}

			var diff = System.Math.Abs(other._beliefs[key].Value - _beliefs[key].Value);

			float increment = 0f;

			increment = diff * Util.RandomFloat(0.5f, 1f);

			if (other._beliefs[key].Value < _beliefs[key].Value)
			{
				increment = -increment;
			}

			_beliefs[key].Value += increment;
		}
	}

	void OnDestroy()
	{
		if(_passionLevel == EPassionLevel.Max && _registred)
			PeopleInfo.Instance.UnregsterMax(this);

		PeopleInfo.Instance.UnregsterPerson(this);
	}
}
