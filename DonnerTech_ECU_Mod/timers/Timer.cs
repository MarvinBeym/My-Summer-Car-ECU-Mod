using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.timers
{
	public class Timer
	{
		private float timer = 0;

		private Action function;
		private float delay;
		private bool callOnInit;

		public Timer(Action function, float delay, bool callOnInit = true)
		{
			this.function = function;
			this.delay = delay;
			this.callOnInit = callOnInit;
			if (callOnInit)
			{
				function.Invoke();
			}
		}

		public void Call()
		{
			timer += Time.deltaTime;
			if (timer >= delay)
			{
				timer = 0;
				function.Invoke();
			}
		}
	}
}