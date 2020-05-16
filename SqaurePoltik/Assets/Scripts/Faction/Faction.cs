using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Faction
{
	const int MEMBER_COUNT_MULITPLIER = 5;

	List<FactionCom> _members = new List<FactionCom>();
	Color32 _factionColor;
	bool _fighting;
	Faction _fightingWith;

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
			return System.Math.Max(_members.Count / MEMBER_COUNT_MULITPLIER, 1);
		}
	}

	public bool Fighting
	{
		get
		{
			return _fighting;
		}
	}

	public string Name
	{
		get;
		set;
	}

	public bool LeaderReadyToFight
	{
		get
		{
			return Leader.normalPersonAI.LeaderReadyToFight;
		}
	}

	List<FactionCom> MembersReadyToFight
	{
		get
		{
			var result = _members.Where(i => CanFactionFight(i)).ToList();

			result.Add(Leader);

			return result;
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
		Debug.Assert(Leader.factionBubble.activeSelf);

		_members.Add(newMember);
		Leader.factionBubble.GetComponent<SphereCollider>().radius = MemberMultipler * 5;
	}

	public void RemoveMember(FactionCom memeber, bool fromOnDestory = false)
	{
		_members.Remove(memeber);

		if(Leader != null)
			Leader.factionBubble.GetComponent<SphereCollider>().radius = MemberMultipler * 5;
	}

	public void Destroy()
	{
		_members.ToList().ForEach(i => i.FollowerJob.Worker.QuitJob());
		_members.Clear();
		Leader = null;
	}

	bool CanFactionFight(FactionCom member)
	{
		return member.normalPersonAI.Working;
	}

	public void StartFight(Faction other)
	{
		Debug.Assert(!Fighting);

		if (!LeaderReadyToFight || !other.LeaderReadyToFight)
		{
			return;
		}

		var a = other.MembersReadyToFight.ToList();
		var b = MembersReadyToFight.ToList();

		if (a.Count <= 1 || b.Count <= 1)
		{
			return;
		}

		_fightingWith = other;
		other._fightingWith = this;

		_fighting = true;
		other._fighting = true;

		NotifyMemebers(a);
		NotifyMemebers(b);

		return;
	}

	void NotifyMemebers(IEnumerable<FactionCom> members)
	{
		foreach(var i in members)
		{
			if(i.FollowerJob != null)
			{
				i.FollowerJob.FightMode();
			}
			else
			{
				((MaxJob)i.Faction.Leader.GetComponent<Worker>().Job).StartFactionFight();
			}

		}
	}

	public FactionCom GetNextTarget(FactionCom member)
	{
		Debug.Assert(_fightingWith.Fighting && Fighting);

		var fightingMembers = _fightingWith.MembersReadyToFight;

		var cloeset = Helper.FindClosest(fightingMembers, member.transform);

		if(cloeset == null)
		{
			_fightingWith._fighting = false;
			_fightingWith._fightingWith = null;

			_fighting = false;
			_fightingWith = null;

			return null;
		}

		return cloeset;
	}
}
