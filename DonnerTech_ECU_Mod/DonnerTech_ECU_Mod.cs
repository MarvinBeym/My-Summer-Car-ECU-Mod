using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DonnerTech_ECU_Mod.fuelsystem;
using DonnerTech_ECU_Mod.part;
using DonnerTech_ECU_Mod.part.Module;
using DonnerTech_ECU_Mod.Parts;
using HutongGames.PlayMaker;
using MSCLoader;
using MscModApi;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using MscModApi.Shopping;
using MscModApi.Tools;
using Tools.gui;
using UnityEngine;
using InfoPanel = DonnerTech_ECU_Mod.part.InfoPanel;
using Object = UnityEngine.Object;

namespace DonnerTech_ECU_Mod
{
	public class DonnerTech_ECU_Mod : Mod
	{
#if DEBUG
		/*  ToDo:
		 *  Remove Autotune?`-> maybe replace with something else
		 *  
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
		 *
		 *
		 *
		 * BUGS/Need to fix
		 * ECU mod: add ERRor to display if something is wrong
		 *
		 * ToDo:
		 * - Maybe make rev limiter bounce more instead of having the rpm stay exactly at the set rpm
		 * - Ability to hide pages on info page based on condition (eg no turbo installed = don't show page at all)
		 * - Maybe remove the faults page -> maybe replace with actual Fault codes that get displayed
		 * - Create a manifold (either in ecu or turbo mod to be able to connect fuel injection to turbo mod)
		 * - Only show Options (ABS, ESP, TCS, ... when module is installed)
		 * - Optimize and cleanup code (use logic classes, change names to camelCase, reduce code size, ...)
		 */
		/* Changelog (v1.5.7)
		 * Fixed started loading & finished loading console message
		 */
#endif
		public override string ID => "DonnerTech_ECU_Mod"; //Your mod ID (unique)
		public override string Name => "DonnerTechRacing ECUs"; //You mod name
		public override string Author => "DonnerPlays"; //Your Username
		public override string Version => "1.6.0"; //Version
		public override bool UseAssetsFolder => true;

		public AssetBundle assetBundle;
		public GuiDebug guiDebug;
		public bool turboModInstalled;

		public GameObject ecu_mod_gameObject;

		internal static List<Part> partsList;

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
		public FuelSystem fuelSystem;

		internal static PartBaseInfo partBaseInfo;

		public GamePart dashboard;
		public GamePart bootlid;
		public GamePart cylinderHead;
		public GamePart block;

		public AbsModule absModule;
		public EspModule espModule;
		public TcsModule tcsModule;

		public CableHarness cableHarness;
		public MountingPlate mountingPlate;

		public SmartEngineModule smartEngineModule;
		public CruiseControlPanel cruiseControlPanel;
		public InfoPanel infoPanel;

		public ReverseCamera reverseCamera;
		public RainLightSensorBoard rainLightSensorboard;

		public FuelPumpCover fuelPumpCover;
		public FuelInjectionManifold fuelInjectionManifold;
		public FuelRail fuelRail;
		public ElectricFuelPump electricFuelPump;

		public MscModApi.Parts.PartBox.Box fuelInjectorsBox;
		public MscModApi.Parts.PartBox.Box throttleBodiesBox;


		public ChipProgrammer chipProgrammer;


		private Settings debugGuiSetting = new Settings("debugGuiSetting", "Show DEBUG GUI", false);
		private Settings resetPosSetting = new Settings("resetPos", "Reset Part position", Helper.WorkAroundAction);

		public Settings settingThrottleBodyValveRotation =
			new Settings("settingThrottleBodyValveRotation", "Throttle body valve rotation", true);

		public Settings enableAirrideInfoPanelPage = new Settings("enableAirrideInfoPanelPage",
			"Enable Airride (enabled/disabled before load)", false);


		public override void OnNewGame()
		{
			MscModApi.MscModApi.NewGameCleanUp(this);
			foreach (var file in Directory.GetFiles(Path.Combine(ModLoader.GetModSettingsFolder(this), "fuelMaps"), "chip_*_saveFile.json", SearchOption.TopDirectoryOnly))
			{
				File.Delete(file);
			}
		}

