using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonnerTech_ECU_Mod.part;
using MscModApi.Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
	class Assistance : InfoPanelPage
	{
		public Assistance(string pageName, InfoPanel infoPanel, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanel, infoPanelBaseInfo)
		{
		}

		public override string[] guiTexts => new string[]
		{
			"Enable Rainsensor",
			"Enable Lightsensor",
			"Select Shift Indicator green line",
			"Select Shift Indicator red line",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
		};

		public override void DisplayValues()
		{
			display_values["value_1"].text = infoPanel.rainLightSensorboard.rainSensorEnabled.ToOnOff();
			display_values["value_2"].text = infoPanel.rainLightSensorboard.lightSensorEnabled.ToOnOff();
			display_values["value_3"].text = infoPanel.shiftIndicatorGreenLine.ToString();
			display_values["value_4"].text = infoPanel.shiftIndicatorRedLine.ToString();
			switch (logic.GetSelectedSetting())
			{
				case "Select Shift Indicator green line":
					display_values["value_3"].color = Color.green;
					display_values["value_4"].color = Color.white;
					break;
				case "Select Shift Indicator red line":
					display_values["value_3"].color = Color.white;
					display_values["value_4"].color = Color.green;
					break;
				default:
					display_values["value_3"].color = Color.white;
					display_values["value_4"].color = Color.white;
					break;
			}
		}

		public override void Handle()
		{
			logic.HandleTouchPresses(guiTexts, this);
			DisplayValues();
		}

		public override void PressedButton(InfoPanel_Logic.PressedButton pressedButton, string action)
		{
			switch (pressedButton)
			{
				case InfoPanel_Logic.PressedButton.Plus:
					switch (action)
					{
						case "Select Shift Indicator green line":
							infoPanel.shiftIndicatorGreenLine += 100;
							break;
						case "Select Shift Indicator red line":
							infoPanel.shiftIndicatorRedLine += 100;
							break;
					}

					break;
				case InfoPanel_Logic.PressedButton.Minus:
					switch (action)
					{
						case "Select Shift Indicator green line":
							infoPanel.shiftIndicatorGreenLine -= 100;
							break;
						case "Select Shift Indicator red line":
							infoPanel.shiftIndicatorRedLine -= 100;
							break;
					}

					break;
			}

			if (infoPanel.shiftIndicatorGreenLine >= infoPanel.shiftIndicatorRedLine)
			{
				infoPanel.shiftIndicatorGreenLine -= 100;
			}

			if (infoPanel.shiftIndicatorGreenLine <= infoPanel.shiftIndicatorBaseLine)
			{
				infoPanel.shiftIndicatorGreenLine += 100;
			}

			if (infoPanel.shiftIndicatorRedLine <= infoPanel.shiftIndicatorGreenLine)
			{
				infoPanel.shiftIndicatorRedLine += 100;
			}

			infoPanel.shiftIndicatorLogic.InitGradient();
		}

		public override void Pressed_Display_Value(string value)
		{
			switch (value)
			{
				case "Enable Rainsensor":
					infoPanel.rainLightSensorboard.rainSensorEnabled = !infoPanel.rainLightSensorboard.rainSensorEnabled;
					break;
				case "Enable Lightsensor":
					infoPanel.rainLightSensorboard.lightSensorEnabled = !infoPanel.rainLightSensorboard.rainSensorEnabled;
					break;
				case "Select Shift Indicator green line":
					if (logic.GetSelectedSetting() == "Select Shift Indicator green line")
					{
						logic.SetSelectedSetting("");
						break;
					}

					logic.SetSelectedSetting("Select Shift Indicator green line");
					break;
				case "Select Shift Indicator red line":
					if (logic.GetSelectedSetting() == "Select Shift Indicator red line")
					{
						logic.SetSelectedSetting("");
						break;
					}

					logic.SetSelectedSetting("Select Shift Indicator red line");
					break;
			}
		}
	}
}