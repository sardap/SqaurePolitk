using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IJobAction
{
	Worker Worker { get; set; }

	void Start();

	void Step();

	void Stop();

	void Quit();

	Color32 HatColor { get; }

	bool Walk { get; }
}
