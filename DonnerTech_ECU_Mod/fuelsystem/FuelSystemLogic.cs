using UnityEngine;
using System.Collections;
using DonnerTech_ECU_Mod;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using DonnerTech_ECU_Mod.fuelsystem;
using System;
using DonnerTech_ECU_Mod.Parts;
using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;


namespace DonnerTech_ECU_Mod
{
	public class FuelSystemLogic : MonoBehaviour
	{
		protected const int RPM_STEP = 500;
		protected const int RPM_INDEX_LIMIT = 16;
		private DonnerTech_ECU_Mod mod;
		private FuelSystem fuelSystem;
		private Transform[] throttleBodyValves;

		void LateUpdate()
		{
			if (CarH.running && fuelSystem != null && fuelSystem.replaced)
			{
				if ((bool) mod.settingThrottleBodyValveRotation.Value)
				{
					HandleThrottleBodyMovement();
				}

				if (fuelSystem.installedChip == null) return;

				if (fuelSystem.installedChip.startAssist)
				{
					if (CarH.drivetrain.differentialSpeed < 15 &&
					    (CarH.drivetrain.gear == 2 || CarH.drivetrain.gear == 0))
					{
						CarH.drivetrain.canStall = false;
					}
					else
					{
						CarH.drivetrain.canStall = true;
					}
				}

				var mapRpmIndex = GetRPMIndex(Convert.ToInt32(CarH.drivetrain.rpm));
				var mapThrottleIndex = GetThrottleIndex((int) (CarH.axisCarController.throttle) * 100);
				fuelSystem.airFuelMixture.Value = fuelSystem.installedChip.fuelMap[mapThrottleIndex, mapRpmIndex];
				fuelSystem.racingCarb_adjustAverage.Value = fuelSystem.racingCarb_idealSetting.Value;
				fuelSystem.distributor_sparkAngle.Value = fuelSystem.installedChip.sparkAngle;
			}
		}

		private void HandleThrottleBodyMovement()
		{
			foreach (var throttleBodyValve in throttleBodyValves)
			{
				throttleBodyValve.localRotation =
					Quaternion.Euler(new Vector3(Mathf.Clamp(CarH.axisCarController.throttle * 4, 0, 1) * -90, 0, 0));
			}
		}

		private int GetThrottleIndex(int throttle)
		{
			switch (throttle)
			{
				case int t when t <= 0:
					return 0;
				case int t when t <= 5:
					return 1;
				case int t when t <= 25:
					return 2;
				case int t when t <= 30:
					return 3;
				case int t when t <= 40:
					return 4;
				case int t when t <= 45:
					return 5;
				case int t when t <= 50:
					return 6;
				case int t when t <= 55:
					return 7;
				case int t when t <= 60:
					return 8;
				case int t when t <= 65:
					return 9;
				case int t when t <= 75:
					return 10;
				case int t when t <= 80:
					return 11;
				case int t when t <= 95:
					return 12;
				default:
					return 13;
			}
		}

		private int GetRPMIndex(int rpm)
		{
			int index = Mathf.FloorToInt(rpm / RPM_STEP) - 1;

			if (index >= RPM_INDEX_LIMIT)
			{
				index = RPM_INDEX_LIMIT;
			}

			return index > 0 ? index : 0;
		}

		public void Init(FuelSystem fuelSystem, DonnerTech_ECU_Mod mod)
		{
			this.mod = mod;
			this.fuelSystem = fuelSystem;
			throttleBodyValves = new Transform[fuelSystem.throttleBodyParts.Count];
			for (var i = 0; i < fuelSystem.throttleBodyParts.Count; i++)
			{
				throttleBodyValves[i] = ((Part)fuelSystem.throttleBodyParts[i]).transform.FindChild("Butterfly-Valve");
			}
		}
	}
}