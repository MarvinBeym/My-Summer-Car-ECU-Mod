using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DonnerTech_ECU_Mod.fuelsystem;
using DonnerTech_ECU_Mod.infoPanel;
using DonnerTech_ECU_Mod.Parts;
using HutongGames.PlayMaker;
using ModShop;
using MSCLoader;
using MscModApi;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Shopping;
using MscModApi.Tools;
using Tools.gui;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DonnerTech_ECU_Mod
{
	public class DonnerTech_ECU_Mod : Mod
	{
#if DEBUG
		/*  ToDo:
		 *  Remove Autotune?`-> maybe replace with something else
		 *  Maybe remove the faults page -> maybe replace with actual Fault codes that get displayed
		 *  
		 *  Add option to chip that helps with starting the car from a standstill
		 *  
		 *  finish fuel injection logic
		 *  Prevent distzributor and electrics from beeing installed if any fuel system part is installed
		 *  Prevent fuel system part install when distributor or electrics is installed
		 *  Make some/all plugs larger to prevent them from clipping
		 *  
		 *  ?Somehow make replacing parts easier
		 *  Check if logic is working
		 *  Improve code by moving stuff to new classes/writing small classes to handle multiple times doing the same
		 *  ?Move creating/setting of all fuel system parts (loading from asset bundle, creating parts (SimplePart, ...) into fuel system class
		 *  Fix parts falling through game world (make box collider larger)
		 *  Reduce 3d model vertex (Clean up) (?Maybe there is an automatic extractor when exporting as .obj)
		 *  Create class for handling boxes (defining what (and how many parts a box should be able to spawn and handle the logic in one class (+ logic mono behaviour class)
		 *  ?Wear of parts (with malfunction like backfire or wrong A/F Ratio)
		 *  Test mod (with / without fuel injection installed, when removing fuel injection, when installing fuel injection)
		 *    
		 *  
		 *  ========> Turbo
		 *  ???make person die if head in big turbo  :D
		 *  Make manifold for fuel injection.
		 *  Make parts boltable
		 *  
		 *  
		 *  https://en.wikipedia.org/wiki/Advanced_driver-assistance_systems#Feature_Examples
		 *  possible features:
		 *  Automatic parking
		 *  (not possible) Navigation (GPS)
		 *  Backup camera
		 *  Blind spot monitor
		 *  Collision avoidance system
		 *  Driver drowsiness detection (drunk?)
		 *  Driver Monitoring System
		 *  Emergency driver assistant
		 *  Forward Collision Warning
		 *  Hill descent control
		 *  Hill-Start Assist
		 *  Normal-Start Assist
		 *  (not possible) Lane centering
		 *  (maybe still possible) Lane departure warning system
		 *  Parking sensor
		 *  Pedestrian protection system -> Gameobject HumanTriggerCrime?Vi
		 *  Rain sensor
		 *  Tire Pressure Monitoring
		 *  Traffic sign recognition
		 *  Turning assistant
		 *  (not possible) Wrong-way driving warning
		 *  Save state of modules and load them
		 *  Save all information in single file/object
		 */

		/*  Changelog (v1.4.12)
		 *  Improved debug gui
		 *  Massive code refactoring
		 *  Code improvement
		 *  Code improvement
		 *  Loading time improvement
		 *  Performance improvement
		 *  Added copyright notice to mod settings
		 *  Remove BugReporter
		 *  Change to using MscModApi instead of ModApi & ScrewablePartApi
		 *  Replace ModsShop with MscModApi Shop
		 *  Fuel injection now actually uses air/fuel ratio in the programmer
		 *  Remove smooth input option from settings
		 *  Fix parts no longer resetting
		 *  
		 */
		/* BUGS/Need to fix
		 * WARNING: both twincarb and fuel injection manifold can be installed (twincarb und carb trigger shold be disabled when anyInstalled
		 * Racing Carb bleibt installiert nachdem ein teil vom fuel injection entfernt wird. *Bis manifold entfernt wird

		 * ECU mod: add ERRor to display if something is wrong
		 */
#endif
		public override string ID => "DonnerTech_ECU_Mod"; //Your mod ID (unique)
		public override string Name => "DonnerTechRacing ECUs"; //You mod name
		public override string Author => "DonnerPlays"; //Your Username
		public override string Version => "1.4.11"; //Version
		public override bool UseAssetsFolder => true;

		public AssetBundle assetBundle;
		public GuiDebug guiDebug;
		public bool turboModInstalled;

		public GameObject ecu_mod_gameObject;

		internal static List<Part> partsList { get; set; }= new List<Part>();

		//Keybinds
		public Keybind highestKeybind = new Keybind("airride_highest", "Airride Highest", KeyCode.LeftArrow);
		public Keybind lowestKeybind = new Keybind("airride_lowest", "Airride Lowest", KeyCode.RightArrow);
		public Keybind increaseKeybind = new Keybind("airride_increase", "Airride Increase", KeyCode.UpArrow);
		public Keybind decreaseKeybind = new Keybind("airride_decrease", "Airride Decrease", KeyCode.DownArrow);
		public Keybind arrowUp = new Keybind("info_panel_arrowUp", "Arrow Up", KeyCode.Keypad8);
		public Keybind arrowDown = new Keybind("info_panel_arrowDown", "Arrow Down", KeyCode.Keypad2);
		public Keybind circle = new Keybind("info_panel_circle", "Circle", KeyCode.KeypadEnter);
		public Keybind cross = new Keybind("info_panel_cross", "Cross", KeyCode.KeypadPeriod);
		public Keybind plus = new Keybind("info_panel_plus", "Plus", KeyCode.KeypadPlus);
		public Keybind minus = new Keybind("info_panel_minus", "Minus", KeyCode.KeypadMinus);

		//FuelSystem
		public FuelSystem fuel_system;

		//InfoPanel
		public InfoPanel info_panel;

		public FsmString playerCurrentVehicle;

		//Part logic
		public CruiseControl_Logic cruise_control_logic;
		public SmartEngineModule_Logic smart_engine_module_logic;

		public ReverseCamera_Logic reverse_camera_logic;


		public Box fuel_injectors_box;
		public Box throttle_bodies_box;

		internal PartBaseInfo partBaseInfo;

		public Part abs_module_part;
		public Part esp_module_part;
		public Part tcs_module_part;

		public Part cable_harness_part;
		public Part mounting_plate_part;

		public Part smart_engine_module_part;
		public Part cruise_control_panel_part;
		public Part info_panel_part;

		public Part reverse_camera_part;
		public Part rain_light_sensorboard_part;

		public Part fuel_pump_cover_part;
		public Part fuel_injection_manifold_part;
		public Part fuel_rail_part;
		public Part electric_fuel_pump_part;

		public Part chip_programmer_part;

		public MeshRenderer wires_injectors_pumps;
		public MeshRenderer wires_sparkPlugs1;
		public MeshRenderer wires_sparkPlugs2;

		public Vector3 reverse_camera_installLocation = new Vector3(0, -0.343f, -0.157f);
		public Vector3 rain_light_sensorboard_installLocation = new Vector3(-0.0015f, 0.086f, 0.1235f);

		public Vector3 fuel_injector1_installLocation = new Vector3(0.105f, 0.0074f, -0.0012f);
		public Vector3 fuel_injector2_installLocation = new Vector3(0.0675f, 0.0074f, -0.0012f);
		public Vector3 fuel_injector3_installLocation = new Vector3(-0.068f, 0.0074f, -0.0012f);
		public Vector3 fuel_injector4_installLocation = new Vector3(-0.105f, 0.0074f, -0.0012f);

		public Vector3 throttle_body1_installLocation = new Vector3(0.095f, 0.034f, 0.0785f);
		public Vector3 throttle_body2_installLocation = new Vector3(0.033f, 0.034f, 0.0785f);
		public Vector3 throttle_body3_installLocation = new Vector3(-0.033f, 0.034f, 0.0785f);
		public Vector3 throttle_body4_installLocation = new Vector3(-0.095f, 0.034f, 0.0785f);

		public Vector3 electric_fuel_pump_installLocation = new Vector3(-0.0822f, 0.125f, 0.9965f);
		public Vector3 fuel_pump_cover_installLocation = new Vector3(-0.0515f, 0.105f, 0.006f);
		public Vector3 fuel_injection_manifold_installLocation = new Vector3(-0.009f, -0.0775f, 0.02f);
		public Vector3 fuel_rail_installLocation = new Vector3(0, 0.03f, 0.012f);

		public Vector3 cruise_control_panel_installLocation = new Vector3(0.5f, -0.095f, 0.08f);
		public Vector3 info_panel_installLocation = new Vector3(0.25f, -0.088f, -0.01f);

		public Vector3 abs_module_installLocation = new Vector3(0.058f, 0.022f, 0.116f);
		public Vector3 esp_module_installLocation = new Vector3(0.0235f, 0.023f, -0.0245f);
		public Vector3 smart_engine_module_installLocation = new Vector3(0.072f, 0.024f, -0.1425f);
		public Vector3 tcs_module_installLocation = new Vector3(-0.03f, 0.0235f, -0.154f);
		public Vector3 cable_harness_installLocation = new Vector3(-0.117f, 0.0102f, -0.024f);
		public Vector3 mounting_plate_installLocation = new Vector3(0.3115f, -0.276f, -0.0393f);

		private Settings debugGuiSetting = new Settings("debugGuiSetting", "Show DEBUG GUI", false);
		private Settings resetPosSetting = new Settings("resetPos", "Reset", Helper.WorkAroundAction);

		public Settings settingThrottleBodyValveRotation =
			new Settings("settingThrottleBodyValveRotation", "Throttle body valve rotation", true);

		private Settings toggleSixGears =
			new Settings("toggleSixGears", "SixGears Mod (with gear ratio changes)", false);

		public Settings enableAirrideInfoPanelPage = new Settings("enableAirrideInfoPanelPage",
			"Enable airride info panel page & airride logic (Has to be enabled/disabled before load)", false);

		private Settings toggleAWD = new Settings("toggleAWD", "All Wheel Drive (AWD)", false);

		private static float[] originalGearRatios;

		private static float[] newGearRatio =
		{
			-4.093f, // reverse
			0f, // neutral
			3.4f, // 1st
			1.8f, // 2nd
			1.4f, // 3rd
			1.0f, // 4th
			0.8f, // 5th
			0.65f // 6th
		};


		public override void OnNewGame()
		{
			MscModApi.MscModApi.NewGameCleanUp(this);

			/*
			fuel_system.chips.ForEach(delegate (Chip chip) {
				SaveLoad.SerializeSaveFile<ChipSave>(this, null, chip.mapSaveFile);
			});
			*/
		}

		public override void OnLoad()
		{
			ModConsole.Print(Name + $" [v{Version}");

			turboModInstalled = ModLoader.IsModPresent("SatsumaTurboCharger");
			guiDebug = new GuiDebug(turboModInstalled ? Screen.width - 310 - 310 : Screen.width - 310, 50, 300,
				"ECU MOD DEBUG", new[]
				{
					new GuiDebugElement("Cruise control"),
				});

			resetPosSetting.DoAction = PosReset;
			toggleAWD.DoAction = ToggleAWD;
			toggleSixGears.DoAction = ToggleSixGears;

			assetBundle = Helper.LoadAssetBundle(this, "ecu-mod.unity3d");

			Keybind.AddHeader(this, "ECU-Panel Keybinds");
			Keybind.Add(this, arrowUp);
			Keybind.Add(this, arrowDown);
			Keybind.Add(this, circle);
			Keybind.Add(this, cross);
			Keybind.Add(this, plus);
			Keybind.Add(this, minus);

			if ((bool) enableAirrideInfoPanelPage.Value) {
				Keybind.AddHeader(this, "Airride Keybinds");
				Keybind.Add(this, highestKeybind);
				Keybind.Add(this, lowestKeybind);
				Keybind.Add(this, increaseKeybind);
				Keybind.Add(this, decreaseKeybind);
			}


			ecu_mod_gameObject = GameObject.Instantiate(new GameObject());
			ecu_mod_gameObject.name = ID;

			originalGearRatios = CarH.drivetrain.gearRatios;

			var fuel_injector = (assetBundle.LoadAsset<GameObject>("fuel_injector.prefab"));
			var throttle_body = (assetBundle.LoadAsset<GameObject>("throttle_body.prefab"));

			var fuel_injectors_box_gameObject = GameObject.Instantiate((assetBundle.LoadAsset<GameObject>("fuel_injectors_box.prefab")));
			var throttle_bodies_box_gameObject = GameObject.Instantiate((assetBundle.LoadAsset<GameObject>("throttle_bodies_box.prefab")));
			ChipPart.prefab = assetBundle.LoadAsset<GameObject>("chip.prefab");

			var wires_injectors_pumps_gameObject =
				GameObject.Instantiate((assetBundle.LoadAsset<GameObject>("wires_injectors_pumps.prefab")));
			var wires_sparkPlugs1_gameObject =
				GameObject.Instantiate((assetBundle.LoadAsset<GameObject>("wires_sparkPlugs_1.prefab")));
			var wires_sparkPlugs2_gameObject =
				GameObject.Instantiate((assetBundle.LoadAsset<GameObject>("wires_sparkPlugs_2.prefab")));

			wires_injectors_pumps = wires_injectors_pumps_gameObject.transform.FindChild("default")
				.GetComponent<MeshRenderer>();
			wires_sparkPlugs1 =
				wires_sparkPlugs1_gameObject.transform.FindChild("default").GetComponent<MeshRenderer>();
			wires_sparkPlugs2 =
				wires_sparkPlugs2_gameObject.transform.FindChild("default").GetComponent<MeshRenderer>();

			fuel_injectors_box_gameObject.SetNameLayerTag("Fuel Injectors(Clone)");
			throttle_bodies_box_gameObject.SetNameLayerTag("Throttle Bodies(Clone)");
			wires_injectors_pumps_gameObject.SetNameLayerTag("wires_injectors_pumps");
			wires_sparkPlugs1_gameObject.SetNameLayerTag("wires_sparkPlugs1");
			wires_sparkPlugs2_gameObject.SetNameLayerTag("wires_sparkPlugs2");

			partBaseInfo = new PartBaseInfo(this, assetBundle, partsList);

			mounting_plate_part = new Part("mounting_plate",
				"ECU Mounting Plate", CarH.satsuma,
				mounting_plate_installLocation, new Vector3(0, 180, 0), partBaseInfo);

			abs_module_part = new Part("abs_module",
				"ABS Module", mounting_plate_part,
				abs_module_installLocation, new Vector3(0, 0, 0), partBaseInfo);
			
			esp_module_part = new Part("esp_module",
				"ESP Module", mounting_plate_part,
				esp_module_installLocation, new Vector3(0, 0, 0), partBaseInfo);

			tcs_module_part = new Part("tcs_module",
				"TCS Module", mounting_plate_part,
				tcs_module_installLocation, new Vector3(0, 0, 0), partBaseInfo);

			smart_engine_module_part = new Part("smart_engine_module",
				"Smart Engine ECU", mounting_plate_part,
				smart_engine_module_installLocation, new Vector3(0, 0, 0), partBaseInfo);

			smart_engine_module_logic = smart_engine_module_part.AddWhenInstalledMono<SmartEngineModule_Logic>();
			smart_engine_module_logic.Init(smart_engine_module_part, abs_module_part, esp_module_part, tcs_module_part);

			cable_harness_part = new Part("cable_harness",
				"ECU Cable Harness", mounting_plate_part,
				cable_harness_installLocation, new Vector3(0, 0, 0), partBaseInfo);

			cruise_control_panel_part = new Part("cruise_control_panel",
				"Cruise Control Panel", Cache.Find("dashboard(Clone)"),
				cruise_control_panel_installLocation, new Vector3(90, 0, 0), partBaseInfo);

			cruise_control_logic = cruise_control_panel_part.AddWhenInstalledMono<CruiseControl_Logic>();

			info_panel_part = new Part("info_panel",
				"Info Panel", Cache.Find("dashboard(Clone)"),
				info_panel_installLocation, new Vector3(0, 180, 180), partBaseInfo);
			info_panel = new InfoPanel(this, info_panel_part, assetBundle);

			rain_light_sensorboard_part = new Part("rain_light_sensorboard",
				"Rain & Light Sensorboard", Cache.Find("dashboard(Clone)"),
				rain_light_sensorboard_installLocation, new Vector3(90, 0, 0), partBaseInfo);

			reverse_camera_part = new Part("reverse_camera",
				"Reverse Camera", Cache.Find("bootlid(Clone)"),
				reverse_camera_installLocation, new Vector3(120, 0, 0), partBaseInfo);
			reverse_camera_logic = reverse_camera_part.AddWhenInstalledMono<ReverseCamera_Logic>();

			fuel_injection_manifold_part = new Part("fuel_injection_manifold",
				"Fuel Injection Manifold", Cache.Find("cylinder head(Clone)"),
				fuel_injection_manifold_installLocation, new Vector3(90, 0, 0), partBaseInfo);

			fuel_pump_cover_part = new Part("fuel_pump_cover",
				"Fuel Pump Cover", Cache.Find("block(Clone)"),
				fuel_pump_cover_installLocation, new Vector3(90, 0, 0), partBaseInfo);

			fuel_rail_part = new Part("fuel_rail",
				"Fuel Rail", fuel_injection_manifold_part,
				fuel_rail_installLocation, new Vector3(30, 0, 0), partBaseInfo);

			chip_programmer_part = new Part("chip_programmer",
				"Chip Programmer",
				new Vector3(0, 0, 0), new Vector3(0, 0, 0), partBaseInfo);

			electric_fuel_pump_part = new Part("electric_fuel_pump",
				"Electric Fuel Pump", CarH.satsuma,
				electric_fuel_pump_installLocation, new Vector3(0, 180, 0), partBaseInfo);
			electric_fuel_pump_part.transform.FindChild("fuelLine-1").GetComponent<Renderer>().enabled = true;
			electric_fuel_pump_part.transform.FindChild("fuelLine-2").GetComponent<Renderer>().enabled = true;

			fuel_injectors_box = new Box("fuel_injector", "Fuel Injector", fuel_injectors_box_gameObject, fuel_injector,
				4, fuel_injection_manifold_part,
				new[]
				{
					fuel_injector1_installLocation,
					fuel_injector2_installLocation,
					fuel_injector3_installLocation,
					fuel_injector4_installLocation,
				},
				new[]
				{
					new Vector3(30, 0, 0),
					new Vector3(30, 0, 0),
					new Vector3(30, 0, 0),
					new Vector3(30, 0, 0),
				});
			foreach (var part in fuel_injectors_box.parts) {
				part.AddPreSaveAction(fuel_injectors_box.CheckUnpackedOnSave);
			}

			throttle_bodies_box = new Box("throttle_body", "Throttle Body", throttle_bodies_box_gameObject,
				throttle_body, 4, fuel_injection_manifold_part,
				new[]
				{
					throttle_body1_installLocation,
					throttle_body2_installLocation,
					throttle_body3_installLocation,
					throttle_body4_installLocation,
				},
				new[]
				{
					new Vector3(-40, 0, 0),
					new Vector3(-40, 0, 0),
					new Vector3(-40, 0, 0),
					new Vector3(-40, 0, 0),
				});
			foreach (var part in throttle_bodies_box.parts) {
				part.AddPreSaveAction(throttle_bodies_box.CheckUnpackedOnSave);
			}

			throttle_bodies_box.AddScrews(
				new[]
				{
					new Screw(new Vector3(0.016f, -0.016f, -0.011f), new Vector3(0, 0, 0)),
					new Screw(new Vector3(-0.016f, 0.016f, -0.011f), new Vector3(0, 0, 0)),
				}, 0.6f, 8);


			wires_injectors_pumps_gameObject.transform.parent = fuel_injection_manifold_part.transform;
			wires_sparkPlugs1_gameObject.transform.parent = Cache.Find("cylinder head(Clone)").transform;
			wires_sparkPlugs2_gameObject.transform.parent = CarH.satsuma.transform;

			wires_injectors_pumps_gameObject.transform.localPosition = new Vector3(0.0085f, 0.053f, 0.0366f); //Temp
			wires_sparkPlugs1_gameObject.transform.localPosition = new Vector3(-0.001f, 0.088f, 0.055f); //Temp
			wires_sparkPlugs2_gameObject.transform.localPosition = new Vector3(0.105f, 0.233f, 0.97f); //Temp

			wires_injectors_pumps_gameObject.transform.localRotation =
				new Quaternion { eulerAngles = new Vector3(0, 0, 0) }; //Temp
			wires_sparkPlugs1_gameObject.transform.localRotation =
				new Quaternion { eulerAngles = new Vector3(90, 0, 0) }; //Temp
			wires_sparkPlugs2_gameObject.transform.localRotation =
				new Quaternion { eulerAngles = new Vector3(0, 180, 0) }; //Temp

			wires_injectors_pumps.enabled = false;
			wires_sparkPlugs1.enabled = false;
			wires_sparkPlugs2.enabled = false;

			abs_module_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(0.0558f, -0.0025f, -0.0525f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(0.0558f, -0.0025f, 0.0525f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.0558f, -0.0025f, 0.0525f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.0558f, -0.0025f, -0.0525f), new Vector3(-90, 0, 0)),
				}, 0.8f, 8);

			esp_module_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(0.09f, -0.002f, -0.052f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(0.09f, -0.002f, 0.0528f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.092f, -0.002f, 0.0528f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.092f, -0.002f, -0.052f), new Vector3(-90, 0, 0)),
				}, 0.8f, 8);

			tcs_module_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(0.0388f, 0.002f, -0.0418f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(0.0388f, 0.002f, 0.0422f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.0382f, 0.002f, 0.0422f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.0382f, 0.002f, -0.0418f), new Vector3(-90, 0, 0)),
				}, 0.8f, 8);

			smart_engine_module_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(-0.028f, -0.003f, 0.039f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(0.049f, -0.003f, 0.039f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(0.049f, -0.003f, -0.0625f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.028f, -0.003f, -0.0625f), new Vector3(-90, 0, 0)),
				}, 0.8f, 8);

			mounting_plate_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(-0.1240f, 0.0180f, 0.0040f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.1240f, 0.0180f, 0.2070f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(0.0020f, 0.0180f, 0.2070f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(0.1280f, 0.0180f, 0.2070f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.1240f, 0.0180f, -0.2000f), new Vector3(-90, 0, 0))
				}, 1.2f, 12);

			rain_light_sensorboard_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(0.078f, 0.0055f, 0f), new Vector3(-90, 0, 0)),
					new Screw(new Vector3(-0.078f, 0.0055f, 0f), new Vector3(-90, 0, 0)),
				}, 0.5f, 8);

			info_panel_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(0f, -0.025f, -0.067f), new Vector3(180, 0, 0)),
				}, 0.8f, 8);

			reverse_camera_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(0f, -0.015f, 0.0055f), new Vector3(0, 0, 0)),
				}, 0.5f, 5);


			fuel_pump_cover_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(-0.02f, 0.003f, -0.009f), new Vector3(0, 180, 0), Screw.Type.Nut),
					new Screw(new Vector3(0.018f, 0.003f, -0.009f), new Vector3(0, 180, 0), Screw.Type.Nut),
				}, 0.6f, 7);

			fuel_injection_manifold_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(0.0875f, -0.001f, -0.0105f), new Vector3(0, 0, 0), Screw.Type.Nut),
					new Screw(new Vector3(0.053f, -0.043f, -0.0105f), new Vector3(0, 0, 0), Screw.Type.Nut),
					new Screw(new Vector3(-0.051f, -0.043f, -0.0105f), new Vector3(0, 0, 0), Screw.Type.Nut),
					new Screw(new Vector3(-0.0865f, -0.001f, -0.0105f), new Vector3(0, 0, 0), Screw.Type.Nut),
				}, 0.6f, 8);

			electric_fuel_pump_part.AddScrews(
				new[]
				{
					new Screw(new Vector3(0f, 0.04f, 0.01f), new Vector3(0, 180, 0)),
					new Screw(new Vector3(0f, -0.04f, 0.01f), new Vector3(0, 180, 0)),
				}, 0.6f, 8);

			var shopBaseInfo = new ShopBaseInfo(this, assetBundle);

			var shopSpawnLocation = Shop.SpawnLocation.Fleetari.Counter;

			Shop.Add(shopBaseInfo, Shop.ShopLocation.Fleetari, new ShopItem[]
			{
				new ShopItem("ABS Module", 800, shopSpawnLocation, abs_module_part, "abs-module_productImage.png"),
				new ShopItem("ESP Module", 1200, shopSpawnLocation, esp_module_part, "esp-module_productImage.png"),
				new ShopItem("TCS Module", 1800, shopSpawnLocation, tcs_module_part, "tcs-module_productImage.png"),
				new ShopItem("ECU Cable Harness", 300, shopSpawnLocation, cable_harness_part, "cable-harness_productImage.png"),
				new ShopItem("ECU Mounting Plate", 100, shopSpawnLocation, mounting_plate_part, "mounting-plate_productImage.png"),
				new ShopItem("Smart Engine Module ECU", 4600, shopSpawnLocation, smart_engine_module_part, "smart-engine-module_productImage.png"),
				new ShopItem("Cruise Control Panel with Controller", 2000, shopSpawnLocation, cruise_control_panel_part, "cruise-control_productImage.png"),
				new ShopItem("ECU Info Panel", 4000, shopSpawnLocation, info_panel.part, "info-panel_productImage.png"),
				new ShopItem("Rain & Light Sensorboard", 1000, shopSpawnLocation, rain_light_sensorboard_part, "rain-light-sensorboard_productImage.png"),
				new ShopItem("Reverse Camera", 1500, shopSpawnLocation, reverse_camera_part, "reverse-camera_productImage.png"),
				new ShopItem("Fuel Pump Cover", 120, shopSpawnLocation, fuel_pump_cover_part, "fuel-pump-cover-plate_productImage.png"),
				new ShopItem("Fuel Injection Manifold", 1600, shopSpawnLocation, fuel_injection_manifold_part, "fuel-injection-manifold_productImage.png"),
				new ShopItem("Fuel Rail", 375, shopSpawnLocation, fuel_rail_part, "fuel-rail_productImage.png"),
				new ShopItem("Chip Programmer", 3799, shopSpawnLocation, chip_programmer_part, "chip-programmer_productImage.png"),
				new ShopItem("Electric Fuel Pump", 500, shopSpawnLocation, electric_fuel_pump_part, "electric-fuel-pump_productImage.png"),
				new ShopItem("Programmable chip", 500, Shop.SpawnLocation.Fleetari.Counter, delegate
				{
					var chipPart = new ChipPart(
						$"chip_{ChipPart.counter}",
						$"Chip {ChipPart.counter + 1}",
						smart_engine_module_part,
						partBaseInfo
						);
					chipPart.SetDefaultPosition(shopSpawnLocation);
					chipPart.SetBought(true);
					chipPart.ResetToDefault();

				}, "chip_productImage.png"),
			});

			if (!fuel_injectors_box.IsBought()) {
				Shop.Add(
					shopBaseInfo,
					Shop.ShopLocation.Fleetari,
					new ShopItem("Fuel Injectors", 800, shopSpawnLocation, delegate
					{
						fuel_injectors_box.box.transform.position = shopSpawnLocation;
						fuel_injectors_box.box.SetActive(true);
						foreach (var part in fuel_injectors_box.parts) {
							part.SetActive(false);
							part.SetBought(true);
							part.SetDefaultPosition(shopSpawnLocation);
							part.ResetToDefault();
						}
					}, "fuel-injectors-box_productImage.png"));
			}

			if (!throttle_bodies_box.IsBought()) {
				Shop.Add(
					shopBaseInfo,
					Shop.ShopLocation.Fleetari,
					new ShopItem("Throttle Bodies", 1200, shopSpawnLocation, delegate {
						throttle_bodies_box.box.transform.position = shopSpawnLocation;
						throttle_bodies_box.box.SetActive(true);
						foreach (var part in throttle_bodies_box.parts) {
							part.SetActive(false);
							part.SetBought(true);
							part.SetDefaultPosition(shopSpawnLocation);
							part.ResetToDefault();
						}
					}, "throttle-bodies-box_productImage.png"));
			}

			fuel_system = new FuelSystem(this, fuel_injectors_box.parts, throttle_bodies_box.parts);

			assetBundle.Unload(false);
			Object.Destroy(fuel_injector);
			Object.Destroy(throttle_body);

			ModConsole.Print(Name + $" [v{Version}");
		}

		public void SetReverseCameraEnabled(bool enabled)
		{
			reverse_camera_logic.SetReverseCameraEnabled(enabled);
		}

		public override void ModSettings()
		{
			Settings.HideResetAllButton(this);
			Settings.AddHeader(this, "DEBUG");
			Settings.AddCheckBox(this, debugGuiSetting);
			Settings.AddButton(this, resetPosSetting, "Reset uninstalled part location");
			Settings.AddHeader(this, "Settings");
			Settings.AddCheckBox(this, enableAirrideInfoPanelPage);
			Settings.AddCheckBox(this, toggleSixGears);
			Settings.AddCheckBox(this, toggleAWD);
			Settings.AddCheckBox(this, settingThrottleBodyValveRotation);
			Settings.AddHeader(this, "", Color.clear);

			Settings.AddText(this, "New Gear ratios + 5th & 6th gear\n" +
								   "1.Gear: " + newGearRatio[2] + "\n" +
								   "2.Gear: " + newGearRatio[3] + "\n" +
								   "3.Gear: " + newGearRatio[4] + "\n" +
								   "4.Gear: " + newGearRatio[5] + "\n" +
								   "5.Gear: " + newGearRatio[6] + "\n" +
								   "6.Gear: " + newGearRatio[7]
			);
			Settings.AddText(this, "Copyright © Marvin Beym 2020-2021");
		}

		public override void OnSave()
		{
			fuel_system.Save();
		}


		public override void OnGUI()
		{
			if ((bool) debugGuiSetting.Value) {
				guiDebug.Handle(new[]
				{
					new GuiDebugInfo("Cruise control", "true = Good"),
					new GuiDebugInfo("Cruise control", "false = Bad (cruise control won't work)"),
					new GuiDebugInfo("Cruise control", "Gear not R", (CarH.drivetrain.gear != 0).ToString()),
					new GuiDebugInfo("Cruise control", "cruise panel installed",
						cruise_control_panel_part.IsInstalled().ToString()),
					new GuiDebugInfo("Cruise control", "mounting plate installed",
						mounting_plate_part.IsFixed().ToString()),
					new GuiDebugInfo("Cruise control", "smart engine module installed",
						smart_engine_module_part.IsFixed().ToString()),
					new GuiDebugInfo("Cruise control", "not on throttle",
						(CarH.carController.throttleInput <= 0f).ToString()),
					new GuiDebugInfo("Cruise control", "speed above 20km/h",
						(CarH.drivetrain.differentialSpeed >= 20f).ToString()),
					new GuiDebugInfo("Cruise control", "brake not pressed", (!CarH.carController.brakeKey).ToString()),
					new GuiDebugInfo("Cruise control", "clutch not pressed", (!cInput.GetKey("Clutch")).ToString()),
					new GuiDebugInfo("Cruise control", "handbrake not pressed",
						(CarH.carController.handbrakeInput <= 0f).ToString()),
					new GuiDebugInfo("Cruise control", "car electricity on", CarH.hasPower.ToString()),
					new GuiDebugInfo("Cruise control", "current speed", CarH.drivetrain.differentialSpeed.ToString()),
				});
			}
		}

		public override void Update()
		{
			fuel_system.Handle();
			info_panel.Handle();
		}

		private void PosReset()
		{
			try
			{
				foreach (var part in partsList.Where(part => !part.IsInstalled()))
				{
					part.ResetToDefault();
				}
			} catch (Exception ex) {
				ModConsole.Error(ex.Message);
			}
		}

		private void ToggleSixGears()
		{
			if (toggleSixGears.Value is bool value) {
				if (value) {
					CarH.drivetrain.gearRatios = newGearRatio;
					return;
				}
			}

			CarH.drivetrain.gearRatios = originalGearRatios;
		}

		private void ToggleAWD()
		{
			if (toggleAWD.Value is bool value) {
				if (value) {
					CarH.drivetrain.SetTransmission(Drivetrain.Transmissions.AWD);
					return;
				}
			}

			CarH.drivetrain.SetTransmission(Drivetrain.Transmissions.FWD);
		}
	}
}