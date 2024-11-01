using DonnerTech_ECU_Mod.info_panel_pages;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using DonnerTech_ECU_Mod.part;
using MscModApi.Caching;
using MscModApi.Tools;
using Tools;
using UnityEngine;
using Helper = MscModApi.Tools.Helper;

namespace DonnerTech_ECU_Mod
{
	public class InfoPanel_Logic : MonoBehaviour
	{
		public enum PressedButton
		{
			Circle,
			Cross,
			Plus,
			Minus,
		};

		public GameObject panel;
		private InfoPanel infoPanel;

		private DonnerTech_ECU_Mod mod;

		private GameObject turnSignals;

		private GameObject blinkerRight;
		private GameObject blinkerLeft;

		private int currentPage = 0;

		//Animation
		private const float ecu_InfoPanel_Needle_maxAngle = 270;
		private const float ecu_InfoPanel_Needle_minAngle = 0;
		private const float ecu_InfoPanel_Needle_maxRPM = 9000;
		private float rpmIncrementer = 0;
		private float rpmDecrementer = 9000;
		private float tenIncrementer = 0;
		private float hundredIncrementer = 0;
		private float thousandIncrementer = 0;


		private RaycastHit hit;

		private string selectedSetting = "";

		private GameObject beamShort;
		private GameObject beamLong;

