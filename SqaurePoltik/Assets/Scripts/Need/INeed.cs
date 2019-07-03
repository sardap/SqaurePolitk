using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface INeed
{
	float MaxValue { get; }

	//float MoodModifer { get; }

	float Value { get; set; }

	float Tick { get; }

	bool Fatal { get; }
}
