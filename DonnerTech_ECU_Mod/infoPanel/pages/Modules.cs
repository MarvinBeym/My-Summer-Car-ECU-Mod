using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonnerTech_ECU_Mod.part;
using MscModApi.Caching;
using MscModApi.Tools;
using UnityEngine;
using static DonnerTech_ECU_Mod.SmartEngineModule_Logic;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
	public class Modules : InfoPanelPage
	{
		private GameObject needle;
		private SmartEngineModule_Logic smartEngineLogic;

		private FsmInt odometerKM;


		public Modules(string pageName, InfoPanel infoPanel, GameObject needle, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanel, infoPanelBaseInfo)
		{
			smartEngineLogic = mod.smartEngineModule.logic;
			this.needle = needle;
			needleUsed = true;

			GameObject odometer = Cache.Find("dashboard meters(Clone)/Gauges/Odometer");
			PlayMakerFSM odometerFSM = odometer.GetComponentInChildren<PlayMakerFSM>();
			this.odometerKM = odometerFSM.FsmVariables.FindFsmInt("OdometerReading");
		}

		public override string[] guiTexts => new string[]
		{
			"Enable ABS",
			"Enable ESP",
			"Enable TCS",
			"Enable 2StepRevLimiter",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"Enable Antilag",
			"",
			"",
			"Select 2Step RPM",
		};

		public override void Handle()
		{
			logic.HandleTouchPresses(guiTexts, this);
			DisplayValues();
		}

		public override void PressedButton(InfoPanel_Logic.PressedButton pressedButton, string action)
		{
			switch (action)
			{
				case "Select 2Step RPM":
					switch (pressedButton)
					{
						case InfoPanel_Logic.PressedButton.Plus:
							smartEngineLogic.twoStepRpm.Value += 100;
							break;
						case InfoPanel_Logic.PressedButton.Minus:
							smartEngineLogic.twoStepRpm.Value -= 100;
							break;
					}

					if (smartEngineLogic.twoStepRpm.Value >= 10000)
					{
						smartEngineLogic.twoStepRpm.Value = 10000;
					}

					if (smartEngineLogic.twoStepRpm.Value <= 2000)
					{
						smartEngineLogic.twoStepRpm.Value = 2000;
					}

					break;
			}
		}

		public override void DisplayValues()
		{
			infoPanel
				.SetDisplayValue(InfoPanel.VALUE_1, infoPanel.absModule.enabled.ToOnOff())
				.SetDisplayValue(InfoPanel.VALUE_2, infoPanel.espModule.enabled.ToOnOff())
				.SetDisplayValue(InfoPanel.VALUE_3, infoPanel.tcsModule.enabled.ToOnOff())
				.SetDisplayValue(InfoPanel.VALUE_4, smartEngineLogic.twoStepModuleEnabled.Value.ToOnOff())
				.SetDisplayValue(InfoPanel.VALUE_13, smartEngineLogic.alsModuleEnabled.Value.ToOnOff())
				.SetDisplayValue(InfoPanel.VALUE_16, smartEngineLogic.twoStepRpm.Value);
			if (logic.GetSelectedSetting() == "Select 2Step RPM")
			{
				infoPanel.SetDisplayValueColor(InfoPanel.VALUE_16, Color.green);
			}
			else
			{
				infoPanel.SetDisplayValueColor(InfoPanel.VALUE_16, Color.white);
			}

			infoPanel
				.SetDisplayValue(InfoPanel.VALUE_KM, odometerKM.Value)
				.SetDisplayValue(InfoPanel.VALUE_KMH, Convert.ToInt32(CarH.drivetrain.differentialSpeed))
				.SetDisplayValue(InfoPanel.VALUE_GEAR, GearToString());


			needle.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(-1f), 0));
		}

		public override void Pressed_Display_Value(string value)
		{
			switch (value)
			{
				case "Enable ABS":
					infoPanel.absModule.Toggle();
					break;
				case "Enable ESP":
					infoPanel.espModule.Toggle();
					break;
				case "Enable TCS":
					infoPanel.tcsModule.Toggle();
					break;
				case "Enable 2StepRevLimiter":
					smartEngineLogic.ToggleModule(Module.TwoStep);
					break;
				case "Enable Antilag":
					smartEngineLogic.ToggleModule(Module.Als);
					break;
				case "Select 2Step RPM":
					if (logic.GetSelectedSetting() == "Select 2Step RPM")
					{
						logic.SetSelectedSetting("");
						break;
					}

					logic.SetSelectedSetting("Select 2Step RPM");
					break;
			}
		}
	}
}