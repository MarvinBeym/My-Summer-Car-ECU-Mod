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
		private GameObject beamShort;
		private GameObject beamLong;
		private GameObject blinkerRight;
		private GameObject blinkerLeft;

		//Pages
		private List<InfoPanelPage> pages;

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


		//Display
		private bool isBooted = false;
		private bool isBooting = false;

		private SpriteRenderer ecu_InfoPanel_Needle;
		private SpriteRenderer ecu_InfoPanel_TurboWheel;
		private SpriteRenderer ecu_InfoPanel_Background;
		private SpriteRenderer ecu_InfoPanel_IndicatorLeft;
		private SpriteRenderer ecu_InfoPanel_IndicatorRight;
		private SpriteRenderer ecu_InfoPanel_Handbrake;
		private SpriteRenderer ecu_InfoPanel_LowBeam;
		private SpriteRenderer ecu_InfoPanel_HighBeam;

		private GameObject ecu_InfoPanel_NeedleObject;
		private GameObject ecu_InfoPanel_TurboWheelObject;

		//Reverse Camera stuff
		private MeshRenderer ecu_InfoPanel_Display_Reverse_Camera;

		//Lightsensor stuff
		private bool isNight = false;

		private Dictionary<string, TextMesh> display_values = new Dictionary<string, TextMesh>();

		private RaycastHit hit;

		private string selectedSetting = "";

		private FsmFloat rainIntensity;
		PlayMakerFSM wiperLogicFSM;
		public bool rainsensor_enabled = false;
		private bool rainsensor_wasEnabled = false;


		public bool lightsensor_enabled = false;
		private bool lightsensor_wasEnabled = false;

		private Sprite needleSprite;
		private Sprite turbineWheelSprite;
		private Sprite handbrakeSprite;
		private Sprite blinkerLeftSprite;
		private Sprite blinkerRightSprite;
		private Sprite highBeamSprite;
		private Sprite lowBeamSprite;

		private MeshRenderer shift_indicator_renderer;
		private Gradient shift_indicator_gradient;

		private float shift_indicator_blink_timer = 0;
		public int shift_indicator_baseLine = 3500;
		public int shift_indicator_greenLine = 6500;
		public int shift_indicator_redLine = 7500;

		private void Start()
		{
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


			try
			{
				GameObject powerON = Cache.Find("SATSUMA(557kg, 248)/Electricity/PowerON");
				for (int i = 0; i < powerON.transform.childCount; i++)
				{
					GameObject tmp = powerON.transform.GetChild(i).gameObject;
					if (tmp.name == "BeamsShort")
					{
						beamShort = tmp;
					}
					else if (tmp.name == "BeamsLong")
					{
						beamLong = tmp;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.New("Unable to find powerOn object of BeamsShort/BeamsLong",
					"SATSUMA(557kg, 248)/Electricity/PowerON", ex);
			}

			rainIntensity = PlayMakerGlobals.Instance.Variables.FindFsmFloat("RainIntensity");
			GameObject buttonWipers =
				Cache.Find(
					"SATSUMA(557kg, 248)/Dashboard/pivot_dashboard/dashboard(Clone)/pivot_meters/dashboard meters(Clone)/Knobs/ButtonsDash/ButtonWipers");
			PlayMakerFSM[] buttonWipersFSMs = buttonWipers.GetComponents<PlayMakerFSM>();
			foreach (PlayMakerFSM buttonWiperFSM in buttonWipersFSMs)
			{
				if (buttonWiperFSM.FsmName == "Function")
				{
					wiperLogicFSM = buttonWiperFSM;
					break;
				}
			}

			FsmHook.FsmInject(Cache.Find("StreetLights"), "Day", new Action(delegate() { isNight = false; }));
			FsmHook.FsmInject(Cache.Find("StreetLights"), "Night", new Action(delegate() { isNight = true; }));
		}

		private void LoadECU_PanelImageOverride()
		{
			handbrakeSprite = Helper.LoadNewSprite(handbrakeSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_handbrake_icon.png"));
			ecu_InfoPanel_Handbrake.sprite = handbrakeSprite;

			blinkerLeftSprite = Helper.LoadNewSprite(blinkerLeftSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_blinker_left_icon.png"));
			ecu_InfoPanel_IndicatorLeft.sprite = blinkerLeftSprite;

			blinkerRightSprite = Helper.LoadNewSprite(blinkerRightSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_blinker_right_icon.png"));
			ecu_InfoPanel_IndicatorRight.sprite = blinkerRightSprite;

			lowBeamSprite = Helper.LoadNewSprite(lowBeamSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_low_beam_icon.png"));
			ecu_InfoPanel_LowBeam.sprite = lowBeamSprite;

			highBeamSprite = Helper.LoadNewSprite(highBeamSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_high_beam_icon.png"));
			ecu_InfoPanel_HighBeam.sprite = highBeamSprite;

			needleSprite = Helper.LoadNewSprite(needleSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_needle_icon.png"));
			ecu_InfoPanel_Needle.sprite = needleSprite;

			turbineWheelSprite = Helper.LoadNewSprite(turbineWheelSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_turbine_icon.png"));
			ecu_InfoPanel_TurboWheel.sprite = turbineWheelSprite;
		}

		void Update()
		{
			if (CarH.hasPower && infoPanel.bolted)
			{
				if (!isBooted)
				{
					HandleBootAnimation();
				}
				else
				{
					HandleShiftIndicator();
					HandleKeybinds();
					HandleButtonPresses();

					HandleReverseCamera();
					if (rainsensor_enabled)
						HandleRainsensorLogic();
					else if (rainsensor_wasEnabled)
					{
						FsmBool wiperOn = wiperLogicFSM.FsmVariables.FindFsmBool("On");
						FsmFloat wiperDelay = wiperLogicFSM.FsmVariables.FindFsmFloat("Delay");
						wiperOn.Value = false;
						wiperDelay.Value = 0f;
						rainsensor_wasEnabled = false;
					}

					if (lightsensor_enabled)
						HandleLightsensorLogic();
					else if (lightsensor_wasEnabled)
					{
						beamShort.SetActive(false);
						lightsensor_wasEnabled = false;
					}


					DisplayGeneralInfomation();
					pages[currentPage].Handle();
				}
			}
			else
			{
				try
				{
					shift_indicator_renderer.material.color = Color.black;
					ecu_InfoPanel_NeedleObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0, 0));
					rpmIncrementer = 0;
					rpmDecrementer = 9000;

					tenIncrementer = 0;
					hundredIncrementer = 0;
					thousandIncrementer = 0;

					ecu_InfoPanel_Needle.enabled = false;
					ecu_InfoPanel_TurboWheel.enabled = false;
					ecu_InfoPanel_Background.enabled = false;
					ecu_InfoPanel_IndicatorLeft.enabled = false;
					ecu_InfoPanel_IndicatorRight.enabled = false;
					ecu_InfoPanel_Handbrake.enabled = false;
					ecu_InfoPanel_LowBeam.enabled = false;
					ecu_InfoPanel_HighBeam.enabled = false;

					foreach (KeyValuePair<string, TextMesh> display_value in display_values)
					{
						display_value.Value.text = "";
						display_value.Value.color = Color.white;
					}

					isBooting = false;
					isBooted = false;
					currentPage = 0;
				}
				catch
				{
				}
			}
		}

		private void HandleReverseCamera()
		{
			if (!mod.reverseCamera.bolted)
			{
				ecu_InfoPanel_Display_Reverse_Camera.enabled = false;
				mod.SetReverseCameraEnabled(false);
				return;
			}

			if (CarH.drivetrain.gear == 0)
			{
				ecu_InfoPanel_Display_Reverse_Camera.enabled = true;
				mod.SetReverseCameraEnabled(true);
				return;
			}

			ecu_InfoPanel_Display_Reverse_Camera.enabled = false;
			mod.SetReverseCameraEnabled(false);
			return;
		}

		private void HandleRainsensorLogic()
		{
			rainsensor_wasEnabled = true;
			FsmBool wiperOn = wiperLogicFSM.FsmVariables.FindFsmBool("On");
			FsmFloat wiperDelay = wiperLogicFSM.FsmVariables.FindFsmFloat("Delay");
			if (rainsensor_enabled)
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
			else
			{
				wiperOn.Value = false;
				wiperDelay.Value = 0f;
			}
		}

		private void HandleLightsensorLogic()
		{
			lightsensor_wasEnabled = true;
			if (lightsensor_enabled)
			{
				if (this.isNight)
				{
					beamShort.SetActive(true);
					return;
				}

				beamShort.SetActive(false);
			}
		}

		private void HandleBootAnimation()
		{
			if (isBooting)
			{
				Play_ECU_InfoPanel_Animation();
			}
			else
			{
				currentPage = 0;
				ChangeInfoPanelPage(currentPage);
				ecu_InfoPanel_Background.sprite = pages[currentPage].pageSprite;

				ecu_InfoPanel_Needle.enabled = pages[currentPage].needleUsed;
				ecu_InfoPanel_TurboWheel.enabled = pages[currentPage].turbineUsed;
				ecu_InfoPanel_Background.enabled = true;
				ecu_InfoPanel_IndicatorLeft.enabled = false;
				ecu_InfoPanel_IndicatorRight.enabled = false;
				ecu_InfoPanel_Handbrake.enabled = false;
				ecu_InfoPanel_LowBeam.enabled = false;
				ecu_InfoPanel_HighBeam.enabled = false;
				foreach (KeyValuePair<string, TextMesh> display_value in display_values)
				{
					display_value.Value.gameObject.GetComponent<MeshRenderer>().enabled = true;
					display_value.Value.text = "";
				}

				isBooting = true;
			}
		}

		public void SetupShiftIndicator()
		{
			shift_indicator_gradient = new Gradient();
			GradientColorKey[] colorKey = new GradientColorKey[3];
			colorKey[0].color = new Color(1.0f, 0.64f, 0.0f); //Orange
			colorKey[0].time = (float) shift_indicator_baseLine / 10000;

			colorKey[1].color = Color.green;
			colorKey[1].time = (float) shift_indicator_greenLine / 10000;

			colorKey[2].color = Color.red;
			colorKey[2].time = (float) shift_indicator_redLine / 10000;

			GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
			alphaKey[0].alpha = 1f - (float) shift_indicator_baseLine / 10000;
			alphaKey[0].time = (float) shift_indicator_baseLine / 10000;

			alphaKey[1].alpha = 1f - (float) shift_indicator_greenLine / 10000;
			alphaKey[1].time = (float) shift_indicator_greenLine / 10000;

			alphaKey[2].alpha = 1f - (float) shift_indicator_redLine / 10000;
			alphaKey[2].time = (float) shift_indicator_redLine / 10000;

			shift_indicator_gradient.SetKeys(colorKey, alphaKey);
		}

		private void HandleShiftIndicator()
		{
			if (CarH.running)
			{
				float gradientValue = CarH.drivetrain.rpm / 10000;

				if (CarH.drivetrain.rpm >= 7500)
				{
					shift_indicator_blink_timer += Time.deltaTime;

					if (shift_indicator_blink_timer <= 0.15f)
					{
						shift_indicator_renderer.material.color = Color.black;
					}

					if (shift_indicator_blink_timer >= 0.3f)
					{
						shift_indicator_blink_timer = 0;

						shift_indicator_renderer.material.color = shift_indicator_gradient.Evaluate(gradientValue);
					}
				}
				else
				{
					shift_indicator_renderer.material.color = shift_indicator_gradient.Evaluate(gradientValue);
				}
			}
			else
			{
				shift_indicator_renderer.material.color = Color.black;
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
					pages[currentPage].PressedButton(PressedButton.Circle, selectedSetting);
				}
				else if (mod.cross.GetKeybindDown())
				{
					pages[currentPage].PressedButton(PressedButton.Cross, selectedSetting);
				}
				else if (mod.plus.GetKeybindDown())
				{
					pages[currentPage].PressedButton(PressedButton.Plus, selectedSetting);
				}
				else if (mod.minus.GetKeybindDown())
				{
					pages[currentPage].PressedButton(PressedButton.Minus, selectedSetting);
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
					pages[pageNumer].PressedButton(PressedButton.Circle, selectedSetting);
					return;
				case "buttonCross":
					pages[pageNumer].PressedButton(PressedButton.Cross, selectedSetting);
					return;
				case "buttonMinus":
					pages[pageNumer].PressedButton(PressedButton.Minus, selectedSetting);
					return;
				case "buttonPlus":
					pages[pageNumer].PressedButton(PressedButton.Plus, selectedSetting);
					return;
			}

			return;
		}

		private void OnArrowUp()
		{
			currentPage++;
			if (currentPage > pages.Count - 1)
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
				currentPage = pages.Count - 1;
			}

			ChangeInfoPanelPage(currentPage);
		}

		private void ChangeInfoPanelPage(int currentPage)
		{
			ecu_InfoPanel_Background.sprite = pages[currentPage].pageSprite;
			ecu_InfoPanel_Needle.enabled = pages[currentPage].needleUsed;
			ecu_InfoPanel_TurboWheel.enabled = pages[currentPage].turbineUsed;

			foreach (KeyValuePair<string, TextMesh> display_value in display_values)
			{
				display_value.Value.text = "";
				display_value.Value.color = Color.white;
			}

			selectedSetting = "";
		}

		private void DisplayGeneralInfomation()
		{
			ecu_InfoPanel_LowBeam.enabled = beamShort.activeSelf;
			ecu_InfoPanel_HighBeam.enabled = beamLong.activeSelf;
			if (CarH.carController.handbrakeInput > 0)
			{
				ecu_InfoPanel_Handbrake.enabled = true;
			}
			else
			{
				ecu_InfoPanel_Handbrake.enabled = false;
			}


			if (blinkerLeft == null)
			{
				blinkerLeft = Cache.Find("BlinkLeft");
			}
			else
			{
				ecu_InfoPanel_IndicatorLeft.enabled = blinkerLeft.activeSelf;
			}

			if (blinkerRight == null)
			{
				blinkerRight = Cache.Find("BlinkRight");
			}
			else
			{
				ecu_InfoPanel_IndicatorRight.enabled = blinkerRight.activeSelf;
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

				ecu_InfoPanel_NeedleObject.transform.localRotation =
					Quaternion.Euler(new Vector3(-90f, GetRPMRotation(rpmDecrementer), 0));

				display_values["value_1"].text = rpmDecrementer.ToString();
				display_values["value_2"].text = hundredIncrementer.ToString();
				display_values["value_3"].text = tenIncrementer.ToString() + "." + tenIncrementer.ToString();
				display_values["value_4"].text = tenIncrementer.ToString();
				display_values["value_13"].text = "";
				display_values["value_14"].text = tenIncrementer.ToString("00 .0") + "V";
				display_values["value_15"].text = thousandIncrementer.ToString();
				display_values["value_16"].text = tenIncrementer.ToString();
				display_values["value_kmh"].text = hundredIncrementer.ToString();
				display_values["value_km"].text = (thousandIncrementer * 10f).ToString();
			}
			else if (rpmIncrementer < 9000)
			{
				rpmIncrementer += rpmAdder;
				tenIncrementer += tenAdder;
				hundredIncrementer += hundredAdder;
				thousandIncrementer += thousandAdder;

				ecu_InfoPanel_NeedleObject.transform.localRotation =
					Quaternion.Euler(new Vector3(-90f, GetRPMRotation(rpmIncrementer), 0));

				display_values["value_1"].text = rpmIncrementer.ToString();
				display_values["value_2"].text = hundredIncrementer.ToString();
				display_values["value_3"].text = tenIncrementer.ToString() + "." + tenIncrementer.ToString();
				display_values["value_4"].text = tenIncrementer.ToString();
				display_values["value_13"].text = "Boot up";
				display_values["value_14"].text = tenIncrementer.ToString("00 .0") + "V";
				display_values["value_15"].text = thousandIncrementer.ToString();
				display_values["value_16"].text = tenIncrementer.ToString();
				display_values["value_kmh"].text = hundredIncrementer.ToString();
				display_values["value_km"].text = (thousandIncrementer * 10f).ToString();
			}

			if (rpmIncrementer >= 9000 && rpmDecrementer <= 0)
			{
				rpmIncrementer = 0;
				rpmDecrementer = 9000;
				isBooted = true;
				isBooting = false;
			}
		}

		public void Init(InfoPanel infoPanel, DonnerTech_ECU_Mod mod, AssetBundle assetBundle)
		{
			panel = this.gameObject;
			this.infoPanel = infoPanel;
			this.mod = mod;

			TextMesh[] ecu_InfoPanel_TextMeshes = panel.GetComponentsInChildren<TextMesh>(true);
			foreach (TextMesh textMesh in ecu_InfoPanel_TextMeshes)
			{
				switch (textMesh.name)
				{
					case "displayGear":
						display_values.Add("value_gear", textMesh);
						break;
					case "displayKmH":
						display_values.Add("value_kmh", textMesh);
						break;
					case "displayKm":
						display_values.Add("value_km", textMesh);
						break;
				}

				for (int i = 1; i <= 16; i++)
				{
					if (textMesh.name == ("displayValue_" + i.ToString().PadLeft(2, '0')))
					{
						display_values.Add("value_" + i, textMesh);
						continue;
					}
				}

				textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
			}

			SpriteRenderer[] ecu_InfoPanel_SpriteRenderer = panel.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (SpriteRenderer spriteRenderer in ecu_InfoPanel_SpriteRenderer)
			{
				switch (spriteRenderer.name)
				{
					case "needle":
						ecu_InfoPanel_Needle = spriteRenderer;
						break;
					case "turbine":
						ecu_InfoPanel_TurboWheel = spriteRenderer;
						break;
					case "background":
						ecu_InfoPanel_Background = spriteRenderer;
						break;
					case "indicatorLeft":
						ecu_InfoPanel_IndicatorLeft = spriteRenderer;
						break;
					case "indicatorRight":
						ecu_InfoPanel_IndicatorRight = spriteRenderer;
						break;
					case "handbrake":
						ecu_InfoPanel_Handbrake = spriteRenderer;
						break;
					case "lowBeam":
						ecu_InfoPanel_LowBeam = spriteRenderer;
						break;
					case "highBeam":
						ecu_InfoPanel_HighBeam = spriteRenderer;
						break;
				}

				spriteRenderer.enabled = false;
			}

			ecu_InfoPanel_NeedleObject = panel.transform.FindChild("needle").gameObject;
			ecu_InfoPanel_TurboWheelObject = panel.transform.FindChild("turbine").gameObject;
			ecu_InfoPanel_Display_Reverse_Camera =
				panel.transform.FindChild("reverseCamera").GetComponent<MeshRenderer>();
			ecu_InfoPanel_Display_Reverse_Camera.enabled = false;


			shift_indicator_renderer = panel.transform.FindChild("shiftIndicator").GetComponent<MeshRenderer>();
			SetupShiftIndicator();

			needleSprite = assetBundle.LoadAsset<Sprite>("Rpm-Needle.png");
			turbineWheelSprite = assetBundle.LoadAsset<Sprite>("TurbineWheel.png");

			handbrakeSprite = assetBundle.LoadAsset<Sprite>("Handbrake-Icon.png");
			blinkerLeftSprite = assetBundle.LoadAsset<Sprite>("Indicator-Left-Icon.png");
			blinkerRightSprite = assetBundle.LoadAsset<Sprite>("Indicator-Right-Icon.png");
			highBeamSprite = assetBundle.LoadAsset<Sprite>("HighBeam-Icon.png");
			lowBeamSprite = assetBundle.LoadAsset<Sprite>("LowBeam-Icon.png");

			LoadECU_PanelImageOverride();
			InfoPanelBaseInfo infoPanelBaseInfo = new InfoPanelBaseInfo(mod, assetBundle, display_values, this);
			pages = new List<InfoPanelPage>
			{
				new Main("main_page", ecu_InfoPanel_NeedleObject, infoPanelBaseInfo),
				new Modules("modules_page", ecu_InfoPanel_NeedleObject, infoPanelBaseInfo),
				new Faults("faults_page", infoPanelBaseInfo),
				new Faults2("faults2_page", infoPanelBaseInfo),
				new Assistance("assistance_page", infoPanelBaseInfo),
			};


			if (mod.turboModInstalled)
			{
				pages.Add(new Turbocharger("turbocharger_page", ecu_InfoPanel_TurboWheelObject, infoPanelBaseInfo));
			}

			if ((bool) mod.enableAirrideInfoPanelPage.Value)
			{
				pages.Add(new Airride("airride_page", infoPanelBaseInfo));
			}
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