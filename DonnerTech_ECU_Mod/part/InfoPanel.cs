using System.Collections.Generic;
using System.IO;
using DonnerTech_ECU_Mod.info_panel_pages;
using MSCLoader;
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

		protected bool workaroundChildDisableDone = false;
		public InfoPanel_Logic logic;

		public Dictionary<string, TextMesh> displayValues;
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

		public float needleRotation
		{
			set => needleObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, value, 0));
		}


		public InfoPanel(GamePart parent, AbsModule absModule, EspModule espModule, TcsModule tcsModule, DonnerTech_ECU_Mod mod, AssetBundle assetBundle) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			AddEventListener(PartEvent.Time.Post, PartEvent.Type.Install,
				delegate { transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); });
			AddScrews(new[]
			{
				new Screw(new Vector3(0f, -0.025f, -0.082f), new Vector3(180, 0, 0))
			}, 0.8f, 8);


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
			}

			displayValues = LoadDisplayValues();
			LoadRenderers();
			LoadSpriteOverride(mod, assetBundle);

			needleObject = transform.FindChild("needle").gameObject;
			InfoPanelBaseInfo infoPanelBaseInfo = new InfoPanelBaseInfo(mod, assetBundle, displayValues, logic);
			pages = new List<InfoPanelPage>
			{
				new Main("main_page", needleObject, infoPanelBaseInfo),
				new Modules("modules_page", needleObject, infoPanelBaseInfo),
				new Faults("faults_page", infoPanelBaseInfo),
				new Faults2("faults2_page", infoPanelBaseInfo),
				new Assistance("assistance_page", infoPanelBaseInfo),
			};


			if (mod.turboModInstalled)
			{
				pages.Add(new Turbocharger("turbocharger_page", transform.FindChild("turbine").gameObject, infoPanelBaseInfo));
			}

			if ((bool)mod.enableAirrideInfoPanelPage.Value)
			{
				pages.Add(new Airride("airride_page", this, infoPanelBaseInfo));
			}

			logic = AddEventBehaviour<InfoPanel_Logic>(PartEvent.Type.Install);
			logic.Init(this, mod, assetBundle);
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

				textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
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
			if (transform.localScale.x < 1.5f)
			{
				transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
			}

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
		}
	}
}