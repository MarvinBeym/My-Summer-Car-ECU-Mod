using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonnerTech_ECU_Mod.part;
using MSCLoader;
using MscModApi.Caching;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
	public class LightSensorLogic : MonoBehaviour
	{
		private RainLightSensorBoard rainLightSensorBoard;

		private bool _enabled = false;
		private bool isNight = false;

		private GameObject beamShort;

		void Update()
		{
			if (CarH.hasPower)
			{
				if (rainLightSensorBoard.lightSensorEnabled)
				{
					beamShort.SetActive(isNight);
				}
			}
		}

		public void Init(RainLightSensorBoard rainLightSensorBoard)
		{
			this.rainLightSensorBoard = rainLightSensorBoard;

			beamShort = Cache.Find("SATSUMA(557kg, 248)/Electricity/PowerON/BeamsShort");

			FsmHook.FsmInject(Cache.Find("StreetLights"), "Day", new Action(delegate () { isNight = false; }));
			FsmHook.FsmInject(Cache.Find("StreetLights"), "Night", new Action(delegate () { isNight = true; }));
		}

		public bool enabled
		{
			get => _enabled;
			set
			{
				if (!value)
				{
					beamShort.SetActive(false);
				}

				_enabled = value;
			}
		}
	}
}
