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
         *  Pedestrian protection system -> Gameobject HumanTriggerCrime?
         *  Rain sensor
         *  Tire Pressure Monitoring
         *  Traffic sign recognition
         *  Turning assistant
         *  (not possible) Wrong-way driving warning
         *  Save state of modules and load them
         *  Save all information in single file/object
         */

        /*  Changelog (v1.4.4)
         *  Tiny update to get mod ready for next turbo mod update.
         *  Fixed issue with mod spamming output_log.txt file with missing object
         *  Fixed issue with mod spamming ecu_mod_logs.txt when pressing the write chip button and the programmer would write the error messages to the screen
         */
        /* BUGS/Need to fix
         * Optimize code both turbo and ecu (only update when needed)
         * Optimize code both turbo and ecu (only have one method instead of two for small and big turbo)
         * improve fps when using info panel -> don't update each frame
         * EDU mod: adjust triggers to be at the same location as the part itself and smaller trigger area
         * ECU mod: add ERRor to display if something is wrong
         */
#endif
        public override string ID => "DonnerTech_ECU_Mod"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing ECUs"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.4.4"; //Version
        public override bool UseAssetsFolder => true;

        SaveFileRenamer saveFileRenamer;
        OverrideFileRenamer overrideFileRenamer;
        public AssetBundle assetBundle;
        public AssetBundle screwableAssetsBundle;
        public Logger logger;

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
        private const string airride_fl_saveFile = "airride_fl_saveFile.txt";
        private const string airride_fr_saveFile = "airride_fr_saveFile.txt";
        private const string awd_gearbox_saveFile = "awd_gearbox_saveFile.txt";
        private const string awd_propshaft_saveFile = "awd_propshaft_saveFile.txt";
        private const string awd_differential_saveFile = "awd_differntial_saveFile.txt";