		public override void OnLoad()
		{
			NullGamePart.LoadCleanup();

			ModConsole.Print(
				$"<color=white><color=blue>{Name}</color> [<color=green>v{Version}</color>] started loading</color>");

			partsList = new List<Part>();

			//MscModApi.MscModApi.EnableScrewPlacementForAllParts(this);



			turboModInstalled = ModLoader.IsModPresent("SatsumaTurboCharger");
			guiDebug = new GuiDebug(turboModInstalled ? Screen.width - 310 - 310 : Screen.width - 310, 50, 300,
				"ECU MOD DEBUG", new[]
				{
					new GuiDebugElement("Cruise control"),
				});

			resetPosSetting.DoAction = PosReset;

			assetBundle = Helper.LoadAssetBundle(this, "ecu-mod.unity3d");

			Keybind.AddHeader(this, "ECU-Panel Keybinds");
			Keybind.Add(this, arrowUp);
			Keybind.Add(this, arrowDown);
			Keybind.Add(this, circle);
			Keybind.Add(this, cross);
			Keybind.Add(this, plus);
			Keybind.Add(this, minus);

			if ((bool) enableAirrideInfoPanelPage.Value)
			{
				Keybind.AddHeader(this, "Airride Keybinds");
				Keybind.Add(this, highestKeybind);
				Keybind.Add(this, lowestKeybind);
				Keybind.Add(this, increaseKeybind);
				Keybind.Add(this, decreaseKeybind);
			}

			dashboard = new GamePart("Database/DatabaseMechanics/Dashboard");
			bootlid = new GamePart("Database/DatabaseBody/Bootlid");
			cylinderHead = new GamePart("Database/DatabaseMotor/Cylinderhead");
			block = new BlockGamePart();

			ecu_mod_gameObject = GameObject.Instantiate(new GameObject());
			ecu_mod_gameObject.name = ID;

			var fuel_injector = (assetBundle.LoadAsset<GameObject>("fuel_injector.prefab"));
			var throttle_body = (assetBundle.LoadAsset<GameObject>("throttle_body.prefab"));

			var fuel_injectors_box_gameObject =
				GameObject.Instantiate((assetBundle.LoadAsset<GameObject>("fuel_injectors_box.prefab")));
			var throttle_bodies_box_gameObject =
				GameObject.Instantiate((assetBundle.LoadAsset<GameObject>("throttle_bodies_box.prefab")));
			ChipPart.prefab = assetBundle.LoadAsset<GameObject>("chip.prefab");

			fuel_injectors_box_gameObject.SetNameLayerTag("Fuel Injectors(Clone)");
			throttle_bodies_box_gameObject.SetNameLayerTag("Throttle Bodies(Clone)");

			partBaseInfo = new PartBaseInfo(this, assetBundle, partsList);

			mountingPlate = new MountingPlate(SatsumaGamePart.GetInstance());
			absModule = new AbsModule(mountingPlate);

			espModule = new EspModule(mountingPlate);
			tcsModule = new TcsModule(mountingPlate);

			smartEngineModule = new SmartEngineModule(mountingPlate);

			cableHarness = new CableHarness(mountingPlate);

			rainLightSensorboard = new RainLightSensorBoard(dashboard);

			infoPanel = new InfoPanel(dashboard, absModule, espModule, tcsModule, rainLightSensorboard, this, assetBundle);

			reverseCamera = new ReverseCamera(bootlid, infoPanel);



			fuelInjectionManifold = new FuelInjectionManifold(assetBundle, cylinderHead);
			fuelPumpCover = new FuelPumpCover(block);
			fuelRail = new FuelRail(fuelInjectionManifold);

			electricFuelPump = new ElectricFuelPump(SatsumaGamePart.GetInstance());

			fuelInjectorsBox = new MscModApi.Parts.PartBox.Box(
				"Fuel Injectors",
				"fuel_injector",
				"Fuel Injector",
				fuel_injectors_box_gameObject,
				fuel_injector,
				4,
				fuelInjectionManifold,
				new[]
				{
					new Vector3(0.105f, 0.0074f, -0.0012f),
					new Vector3(0.0675f, 0.0074f, -0.0012f),
					new Vector3(-0.068f, 0.0074f, -0.0012f),
					new Vector3(-0.105f, 0.0074f, -0.0012f)
				},
				new[]
				{
					new Vector3(30, 0, 0),
					new Vector3(30, 0, 0),
					new Vector3(30, 0, 0),
					new Vector3(30, 0, 0),
				},
				Shop.SpawnLocation.Fleetari.Counter
			);

			throttleBodiesBox = new ThrottleBodiesBox(fuelInjectionManifold, throttle_bodies_box_gameObject, throttle_body);

			fuelSystem = new FuelSystem(this, fuelInjectorsBox.childs, throttleBodiesBox.childs, fuelInjectionManifold);
			chipProgrammer = new ChipProgrammer(this, fuelSystem);
			fuelSystem.LoadChips();

			cruiseControlPanel = new CruiseControlPanel(dashboard, smartEngineModule, cableHarness, mountingPlate, fuelSystem);

			var shopBaseInfo = new ShopBaseInfo(this, assetBundle);

			var shopSpawnLocation = Shop.SpawnLocation.Fleetari.Counter;

			Shop.Add(shopBaseInfo, Shop.ShopLocation.Fleetari, new[]
			{
				new ShopItem("ABS Module", 800, shopSpawnLocation, absModule, "abs-module_productImage.png"),
				new ShopItem("ESP Module", 1200, shopSpawnLocation, espModule, "esp-module_productImage.png"),
				new ShopItem("TCS Module", 1800, shopSpawnLocation, tcsModule, "tcs-module_productImage.png"),
				new ShopItem("ECU Cable Harness", 300, shopSpawnLocation, cableHarness,
					"cable-harness_productImage.png"),
				new ShopItem("ECU Mounting Plate", 100, shopSpawnLocation, mountingPlate,
					"mounting-plate_productImage.png"),
				new ShopItem("Smart Engine Module ECU", 4600, shopSpawnLocation, smartEngineModule,
					"smart-engine-module_productImage.png"),
				new ShopItem("Cruise Control Panel with Controller", 2000, shopSpawnLocation, cruiseControlPanel,
					"cruise-control_productImage.png"),
				new ShopItem("ECU Info Panel", 4000, shopSpawnLocation, infoPanel, "info-panel_productImage.png"),
				new ShopItem("Rain & Light Sensorboard", 1000, shopSpawnLocation, rainLightSensorboard,
					"rain-light-sensorboard_productImage.png"),
				new ShopItem("Reverse Camera", 1500, shopSpawnLocation, reverseCamera,
					"reverse-camera_productImage.png"),
				new ShopItem("Fuel Pump Cover", 120, shopSpawnLocation, fuelPumpCover,
					"fuel-pump-cover-plate_productImage.png"),
				new ShopItem("Fuel Injection Manifold", 1600, shopSpawnLocation, fuelInjectionManifold,
					"fuel-injection-manifold_productImage.png"),
				new ShopItem("Fuel Rail", 375, shopSpawnLocation, fuelRail, "fuel-rail_productImage.png"),
				new ShopItem("Chip Programmer", 3799, shopSpawnLocation, chipProgrammer,
					"chip-programmer_productImage.png"),
				new ShopItem("Electric Fuel Pump", 500, shopSpawnLocation, electricFuelPump,
					"electric-fuel-pump_productImage.png"),
				new ShopItem("Fuel Injectors", 800, shopSpawnLocation, fuelInjectorsBox, "fuel-injectors-box_productImage.png"),
				new ShopItem("Throttle Bodies", 1200, shopSpawnLocation, throttleBodiesBox, "throttle-bodies-box_productImage.png"),
			new ShopItem("Programmable chip", 500, Shop.SpawnLocation.Fleetari.Counter, delegate
				{
					CreateChipPart($"chip_{ChipPart.counter}", shopSpawnLocation, true);
				}, "chip_productImage.png"),
			});

			assetBundle.Unload(false);
			Object.Destroy(fuel_injector);
			Object.Destroy(throttle_body);

			ModConsole.Print(
				$"<color=white><color=blue>{Name}</color> [<color=green>v{Version}</color>] finished loading</color>");
		}

