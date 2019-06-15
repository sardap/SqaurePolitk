using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IJobAction
{
	void Start();

	void Step();

	void Stop();

	Color32 HatColor { get; }

	bool Walk { get; }
}
