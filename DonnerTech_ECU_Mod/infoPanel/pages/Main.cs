using DonnerTech_ECU_Mod.timers;
using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonnerTech_ECU_Mod.part;
using MscModApi.Caching;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
	class Main : InfoPanelPage
	{
		private GameObject needle;

		private FsmInt odometerKM;
		private FsmFloat coolantPressurePSI;
		private FsmFloat coolantTemp;
		private FsmFloat clockHours;
		private FsmFloat voltage;

		private FsmBool carbInstalled;
		private FsmBool racingCarbInstalled;
		private FsmBool twinCarbInstalled;

		private FsmFloat afRatioCarb;
		private FsmFloat afRatioRacingCarb;
		private FsmFloat afRatioTwinCarb;

		private float timerFuel = 0;
		private float fuelConsumption = 0;
		private float oldValue = 0;
		private float consumptionCounter = 0;
		private FsmFloat fuelLeft;

		private Timer clockUpdateTimer;
		private Timer gearUpdateTimer;
		private Timer odometerUpdateTimer;
		private Timer voltageUpdateTimer;

		public Main(string pageName, InfoPanel infoPanel, GameObject needle, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanel, infoPanelBaseInfo)
		{
			this.needle = needle;
			needleUsed = true;

			PlayMakerFSM carbFSM = Cache.Find("Carburator").GetComponent<PlayMakerFSM>();
			PlayMakerFSM racingCarbFSM = Cache.Find("Racing Carburators").GetComponent<PlayMakerFSM>();
			PlayMakerFSM twinCarb = Cache.Find("Twin Carburators").GetComponent<PlayMakerFSM>();

			carbInstalled = carbFSM.FsmVariables.FindFsmBool("Installed");
			racingCarbInstalled = racingCarbFSM.FsmVariables.FindFsmBool("Installed");
			twinCarbInstalled = twinCarb.FsmVariables.FindFsmBool("Installed");

			afRatioCarb = carbFSM.FsmVariables.FindFsmFloat("IdleAdjust");
			afRatioRacingCarb = racingCarbFSM.FsmVariables.FindFsmFloat("AdjustAverage");
			afRatioTwinCarb = twinCarb.FsmVariables.FindFsmFloat("IdleAdjust");

			GameObject dataBaseMechanics = Cache.Find("Database/DatabaseMechanics");
			PlayMakerFSM[] dataBaseMechanicsFSMs = dataBaseMechanics.GetComponentsInChildren<PlayMakerFSM>(true);

			GameObject odometer = Cache.Find("dashboard meters(Clone)/Gauges/Odometer");
			PlayMakerFSM odometerFSM = odometer.GetComponentInChildren<PlayMakerFSM>();
			odometerKM = odometerFSM.FsmVariables.FindFsmInt("OdometerReading");

			GameObject electrics = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Electrics");
			PlayMakerFSM electricsFSM = electrics.GetComponent<PlayMakerFSM>();
			voltage = electricsFSM.FsmVariables.FindFsmFloat("Volts");


			GameObject cooling = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Cooling").gameObject;
			PlayMakerFSM coolingFSM = PlayMakerFSM.FindFsmOnGameObject(cooling, "Cooling");
			coolantTemp = coolingFSM.FsmVariables.FindFsmFloat("CoolantTemp");
			coolantPressurePSI = coolingFSM.FsmVariables.FindFsmFloat("WaterPressurePSI");

			clockHours = PlayMakerGlobals.Instance.Variables.FindFsmFloat("TimeRotationHour");


			foreach (PlayMakerFSM fsm in dataBaseMechanicsFSMs)
			{
				if (fsm.name == "FuelTank")
				{
					fuelLeft = fsm.FsmVariables.FindFsmFloat("FuelLevel");
					break;
				}
			}

			InitTimers();
		}

		public override string[] guiTexts => new string[0];

		public void InitTimers()
		{
			clockUpdateTimer =
				new Timer(delegate() { infoPanel.SetDisplayValue(InfoPanel.VALUE_13, ConvertToDigitalTime(clockHours.Value)); }, 1);
			gearUpdateTimer = new Timer(delegate() { infoPanel.SetDisplayValue(InfoPanel.VALUE_GEAR, GearToString()); }, 0.1f);
			odometerUpdateTimer =
				new Timer(delegate() { infoPanel.SetDisplayValue(InfoPanel.VALUE_KM, odometerKM.Value); }, 1.0f);
			voltageUpdateTimer =
				new Timer(delegate() { infoPanel.SetDisplayValue(InfoPanel.VALUE_14, voltage.Value.ToString("00 .0") + "V"); },
					0.2f);
		}

		public override void DisplayValues()
		{
			float[] fuelCalculated = FuelKMCalculate();

			infoPanel
				.SetDisplayValue(InfoPanel.VALUE_1, Convert.ToInt32(CarH.drivetrain.rpm))
				.SetDisplayValue(InfoPanel.VALUE_2, Convert.ToInt32(coolantTemp.Value));

			if (carbInstalled.Value)
			{
				infoPanel.SetDisplayValue(InfoPanel.VALUE_3, afRatioCarb.Value, "00.00");
			}
			else if (racingCarbInstalled.Value)
			{
				infoPanel.SetDisplayValue(InfoPanel.VALUE_3, afRatioRacingCarb.Value, "00.00");
			}
			else if (twinCarbInstalled.Value)
			{
				infoPanel.SetDisplayValue(InfoPanel.VALUE_3, afRatioTwinCarb.Value, "00.00");
			}
			else
			{
				infoPanel.SetDisplayValue(InfoPanel.VALUE_3, "00.00");
			}

			infoPanel.SetDisplayValue(InfoPanel.VALUE_4, Convert.ToInt32(coolantPressurePSI.Value));

			if (fuelCalculated != null && fuelCalculated[1] >= 0 && fuelCalculated[1] <= 9000)
			{
				if (fuelCalculated[1] >= 0 && fuelCalculated[1] <= 9000)
				{
					infoPanel
						.SetDisplayValue(InfoPanel.VALUE_15, Convert.ToInt32(fuelCalculated[1]))
						.SetDisplayValue(InfoPanel.VALUE_16, Convert.ToInt32(fuelCalculated[0]));
				}
			}

			infoPanel.SetDisplayValue(InfoPanel.VALUE_KMH, Convert.ToInt32(CarH.drivetrain.differentialSpeed));

			clockUpdateTimer.Call();
			voltageUpdateTimer.Call();
			odometerUpdateTimer.Call();
			gearUpdateTimer.Call();

			needle.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(-1f), 0));
		}

		public override void Handle()
		{
			logic.HandleTouchPresses(guiTexts, this);
			DisplayValues();
		}

		private string ConvertToDigitalTime(float hours)
		{
			if (hours > 0)
			{
				float calculatedHours = ((360 - hours) / 30) + 2f;
				if (hours > 180)
				{
					calculatedHours += 12;
				}

				float calculatedMinutes = calculatedHours * 60f;
				TimeSpan ts = TimeSpan.FromMinutes(calculatedMinutes);
				return new DateTime(ts.Ticks).ToString("HH:mm");
			}
			else
			{
				return infoPanel.GetDisplayValue(InfoPanel.VALUE_13);
			}
		}

		private float[] FuelKMCalculate()
		{
			timerFuel += Time.deltaTime;

			oldValue -= fuelLeft.Value;
			if (oldValue > 0)
			{
				fuelConsumption = oldValue;
				consumptionCounter += fuelConsumption;
			}

			oldValue = fuelLeft.Value;

			if (timerFuel >= 1f)
			{
				float Lper100km = Mathf.Clamp((consumptionCounter * 60) * 60, 0, 7);
				float kmLeft = (fuelLeft.Value / Lper100km) * 100;
				timerFuel = 0;
				consumptionCounter = 0;
				return new float[] { Lper100km, kmLeft };
			}

			return null;
		}

		public override void Pressed_Display_Value(string value)
		{
			/*
			switch (value)
			{
			    
			}
			playTouchSound(gameObjectHit);
			*/
		}
	}
}