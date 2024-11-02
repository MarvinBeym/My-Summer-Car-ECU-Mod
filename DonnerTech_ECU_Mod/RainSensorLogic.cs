using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonnerTech_ECU_Mod.part;
using HutongGames.PlayMaker;
using MscModApi.Caching;
using MscModApi.Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
	public class RainSensorLogic : MonoBehaviour
	{
		private RainLightSensorBoard rainLightSensorBoard;

		private FsmFloat rainIntensity;
		private PlayMakerFSM wiperLogicFSM;
		private FsmBool wiperOn;
		private FsmFloat wiperDelay;

		private bool _enabled = false;

		void Update()
		{
			if (CarH.hasPower)
			{
				if (rainLightSensorBoard.rainSensorEnabled)
				{
					switch (rainIntensity.Value)
					{
						case float f when f >= 0.5f:
							wiperOn.Value = true;
							wiperDelay.Value = 0f;
							break;
						case float f when f > 0f:
							wiperOn.Value = true;
							wiperDelay.Value = 3f;
							break;
						default:
							wiperOn.Value = false;
							wiperDelay.Value = 0f;
							break;
					}
				}
			}

		}

		public void Init(RainLightSensorBoard rainLightSensorBoard)
		{
			this.rainLightSensorBoard = rainLightSensorBoard;

			rainIntensity = PlayMakerGlobals.Instance.Variables.FindFsmFloat("RainIntensity");
			GameObject buttonWipers = Cache.Find(
					"dashboard meters(Clone)/Knobs/ButtonsDash/ButtonWipers"
			);

			wiperLogicFSM = buttonWipers.FindFsm("Function");
			wiperOn = wiperLogicFSM.FsmVariables.FindFsmBool("On");
			wiperDelay = wiperLogicFSM.FsmVariables.FindFsmFloat("Delay");
		}

		public bool enabled
		{
			get => _enabled;
			set
			{
				if (!value)
				{
					wiperOn.Value = false;
					wiperDelay.Value = 0f;
				}

				_enabled = value;
			}
		}
	}
}
