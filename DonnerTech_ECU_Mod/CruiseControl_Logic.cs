using MSCLoader;
using UnityEngine;
using HutongGames.PlayMaker;
using System;
using DonnerTech_ECU_Mod.fuelsystem;
using DonnerTech_ECU_Mod.part;
using MscModApi.Caching;
using MscModApi.Tools;

namespace DonnerTech_ECU_Mod
{
	public class CruiseControl_Logic : MonoBehaviour
	{
		private DonnerTech_ECU_Mod mod;

		private CruiseControlPanel cruiseControlPanel;
		private TextMesh cruiseControlText;

		private AudioSource dashButtonAudio;

		private RaycastHit hit;

		//Cruise control
		private int setCruiseControlSpeed = 0;
		private bool cruiseControlModuleEnabled = false;
		private GameObject switchMinus;
		private GameObject switchPlus;
		private GameObject switchSet;
		private GameObject switchReset;


		// Use this for initialization
		void Start()
		{
		}

		public void Init(CruiseControlPanel cruiseControlPanel)
		{
			this.cruiseControlPanel = cruiseControlPanel;

			switchMinus = cruiseControlPanel.transform.FindChild("ECU-Mod_CruiseControlPanel_Switch_Minus").gameObject;
			switchPlus = cruiseControlPanel.transform.FindChild("ECU-Mod_CruiseControlPanel_Switch_Plus").gameObject;
			switchSet = cruiseControlPanel.transform.FindChild("ECU-Mod_CruiseControlPanel_Switch_Set").gameObject;
			switchReset = cruiseControlPanel.transform.FindChild("ECU-Mod_CruiseControlPanel_Switch_Reset").gameObject;
			cruiseControlText = cruiseControlPanel.transform
				.FindChild("ECU-Mod_CruiseControlPanel_Set_Speed_Text")
				.GetComponent<TextMesh>();
		}
		void Update()
		{
			if (CarH.hasPower && cruiseControlPanel.functional && CarH.playerInCar)
			{
				HandleButtonPresses();

				SetCruiseControlSpeedText(setCruiseControlSpeed.ToString());
				if (CarH.drivetrain.gear != 0 && cruiseControlModuleEnabled && CarH.carController.throttleInput <= 0f)
				{
					float valueToThrottle = 0f;
					if (CarH.drivetrain.differentialSpeed >= (setCruiseControlSpeed - 0.5) &&
					    CarH.drivetrain.differentialSpeed <= (setCruiseControlSpeed + 0.5))
					{
						valueToThrottle = 0.5f;
					}
					else if (CarH.drivetrain.differentialSpeed < (setCruiseControlSpeed - 0.5))
					{
						valueToThrottle = 1f;
					}
					else if (CarH.drivetrain.differentialSpeed >= (setCruiseControlSpeed + 0.5f))
					{
						valueToThrottle = 0f;
					}
					else if (CarH.drivetrain.differentialSpeed >= setCruiseControlSpeed)
					{
						valueToThrottle = 0.3f;
					}

					CarH.drivetrain.idlethrottle = valueToThrottle;
					if (CarH.drivetrain.differentialSpeed < 19f || CarH.carController.brakeInput > 0f ||
					    CarH.carController.clutchInput > 0f || CarH.carController.handbrakeInput > 0f)
					{
						ResetCruiseControl();
					}
				}
				else if (cruiseControlModuleEnabled && CarH.carController.throttleInput <= 0f)
				{
					ResetCruiseControl();
					setCruiseControlSpeed = 0;
				}
			}
			else
			{
				setCruiseControlSpeed = 0;
				cruiseControlModuleEnabled = false;
				SetCruiseControlSpeedText("");
			}
		}

		private void HandleButtonPresses()
		{
			Action actionToPerform = null;
			string guiText = null;

			if (switchMinus.IsLookingAt())
			{
				actionToPerform = DecreaseCruiseControl;
				guiText = "decrease cruise speed";
			}

			if (switchPlus.IsLookingAt())
			{
				actionToPerform = IncreaseCruiseControl;
				guiText = "increase cruise speed";
			}

			if (switchReset.IsLookingAt())
			{
				actionToPerform = ResetCruiseControl;
				guiText = "reset/disable cruise control";
			}

			if (switchSet.IsLookingAt())
			{
				actionToPerform = SetCruiseControl;
				guiText = "set/enable cruise control";
			}

			if (actionToPerform != null && guiText != null)
			{
				UserInteraction.GuiInteraction(guiText);
				if (UserInteraction.UseButtonDown)
				{
					actionToPerform.Invoke();
					cruiseControlPanel.gameObject.PlayTouch();
				}
			}
		}

		private void DecreaseCruiseControl()
		{
			if (setCruiseControlSpeed > 20)
			{
				setCruiseControlSpeed -= 2;
			}
		}

		private void IncreaseCruiseControl()
		{
			setCruiseControlSpeed += 2;
		}

		private void SetCruiseControl()
		{
			if (CarH.drivetrain.differentialSpeed >= 20)
			{
				int speedToSet = Convert.ToInt32(CarH.drivetrain.differentialSpeed);
				if (speedToSet % 2 != 0)
				{
					speedToSet--;
				}

				setCruiseControlSpeed = speedToSet;

				SetCruiseControlSpeedTextColor(Color.green);
				cruiseControlModuleEnabled = true;
			}
		}

		private void ResetCruiseControl()
		{
			if (!cruiseControlModuleEnabled)
			{
				setCruiseControlSpeed = 0;
			}

			SetCruiseControlSpeedTextColor(Color.white);
			cruiseControlModuleEnabled = false;
		}

		private void SetCruiseControlSpeedText(string toSet)
		{
			cruiseControlText.text = toSet;
		}

		private void SetCruiseControlSpeedTextColor(Color colorToSet)
		{
			cruiseControlText.color = colorToSet;
		}
	}
}