		private void Start()
		{

			beamShort = Cache.Find("SATSUMA(557kg, 248)/Electricity/PowerON/BeamsShort");
			beamLong = Cache.Find("SATSUMA(557kg, 248)/Electricity/PowerON/BeamsLong");
			try
			{
				turnSignals = Cache.Find("SATSUMA(557kg, 248)/Dashboard/TurnSignals");
				PlayMakerFSM blinkers = null;
				PlayMakerFSM[] turnSignalComps = turnSignals.GetComponents<PlayMakerFSM>();
				foreach (PlayMakerFSM turnSignalComp in turnSignalComps)
				{
					if (turnSignalComp.FsmName == "Blinkers")
					{
						blinkers = turnSignalComp;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.New("Unable to find turn signal gameobject or Blinkers fsm",
					"SATSUMA(557kg, 248)/Dashboard/TurnSignals", ex);
			}
		}

		void Update()
		{
			if (CarH.hasPower && infoPanel.bolted)
			{
				if (!infoPanel.isBooted)
				{
					HandleBootAnimation();
				}
				else
				{
					HandleKeybinds();
					HandleButtonPresses();

					DisplayGeneralInfomation();
					infoPanel.pages[currentPage].Handle();
				}
			}
			else
			{
				try
				{
					infoPanel.needleRotation = 0;
					rpmIncrementer = 0;
					rpmDecrementer = 9000;

					tenIncrementer = 0;
					hundredIncrementer = 0;
					thousandIncrementer = 0;

					infoPanel.SetAllRendererEnabledState(false);

					infoPanel.ClearDisplayValues();

					infoPanel.isBooting = false;
					infoPanel.isBooted = false;
					currentPage = 0;
				}
				catch
				{
				}
			}
		}

		private void HandleBootAnimation()
		{
			if (infoPanel.isBooting)
			{
				Play_ECU_InfoPanel_Animation();
			}
			else
			{
				currentPage = 0;
				ChangeInfoPanelPage(currentPage);
				infoPanel.SetAllRendererEnabledState(false);
				infoPanel.background.sprite = infoPanel.pages[currentPage].pageSprite;
				infoPanel.needle.enabled = infoPanel.pages[currentPage].needleUsed;
				infoPanel.turboWheel.enabled = infoPanel.pages[currentPage].turbineUsed;
				infoPanel.background.enabled = true;

				infoPanel.ClearDisplayValues();

				infoPanel.isBooting = true;
			}
		}


		private void HandleKeybinds()
		{
			if (CarH.playerInCar)
			{
				if (mod.arrowUp.GetKeybindDown())
				{
					OnArrowUp();
				}
				else if (mod.arrowDown.GetKeybindDown())
				{
					OnArrowDown();
				}
				else if (mod.circle.GetKeybindDown())
				{
					infoPanel.pages[currentPage].PressedButton(PressedButton.Circle, selectedSetting);
				}
				else if (mod.cross.GetKeybindDown())
				{
					infoPanel.pages[currentPage].PressedButton(PressedButton.Cross, selectedSetting);
				}
				else if (mod.plus.GetKeybindDown())
				{
					infoPanel.pages[currentPage].PressedButton(PressedButton.Plus, selectedSetting);
				}
				else if (mod.minus.GetKeybindDown())
				{
					infoPanel.pages[currentPage].PressedButton(PressedButton.Minus, selectedSetting);
				}
			}
		}

		public void HandleTouchPresses(string[] guiTexts, InfoPanelPage page)
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
						string valueToPass = null;
						for (int i = 1; i <= guiTexts.Length; i++)
						{
							if (gameObjectHit.name == ("displayValue_" + i.ToString().PadLeft(2, '0')))
							{
								valueToPass = guiTexts[i - 1];
								guiText = guiTexts[i - 1];
								foundObject = true;
								break;
							}
						}

						if (foundObject)
						{
							UserInteraction.GuiInteraction(guiText);
							if (UserInteraction.UseButtonDown || UserInteraction.LeftMouseDown)
							{
								page.Pressed_Display_Value(valueToPass, gameObjectHit);
							}
						}
					}
				}
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
					string guiText = "";
					gameObjectHit = hit.collider?.gameObject;
					if (gameObjectHit != null)
					{
						Action actionToPerform = delegate() { ButtonPress(currentPage, gameObjectHit.name); };

						if (!gameObjectHit.name.StartsWith("button"))
						{
							return;
						}

						UserInteraction.GuiInteraction(gameObjectHit.name.Replace("button", "").Replace("Arrow", ""));
						if (UserInteraction.UseButtonDown || UserInteraction.LeftMouseDown)
						{
							actionToPerform.Invoke();
							gameObjectHit.PlayTouch();
						}
					}
				}
			}
		}

		private void ButtonPress(int pageNumer, string action)
		{
			switch (action)
			{
				case "buttonArrowDown":
					OnArrowDown();
					return;
				case "buttonArrowUp":
					OnArrowUp();
					return;
				case "buttonCircle":
					infoPanel.pages[pageNumer].PressedButton(PressedButton.Circle, selectedSetting);
					return;
				case "buttonCross":
					infoPanel.pages[pageNumer].PressedButton(PressedButton.Cross, selectedSetting);
					return;
				case "buttonMinus":
					infoPanel.pages[pageNumer].PressedButton(PressedButton.Minus, selectedSetting);
					return;
				case "buttonPlus":
					infoPanel.pages[pageNumer].PressedButton(PressedButton.Plus, selectedSetting);
					return;
			}

			return;
		}

		private void OnArrowUp()
		{
			currentPage++;
			if (currentPage > infoPanel.pages.Count - 1)
			{
				currentPage = 0;
			}

			ChangeInfoPanelPage(currentPage);
		}

		private void OnArrowDown()
		{
			currentPage--;
			if (currentPage < 0)
			{
				currentPage = infoPanel.pages.Count - 1;
			}

			ChangeInfoPanelPage(currentPage);
		}

		private void ChangeInfoPanelPage(int currentPage)
		{
			infoPanel.background.sprite = infoPanel.pages[currentPage].pageSprite;
			infoPanel.needle.enabled = infoPanel.pages[currentPage].needleUsed;
			infoPanel.turboWheel.enabled = infoPanel.pages[currentPage].turbineUsed;

			infoPanel.ClearDisplayValues();

			selectedSetting = "";
		}

		private void DisplayGeneralInfomation()
		{
			infoPanel.lowBeam.enabled = beamShort.activeSelf;
			infoPanel.highBeam.enabled = beamLong.activeSelf;
			if (CarH.carController.handbrakeInput > 0)
			{
				infoPanel.handbrake.enabled = true;
			}
			else
			{
				infoPanel.handbrake.enabled = false;
			}


			if (blinkerLeft == null)
			{
				blinkerLeft = Cache.Find("BlinkLeft");
			}
			else
			{
				infoPanel.indicatorLeft.enabled = blinkerLeft.activeSelf;
			}

			if (blinkerRight == null)
			{
				blinkerRight = Cache.Find("BlinkRight");
			}
			else
			{
				infoPanel.indicatorRight.enabled = blinkerRight.activeSelf;
			}
		}

		private float GetRPMRotation(float rpmOverride)
		{
			if (rpmOverride >= 0)
			{
				float totalAngleSize = ecu_InfoPanel_Needle_minAngle - ecu_InfoPanel_Needle_maxAngle;
				float rpmNormalized = rpmOverride / ecu_InfoPanel_Needle_maxRPM;
				return ecu_InfoPanel_Needle_minAngle - rpmNormalized * totalAngleSize;
			}
			else
			{
				float totalAngleSize = ecu_InfoPanel_Needle_minAngle - ecu_InfoPanel_Needle_maxAngle;
				float rpmNormalized = CarH.drivetrain.rpm / ecu_InfoPanel_Needle_maxRPM;
				return ecu_InfoPanel_Needle_minAngle - rpmNormalized * totalAngleSize;
			}
		}

		private void Play_ECU_InfoPanel_Animation()
		{
			const float rpmAdder = 300;
			const float tenAdder = 1;
			const float hundredAdder = 10;
			const float thousandAdder = 100;
			if (rpmIncrementer == 9000)
			{
				rpmDecrementer -= rpmAdder;
				tenIncrementer -= tenAdder;
				hundredIncrementer -= hundredAdder;
				thousandIncrementer -= thousandAdder;

				infoPanel.needleRotation = GetRPMRotation(rpmDecrementer);

				infoPanel.displayValues["value_1"].text = rpmDecrementer.ToString();
				infoPanel.displayValues["value_2"].text = hundredIncrementer.ToString();
				infoPanel.displayValues["value_3"].text = tenIncrementer.ToString() + "." + tenIncrementer.ToString();
				infoPanel.displayValues["value_4"].text = tenIncrementer.ToString();
				infoPanel.displayValues["value_13"].text = "";
				infoPanel.displayValues["value_14"].text = tenIncrementer.ToString("00 .0") + "V";
				infoPanel.displayValues["value_15"].text = thousandIncrementer.ToString();
				infoPanel.displayValues["value_16"].text = tenIncrementer.ToString();
				infoPanel.displayValues["value_kmh"].text = hundredIncrementer.ToString();
				infoPanel.displayValues["value_km"].text = (thousandIncrementer * 10f).ToString();
			}
			else if (rpmIncrementer < 9000)
			{
				rpmIncrementer += rpmAdder;
				tenIncrementer += tenAdder;
				hundredIncrementer += hundredAdder;
				thousandIncrementer += thousandAdder;

				infoPanel.needleRotation = GetRPMRotation(rpmIncrementer);

				infoPanel.displayValues["value_1"].text = rpmIncrementer.ToString();
				infoPanel.displayValues["value_2"].text = hundredIncrementer.ToString();
				infoPanel.displayValues["value_3"].text = tenIncrementer.ToString() + "." + tenIncrementer.ToString();
				infoPanel.displayValues["value_4"].text = tenIncrementer.ToString();
				infoPanel.displayValues["value_13"].text = "Boot up";
				infoPanel.displayValues["value_14"].text = tenIncrementer.ToString("00 .0") + "V";
				infoPanel.displayValues["value_15"].text = thousandIncrementer.ToString();
				infoPanel.displayValues["value_16"].text = tenIncrementer.ToString();
				infoPanel.displayValues["value_kmh"].text = hundredIncrementer.ToString();
				infoPanel.displayValues["value_km"].text = (thousandIncrementer * 10f).ToString();
			}

			if (rpmIncrementer >= 9000 && rpmDecrementer <= 0)
			{
				rpmIncrementer = 0;
				rpmDecrementer = 9000;
				infoPanel.isBooted = true;
				infoPanel.isBooting = false;
			}
		}

		public void Init(InfoPanel infoPanel, DonnerTech_ECU_Mod mod, AssetBundle assetBundle)
		{
			panel = this.gameObject;
			this.infoPanel = infoPanel;
			this.mod = mod;
		}

		public string GetSelectedSetting()
		{
			return selectedSetting;
		}

		public void SetSelectedSetting(string selectedSetting)
		{
			this.selectedSetting = selectedSetting;
		}
	}
}