using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BuildingTypeData", menuName = "Building Type Data", order = 51)]
public class BuildingTypeData : ScriptableObject
{
	[SerializeField]
	public int MaxCapaticy;

	[SerializeField]
	public int MaxHeight;
}