#endif
        public PartBuySave partBuySave;




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


        private static bool cruiseControlDebugEnabled = false;
        private int setCruiseControlSpeed = 0;
        private bool cruiseControlModuleEnabled = false;

        private static string modAssetsFolder;

        public AbsPart abs_module_part { get; set; }
        public EspPart esp_module_part { get; set; }
        public TcsPart tcs_module_part { get; set; }
        public SimplePart cable_harness_part { get; set; }
        public SimplePart mounting_plate_part { get; set; }
        public SimplePart smart_engine_module_part { get; set; }
        public SimplePart cruise_control_panel_part { get; set; }
        
        public SimplePart reverse_camera_part { get; set; }
        public SimplePart rain_light_sensor_board_part { get; set; }

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
        public Vector3 rain_light_sensor_board_installLocation = new Vector3(-0.0015f, 0.086f, 0.1235f);

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

        public Vector3 abs_module_installLocation = new Vector3(0.058f, 0.022f, 0.116f);
        public Vector3 esp_module_installLocation = new Vector3(0.0235f, 0.023f, -0.0245f);
        public Vector3 smart_engine_module_installLocation = new Vector3(0.072f, 0.024f, -0.1425f);
        public Vector3 tcs_module_installLocation = new Vector3(-0.03f, 0.0235f, -0.154f);
        public Vector3 cable_harness_installLocation = new Vector3(-0.117f, 0.0102f, -0.024f);
        public Vector3 mounting_plate_installLocation = new Vector3(0.3115f, -0.276f, -0.0393f);


        public GameObject satsuma;
        public Drivetrain satsumaDriveTrain;
        public CarController satsumaCarController;
        public AxisCarController axisCarController;
        public Axles satsumaAxles;
        private FsmBool electricsOK;


        private const string logger_saveFile = "ecu_mod_logs.txt";
        private const string abs_module_saveFile = "abs_module_saveFile.txt";
        private const string esp_module_saveFile = "esp_module_saveFile.txt";
        private const string tcs_module_saveFile = "tcs_module_saveFile.txt";
        private const string cable_harness_saveFile = "cable_harness_saveFile.txt";
        private const string mounting_plate_saveFile = "mounting_plate_saveFile.txt";
        private const string mod_shop_saveFile = "mod_shop_saveFile.txt";
        private const string smart_engine_module_saveFile = "smart_engine_module_saveFile.txt";
        private const string cruise_control_panel_saveFile = "cruise_control_panel_saveFile.txt";

        private const string reverse_camera_saveFile = "reverse_camera_saveFile.txt";
        private const string rain_light_sensor_board_saveFile = "rain_light_sensor_board_saveFile.txt";

        private const string fuel_pump_cover_saveFile = "fuel_pump_cover_saveFile.txt";
        private const string fuel_injection_manifold_saveFile = "fuel_injection_manifold_saveFile.txt";
        private const string fuel_rail_saveFile = "fuel_rail_saveFile.txt";
        private const string chip_programmer_saveFile = "chip_programmer_saveFile.txt";
        private const string electric_fuel_pump_saveFile = "electric_fuel_pump_saveFile.txt";


        private const string screwable_saveFile = "screwable_saveFile.txt";

        private Settings resetPosSetting = new Settings("resetPos", "Reset", Helper.WorkAroundAction);
        private Settings debugCruiseControlSetting = new Settings("debugCruiseControl", "Show/Hide", Helper.WorkAroundAction);
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

        private static AudioSource dashButtonAudioSource
        {
            get
            {
                return GameObject.Find("dash_button").GetComponent<AudioSource>();
            }
        }

        public override void OnNewGame()
        {
            partsList.ForEach(delegate (SimplePart part)
            {
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, part.saveFile);
            });
            fuel_system.chip_parts.ForEach(delegate (ChipPart chip)
            {
                SaveLoad.SerializeSaveFile<ChipSave>(this, null, chip.fuelMap_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, chip.saveFile);
            });
            SaveLoad.SerializeSaveFile<PartBuySave>(this, partBuySave, mod_shop_saveFile);
        }
        public override void OnLoad()
        {
            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " started loading");

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

            List<BugReporter.Report> reports = new List<BugReporter.Report>();
            
            reports.Add(new BugReporter.Report("Mod Settings", new string[] { ModLoader.GetModConfigFolder(this) }, true));
            reports.Add(new BugReporter.Report("ModLoader Output", new string[] { Helper.CombinePaths(new string[] { Path.GetFullPath("."), "mysummercar_Data", "output_log.txt" }) }));

            BugReporter.Report gameSave_report = new BugReporter.Report("MSC Savegame");
            gameSave_report.files = Directory.GetFiles(Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"..\LocalLow\Amistech\My Summer Car\")));

            reports.Add(gameSave_report);

            BugReporter.BugReporter bugReporter = new BugReporter.BugReporter(this, reports);

            resetPosSetting.DoAction = PosReset;
            toggleAWD.DoAction = ToggleAWD;
            toggleSmoothInput.DoAction = ToggleSmoothInput;
            toggleSixGears.DoAction = ToggleSixGears;
            debugCruiseControlSetting.DoAction = SwitchCruiseControlDebug;

            logger = new Logger(this, logger_saveFile, 100);
            if (!ModLoader.CheckSteam())
            {
                ModUI.ShowMessage("Cunt", "CUNT");
                ModConsole.Print("Cunt detected");
            }

            partsList = new List<SimplePart>();

            modAssetsFolder = ModLoader.GetModAssetsFolder(this);
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
            axisCarController = satsuma.GetComponent<AxisCarController>();
            satsumaAxles = satsuma.GetComponent<Axles>();
            
            originalGearRatios = satsumaDriveTrain.gearRatios;

#if DEBUG
            PlayMakerFSM gearboxFSM = GameObject.Find("DatabaseMotor/Gearbox").GetComponent<PlayMakerFSM>();
            PlayMakerFSM drivegearFSM = GameObject.Find("DatabaseMotor/Drivegear").GetComponent<PlayMakerFSM>();
            FsmBool gearbox_installed = gearboxFSM.FsmVariables.FindFsmBool("Installed");
            FsmBool gearbox_bolted = gearboxFSM.FsmVariables.FindFsmBool("Bolted");

            FsmBool drivegear_installed = drivegearFSM.FsmVariables.FindFsmBool("Installed");
            FsmBool drivegear_bolted = drivegearFSM.FsmVariables.FindFsmBool("Bolted");
#endif


            ToggleSixGears();
            ToggleAWD();


            assetBundle = LoadAssets.LoadBundle(this, "ecu-mod.unity3d");
            screwableAssetsBundle = LoadAssets.LoadBundle(this, "screwableapi.unity3d");
            if (assetBundle == null)
            {
                ModConsole.Warning("There was an error while trying to load the assets file");
                ModConsole.Warning("Mod will try to load the file again now...");
                assetBundle = LoadAssets.LoadBundle(this, "ecu-mod.unity3d");
                if (assetBundle == null)
                {
                    ModConsole.Warning("There was an error while trying to load the assets file");
                    ModConsole.Warning("A retry also did not solve this problem");
                    ModConsole.Error("Error while trying to load " + "'" + modAssetsFolder + "ecu-mod.unity3d" + "'");
                }
            }
            GameObject abs_module = (assetBundle.LoadAsset("abs-module.prefab") as GameObject);
            GameObject esp_module = (assetBundle.LoadAsset("esp-module.prefab") as GameObject);
            GameObject tcs_module = (assetBundle.LoadAsset("tcs-module.prefab") as GameObject);
            GameObject cable_harness = (assetBundle.LoadAsset("cable-harness.prefab") as GameObject);
            GameObject mounting_plate = (assetBundle.LoadAsset("mounting-plate.prefab") as GameObject);
            GameObject smart_engine_module = (assetBundle.LoadAsset("engine-module.prefab") as GameObject);
            GameObject cruise_control_panel = (assetBundle.LoadAsset("cruise-control-panel.prefab") as GameObject);
            
            
            GameObject reverse_camera = (assetBundle.LoadAsset("reverse-camera.prefab") as GameObject);
            GameObject rain_light_sensor_board = (assetBundle.LoadAsset("rain-light-sensorboard.prefab") as GameObject);


            GameObject fuel_injector = (assetBundle.LoadAsset("fuel-injector.prefab") as GameObject);
            GameObject throttle_body = (assetBundle.LoadAsset("throttle-body.prefab") as GameObject);

            fuel_injectors_box_gameObject =  GameObject.Instantiate((assetBundle.LoadAsset("fuel-injectors-box.prefab") as GameObject));
            throttle_bodies_box_gameObject = GameObject.Instantiate((assetBundle.LoadAsset("throttle-bodies-box.prefab") as GameObject));

            GameObject fuel_pump_cover = (assetBundle.LoadAsset("fuel-pump-cover.prefab") as GameObject);
            GameObject fuel_injection_manifold = (assetBundle.LoadAsset("fuel-injection-manifold.prefab") as GameObject);
            GameObject fuel_rail = (assetBundle.LoadAsset("fuel-rail.prefab") as GameObject);
            GameObject chip_programmer = (assetBundle.LoadAsset("chip-programmer.prefab") as GameObject);

            chip = GameObject.Instantiate((assetBundle.LoadAsset("chip.prefab") as GameObject));
            chip.SetActive(false);

            GameObject electric_fuel_pump = (assetBundle.LoadAsset("electric-fuel-pump.prefab") as GameObject);
            GameObject wires_injectors_pumps_gameObject = GameObject.Instantiate((assetBundle.LoadAsset("wires-injectors-pumps.prefab") as GameObject));
            GameObject wires_sparkPlugs1_gameObject = GameObject.Instantiate((assetBundle.LoadAsset("wires-sparkPlugs-1.prefab") as GameObject));
            GameObject wires_sparkPlugs2_gameObject = GameObject.Instantiate((assetBundle.LoadAsset("wires-sparkPlugs-2.prefab") as GameObject));

            wires_injectors_pumps = wires_injectors_pumps_gameObject.transform.FindChild("default").GetComponent<MeshRenderer>();
            wires_sparkPlugs1 = wires_sparkPlugs1_gameObject.transform.FindChild("default").GetComponent<MeshRenderer>();
            wires_sparkPlugs2 = wires_sparkPlugs2_gameObject.transform.FindChild("default").GetComponent<MeshRenderer>();
#if DEBUG
            GameObject awd_gearbox = (assetBundle.LoadAsset("AWD-Gearbox.prefab") as GameObject);
            GameObject awd_differential = (assetBundle.LoadAsset("AWD-Differential.prefab") as GameObject);
            GameObject awd_propshaft = (assetBundle.LoadAsset("AWD-Propshaft.prefab") as GameObject);
            GameObject airride_fl = (assetBundle.LoadAsset("Airride_FL.prefab") as GameObject);
            GameObject airride_fr = (assetBundle.LoadAsset("Airride_FL.prefab") as GameObject);
            Helper.SetObjectNameTagLayer(awd_gearbox, "AWD Gearbox");
            Helper.SetObjectNameTagLayer(awd_differential, "AWD Differential");
            Helper.SetObjectNameTagLayer(awd_propshaft, "AWD Propshaft");
            Helper.SetObjectNameTagLayer(airride_fl, "Airride FL");
            Helper.SetObjectNameTagLayer(airride_fr, "Airride FR");
#endif
            Helper.SetObjectNameTagLayer(abs_module, "ABS Module");
            Helper.SetObjectNameTagLayer(esp_module, "ESP Module");
            Helper.SetObjectNameTagLayer(tcs_module, "TCS Module");
            Helper.SetObjectNameTagLayer(cable_harness, "ECU Cable Harness");
            Helper.SetObjectNameTagLayer(mounting_plate, "ECU Mounting Plate");
            Helper.SetObjectNameTagLayer(smart_engine_module, "Smart Engine ECU");
            Helper.SetObjectNameTagLayer(cruise_control_panel, "Cruise Control Panel");

            Helper.SetObjectNameTagLayer(reverse_camera, "Reverse Camera");
            Helper.SetObjectNameTagLayer(rain_light_sensor_board, "Rain & Light Sensorboard");

            Helper.SetObjectNameTagLayer(fuel_injectors_box_gameObject, "Fuel Injectors(Clone)");

            Helper.SetObjectNameTagLayer(throttle_bodies_box_gameObject, "Throttle Bodies(Clone)");

            Helper.SetObjectNameTagLayer(chip, "Chip");
            Helper.SetObjectNameTagLayer(chip_programmer, "Chip Programmer");

            Helper.SetObjectNameTagLayer(fuel_pump_cover, "Fuel Pump Cover");
            Helper.SetObjectNameTagLayer(fuel_injection_manifold, "Fuel Injection Manifold");
            Helper.SetObjectNameTagLayer(fuel_rail, "Fuel Rail");

            Helper.SetObjectNameTagLayer(electric_fuel_pump, "Electric Fuel Pump");
            Helper.SetObjectNameTagLayer(wires_injectors_pumps_gameObject, "wires_injectors_pumps");
            Helper.SetObjectNameTagLayer(wires_sparkPlugs1_gameObject, "wires_sparkPlugs1");
            Helper.SetObjectNameTagLayer(wires_sparkPlugs2_gameObject, "wires_sparkPlugs2");

            partBuySave = Helper.LoadSaveOrReturnNew<PartBuySave>(this, mod_shop_saveFile);

            saveFileRenamer = new SaveFileRenamer(this);
            overrideFileRenamer = new OverrideFileRenamer(this);

            SortedList<String, Screws> screwListSave = ScrewablePart.LoadScrews(this, screwable_saveFile);

#if DEBUG
            airride_fl_part = new SimplePart(
                    SimplePart.LoadData(this, airride_fl_saveFile, partBuySave.bought_airrideFL),
                    airride_fl,
                    GameObject.Find("Chassis/FL"),
                    new Trigger("airride_fl_trigger", GameObject.Find("Chassis/FL"), airride_fl_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    airride_fl_installLocation,
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 0, 0)
                    }
                );
            awd_gearbox_part = new SimplePart(
                    SimplePart.LoadData(this, awd_gearbox_saveFile, partBuySave.boughtAwdGearbox),
                    awd_gearbox,
                    GameObject.Find("pivot_gearbox"),
                     new Trigger("ECU_MOD_AWD_Gearbox_Trigger", GameObject.Find("pivot_gearbox"), awd_gearbox_part_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    awd_gearbox_part_installLocation,
                    new Quaternion
                    {
                        eulerAngles = new Vector3(90, 0, 0)
                    }
                );
            awd_differential_part = new SimplePart(
                SimplePart.LoadData(this, awd_differential_saveFile, partBuySave.boughtAwdDifferential),
                awd_differential,
                satsuma,
                new Trigger("ECU_MOD_AWD_Differential_Trigger", satsuma, awd_differential_part_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                awd_differential_part_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 0, 0)
                }
            );

            awd_propshaft_part = new SimplePart(
                SimplePart.LoadData(this, awd_propshaft_saveFile, partBuySave.boughtAwdPropshaft),
                awd_propshaft,
                satsuma,
                new Trigger("ECU_MOD_AWD_Propshaft_Trigger", satsuma, awd_propshaft_part_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                awd_propshaft_part_installLocation,
                new Quaternion
                {
                    eulerAngles = new Vector3(0, 180, 0)
                }
            );
            partsList.Add(airride_fl_part);
            partsList.Add(awd_gearbox_part);
            partsList.Add(awd_differential_part);
            partsList.Add(awd_propshaft_part);
#endif

            mounting_plate_part = new SimplePart(
                SimplePart.LoadData(this, mounting_plate_saveFile, partBuySave.boughtMountingPlate),
                mounting_plate,
                satsuma,
                new Trigger("mounting_plate_Trigger", satsuma, mounting_plate_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.32f, 0.02f, 0.45f), false),
                mounting_plate_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );
            mounting_plate_part.SetDisassembleFunction(new Action(DisassembleMountingPlate));


            smart_engine_module_part = new SimplePart(
                SimplePart.LoadData(this, smart_engine_module_saveFile, partBuySave.boughtSmartEngineModule),
                smart_engine_module,
                mounting_plate_part.rigidPart,
                new Trigger("smart_engine_module_Trigger", mounting_plate_part.rigidPart, smart_engine_module_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.02f, 0.13f), false),
                smart_engine_module_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );
            smart_engine_module_part.SetDisassembleFunction(new Action(DisassembleSmartEngineModule));
            smart_engine_module_logic = smart_engine_module_part.rigidPart.AddComponent<SmartEngineModule_Logic>();

            
            abs_module_part = new AbsPart(
                SimplePart.LoadData(this, abs_module_saveFile, partBuySave.boughtABSModule),
                abs_module,
                mounting_plate_part.rigidPart,
                new Trigger("abs_module_Trigger", mounting_plate_part.rigidPart, abs_module_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.14f, 0.02f, 0.125f), false),
                abs_module_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );
            
            esp_module_part = new EspPart(
                SimplePart.LoadData(this, esp_module_saveFile, partBuySave.boughtESPModule),
                esp_module,
                mounting_plate_part.rigidPart,
                 new Trigger("esp_module_Trigger", mounting_plate_part.rigidPart, esp_module_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.21f, 0.02f, 0.125f), false),
                esp_module_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );
            
            tcs_module_part = new TcsPart(
                SimplePart.LoadData(this, tcs_module_saveFile, partBuySave.boughtTCSModule),
                tcs_module,
                mounting_plate_part.rigidPart,
                new Trigger("tcs_module_Trigger", mounting_plate_part.rigidPart, tcs_module_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.104f, 0.02f, 0.104f), false),
                tcs_module_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );
            
            cable_harness_part = new SimplePart(
                SimplePart.LoadData(this, cable_harness_saveFile, partBuySave.boughtCableHarness),
                cable_harness,
                mounting_plate_part.rigidPart,
                new Trigger("cable_harness_Trigger", mounting_plate_part.rigidPart, cable_harness_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.02f, 0.3f), false),
                cable_harness_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
            );

            cruise_control_panel_part = new SimplePart(
                SimplePart.LoadData(this, cruise_control_panel_saveFile, partBuySave.boughtCruiseControlPanel),
                cruise_control_panel,
                GameObject.Find("dashboard(Clone)"),
                new Trigger("cruise_control_panel_Trigger", GameObject.Find("dashboard(Clone)"), cruise_control_panel_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                cruise_control_panel_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            cruise_control_logic = cruise_control_panel_part.rigidPart.AddComponent<CruiseControl_Logic>();

            info_panel = new InfoPanel(this, screwListSave);



            rain_light_sensor_board_part = new SimplePart(
                SimplePart.LoadData(this, rain_light_sensor_board_saveFile, partBuySave.bought_rainLightSensorboard),
                rain_light_sensor_board,
                GameObject.Find("dashboard(Clone)"),
                new Trigger("rain_light_sensor_board_trigger", GameObject.Find("dashboard(Clone)"), rain_light_sensor_board_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                rain_light_sensor_board_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );

            reverse_camera_part = new SimplePart(
                SimplePart.LoadData(this, reverse_camera_saveFile, partBuySave.bought_reverseCamera),
                reverse_camera,
                GameObject.Find("bootlid(Clone)"),
                new Trigger("reverse_camera_trigger", GameObject.Find("bootlid(Clone)"), reverse_camera_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                reverse_camera_installLocation,
                new Quaternion { eulerAngles = new Vector3(120, 0, 0) }
            );
            reverse_camera_logic = reverse_camera_part.rigidPart.AddComponent<ReverseCamera_Logic>();

            fuel_injection_manifold_part = new SimplePart(
                SimplePart.LoadData(this, fuel_injection_manifold_saveFile, partBuySave.bought_fuel_injection_manifold),
                fuel_injection_manifold,
                GameObject.Find("cylinder head(Clone)"),
                new Trigger("fuel_injection", GameObject.Find("cylinder head(Clone)"), fuel_injection_manifold_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                fuel_injection_manifold_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }/*,
                GameObject.Find("Racing Carburators")*/
            );

            fuel_injection_manifold_part.SetDisassembleFunction(new Action(DisassembleFuelInjectionManifold));

            fuel_pump_cover_part = new SimplePart(
                SimplePart.LoadData(this, fuel_pump_cover_saveFile, partBuySave.bought_fuel_pump_cover),
                fuel_pump_cover,
                GameObject.Find("block(Clone)"),
                new Trigger("fuel_pump_cover", GameObject.Find("block(Clone)"), fuel_pump_cover_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                fuel_pump_cover_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            fuel_pump_cover_part.SetDisassembleFunction(DisassembleFuelInjectionPump);

            fuel_rail_part = new SimplePart(
                SimplePart.LoadData(this, fuel_rail_saveFile, partBuySave.bought_fuel_rail),
                fuel_rail,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("fuel_rail", fuel_injection_manifold_part.rigidPart, fuel_rail_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                fuel_rail_installLocation,
                new Quaternion { eulerAngles = new Vector3(30, 0, 0) }
            );

            GameObject emptyGameObject = new GameObject();
            chip_programmer_part = new SimplePart(
                SimplePart.LoadData(this, chip_programmer_saveFile, partBuySave.bought_chip_programmer),
                chip_programmer,
                emptyGameObject,
                new Trigger("chip_programmer", emptyGameObject, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                new Vector3(0, 0, 0),
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
                );

            electric_fuel_pump_part = new SimplePart(
                SimplePart.LoadData(this, electric_fuel_pump_saveFile, partBuySave.bought_electric_fuel_pump),
                electric_fuel_pump,
                satsuma,
                new Trigger("electric_fuel_pump", satsuma, electric_fuel_pump_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                electric_fuel_pump_installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 0) }
            );
            electric_fuel_pump_part.SetDisassembleFunction(DisassembleFuelInjectionPump);
            electric_fuel_pump_part.rigidPart.transform.FindChild("fuelLine-1").GetComponent<Renderer>().enabled = true;
            electric_fuel_pump_part.rigidPart.transform.FindChild("fuelLine-2").GetComponent<Renderer>().enabled = true;

            fuel_injectors_box = new Box(this, fuel_injectors_box_gameObject, fuel_injector, "fuel_injector", 4, "Fuel Injector", fuel_injection_manifold_part, partBuySave.bought_fuel_injectors_box,
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

            throttle_bodies_box = new Box(this, throttle_bodies_box_gameObject, throttle_body, "throttle_body", 4, "Throttle Body", fuel_injection_manifold_part, partBuySave.bought_throttle_bodies_box,
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
            throttle_bodies_box.AddScrewable(screwListSave, screwableAssetsBundle,
                new Screw[] {
                            new Screw(new Vector3(0.016f, -0.016f, -0.011f), new Vector3(0, 0, 0), 0.6f, 8),
                            new Screw(new Vector3(-0.016f, 0.016f, -0.011f), new Vector3(0, 0, 0), 0.6f, 8),
                });


            /*
            fuel_injection_manifold_part.SetConditions(new SimplePart[] {
                                fuel_rail_part,
                                throttle_body1_part,
                                throttle_body2_part,
                                throttle_body3_part,
                                throttle_body4_part,
                                fuel_injector1_part,
                                fuel_injector2_part,
                                fuel_injector3_part,
                                fuel_injector4_part
                });
            fuel_injection_manifold_part.SetOriginalConditions(new GameObject[]
            {
                GameObject.Find()
            });
            */

            partsList.Add(abs_module_part);
            partsList.Add(esp_module_part);
            partsList.Add(tcs_module_part);
            partsList.Add(cable_harness_part);
            partsList.Add(mounting_plate_part);
            partsList.Add(smart_engine_module_part);
            partsList.Add(cruise_control_panel_part);

            partsList.Add(rain_light_sensor_board_part);
            partsList.Add(reverse_camera_part);

            partsList.Add(fuel_pump_cover_part);
            partsList.Add(fuel_injection_manifold_part);
            partsList.Add(fuel_rail_part);
            partsList.Add(chip_programmer_part);
            partsList.Add(electric_fuel_pump_part);

            wires_injectors_pumps_gameObject.transform.parent = fuel_injection_manifold_part.rigidPart.transform;
            wires_sparkPlugs1_gameObject.transform.parent = GameObject.Find("cylinder head(Clone)").transform;
            wires_sparkPlugs2_gameObject.transform.parent = satsuma.transform;

            wires_injectors_pumps_gameObject.transform.localPosition = new Vector3(0.0085f, 0.053f, 0.0366f); //Temp
            wires_sparkPlugs1_gameObject.transform.localPosition = new Vector3(-0.001f, 0.088f, 0.055f); //Temp
            wires_sparkPlugs2_gameObject.transform.localPosition = new Vector3(0.105f, 0.233f, 0.97f); //Temp

            wires_injectors_pumps_gameObject.transform.localRotation = new Quaternion() { eulerAngles = new Vector3(0, 0, 0) }; //Temp
            wires_sparkPlugs1_gameObject.transform.localRotation = new Quaternion() { eulerAngles = new Vector3(90, 0, 0) }; //Temp
            wires_sparkPlugs2_gameObject.transform.localRotation = new Quaternion() { eulerAngles = new Vector3(0, 180, 0) }; //Temp

            wires_injectors_pumps.enabled = false;
            wires_sparkPlugs1.enabled = false;
            wires_sparkPlugs2.enabled = false;


            abs_module_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, abs_module_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.0558f, -0.0025f, -0.0525f), new Vector3(-90, 0, 0), 0.8f, 8),
                    new Screw(new Vector3(0.0558f, -0.0025f, 0.0525f), new Vector3(-90, 0, 0), 0.8f, 8),
                    new Screw(new Vector3(-0.0558f, -0.0025f, 0.0525f), new Vector3(-90, 0, 0), 0.8f, 8),
                    new Screw(new Vector3(-0.0558f, -0.0025f, -0.0525f), new Vector3(-90, 0, 0), 0.8f, 8),
                });

            esp_module_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, esp_module_part.rigidPart,
                new Screw[] {
                        new Screw(new Vector3(0.09f, -0.002f, -0.052f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(0.09f, -0.002f, 0.0528f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.092f, -0.002f, 0.0528f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.092f, -0.002f, -0.052f), new Vector3(-90, 0, 0), 0.8f, 8),
                });

            tcs_module_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, tcs_module_part.rigidPart,
                new Screw[] {
                        new Screw(new Vector3(0.0388f, 0.002f, -0.0418f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(0.0388f, 0.002f, 0.0422f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.0382f, 0.002f, 0.0422f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.0382f, 0.002f, -0.0418f), new Vector3(-90, 0, 0), 0.8f, 8),
                });

            smart_engine_module_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, smart_engine_module_part.rigidPart,
                new Screw[] {
                        new Screw(new Vector3(-0.028f, -0.003f, 0.039f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(0.049f, -0.003f, 0.039f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(0.049f, -0.003f, -0.0625f), new Vector3(-90, 0, 0), 0.8f, 8),
                        new Screw(new Vector3(-0.028f, -0.003f, -0.0625f), new Vector3(-90, 0, 0), 0.8f, 8),
                });

            mounting_plate_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, mounting_plate_part.rigidPart,
                new Screw[] {
                        new Screw(new Vector3(-0.124f, 0.005f, 0.004f), new Vector3(-90, 0, 0), 1.2f, 12),
                        new Screw(new Vector3(-0.124f, 0.005f, 0.207f), new Vector3(-90, 0, 0), 1.2f, 12),
                        new Screw(new Vector3(0.002f, 0.005f, 0.207f), new Vector3(-90, 0, 0), 1.2f, 12),
                        new Screw(new Vector3(0.128f, 0.005f, 0.207f), new Vector3(-90, 0, 0), 1.2f, 12),
                        new Screw(new Vector3(-0.124f, 0.005f, -0.2f), new Vector3(-90, 0, 0), 1.2f, 12),
                });

            rain_light_sensor_board_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, rain_light_sensor_board_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.078f, 0.0055f, 0f), new Vector3(-90, 0, 0), 0.5f, 8),
                    new Screw(new Vector3(-0.078f, 0.0055f, 0f), new Vector3(-90, 0, 0), 0.5f, 8),
                });

            reverse_camera_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, reverse_camera_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0f, -0.015f, 0.0055f), new Vector3(0, 0, 0), 0.5f, 5),
                });


            fuel_pump_cover_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, fuel_pump_cover_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(-0.02f, 0.003f, -0.009f), new Vector3(0, 180, 0), 0.6f, 7, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(0.018f, 0.003f, -0.009f), new Vector3(0, 180, 0), 0.6f, 7, ScrewablePart.ScrewType.Nut),
                });

            fuel_injection_manifold_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, fuel_injection_manifold_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0.0875f, -0.001f, -0.0105f), new Vector3(0, 0, 0), 0.6f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(0.053f, -0.043f, -0.0105f), new Vector3(0, 0, 0), 0.6f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(-0.051f, -0.043f, -0.0105f), new Vector3(0, 0, 0), 0.6f, 8, ScrewablePart.ScrewType.Nut),
                    new Screw(new Vector3(-0.0865f, -0.001f, -0.0105f), new Vector3(0, 0, 0), 0.6f, 8, ScrewablePart.ScrewType.Nut),
                });

            electric_fuel_pump_part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, electric_fuel_pump_part.rigidPart,
                new Screw[] {
                    new Screw(new Vector3(0f, 0.04f, 0.01f), new Vector3(0, 180, 0), 0.6f, 8),
                    new Screw(new Vector3(0f, -0.04f, 0.01f), new Vector3(0, 180, 0), 0.6f, 8),
                });

            if (GameObject.Find("Shop for mods") != null)
            {
                ModsShop.ShopItem modsShop = GameObject.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();

                List<ProductInformation> shopItems = new List<ProductInformation>();
           
                shopItems.Add(new ProductInformation(abs_module_part.activePart, "ABS Module", 800, "abs-module_productImage.png", abs_module_part.bought));
                shopItems.Add(new ProductInformation(esp_module_part.activePart, "ESP Module", 1200, "esp-module_productImage.png", esp_module_part.bought));
                shopItems.Add(new ProductInformation(tcs_module_part.activePart, "TCS Module", 1800, "tcs-module_productImage.png", tcs_module_part.bought));
                shopItems.Add(new ProductInformation(cable_harness_part.activePart, "ECU Cable Harness", 300, "cable-harness_productImage.png", cable_harness_part.bought));
                shopItems.Add(new ProductInformation(mounting_plate_part.activePart, "ECU Mounting Plate", 100, "mounting-plate_productImage.png", mounting_plate_part.bought));
                shopItems.Add(new ProductInformation(smart_engine_module_part.activePart, "Smart Engine Module ECU", 4600, "smart-engine-module_productImage.png", smart_engine_module_part.bought));
                shopItems.Add(new ProductInformation(cruise_control_panel_part.activePart, "Cruise Control Panel with Controller", 2000, "cruise-control_productImage.png", cruise_control_panel_part.bought));
                shopItems.Add(new ProductInformation(info_panel.part.activePart, "ECU Info Panel", 4000, "info-panel_productImage.png", info_panel.part.bought));
                shopItems.Add(new ProductInformation(rain_light_sensor_board_part.activePart, "Rain & Light Sensorboard", 1000, "rain-light-sensorboard_productImage.png", rain_light_sensor_board_part.bought));
                shopItems.Add(new ProductInformation(reverse_camera_part.activePart, "Reverse Camera", 1500, "reverse-camera_productImage.png", reverse_camera_part.bought));
#if DEBUG
                shopItems.Add(new ProductInformation(airride_fl_part.activePart, "Airride fl", 0, null, airride_fl_part.bought));
                shopItems.Add(new ProductInformation(awd_gearbox_part.activePart, "AWD Gearbox", 0, null, awd_gearbox_part.bought));
                shopItems.Add(new ProductInformation(awd_differential_part.activePart, "AWD Differential", 0, null, awd_differential_part.bought));
                shopItems.Add(new ProductInformation(awd_propshaft_part.activePart, "AWD Propshaft", 0, null, awd_propshaft_part.bought));
#endif

                shopItems.Add(new ProductInformation(fuel_injectors_box_gameObject, "Fuel Injectors", 800, "fuel-injectors-box_productImage.png", partBuySave.bought_fuel_injectors_box));
                shopItems.Add(new ProductInformation(throttle_bodies_box_gameObject, "Throttle Bodies", 1200, "throttle-bodies-box_productImage.png", partBuySave.bought_throttle_bodies_box));

                shopItems.Add(new ProductInformation(fuel_pump_cover_part.activePart, "Fuel Pump Cover", 120, "fuel-pump-cover-plate_productImage.png", partBuySave.bought_fuel_pump_cover));
                shopItems.Add(new ProductInformation(fuel_injection_manifold_part.activePart, "Fuel Injection Manifold", 1600, "fuel-injection-manifold_productImage.png", partBuySave.bought_fuel_injection_manifold));
                shopItems.Add(new ProductInformation(fuel_rail_part.activePart, "Fuel Rail", 375, "fuel-rail_productImage.png", partBuySave.bought_fuel_rail));
                shopItems.Add(new ProductInformation(chip_programmer_part.activePart, "Chip Programmer", 3799, "chip-programmer_productImage.png", partBuySave.bought_chip_programmer));
                shopItems.Add(new ProductInformation(electric_fuel_pump_part.activePart, "Electric Fuel Pump", 500, "electric-fuel-pump_productImage.png", partBuySave.bought_electric_fuel_pump));

                //Shop shop = new Shop(this, modsShopItem, assetBundle, partBuySave, shopItems);
                Shop shop = new Shop(this, modsShop, assetBundle, partBuySave, shopItems);
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
            screwableAssetsBundle.Unload(false);
            UnityEngine.Object.Destroy(abs_module);
            UnityEngine.Object.Destroy(esp_module);
            UnityEngine.Object.Destroy(tcs_module);
            UnityEngine.Object.Destroy(cable_harness);
            UnityEngine.Object.Destroy(mounting_plate);
            UnityEngine.Object.Destroy(smart_engine_module);
            UnityEngine.Object.Destroy(cruise_control_panel);
            
            UnityEngine.Object.Destroy(rain_light_sensor_board);
            UnityEngine.Object.Destroy(reverse_camera);
#if DEBUG
            UnityEngine.Object.Destroy(airride_fl);
            UnityEngine.Object.Destroy(awd_gearbox);
            UnityEngine.Object.Destroy(awd_differential);
            UnityEngine.Object.Destroy(awd_propshaft);
#endif
            UnityEngine.Object.Destroy(fuel_pump_cover);
            UnityEngine.Object.Destroy(fuel_injection_manifold);
            UnityEngine.Object.Destroy(fuel_rail);
            UnityEngine.Object.Destroy(fuel_injector);
            UnityEngine.Object.Destroy(throttle_body);
            UnityEngine.Object.Destroy(chip_programmer);
            UnityEngine.Object.Destroy(electric_fuel_pump);

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");

            
            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " finished loading");
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
            Settings.AddButton(this, debugCruiseControlSetting, "DEBUG Cruise Control");
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
            fuel_injectors_box.logic.CheckUnpackedOnSave(partBuySave.bought_fuel_injectors_box);
            throttle_bodies_box.logic.CheckUnpackedOnSave(partBuySave.bought_throttle_bodies_box);
            try
            {
                partsList.ForEach(delegate (SimplePart part)
                {
                    SaveLoad.SerializeSaveFile<PartSaveInfo>(this, part.getSaveInfo(), part.saveFile);
                });
            }
            catch(Exception ex)
            {
                logger.New("Error while trying to save part", "", ex);
            }

            try
            {
                fuel_system.chip_parts.ForEach(delegate (ChipPart chip)
                {

                    SaveLoad.SerializeSaveFile<ChipSave>(this, chip.chipSave, chip.fuelMap_saveFile);
                    SaveLoad.SerializeSaveFile<PartSaveInfo>(this, chip.getSaveInfo(), chip.saveFile);
                });
            }
            catch (Exception ex)
            {
                logger.New("Error while trying to save chip part", "", ex);
            }

            try
            {
                SaveLoad.SerializeSaveFile<PartBuySave>(this, partBuySave, mod_shop_saveFile);
            }
            catch (Exception ex)
            {
                logger.New("Error while trying to save mod shop bought parts file", "", ex);
            }

            try
            {
                ScrewablePart.SaveScrews(this, new ScrewablePart[]
                {
                    abs_module_part.screwablePart,
                    esp_module_part.screwablePart,
                    tcs_module_part.screwablePart,
                    smart_engine_module_part.screwablePart,
                    mounting_plate_part.screwablePart,
                    info_panel.part.screwablePart,
                    rain_light_sensor_board_part.screwablePart,
                    reverse_camera_part.screwablePart,
                    throttle_bodies_box.parts[0].screwablePart,
                    throttle_bodies_box.parts[1].screwablePart,
                    throttle_bodies_box.parts[2].screwablePart,
                    throttle_bodies_box.parts[3].screwablePart,
                    fuel_injection_manifold_part.screwablePart,
                    electric_fuel_pump_part.screwablePart,
                    fuel_pump_cover_part.screwablePart,
                }, screwable_saveFile);
            }
            catch (Exception ex)
            {
                logger.New("Error while trying to save screws ", $"save file: {screwable_saveFile}", ex);
            }

            fuel_system.Save();

        }

        
        public override void OnGUI()
        {
            saveFileRenamer.GuiHandler();
            overrideFileRenamer.GuiHandler();
            if (cruiseControlDebugEnabled)
            {
                GUI.Label(new Rect(20, 400, 500, 100), "------------------------------------");
                GUI.Label(new Rect(20, 420, 500, 100), "true = correct value for cruise control to work");
                GUI.Label(new Rect(20, 440, 500, 100), "false = conition needed to have cruise control working");
                GUI.Label(new Rect(20, 460, 500, 100), "Gear not R: " + (satsumaDriveTrain.gear != 0));
                GUI.Label(new Rect(20, 480, 500, 100), "cruise control panel installed: " + cruise_control_panel_part.installed);
                GUI.Label(new Rect(20, 500, 500, 100), "cruise control enabled: " + cruiseControlModuleEnabled);
                GUI.Label(new Rect(20, 520, 500, 100), "mounting plate fully installed: " + mounting_plate_part.InstalledScrewed());
                GUI.Label(new Rect(20, 540, 500, 100), "smart engine module fully installed: " + smart_engine_module_part.InstalledScrewed());
                GUI.Label(new Rect(20, 560, 500, 100), "not on throttle: " + (satsumaCarController.throttleInput <= 0f));
                GUI.Label(new Rect(20, 580, 500, 100), "speed above 20km/h: " + (satsumaDriveTrain.differentialSpeed >= 20f));
                GUI.Label(new Rect(20, 600, 500, 100), "brake not pressed: " + (!satsumaCarController.brakeKey));
                GUI.Label(new Rect(20, 620, 500, 100), "clutch not pressed: " + (!cInput.GetKey("Clutch")));
                GUI.Label(new Rect(20, 640, 500, 100), "handbrake not pressed: " + (satsumaCarController.handbrakeInput <= 0f));
                GUI.Label(new Rect(20, 660, 500, 100), "set cruise control speed: " + setCruiseControlSpeed);
                GUI.Label(new Rect(20, 680, 500, 100), "car electricity on: " + hasPower);
                GUI.Label(new Rect(20, 700, 500, 100), "current speed: " + satsumaDriveTrain.differentialSpeed);
                GUI.Label(new Rect(20, 720, 500, 100), "------------------------------------");
            }
        }

        public override void Update()
        {
            fuel_system.Handle();
            info_panel.Handle();
        }

        public bool engineRunning
        {
            get
            {
                return satsumaDriveTrain.rpm > 0;
            }
        }

        public bool hasPower
        {
            get
            {
                if (electricsOK == null)
                {
                    GameObject carElectrics = GameObject.Find("SATSUMA(557kg, 248)/Electricity");
                    PlayMakerFSM carElectricsPower = PlayMakerFSM.FindFsmOnGameObject(carElectrics, "Power");
                    electricsOK = carElectricsPower.FsmVariables.FindFsmBool("ElectricsOK");
                }
                
                return electricsOK.Value;
            }
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

                fuel_injectors_box.logic.CheckBoxPosReset(partBuySave.bought_fuel_injectors_box);
                throttle_bodies_box.logic.CheckBoxPosReset(partBuySave.bought_throttle_bodies_box);
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
                    satsumaDriveTrain.gearRatios = newGearRatio;
                    return;
                }
            }
            satsumaDriveTrain.gearRatios = originalGearRatios;
            return;
        }
        private void ToggleSmoothInput()
        {
            axisCarController.smoothInput = !axisCarController.smoothInput;
        }
        private void ToggleAWD()
        {
            if (toggleAWD.Value is bool value)
            {
                if (value)
                {
                    satsumaDriveTrain.SetTransmission(Drivetrain.Transmissions.AWD);
                    return;
                }
            }
            satsumaDriveTrain.SetTransmission(Drivetrain.Transmissions.FWD);
        }

        private void SwitchCruiseControlDebug()
        {
            cruiseControlDebugEnabled = !cruiseControlDebugEnabled;
        }
    }
}
