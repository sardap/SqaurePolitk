using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class BunCreateInfo : IFoodCreateInfo
{
	public float TimeToCreate
	{
		get
		{
			return Util.RandomFloat(2f, 4f);
		}
	}

	public float FillingValue
	{
		get
		{
			return 50f;
		}
	}
}
