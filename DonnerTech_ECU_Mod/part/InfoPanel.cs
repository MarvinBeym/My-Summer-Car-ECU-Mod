﻿using System;
using System.Collections.Generic;
using System.IO;
using DonnerTech_ECU_Mod.info_panel_pages;
using DonnerTech_ECU_Mod.part.Module;
using HutongGames.PlayMaker;
using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using MscModApi.Parts.ReplacePart;
using MscModApi.Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class InfoPanel : DerivablePart
	{
		protected override string partId => "info_panel";
		protected override string partName => "Info Panel";
		protected override Vector3 partInstallPosition => new Vector3(0.25f, -0.088f, -0.01f);
		protected override Vector3 partInstallRotation => new Vector3(0, 180, 180);

		public const string VALUE_1 = "1";
		public const string VALUE_2 = "2";
		public const string VALUE_3 = "3";
		public const string VALUE_4 = "4";
		public const string VALUE_5 = "5";
		public const string VALUE_6 = "6";
		public const string VALUE_7 = "7";
		public const string VALUE_8 = "7";
		public const string VALUE_9 = "9";
		public const string VALUE_10 = "10";
		public const string VALUE_11 = "11";
		public const string VALUE_12 = "12";
		public const string VALUE_13 = "13";
		public const string VALUE_14 = "14";
		public const string VALUE_15 = "15";
		public const string VALUE_16 = "16";
		public const string VALUE_GEAR = "gear";
		public const string VALUE_KMH = "kmh";
		public const string VALUE_KM = "km";


		protected bool workaroundChildDisableDone = false;
		public InfoPanel_Logic logic;

		private Dictionary<string, TextMesh> displayValues;
		public List<InfoPanelPage> pages;

		private List<Renderer> renderers = new List<Renderer>();
		public SpriteRenderer background;
		public SpriteRenderer needle;
		public SpriteRenderer handbrake;
		public SpriteRenderer indicatorLeft;
		public SpriteRenderer indicatorRight;
		public SpriteRenderer lowBeam;
		public SpriteRenderer highBeam;
		public SpriteRenderer turboWheel;
		public MeshRenderer reverseCameraRenderer;

		private GameObject needleObject;

		public bool isBooted = false;
		public bool isBooting = false;

		//ShiftIndicator
		public int shiftIndicatorBaseLine = 3500;
		public int shiftIndicatorGreenLine = 6500;
		public int shiftIndicatorRedLine = 7500;
		public ShiftIndicatorLogic shiftIndicatorLogic;

		public AbsModule absModule;
		public EspModule espModule;
		public TcsModule tcsModule;
		public readonly RainLightSensorBoard rainLightSensorboard;


		//Scale workaround stuff
		private bool infoPanelInHand;

		public float needleRotation
		{
			set => needleObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, value, 0));
		}


		public InfoPanel(GamePart parent, AbsModule absModule, EspModule espModule, TcsModule tcsModule, RainLightSensorBoard rainLightSensorboard, DonnerTech_ECU_Mod mod, AssetBundle assetBundle) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			this.absModule = absModule;
			this.espModule = espModule;
			this.tcsModule = tcsModule;
			this.rainLightSensorboard = rainLightSensorboard;

			SetupScaleFix();

			AddScrews(new[]
			{
				new Screw(new Vector3(0f, -0.025f, -0.082f), new Vector3(180, 0, 0))
			}, 0.8f, 8);

			/*
			TextMesh[] textMeshes = gameObject.GetComponentsInChildren<TextMesh>(true);
			foreach (TextMesh textMesh in textMeshes)
			{
				textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
			}

			SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (SpriteRenderer spriteRenderer in spriteRenderers)
			{
				spriteRenderer.enabled = false;
			}

			if (spriteRenderers.Length > 0 && textMeshes.Length > 0)
			{
				workaroundChildDisableDone = true;
			}*/

			displayValues = LoadDisplayValues();
			LoadRenderers();
			LoadSpriteOverride(mod, assetBundle);

			needleObject = transform.FindChild("needle").gameObject;

			logic = AddEventBehaviour<InfoPanel_Logic>(PartEvent.Type.Install);
			logic.Init(this, mod, assetBundle);

			shiftIndicatorLogic = AddEventBehaviour<ShiftIndicatorLogic>(PartEvent.Type.Install);
			shiftIndicatorLogic.Init(this);

			InfoPanelBaseInfo infoPanelBaseInfo = new InfoPanelBaseInfo(mod, assetBundle, displayValues, logic);
			pages = new List<InfoPanelPage>
			{
				new Main("main_page", this, needleObject, infoPanelBaseInfo),
				new Modules("modules_page", this, needleObject, infoPanelBaseInfo),
				new Faults("faults_page", this, infoPanelBaseInfo),
				new Faults2("faults2_page", this, infoPanelBaseInfo),
				new Assistance("assistance_page", this, infoPanelBaseInfo),
			};


			if (mod.turboModInstalled)
			{
				pages.Add(new Turbocharger("turbocharger_page", this, transform.FindChild("turbine").gameObject, infoPanelBaseInfo));
			}

			if ((bool)mod.enableAirrideInfoPanelPage.Value)
			{
				pages.Add(new Airride("airride_page", this, infoPanelBaseInfo));
			}
		}

		private Dictionary<string, TextMesh> LoadDisplayValues()
		{
			Dictionary<string, TextMesh> displayValues = new Dictionary<string, TextMesh>();

			TextMesh[] ecu_InfoPanel_TextMeshes = gameObject.GetComponentsInChildren<TextMesh>(true);
			foreach (TextMesh textMesh in ecu_InfoPanel_TextMeshes)
			{
				switch (textMesh.name)
				{
					case "displayGear":
						displayValues.Add("value_gear", textMesh);
						break;
					case "displayKmH":
						displayValues.Add("value_kmh", textMesh);
						break;
					case "displayKm":
						displayValues.Add("value_km", textMesh);
						break;
				}

				for (int i = 1; i <= 16; i++)
				{
					if (textMesh.name == ("displayValue_" + i.ToString().PadLeft(2, '0')))
					{
						displayValues.Add("value_" + i, textMesh);
					}
				}
			}

			return displayValues;
		}

		public void ClearDisplayValues()
		{
			foreach (KeyValuePair<string, TextMesh> displayValue in displayValues)
			{
				displayValue.Value.text = "";
				displayValue.Value.color = Color.white;
			}
		}

		private void LoadRenderers()
		{
			foreach (SpriteRenderer spriteRenderer in gameObject.GetComponentsInChildren<SpriteRenderer>(true))
			{
				switch (spriteRenderer.name)
				{
					case "needle":
						needle = spriteRenderer;
						break;
					case "turbine":
						turboWheel = spriteRenderer;
						break;
					case "background":
						background = spriteRenderer;
						break;
					case "indicatorLeft":
						indicatorLeft = spriteRenderer;
						break;
					case "indicatorRight":
						indicatorRight = spriteRenderer;
						break;
					case "handbrake":
						handbrake = spriteRenderer;
						break;
					case "lowBeam":
						lowBeam = spriteRenderer;
						break;
					case "highBeam":
						highBeam = spriteRenderer;
						break;
					default:
						continue;
				}

				renderers.Add(spriteRenderer);
				spriteRenderer.enabled = false;
			}

			reverseCameraRenderer = transform.FindChild("reverseCamera").GetComponent<MeshRenderer>();
			reverseCameraRenderer.enabled = false;
			renderers.Add(reverseCameraRenderer);
		}

		private void LoadSpriteOverride(Mod mod, AssetBundle assetBundle)
		{
			Sprite needleSprite = assetBundle.LoadAsset<Sprite>("Rpm-Needle.png");
			Sprite turbineWheelSprite = assetBundle.LoadAsset<Sprite>("TurbineWheel.png");
			Sprite handbrakeSprite = assetBundle.LoadAsset<Sprite>("Handbrake-Icon.png");
			Sprite blinkerLeftSprite = assetBundle.LoadAsset<Sprite>("Indicator-Left-Icon.png");
			Sprite blinkerRightSprite = assetBundle.LoadAsset<Sprite>("Indicator-Right-Icon.png");
			Sprite lowBeamSprite = assetBundle.LoadAsset<Sprite>("LowBeam-Icon.png");
			Sprite highBeamSprite = assetBundle.LoadAsset<Sprite>("HighBeam-Icon.png");

			needleSprite = Helper.LoadNewSprite(needleSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_needle_icon.png"));
			needle.sprite = needleSprite;

			handbrakeSprite = Helper.LoadNewSprite(handbrakeSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_handbrake_icon.png"));
			handbrake.sprite = handbrakeSprite;

			blinkerLeftSprite = Helper.LoadNewSprite(blinkerLeftSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_blinker_left_icon.png"));
			indicatorLeft.sprite = blinkerLeftSprite;

			blinkerRightSprite = Helper.LoadNewSprite(blinkerRightSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_blinker_right_icon.png"));
			indicatorRight.sprite = blinkerRightSprite;

			lowBeamSprite = Helper.LoadNewSprite(lowBeamSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_low_beam_icon.png"));
			lowBeam.sprite = lowBeamSprite;

			highBeamSprite = Helper.LoadNewSprite(highBeamSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_high_beam_icon.png"));
			highBeam.sprite = highBeamSprite;

			turbineWheelSprite = Helper.LoadNewSprite(turbineWheelSprite,
				Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE_turbine_icon.png"));
			turboWheel.sprite = turbineWheelSprite;

		}

		public void SetAllRendererEnabledState(bool state)
		{
			renderers.ForEach((Renderer renderer) =>
			{
				renderer.enabled = state;
			});
		}

		public void Handle()
		{
			/*
			if (!installed)
			{
				if (!workaroundChildDisableDone)
				{
					TextMesh[] textMeshes = gameObject.GetComponentsInChildren<TextMesh>(true);
					foreach (TextMesh textMesh in textMeshes)
					{
						textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
					}


					SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
					foreach (var spriteRenderer in spriteRenderers)
					{
						spriteRenderer.enabled = false;
					}

					if (spriteRenderers.Length > 0 && textMeshes.Length > 0)
					{
						workaroundChildDisableDone = true;
					}
				}
			}
			*/
		}

		/// <summary>
		/// Should be rather moved to MscModApi (Already exists in MscModApi but not publicly accessible and only available in GamePart class)
		/// </summary>
		private void AddActionAsFirst(FsmState fsmState, Action action)
		{
			if (fsmState == null)
			{
				return;
			}

			var actions = new List<FsmStateAction>(fsmState.Actions);
			actions.Insert(0, new FsmAction(action));
			fsmState.Actions = actions.ToArray();
		}

		/// <summary>
		/// Should be rather moved to MscModApi (Already exists in MscModApi but not publicly accessible and only available in GamePart class)
		/// </summary>
		private void AddActionAsLast(FsmState fsmState, Action action)
		{
			if (fsmState == null)
			{
				return;
			}

			var actions = new List<FsmStateAction>(fsmState.Actions) { new FsmAction(action) };
			fsmState.Actions = actions.ToArray();
		}

		public InfoPanel SetDisplayValue(string index, string value)
		{
			string key = $"value_{index}";
			if (displayValues.TryGetValue(key, out var displayValue))
			{
				displayValue.text = value;
			}

			return this;
		}

		public string GetDisplayValue(string index)
		{
			string key = $"value_{index}";
			if (displayValues.TryGetValue(key, out var displayValue))
			{
				return displayValue.text;
			}

			return "";
		}

		public InfoPanel SetDisplayValueColor(string index, Color color)
		{
			string key = $"value_{index}";
			if (displayValues.TryGetValue(key, out var displayValue))
			{
				displayValue.color = color;
			}

			return this;
		}

		public InfoPanel SetDisplayValue(string index, int value, string format = "")
		{
			SetDisplayValue(index, format != "" ? value.ToString(format) : value.ToString());
			return this;
		}

		public InfoPanel SetDisplayValue(string index, float value, string format = "")
		{
			SetDisplayValue(index, format != "" ? value.ToString(format) : value.ToString());
			return this;
		}

		/// <summary>
		/// Should be fixed by actually scaling the 3D-model in the future so it stays as 1.0 in Unity
		/// </summary>
		private void SetupScaleFix()
		{
			GameObject hand = Cache.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand");

			PlayMakerFSM pickUpFsm = hand.FindFsm("PickUp");
			FsmGameObject pickedGameObject = pickUpFsm.FsmVariables.GetFsmGameObject("PickedObject");
			if (!pickUpFsm.Fsm.Initialized)
			{
				pickUpFsm.InitializeFSM();
			}
			FsmState waitState = pickUpFsm.FindState("Wait");


			AddActionAsFirst(waitState, () =>
			{
				infoPanelInHand = pickedGameObject.Value == gameObject;
			});
			AddActionAsLast(waitState, () =>
			{
				if (infoPanelInHand)
				{
					transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
				}
			});
		}
	}
}