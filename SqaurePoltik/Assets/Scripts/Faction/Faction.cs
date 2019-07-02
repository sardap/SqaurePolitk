using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Faction
{
	const int MEMBER_COUNT_MULITPLIER = 5;

	List<FactionCom> _memebers = new List<FactionCom>();
	Color32 _factionColor;

	public FactionCom Leader { get; set; }

	public Color32 FactionColor { get { return _factionColor; } }

	public float Leaning
	{
		get
		{
			return Leader.beliefControler.TotalLeaning;
		}
	}

	public int MemberMultipler
	{
		get
		{
			return System.Math.Max(_memebers.Count / MEMBER_COUNT_MULITPLIER, 1);
		}
	}

	public Faction(float leaderLeaning)
	{
		_factionColor = new Color32()
		{
			a = byte.MaxValue,
			r = leaderLeaning < 0 ? Util.RandomByte(50, byte.MaxValue) : byte.MinValue,
			g = Util.RandomByte(),
			b = leaderLeaning > 0 ? Util.RandomByte(50, byte.MaxValue) : byte.MinValue,
		};
	}

	public void AddMember(FactionCom newMember)
	{
		_memebers.Add(newMember);
	}

	public void RemoveMember(FactionCom memeber)
	{
		_memebers.Remove(memeber);
	}

	public void Destroy()
	{
		_memebers.ToList().ForEach(i => RemoveMember(i));
		_memebers.Clear();
	}
}
