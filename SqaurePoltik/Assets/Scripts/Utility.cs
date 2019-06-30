using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Util
{
	public static System.Random SharpRandom = new System.Random();

	public static float RandomFloat(float min = float.MinValue, float max = float.MaxValue)
	{
		return (float)SharpRandom.NextDouble() * (max - min) + min;
	}

	public static byte RandomByte(byte min = byte.MinValue, byte max = byte.MaxValue)
	{
		return (byte)SharpRandom.Next(min, max);
	}

	public static bool RandomBool()
	{
		return SharpRandom.Next(0, 2) == 1;
	}

	public static string RandomString(int n)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, n)
		  .Select(s => s[SharpRandom.Next(s.Length)]).ToArray());
	}

	public static T RandomElement<T>(IEnumerable<T> enumable)
	{
		return enumable.ElementAt(SharpRandom.Next(enumable.Count()));
	}

	public static IList<T> Shuffle<T>(IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = SharpRandom.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}

		return list;
	}
}

