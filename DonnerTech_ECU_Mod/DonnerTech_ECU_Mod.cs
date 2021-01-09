using HutongGames.PlayMaker;
using ModApi.Attachable;
using MSCLoader;
using System;
using System.IO;
using UnityEngine;
using ScrewablePartAPI;
using System.Collections.Generic;
using DonnerTech_ECU_Mod.old_file_checker;
using DonnerTech_ECU_Mod.shop;
using DonnerTech_ECU_Mod.fuelsystem;
using DonnerTech_ECU_Mod.parts;
using DonnerTech_ECU_Mod.infoPanel;
using Tools;
using Tools.gui;
using Parts;

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

        SaveFileRenamer saveFileRenamer;
        OverrideFileRenamer overrideFileRenamer;
        public AssetBundle assetBundle;
        public AssetBundle screwableassetBundle;
        public GuiDebug guiDebug;
        public bool turboModInstalled = false;

        public BugReporter.BugReporter bugReporter;

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

#if DEBUG
        private static SimplePart airride_fl_part;
        //private static ECU_Airride_FR_Part ecu_airride_fr_part;
        public static SimplePart awd_gearbox_part;
        public static SimplePart awd_propshaft_part;
        public static SimplePart awd_differential_part;
        public Vector3 airride_fl_installLocation = new Vector3(0, 0, 0);
        public Vector3 awd_gearbox_part_installLocation = new Vector3(0, 0, 0);
        public Vector3 awd_differential_part_installLocation = new Vector3(0, -0.33f, -1.16f);
        public Vector3 awd_propshaft_part_installLocation = new Vector3(-0.05f, -0.3f, -0.105f);
