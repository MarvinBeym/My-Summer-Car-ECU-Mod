using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MscModApi.Caching;
using MscModApi.Tools;
using UnityEngine;
using DonnerTech_ECU_Mod.part;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
	class Page4 : InfoPanelPage
	{
		private bool autoTuneRace_running = false;
		private bool autoTuneEco_running = false;
		private bool autoTune_done = false;
		private int autoTune_state = 0;
		private float autoTune_counter = 0;
		private float autoTune_timer = 0;

		private FsmFloat cylinder1exhaust;
		private FsmFloat cylinder2exhaust;
		private FsmFloat cylinder3exhaust;
		private FsmFloat cylinder4exhaust;

		private FsmFloat cylinder1intake;
		private FsmFloat cylinder2intake;
		private FsmFloat cylinder3intake;
		private FsmFloat cylinder4intake;

		private FsmBool raceCarb_installed;
		private FsmFloat raceCarbAdjustAverage;
		private FsmFloat raceCarbAdjust1;
		private FsmFloat raceCarbAdjust2;
		private FsmFloat raceCarbAdjust3;
		private FsmFloat raceCarbAdjust4;

		private FsmBool twinCarb_installed;
		private FsmFloat twinCarbAdjust;

		private FsmBool carb_installed;
		private FsmFloat carbAdjust;

		public Page4(string pageName, InfoPanel infoPanel, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanel, infoPanelBaseInfo)
		{
			PlayMakerFSM rockershaftFSM = Cache.Find("RockerShaft").GetComponent<PlayMakerFSM>();
			cylinder1exhaust = rockershaftFSM.FsmVariables.FindFsmFloat("cyl1exhaust");
			cylinder2exhaust = rockershaftFSM.FsmVariables.FindFsmFloat("cyl2exhaust");
			cylinder3exhaust = rockershaftFSM.FsmVariables.FindFsmFloat("cyl3exhaust");
			cylinder4exhaust = rockershaftFSM.FsmVariables.FindFsmFloat("cyl4exhaust");

			cylinder1intake = rockershaftFSM.FsmVariables.FindFsmFloat("cyl1intake");
			cylinder2intake = rockershaftFSM.FsmVariables.FindFsmFloat("cyl2intake");
			cylinder3intake = rockershaftFSM.FsmVariables.FindFsmFloat("cyl3intake");
			cylinder4intake = rockershaftFSM.FsmVariables.FindFsmFloat("cyl4intake");

			PlayMakerFSM raceCarbFSM = Cache.Find("Racing Carburators").GetComponent<PlayMakerFSM>();
			raceCarb_installed = raceCarbFSM.FsmVariables.FindFsmBool("Installed");
			raceCarbAdjustAverage = raceCarbFSM.FsmVariables.FindFsmFloat("AdjustAverage");
			raceCarbAdjust1 = raceCarbFSM.FsmVariables.FindFsmFloat("Adjust1");
			raceCarbAdjust2 = raceCarbFSM.FsmVariables.FindFsmFloat("Adjust2");
			raceCarbAdjust3 = raceCarbFSM.FsmVariables.FindFsmFloat("Adjust3");
			raceCarbAdjust4 = raceCarbFSM.FsmVariables.FindFsmFloat("Adjust4");

			PlayMakerFSM twinCarbFSM = Cache.Find("Twin Carburators").GetComponent<PlayMakerFSM>();
			twinCarb_installed = twinCarbFSM.FsmVariables.FindFsmBool("Installed");
			twinCarbAdjust = twinCarbFSM.FsmVariables.FindFsmFloat("IdleAdjust");

			PlayMakerFSM carbFSM = Cache.Find("Carburator").GetComponent<PlayMakerFSM>();
			carb_installed = carbFSM.FsmVariables.FindFsmBool("Installed");
			carbAdjust = carbFSM.FsmVariables.FindFsmFloat("IdleAdjust");
		}

		public override string[] guiTexts => new string[]
		{
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
			"",
			"",
			"Start Autotune ECO",
			"Start Autotune RACE",
		};

		public override void DisplayValues()
		{
			display_values["value_1"].text = cylinder1exhaust.Value.ToString("00.00");
			display_values["value_2"].text = cylinder2exhaust.Value.ToString("00.00");
			display_values["value_3"].text = cylinder3exhaust.Value.ToString("00.00");
			display_values["value_4"].text = cylinder4exhaust.Value.ToString("00.00");
			display_values["value_5"].text = cylinder1intake.Value.ToString("00.00");
			display_values["value_6"].text = cylinder2intake.Value.ToString("00.00");
			display_values["value_7"].text = cylinder3intake.Value.ToString("00.00");
			display_values["value_8"].text = cylinder4intake.Value.ToString("00.00");

			if (raceCarb_installed.Value)
			{
				display_values["value_9"].text = raceCarbAdjust1.Value.ToString("00.00");
				display_values["value_10"].text = raceCarbAdjust2.Value.ToString("00.00");
				display_values["value_11"].text = raceCarbAdjust3.Value.ToString("00.00");
				display_values["value_12"].text = raceCarbAdjust4.Value.ToString("00.00");
			}

			if (twinCarb_installed.Value)
			{
				display_values["value_13"].text = twinCarbAdjust.Value.ToString("00.00");
			}

			if (carb_installed.Value)
			{
				display_values["value_14"].text = carbAdjust.Value.ToString("00.00");
			}

			if (autoTuneEco_running)
			{
				display_values["value_15"].text = "RUNNING";
				display_values["value_16"].text = "";
			}
			else if (autoTuneRace_running)
			{
				display_values["value_15"].text = "";
				display_values["value_16"].text = "RUNNING";
			}
			else
			{
				display_values["value_15"].text = "READY";
				display_values["value_16"].text = "READY";
			}
		}

		public override void Handle()
		{
			logic.HandleTouchPresses(guiTexts, this);
			DisplayValues();
			if (autoTuneEco_running)
			{
				RunEcoCalibrate();
			}
			else if (autoTuneRace_running)
			{
				RunRaceCalibrate();
			}
		}

		private void RunEcoCalibrate()
		{
			if (!autoTune_done)
			{
				autoTuneEco_running = true;
				autoTune_timer += Time.deltaTime;
				if (autoTune_state < 3)
				{
					RunBaseCalibrate();
				}
				else if (autoTune_state != 6)
				{
					RunCarbCalibrate(15.5f);
				}
				else if (autoTune_state == 6)
				{
					autoTune_done = true;
				}
			}
			else
			{
				autoTuneEco_running = false;
				autoTune_done = false;
				autoTune_state = 0;
				autoTune_counter = 0;
				autoTune_timer = 0;
			}
		}

		private void RunRaceCalibrate()
		{
			if (!autoTune_done)
			{
				autoTuneRace_running = true;
				autoTune_timer += Time.deltaTime;
				if (autoTune_state < 3)
				{
					RunBaseCalibrate();
				}
				else if (autoTune_state != 6)
				{
					RunCarbCalibrate(16.7f);
				}
				else if (autoTune_state == 6)
				{
					autoTune_done = true;
				}
			}
			else
			{
				autoTuneRace_running = false;
				autoTune_done = false;
				autoTune_state = 0;
				autoTune_counter = 0;
				autoTune_timer = 0;
			}
		}

		private void RunBaseCalibrate()
		{
			const int minValue = 3;
			const int maxValue = 9;
			const int idealExhaustValue = 7;
			const int idealIntakeValue = 8;
			const float step = 0.2f;
			if (autoTune_timer >= 0.2)
			{
				autoTune_timer = 0;
				if (autoTune_state == 0)
				{
					autoTune_counter = minValue;
					cylinder1exhaust.Value = autoTune_counter;
					cylinder2exhaust.Value = autoTune_counter;
					cylinder3exhaust.Value = autoTune_counter;
					cylinder4exhaust.Value = autoTune_counter;

					cylinder1intake.Value = autoTune_counter;
					cylinder2intake.Value = autoTune_counter;
					cylinder3intake.Value = autoTune_counter;
					cylinder4intake.Value = autoTune_counter;

					autoTune_state = 1;
				}
				else if (autoTune_state == 1)
				{
					if (autoTune_counter < maxValue)
					{
						autoTune_counter += step;
						cylinder1exhaust.Value = autoTune_counter;
						cylinder2exhaust.Value = autoTune_counter;
						cylinder3exhaust.Value = autoTune_counter;
						cylinder4exhaust.Value = autoTune_counter;
						cylinder1intake.Value = autoTune_counter;
						cylinder2intake.Value = autoTune_counter;
						cylinder3intake.Value = autoTune_counter;
						cylinder4intake.Value = autoTune_counter;
					}
					else
					{
						autoTune_state = 2;
					}
				}
				else if (autoTune_state == 2)
				{
					if (autoTune_counter <= idealIntakeValue)
					{
						cylinder1intake.Value = idealIntakeValue;
						cylinder2intake.Value = idealIntakeValue;
						cylinder3intake.Value = idealIntakeValue;
						cylinder4intake.Value = idealIntakeValue;
					}

					if (autoTune_counter <= idealExhaustValue)
					{
						cylinder1exhaust.Value = idealExhaustValue;
						cylinder2exhaust.Value = idealExhaustValue;
						cylinder3exhaust.Value = idealExhaustValue;
						cylinder4exhaust.Value = idealExhaustValue;
						autoTune_state = 3;
					}
					else
					{
						autoTune_counter -= step;
					}
				}
			}
		}

		private void RunCarbCalibrate(float desiredSetting)
		{
			const float step = 0.2f;
			if (autoTune_timer >= 0.2)
			{
				autoTune_timer = 0;
				if (autoTune_state == 3)
				{
					autoTune_counter = 10;
					if (carb_installed.Value)
					{
						carbAdjust.Value = autoTune_counter;
					}
					else if (twinCarb_installed.Value)
					{
						twinCarbAdjust.Value = autoTune_counter;
					}
					else if (raceCarb_installed.Value)
					{
						raceCarbAdjust1.Value = autoTune_counter;
						raceCarbAdjust2.Value = autoTune_counter;
						raceCarbAdjust3.Value = autoTune_counter;
						raceCarbAdjust4.Value = autoTune_counter;
						raceCarbAdjustAverage.Value = autoTune_counter;
					}

					autoTune_state = 4;
				}
				else if (autoTune_state == 4)
				{
					if (autoTune_counter < 22)
					{
						autoTune_counter += step;
						if (carb_installed.Value)
						{
							carbAdjust.Value = autoTune_counter;
						}
						else if (twinCarb_installed.Value)
						{
							twinCarbAdjust.Value = autoTune_counter;
						}
						else if (raceCarb_installed.Value)
						{
							raceCarbAdjust1.Value = autoTune_counter;
							raceCarbAdjust2.Value = autoTune_counter;
							raceCarbAdjust3.Value = autoTune_counter;
							raceCarbAdjust4.Value = autoTune_counter;
							raceCarbAdjustAverage.Value = autoTune_counter;
						}
					}
					else
					{
						autoTune_state = 5;
					}
				}
				else if (autoTune_state == 5)
				{
					autoTune_counter -= step;
					if (carb_installed.Value)
					{
						carbAdjust.Value = autoTune_counter;
					}
					else if (twinCarb_installed.Value)
					{
						twinCarbAdjust.Value = autoTune_counter;
					}
					else if (raceCarb_installed.Value)
					{
						raceCarbAdjust1.Value = autoTune_counter;
						raceCarbAdjust2.Value = autoTune_counter;
						raceCarbAdjust3.Value = autoTune_counter;
						raceCarbAdjust4.Value = autoTune_counter;
						raceCarbAdjustAverage.Value = autoTune_counter;
					}


					if (autoTune_counter <= desiredSetting)
					{
						if (carb_installed.Value)
						{
							carbAdjust.Value = desiredSetting;
						}
						else if (twinCarb_installed.Value)
						{
							twinCarbAdjust.Value = desiredSetting;
						}
						else if (raceCarb_installed.Value)
						{
							raceCarbAdjust1.Value = desiredSetting;
							raceCarbAdjust2.Value = desiredSetting;
							raceCarbAdjust3.Value = desiredSetting;
							raceCarbAdjust4.Value = desiredSetting;
							raceCarbAdjustAverage.Value = desiredSetting;
						}

						autoTune_state = 6;
					}
				}
			}
		}

		public override void Pressed_Display_Value(string value)
		{
			switch (value)
			{
				case "Start Autotune ECO":
				{
					autoTuneEco_running = true;
					RunEcoCalibrate();
					break;
				}
				case "Start Autotune RACE":
				{
					autoTuneRace_running = true;
					RunRaceCalibrate();
					break;
				}
			}
		}

		public void ResetAutoTune()
		{
			autoTuneEco_running = false;
			autoTuneRace_running = false;
			autoTune_counter = 0;
			autoTune_state = 0;
			autoTune_done = false;
			autoTune_timer = 0;
		}
	}
}