using MSCLoader;
using UnityEngine;
using HutongGames.PlayMaker;
using System;
using MscModApi.Caching;
using MscModApi.Tools;

namespace DonnerTech_ECU_Mod
{
	public class CruiseControl_Logic : MonoBehaviour
	{
		private DonnerTech_ECU_Mod mod;

		private GameObject cruiseControlPanel;
		private TextMesh cruiseControlText;

		private AudioSource dashButtonAudio;

		private bool allPartsInstalled =>
		(
			mod.smartEngineModule.bolted &&
			mod.cableHarness.bolted &&
			mod.mountingPlate.bolted &&
			mod.fuel_system.IsFixed()
		);

		private RaycastHit hit;

		//Cruise control
		private int setCruiseControlSpeed = 0;
		private bool cruiseControlModuleEnabled = false;

		// Use this for initialization
		void Start()
		{
			System.Collections.Generic.List<Mod> mods = ModLoader.LoadedMods;
			Mod[] modsArr = mods.ToArray();
			foreach (Mod mod in modsArr)
			{
				if (mod.Name == "DonnerTechRacing ECUs")
				{
					this.mod = (DonnerTech_ECU_Mod) mod;
					break;
				}
			}

			cruiseControlPanel = this.gameObject;
			cruiseControlText = cruiseControlPanel.GetComponentInChildren<TextMesh>();
		}

		void Update()
		{
			if (CarH.hasPower && allPartsInstalled && mod.playerCurrentVehicle.Value == "Satsuma")
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
			if (Camera.main != null)
			{
				if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1f,
					    1 << LayerMask.NameToLayer("DontCollide")) != false)
				{
					GameObject gameObjectHit;
					bool foundObject = false;
					string guiText = "";
					gameObjectHit = hit.collider?.gameObject;
					if (gameObjectHit != null)
					{
						Action actionToPerform = null;
						//CruiseControl Panel
						if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Minus")
						{
							foundObject = true;
							actionToPerform = DecreaseCruiseControl;
							guiText = "decrease cruise speed";
						}

						if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Plus")
						{
							foundObject = true;
							actionToPerform = IncreaseCruiseControl;
							guiText = "increase cruise speed";
						}

						if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Set")
						{
							foundObject = true;
							actionToPerform = SetCruiseControl;
							guiText = "set/enable cruise control";
						}

						if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Reset")
						{
							foundObject = true;
							actionToPerform = ResetCruiseControl;
							guiText = "reset/disable cruise control";
						}

						if (foundObject)
						{
							UserInteraction.GuiInteraction(guiText);
							if (UserInteraction.UseButtonDown)
							{
								actionToPerform.Invoke();
								gameObjectHit.PlayTouch();
							}
						}
					}
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