#endif

        //Saves
        public Dictionary<string, bool> partsBuySave;
        //Files
        private const string logger_saveFile = "ecu_mod_logs.txt";
        private const string modsShop_saveFile = "mod_shop_saveFile.json";
        private const string screwable_saveFile = "screwable_saveFile.json";


        //Installed FSM
        public FsmBool fuelInjection_allInstalled;
        public FsmBool fuelInjection_anyInstalled;

        //Modules FSM
        public PlayMakerFSM modulesFsm;
        public FsmFloat step2_rpm;
        public FsmBool absModule_enabled;
        public FsmBool espModule_enabled;
        public FsmBool tcsModule_enabled;
        public FsmBool alsModule_enabled;
        public FsmBool step2RevLimiterModule_enabled;

        //FuelSystem
        public FuelSystem fuel_system;

        //InfoPanel
        public InfoPanel info_panel;

        public FsmString playerCurrentVehicle;

        //Part logic
        public CruiseControl_Logic cruise_control_logic { get; set; }
        public SmartEngineModule_Logic smart_engine_module_logic { get; set; }

        public ReverseCamera_Logic reverse_camera_logic { get; set; }


        public Box fuel_injectors_box { get; set; }
        public Box throttle_bodies_box { get; set; }

        private int setCruiseControlSpeed = 0;
        private bool cruiseControlModuleEnabled = false;

        private static string modAssetsFolder;

        public SimplePart abs_module_part { get; set; }
        public SimplePart esp_module_part { get; set; }
        public SimplePart tcs_module_part { get; set; }
        public SimplePart cable_harness_part { get; set; }
        public SimplePart mounting_plate_part { get; set; }
        public SimplePart smart_engine_module_part { get; set; }
        public SimplePart cruise_control_panel_part { get; set; }
        public SimplePart info_panel_part;


        public SimplePart reverse_camera_part { get; set; }
        public SimplePart rain_light_sensorboard_part { get; set; }

        public SimplePart fuel_pump_cover_part { get; set; }
        public SimplePart fuel_injection_manifold_part { get; set; }
        public SimplePart fuel_rail_part { get; set; }
        public SimplePart electric_fuel_pump_part { get; set; }
        public static GameObject fuel_injectors_box_gameObject { get; set; }
        public static GameObject throttle_bodies_box_gameObject { get; set; }

        public GameObject chip { get; set; }
        public SimplePart chip_programmer_part { get; set; }

        public MeshRenderer wires_injectors_pumps;
        public MeshRenderer wires_sparkPlugs1;
        public MeshRenderer wires_sparkPlugs2;

        public List<SimplePart> partsList;

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

        public Settings settingThrottleBodieTurning = new Settings("settingThrottleBodieTurning", "Throttle body valve rotation", true);
        private Settings toggleSixGears = new Settings("toggleSixGears", "SixGears Mod (with gear ratio changes)", false);
        public Settings enableAirrideInfoPanelPage = new Settings("enableAirrideInfoPanelPage", "Enable airride info panel page & airride logic (Has to be enabled/disabled before load)", false);
        private Settings toggleAWD = new Settings("toggleAWD", "All Wheel Drive (AWD)", false);
        private Settings toggleSmoothInput = new Settings("toggleSmoothInput", "Smooth throttle input", false);

        private static float[] originalGearRatios;
        private static float[] newGearRatio = new float[]
        {
            -4.093f, // reverse
            0f,      // neutral
            3.4f,  // 1st
            1.8f,  // 2nd
            1.4f,  // 3rd
            1.0f,   // 4th
            0.8f,   // 5th
            0.65f    // 6th
        };


        public override void OnNewGame()
        {
            partsList.ForEach(delegate (SimplePart part)
            {
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, part.saveFile);
            });
            fuel_system.chip_parts.ForEach(delegate (ChipPart chip)
            {
                SaveLoad.SerializeSaveFile<ChipSave>(this, null, chip.mapSaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, chip.saveFile);
            });
            SaveLoad.SerializeSaveFile<Dictionary<string, bool>>(this, null, modsShop_saveFile);
        }
        public override void OnLoad()
        {
            ModConsole.Print(this.Name + $" [v{this.Version} | Screwable v{ScrewablePart.apiVersion}] started loading");
            Logger.InitLogger(this, logger_saveFile, 100);

            turboModInstalled = ModLoader.IsModPresent("SatsumaTurboCharger");
            guiDebug = new GuiDebug(turboModInstalled ? Screen.width - 310 - 310 : Screen.width - 310, 50, 300, "ECU MOD DEBUG", new GuiDebugElement[] {
                new GuiDebugElement("Cruise control"),
            });

            resetPosSetting.DoAction = PosReset;
            toggleAWD.DoAction = ToggleAWD;
            toggleSmoothInput.DoAction = ToggleSmoothInput;
            toggleSixGears.DoAction = ToggleSixGears;

            assetBundle = Helper.LoadAssetBundle(this, "ecu-mod.unity3d");
            screwableassetBundle = Helper.LoadAssetBundle(this, "screwableapi.unity3d");


            List<BugReporter.Report> reports = new List<BugReporter.Report>();

            reports.Add(new BugReporter.Report("Mod Settings", new string[] { ModLoader.GetModConfigFolder(this) }, true));
            reports.Add(new BugReporter.Report("ModLoader Output", new string[] { Helper.CombinePaths(new string[] { Path.GetFullPath("."), "mysummercar_Data", "output_log.txt" }) }));

            BugReporter.Report gameSave_report = new BugReporter.Report("MSC Savegame", 
                Directory.GetFiles(
                    Path.GetFullPath(
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                            @"..\LocalLow\Amistech\My Summer Car\")
                        )
                    ));

            reports.Add(gameSave_report);
            bugReporter = new BugReporter.BugReporter(this, reports, true);

            Keybind.AddHeader(this, "ECU-Panel Keybinds");
            Keybind.Add(this, arrowUp);
            Keybind.Add(this, arrowDown);
            Keybind.Add(this, circle);
            Keybind.Add(this, cross);
            Keybind.Add(this, plus);
            Keybind.Add(this, minus);

            if((bool)enableAirrideInfoPanelPage.Value)
            {
                Keybind.AddHeader(this, "Airride Keybinds");
                Keybind.Add(this, highestKeybind);
                Keybind.Add(this, lowestKeybind);
                Keybind.Add(this, increaseKeybind);
                Keybind.Add(this, decreaseKeybind);
            }


            GameObject ecu_mod_gameObject = GameObject.Instantiate(new GameObject());
            ecu_mod_gameObject.name = this.ID;

            PlayMakerFSM installedFsm = ecu_mod_gameObject.AddComponent<PlayMakerFSM>();
            installedFsm.FsmName = "Installed";

            fuelInjection_allInstalled = new FsmBool("Fuel Injection All");
            fuelInjection_anyInstalled = new FsmBool("Fuel Injection Any");

            installedFsm.FsmVariables.BoolVariables = new FsmBool[]
            {
                fuelInjection_allInstalled,
                fuelInjection_anyInstalled,
            };

            modulesFsm = ecu_mod_gameObject.AddComponent<PlayMakerFSM>();
            modulesFsm.FsmName = "Modules";

            absModule_enabled = new FsmBool("ABS Enabled");
            espModule_enabled = new FsmBool("ESP Enabled");
            tcsModule_enabled = new FsmBool("TCS Enabled");
            alsModule_enabled = new FsmBool("ALS Enabled");
            step2RevLimiterModule_enabled = new FsmBool("Step2RevLimiter Enabled");
            step2_rpm = new FsmFloat("Rpm");
            step2_rpm.Value = 6500;

            modulesFsm.FsmVariables.BoolVariables = new FsmBool[]
            {
                absModule_enabled,
                espModule_enabled,
                tcsModule_enabled,
                alsModule_enabled,
                step2RevLimiterModule_enabled,
            };
            modulesFsm.FsmVariables.FloatVariables = new FsmFloat[]
            {
                step2_rpm
            };

            modAssetsFolder = ModLoader.GetModAssetsFolder(this);
            
            originalGearRatios = CarH.drivetrain.gearRatios;

#if DEBUG
            PlayMakerFSM gearboxFSM = Game.Find("DatabaseMotor/Gearbox").GetComponent<PlayMakerFSM>();
            PlayMakerFSM drivegearFSM = Game.Find("DatabaseMotor/Drivegear").GetComponent<PlayMakerFSM>();
            FsmBool gearbox_installed = gearboxFSM.FsmVariables.FindFsmBool("Installed");
            FsmBool gearbox_bolted = gearboxFSM.FsmVariables.FindFsmBool("Bolted");

            FsmBool drivegear_installed = drivegearFSM.FsmVariables.FindFsmBool("Installed");
            FsmBool drivegear_bolted = drivegearFSM.FsmVariables.FindFsmBool("Bolted");
#endif

            try
            {
                partsBuySave = Helper.LoadSaveOrReturnNew<Dictionary<string, bool>>(this, modsShop_saveFile);
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to deserialize save file", "Please check paths to save files", ex);
            }

            GameObject fuel_injector = (assetBundle.LoadAsset("fuel_injector.prefab") as GameObject);
            GameObject throttle_body = (assetBundle.LoadAsset("throttle_body.prefab") as GameObject);

            fuel_injectors_box_gameObject =  GameObject.Instantiate((assetBundle.LoadAsset("fuel_injectors_box.prefab") as GameObject));
            throttle_bodies_box_gameObject = GameObject.Instantiate((assetBundle.LoadAsset("throttle_bodies_box.prefab") as GameObject));


            chip = GameObject.Instantiate((assetBundle.LoadAsset("chip.prefab") as GameObject));
            chip.SetActive(false);

            GameObject wires_injectors_pumps_gameObject = GameObject.Instantiate((assetBundle.LoadAsset("wires_injectors_pumps.prefab") as GameObject));
            GameObject wires_sparkPlugs1_gameObject = GameObject.Instantiate((assetBundle.LoadAsset("wires_sparkPlugs_1.prefab") as GameObject));
            GameObject wires_sparkPlugs2_gameObject = GameObject.Instantiate((assetBundle.LoadAsset("wires_sparkPlugs_2.prefab") as GameObject));

            wires_injectors_pumps = wires_injectors_pumps_gameObject.transform.FindChild("default").GetComponent<MeshRenderer>();
            wires_sparkPlugs1 = wires_sparkPlugs1_gameObject.transform.FindChild("default").GetComponent<MeshRenderer>();
            wires_sparkPlugs2 = wires_sparkPlugs2_gameObject.transform.FindChild("default").GetComponent<MeshRenderer>();

            Helper.SetObjectNameTagLayer(fuel_injectors_box_gameObject, "Fuel Injectors(Clone)");
            Helper.SetObjectNameTagLayer(throttle_bodies_box_gameObject, "Throttle Bodies(Clone)");
            Helper.SetObjectNameTagLayer(chip, "Chip");
            Helper.SetObjectNameTagLayer(wires_injectors_pumps_gameObject, "wires_injectors_pumps");
            Helper.SetObjectNameTagLayer(wires_sparkPlugs1_gameObject, "wires_sparkPlugs1");
            Helper.SetObjectNameTagLayer(wires_sparkPlugs2_gameObject, "wires_sparkPlugs2");

            saveFileRenamer = new SaveFileRenamer(this, 900);
            overrideFileRenamer = new OverrideFileRenamer(this, 900);

            SortedList<String, Screws> screwListSave = ScrewablePart.LoadScrews(this, screwable_saveFile);

#if DEBUG

            airride_fl_part = new SimplePart(
                SimplePart.LoadData(this, "airride_fl", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "airride_fl.prefab", "Airride FL"),
                Game.Find("Chassis/FL"),
                airride_fl_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );
            awd_gearbox_part = new SimplePart(
                SimplePart.LoadData(this, "awd_gearbox", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "awd_gearbox.prefab", "AWD Gearbox"),
                Game.Find("pivot_gearbox"),
                awd_gearbox_part_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 180, 0) }
            );
            awd_differential_part = new SimplePart(
                SimplePart.LoadData(this, "awd_differential", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "awd_differential.prefab", "AWD Differential"),
                CarH.satsuma,
                awd_differential_part_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );

            awd_propshaft_part = new SimplePart(
                SimplePart.LoadData(this, "awd_propshaft", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "awd_propshaft.prefab", "AWD Propshaft"),
                CarH.satsuma,
                awd_propshaft_part_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );
#endif

            mounting_plate_part = new SimplePart(
                SimplePart.LoadData(this, "mounting_plate", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "mounting_plate.prefab", "ECU Mounting Plate"),
                CarH.satsuma,
                mounting_plate_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );
            mounting_plate_part.SetDisassembleFunction(new Action(DisassembleMountingPlate));


            smart_engine_module_part = new SimplePart(
                SimplePart.LoadData(this, "smart_engine_module", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "smart_engine_module.prefab", "Smart Engine ECU"),
                mounting_plate_part.rigidPart,
                smart_engine_module_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );
            smart_engine_module_part.SetDisassembleFunction(new Action(DisassembleSmartEngineModule));
            smart_engine_module_logic = smart_engine_module_part.rigidPart.AddComponent<SmartEngineModule_Logic>();

            
            abs_module_part = new SimplePart(
                SimplePart.LoadData(this, "abs_module", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "abs_module.prefab", "ABS Module"),
                mounting_plate_part.rigidPart,
                abs_module_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );
            abs_module_part.SetDisassembleFunction(new Action(delegate ()
            {
                if (smart_engine_module_logic != null && absModule_enabled != null && absModule_enabled.Value)
                {
                    smart_engine_module_logic.ToggleABS();
                }
            }));

            esp_module_part = new SimplePart(
                SimplePart.LoadData(this, "esp_module", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "esp_module.prefab", "ESP Module"),
                mounting_plate_part.rigidPart,
                esp_module_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );
            esp_module_part.SetDisassembleFunction(new Action(delegate ()
            {
                if (smart_engine_module_logic != null && espModule_enabled != null && espModule_enabled.Value)
                {
                    smart_engine_module_logic.ToggleESP();
                }
            }));

            tcs_module_part = new SimplePart(
                SimplePart.LoadData(this, "tcs_module", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "tcs_module.prefab", "TCS Module"),
                mounting_plate_part.rigidPart,
                tcs_module_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );
            tcs_module_part.SetDisassembleFunction(new Action(delegate ()
            {
                if (smart_engine_module_logic != null && tcsModule_enabled != null && tcsModule_enabled.Value)
                {
                    smart_engine_module_logic.ToggleTCS();
                }
            }));

            cable_harness_part = new SimplePart(
                SimplePart.LoadData(this, "cable_harness", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "cable_harness.prefab", "ECU Cable Harness"),
                mounting_plate_part.rigidPart,
                cable_harness_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );

            cruise_control_panel_part = new SimplePart(
                SimplePart.LoadData(this, "cruise_control_panel", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "cruise_control_panel.prefab", "Cruise Control Panel"),
                Game.Find("dashboard(Clone)"),
                cruise_control_panel_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            cruise_control_logic = cruise_control_panel_part.rigidPart.AddComponent<CruiseControl_Logic>();

            info_panel_part = new SimplePart(
                SimplePart.LoadData(this, "info_panel", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "info_panel.prefab", "DonnerTech Info Panel"),
                Game.Find("dashboard(Clone)"),
                info_panel_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 180) }
            );
            info_panel = new InfoPanel(this, info_panel_part, assetBundle);

            rain_light_sensorboard_part = new SimplePart(
                SimplePart.LoadData(this, "rain_light_sensorboard", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "rain_light_sensorboard.prefab", "Rain & Light Sensorboard"),
                Game.Find("dashboard(Clone)"),
                rain_light_sensorboard_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );

            reverse_camera_part = new SimplePart(
                SimplePart.LoadData(this, "reverse_camera", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "reverse_camera.prefab", "Reverse Camera"),
                Game.Find("bootlid(Clone)"),                reverse_camera_installLocation,
                new Quaternion { eulerAngles = new Vector3(120, 0, 0) }
            );
            reverse_camera_logic = reverse_camera_part.rigidPart.AddComponent<ReverseCamera_Logic>();



            fuel_injection_manifold_part = new SimplePart(
                SimplePart.LoadData(this, "fuel_injection_manifold", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "fuel_injection_manifold.prefab", "Fuel Injection Manifold"),
                Game.Find("cylinder head(Clone)"),
                fuel_injection_manifold_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );

            fuel_injection_manifold_part.SetDisassembleFunction(new Action(DisassembleFuelInjectionManifold));

            fuel_pump_cover_part = new SimplePart(
                SimplePart.LoadData(this, "fuel_pump_cover", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "fuel_pump_cover.prefab", "Fuel Pump Cover"),
                Game.Find("block(Clone)"),
                fuel_pump_cover_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            fuel_pump_cover_part.SetDisassembleFunction(DisassembleFuelInjectionPump);

            fuel_rail_part = new SimplePart(
                SimplePart.LoadData(this, "fuel_rail", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "fuel_rail.prefab", "Fuel Rail"),
                fuel_injection_manifold_part.rigidPart,
                fuel_rail_installLocation,
                new Quaternion { eulerAngles = new Vector3(30, 0, 0) }
            );

            GameObject emptyGameObject = new GameObject();
            chip_programmer_part = new SimplePart(
                SimplePart.LoadData(this, "chip_programmer", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "chip_programmer.prefab", "Chip Programmer"),
                emptyGameObject,
                new Vector3(0, 0, 0),
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
                );

            electric_fuel_pump_part = new SimplePart(
                SimplePart.LoadData(this, "electric_fuel_pump", partsBuySave),
                Helper.LoadPartAndSetName(assetBundle, "electric_fuel_pump.prefab", "Electric Fuel Pump"),
                CarH.satsuma,
                electric_fuel_pump_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );
            electric_fuel_pump_part.SetDisassembleFunction(DisassembleFuelInjectionPump);
            electric_fuel_pump_part.rigidPart.transform.FindChild("fuelLine-1").GetComponent<Renderer>().enabled = true;
            electric_fuel_pump_part.rigidPart.transform.FindChild("fuelLine-2").GetComponent<Renderer>().enabled = true;

            partsList = new List<SimplePart>
            {
#if DEBUG
                airride_fl_part,
                awd_gearbox_part,
                awd_differential_part,
                awd_propshaft_part,
#endif
                info_panel_part,
                abs_module_part,
                esp_module_part,
                tcs_module_part,
                cable_harness_part,
                mounting_plate_part,
                smart_engine_module_part,
                cruise_control_panel_part,
                rain_light_sensorboard_part,
                reverse_camera_part,
                fuel_pump_cover_part,
                fuel_injection_manifold_part,
                fuel_rail_part,
                chip_programmer_part,
                electric_fuel_pump_part,
        };


            fuel_injectors_box = new Box(this, fuel_injectors_box_gameObject, fuel_injector, "Fuel Injector", 4, fuel_injection_manifold_part, partsBuySave,
                new Vector3[] {
                    fuel_injector1_installLocation,
                    fuel_injector2_installLocation,
                    fuel_injector3_installLocation,
                    fuel_injector4_installLocation,
                },
                new Vector3[] {
                    new Vector3(30, 0, 0),
                    new Vector3(30, 0, 0),
                    new Vector3(30, 0, 0),
                    new Vector3(30, 0, 0),
                });
            throttle_bodies_box = new Box(this, throttle_bodies_box_gameObject, throttle_body, "Throttle Body", 4, fuel_injection_manifold_part, partsBuySave,
                new Vector3[] {
                    throttle_body1_installLocation,
                    throttle_body2_installLocation,
                    throttle_body3_installLocation,
                    throttle_body4_installLocation,
                },
                new Vector3[]
                {
                    new Vector3(-40, 0, 0),
                    new Vector3(-40, 0, 0),
                    new Vector3(-40, 0, 0),
                    new Vector3(-40, 0, 0),
                });
            throttle_bodies_box.AddScrewable(screwListSave, screwableassetBundle,
                new Screw[] {
                    new Screw(new Vector3(0.016f, -0.016f, -0.011f), new Vector3(0, 0, 0), 0.6f, 8),
                    new Screw(new Vector3(-0.016f, 0.016f, -0.011f), new Vector3(0, 0, 0), 0.6f, 8),
                });


            wires_injectors_pumps_gameObject.transform.parent = fuel_injection_manifold_part.rigidPart.transform;
            wires_sparkPlugs1_gameObject.transform.parent = Game.Find("cylinder head(Clone)").transform;
            wires_sparkPlugs2_gameObject.transform.parent = CarH.satsuma.transform;

            wires_injectors_pumps_gameObject.transform.localPosition = new Vector3(0.0085f, 0.053f, 0.0366f); //Temp
            wires_sparkPlugs1_gameObject.transform.localPosition = new Vector3(-0.001f, 0.088f, 0.055f); //Temp
            wires_sparkPlugs2_gameObject.transform.localPosition = new Vector3(0.105f, 0.233f, 0.97f); //Temp

            wires_injectors_pumps_gameObject.transform.localRotation = new Quaternion() { eulerAngles = new Vector3(0, 0, 0) }; //Temp
            wires_sparkPlugs1_gameObject.transform.localRotation = new Quaternion() { eulerAngles = new Vector3(90, 0, 0) }; //Temp
            wires_sparkPlugs2_gameObject.transform.localRotation = new Quaternion() { eulerAngles = new Vector3(0, 180, 0) }; //Temp

            wires_injectors_pumps.enabled = false;
            wires_sparkPlugs1.enabled = false;
            wires_sparkPlugs2.enabled = false;


            abs_module_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, abs_module_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.0558f, -0.0025f, -0.0525f), new Vector3(-90, 0, 0), 0.8f, 8),
                    new Screw(new Vector3(0.0558f, -0.0025f, 0.0525f), new Vector3(-90, 0, 0), 0.8f, 8),
                    new Screw(new Vector3(-0.0558f, -0.0025f, 0.0525f), new Vector3(-90, 0, 0), 0.8f, 8),
                    new Screw(new Vector3(-0.0558f, -0.0025f, -0.0525f), new Vector3(-90, 0, 0), 0.8f, 8),
                });

            esp_module_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, esp_module_part.rigidPart,
                new Screw[] {
                        new Screw(new Vector3(0.09f, -0.002f, -0.052f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(0.09f, -0.002f, 0.0528f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.092f, -0.002f, 0.0528f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.092f, -0.002f, -0.052f), new Vector3(-90, 0, 0), 0.8f, 8),
                });

            tcs_module_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, tcs_module_part.rigidPart,
                new Screw[] {
                        new Screw(new Vector3(0.0388f, 0.002f, -0.0418f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(0.0388f, 0.002f, 0.0422f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.0382f, 0.002f, 0.0422f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.0382f, 0.002f, -0.0418f), new Vector3(-90, 0, 0), 0.8f, 8),
                });

            smart_engine_module_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, smart_engine_module_part.rigidPart,
                new Screw[] {
                        new Screw(new Vector3(-0.028f, -0.003f, 0.039f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(0.049f, -0.003f, 0.039f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(0.049f, -0.003f, -0.0625f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.028f, -0.003f, -0.0625f), new Vector3(-90, 0, 0), 0.8f, 8),
                });

            mounting_plate_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, mounting_plate_part.rigidPart,
                new Screw[] {
                        new Screw(new Vector3(-0.124f, 0.005f, 0.004f), new Vector3(-90, 0, 0), 1.2f, 12),
                        new Screw(new Vector3(-0.124f, 0.005f, 0.207f), new Vector3(-90, 0, 0), 1.2f, 12),
                        new Screw(new Vector3(0.002f, 0.005f, 0.207f), new Vector3(-90, 0, 0), 1.2f, 12),
                        new Screw(new Vector3(0.128f, 0.005f, 0.207f), new Vector3(-90, 0, 0), 1.2f, 12),
                        new Screw(new Vector3(-0.124f, 0.005f, -0.2f), new Vector3(-90, 0, 0), 1.2f, 12),
                });

            rain_light_sensorboard_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, rain_light_sensorboard_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.078f, 0.0055f, 0f), new Vector3(-90, 0, 0), 0.5f, 8),
                    new Screw(new Vector3(-0.078f, 0.0055f, 0f), new Vector3(-90, 0, 0), 0.5f, 8),
                });

            info_panel_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, info_panel_part.rigidPart,
            new Screw[] {
                new Screw(new Vector3(0f, -0.025f, -0.067f), new Vector3(180, 0, 0), 0.8f, 8),
            });

            reverse_camera_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, reverse_camera_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0f, -0.015f, 0.0055f), new Vector3(0, 0, 0), 0.5f, 5),
                });


            fuel_pump_cover_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, fuel_pump_cover_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(-0.02f, 0.003f, -0.009f), new Vector3(0, 180, 0), 0.6f, 7, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(0.018f, 0.003f, -0.009f), new Vector3(0, 180, 0), 0.6f, 7, ScrewablePart.ScrewType.Nut),
                });

            fuel_injection_manifold_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, fuel_injection_manifold_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.0875f, -0.001f, -0.0105f), new Vector3(0, 0, 0), 0.6f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(0.053f, -0.043f, -0.0105f), new Vector3(0, 0, 0), 0.6f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(-0.051f, -0.043f, -0.0105f), new Vector3(0, 0, 0), 0.6f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(-0.0865f, -0.001f, -0.0105f), new Vector3(0, 0, 0), 0.6f, 8, ScrewablePart.ScrewType.Nut),
                });

            electric_fuel_pump_part.screwablePart = new ScrewablePart(screwListSave, screwableassetBundle, electric_fuel_pump_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0f, 0.04f, 0.01f), new Vector3(0, 180, 0), 0.6f, 8),
                    new Screw(new Vector3(0f, -0.04f, 0.01f), new Vector3(0, 180, 0), 0.6f, 8),
                });

            if (Game.Find("Shop for mods") != null)
            {
                ModsShop.ShopItem modsShop = Game.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();

                List<ProductInformation> shopItems = new List<ProductInformation>
                {
                    new ProductInformation(abs_module_part, "ABS Module", 800, "abs-module_productImage.png"),
                    new ProductInformation(esp_module_part, "ESP Module", 1200, "esp-module_productImage.png"),
                    new ProductInformation(tcs_module_part, "TCS Module", 1800, "tcs-module_productImage.png"),
                    new ProductInformation(cable_harness_part, "ECU Cable Harness", 300, "cable-harness_productImage.png"),
                    new ProductInformation(mounting_plate_part, "ECU Mounting Plate", 100, "mounting-plate_productImage.png"),
                    new ProductInformation(smart_engine_module_part, "Smart Engine Module ECU", 4600, "smart-engine-module_productImage.png"),
                    new ProductInformation(cruise_control_panel_part, "Cruise Control Panel with Controller", 2000, "cruise-control_productImage.png"),
                    new ProductInformation(info_panel.part, "ECU Info Panel", 4000, "info-panel_productImage.png"),
                    new ProductInformation(rain_light_sensorboard_part, "Rain & Light Sensorboard", 1000, "rain-light-sensorboard_productImage.png"),
                    new ProductInformation(reverse_camera_part, "Reverse Camera", 1500, "reverse-camera_productImage.png"),
    #if DEBUG
                    new ProductInformation(airride_fl_part, "Airride fl", 0, null),
                    new ProductInformation(awd_gearbox_part, "AWD Gearbox", 0, null),
                    new ProductInformation(awd_differential_part, "AWD Differential", 0, null),
                    new ProductInformation(awd_propshaft_part, "AWD Propshaft", 0, null),
    #endif

                    new ProductInformation(fuel_injectors_box, "Fuel Injectors", 800, "fuel-injectors-box_productImage.png"),
                    new ProductInformation(throttle_bodies_box, "Throttle Bodies", 1200, "throttle-bodies-box_productImage.png"),

                    new ProductInformation(fuel_pump_cover_part, "Fuel Pump Cover", 120, "fuel-pump-cover-plate_productImage.png"),
                    new ProductInformation(fuel_injection_manifold_part, "Fuel Injection Manifold", 1600, "fuel-injection-manifold_productImage.png"),
                    new ProductInformation(fuel_rail_part, "Fuel Rail", 375, "fuel-rail_productImage.png"),
                    new ProductInformation(chip_programmer_part, "Chip Programmer", 3799, "chip-programmer_productImage.png"),
                    new ProductInformation(electric_fuel_pump_part, "Electric Fuel Pump", 500, "electric-fuel-pump_productImage.png"),
                };
                Shop shop = new Shop(this, modsShop, assetBundle, shopItems);
                shop.SetupShopItems();

            }
            else
            {
                ModUI.ShowMessage(
               "You need to install ModsShop by piotrulos for this mod\n" +
               "Please close the game and install the mod\n" +
               "There should have been a ModsShop.dll and unity3d file (inside Assets) inside the archive of this mod",
               "Installation of ModsShop (by piotrulos) needed");
            }

            fuel_system = new FuelSystem(this);

            assetBundle.Unload(false);
            screwableassetBundle.Unload(false);
            UnityEngine.Object.Destroy(fuel_injector);
            UnityEngine.Object.Destroy(throttle_body);

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");

            ModConsole.Print(this.Name + $" [v{this.Version} | Screwable v{ScrewablePart.apiVersion}] finished loading");
        }

        public void SetReverseCameraEnabled(bool enabled)
        {
            reverse_camera_logic.SetReverseCameraEnabled(enabled);


        }

        public void DisassembleFuelInjectionPump()
        {
            foreach(OriginalPart originalPart in fuel_system.allOriginalParts)
            {
                originalPart.SetFakedInstallStatus(false);
            }
        }

        private void DisassembleMountingPlate()
        {
            abs_module_part.removePart();
            esp_module_part.removePart();
            tcs_module_part.removePart();
            smart_engine_module_part.removePart();
            cable_harness_part.removePart();

        }
        public void DisassembleFuelInjectionManifold()
        {
            foreach (SimplePart part in fuel_injectors_box.parts)
            {
                part.removePart();
            }

            fuel_rail_part.removePart();

            foreach (SimplePart part in throttle_bodies_box.parts)
            {
                part.removePart();
            }

            foreach (OriginalPart originalPart in fuel_system.allOriginalParts)
            {
                originalPart.SetFakedInstallStatus(false);
            }
        }
        public void DisassembleSmartEngineModule()
        {
            for(int i = 0; i < fuel_system.chip_parts.Count; i++)
            {
                if (fuel_system.chip_parts[i].installed)
                {
                    fuel_system.chip_parts[i].removePart();
                }
            }
        }

        public override void ModSettings()
        {
            Settings.HideResetAllButton(this);
            ScrewablePart.ScrewablePartApiSettingsShowSize(this);
            Settings.AddHeader(this, "DEBUG");
            Settings.AddCheckBox(this, debugGuiSetting);
            Settings.AddButton(this, resetPosSetting, "Reset uninstalled part location");
            Settings.AddHeader(this, "Settings");
            Settings.AddCheckBox(this, enableAirrideInfoPanelPage);
            Settings.AddCheckBox(this, toggleSixGears);
            Settings.AddCheckBox(this, toggleAWD);
            Settings.AddCheckBox(this, toggleSmoothInput);
            Settings.AddText(this, "This will make throttle input increase over time\n !Dont enable when using controller or any other analog input method!");
            Settings.AddCheckBox(this, settingThrottleBodieTurning);
            Settings.AddHeader(this, "", Color.clear);
            
            Settings.AddText(this, "New Gear ratios + 5th & 6th gear\n" +
                "1.Gear: " + newGearRatio[2] + "\n" +
                "2.Gear: " + newGearRatio[3] + "\n" +
                "3.Gear: " + newGearRatio[4] + "\n" +
                "4.Gear: " + newGearRatio[5] + "\n" +
                "5.Gear: " + newGearRatio[6] + "\n" +
                "6.Gear: " + newGearRatio[7]
                );
            BugReporter.BugReporter.SetupModSettings(this);

        }
        public override void OnSave()
        {
            fuel_injectors_box.CheckUnpackedOnSave();
            throttle_bodies_box.CheckUnpackedOnSave();
            try
            {
                partsList.ForEach(delegate (SimplePart part)
                {
                    SaveLoad.SerializeSaveFile<PartSaveInfo>(this, part.getSaveInfo(), part.saveFile);
                });
            }
            catch(Exception ex)
            {
                Logger.New("Error while trying to save part", ex);
            }

            try
            {
                fuel_system.chip_parts.ForEach(delegate (ChipPart chip)
                {

                    SaveLoad.SerializeSaveFile<ChipSave>(this, chip.chipSave, chip.mapSaveFile);
                    SaveLoad.SerializeSaveFile<PartSaveInfo>(this, chip.getSaveInfo(), chip.saveFile);
                });
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to save chip part", ex);
            }

            try
            {
                foreach (SimplePart part in partsList)
                {
                    partsBuySave = part.GetBought(partsBuySave);
                }
                SaveLoad.SerializeSaveFile<Dictionary<string, bool>>(this, partsBuySave, modsShop_saveFile);
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to save mod shop bought parts file", ex);
            }

            try
            {
                ScrewablePart.SaveScrews(this, Helper.GetScrewablePartsArrayFromPartsList(partsList), screwable_saveFile);
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to save screws ", $"save file: {screwable_saveFile}", ex);
            }
            fuel_system.Save();
        }

        
        
        public override void OnGUI()
        {
            bugReporter.HandleDescriptionGui();
            saveFileRenamer.GuiHandler();
            overrideFileRenamer.GuiHandler();

            if ((bool)debugGuiSetting.Value)
            {
                guiDebug.Handle(new GuiDebugInfo[]
                {
                    new GuiDebugInfo("Cruise control", "true = Good"),
                    new GuiDebugInfo("Cruise control", "false = Bad (cruise control won't work)"),
                    new GuiDebugInfo("Cruise control", "Gear not R", (CarH.drivetrain.gear != 0).ToString()),
                    new GuiDebugInfo("Cruise control", "cruise panel installed", cruise_control_panel_part.installed.ToString()),
                    new GuiDebugInfo("Cruise control", "cruise control enabled", cruiseControlModuleEnabled.ToString()),
                    new GuiDebugInfo("Cruise control", "mounting plate installed", mounting_plate_part.InstalledScrewed().ToString()),
                    new GuiDebugInfo("Cruise control", "smart engine module installed", smart_engine_module_part.InstalledScrewed().ToString()),
                    new GuiDebugInfo("Cruise control", "not on throttle", (CarH.carController.throttleInput <= 0f).ToString()),
                    new GuiDebugInfo("Cruise control", "speed above 20km/h", (CarH.drivetrain.differentialSpeed >= 20f).ToString()),
                    new GuiDebugInfo("Cruise control", "brake not pressed", (!CarH.carController.brakeKey).ToString()),
                    new GuiDebugInfo("Cruise control", "clutch not pressed", (!cInput.GetKey("Clutch")).ToString()),
                    new GuiDebugInfo("Cruise control", "handbrake not pressed", (CarH.carController.handbrakeInput <= 0f).ToString()),
                    new GuiDebugInfo("Cruise control", "set cruise control speed", setCruiseControlSpeed.ToString()),
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
                foreach (Part part in partsList)
                {
                    if (!part.installed)
                    {
                        part.activePart.transform.position = part.defaultPartSaveInfo.position;
                    }
                }

                //fuel_injectors_box.logic.CheckBoxPosReset(partBuySave.bought_fuel_injectors_box);
                //throttle_bodies_box.logic.CheckBoxPosReset(partBuySave.bought_throttle_bodies_box);
                foreach (ChipPart chip in fuel_system.chip_parts)
                {
                    if (!chip.installed)
                    {
                        chip.activePart.transform.position = chip.defaultPartSaveInfo.position;
                    }
                }

            }
            catch (Exception ex)
            {

                ModConsole.Error("Error while trying to save parts: " + ex.Message);
            }
        }

        private void ToggleSixGears()
        {
            if (toggleSixGears.Value is bool value)
            {
                if (value)
                {
                    CarH.drivetrain.gearRatios = newGearRatio;
                    return;
                }
            }
            CarH.drivetrain.gearRatios = originalGearRatios;
            return;
        }
        private void ToggleSmoothInput()
        {
            CarH.axisCarController.smoothInput = !CarH.axisCarController.smoothInput;
        }
        private void ToggleAWD()
        {
            if (toggleAWD.Value is bool value)
            {
                if (value)
                {
                    CarH.drivetrain.SetTransmission(Drivetrain.Transmissions.AWD);
                    return;
                }
            }
            CarH.drivetrain.SetTransmission(Drivetrain.Transmissions.FWD);
        }
    }
}