		public void CreateChipPart(string id, Vector3 spawnLocation, bool resetToDefault = false)
		{
			var chipPart = new ChipPart(
				id,
				$"Chip {ChipPart.counter + 1}",
				smartEngineModule,
				partBaseInfo,
				chipProgrammer
			);
			chipPart.defaultPosition = spawnLocation;
			chipPart.bought = true;
			fuelSystem.AddChip(chipPart);

			if (resetToDefault)
			{
				chipPart.ResetToDefault();
			}
		}

		public override void ModSettings()
		{
			Settings.HideResetAllButton(this);
			Settings.AddHeader(this, "DEBUG");
			Settings.AddCheckBox(this, debugGuiSetting);
			Settings.AddButton(this, resetPosSetting, "Reset uninstalled part location");
			Settings.AddHeader(this, "Settings");
			Settings.AddCheckBox(this, enableAirrideInfoPanelPage);
			Settings.AddCheckBox(this, settingThrottleBodyValveRotation);
			Settings.AddHeader(this, "", Color.clear);

			TransmissionHandler.SetupSettings(this);
			GearRatiosHandler.SetupSettings(this);

			Settings.AddText(this, "Copyright © Marvin Beym 2020-2024");
		}

		public override void OnSave()
		{
			//Nothing to do
		}


		public override void OnGUI()
		{
			if ((bool) debugGuiSetting.Value)
			{
				guiDebug.Handle(new[]
				{
					new GuiDebugInfo("Cruise control", "true = Good"),
					new GuiDebugInfo("Cruise control", "false = Bad (cruise control won't work)"),
					new GuiDebugInfo("Cruise control", "Gear not R", (CarH.drivetrain.gear != 0).ToString()),
					new GuiDebugInfo("Cruise control", "cruise panel installed",
						cruiseControlPanel.installed.ToString()),
					new GuiDebugInfo("Cruise control", "mounting plate installed",
						mountingPlate.bolted.ToString()),
					new GuiDebugInfo("Cruise control", "smart engine module installed",
						smartEngineModule.bolted.ToString()),
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
			TransmissionHandler.Handle();
			GearRatiosHandler.Handle();

			infoPanel.Handle();
		}

		private void PosReset()
		{
			try
			{
				partsList.ForEach(delegate(Part part) { part.ResetToDefault(); });
			}
			catch (Exception ex)
			{
				ModConsole.Error(ex.Message);
			}
		}
	}
}