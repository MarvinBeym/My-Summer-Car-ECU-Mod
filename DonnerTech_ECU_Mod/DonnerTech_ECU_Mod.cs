using HutongGames.PlayMaker;
using ModApi.Attachable;
using ModApi;
using MSCLoader;
using System;
using System.IO;
using UnityEngine;
using ScrewablePartAPI;
using System.Collections.Generic;
using ModsShop;
using DonnerTech_ECU_Mod.old_file_checker;
using DonnerTech_ECU_Mod.shop;
using DonnerTech_ECU_Mod.fuelsystem;

namespace DonnerTech_ECU_Mod
{
    public class DonnerTech_ECU_Mod : Mod
    {

        /*  ToDo:
         *  finish fuel injection logic
         *  Find a way to prevent race carb and fuel pump installing themself after game load
         *  -> currently they get uninstalled but then the lie in the engine bay
         *  -> maybe teleport them away? -> by getting the location, then uninstalling, then teleporting away
         *  -> or saving the location in a file and restoring it from there
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

        /*  Changelog (v1.4)
         *  Code Optimization
         *  Added missing override for turbo page
         *  Added missing screw reset for info panel
         *  Added model for reverse camera
         *  Added model for rain & light sensorboard
         *  Added logic for rain sensor
         *  Added logic for reverse camera
         *  Added reverse light to camera
         *  Changed resolution of reverse camera to 720p
         *  Added settings change prevention for 2step rev limiter rpm value (you can now only change the value if you are on the page for it
         *  Fixed RPM needle OVERRIDE replacing lowBeam image.
         *  Fixed carb not beeing adjusted when using racing carb.
         *  Added product images for reverse camera and rain&light sensorboard
         *  Full code reworked
         *  Added logic for light sensor
         *  Added shift indicator
         *  Added ability to set shift indicator green line and red line (it's a gradient. so ex. 6500 won't mean it will instantly switch to green at 6500)
         *  
         *  
         *  Changed image override file names:
             *  OVERRIDE_ECU-Mod-Panel-Page0.png				=>          *  OVERRIDE_main_page.png
             *  OVERRIDE_ECU-Mod-Panel_Modules-Page1.png		=>          *  OVERRIDE_modules_page.png
             *  OVERRIDE_ECU-Mod-Panel_Faults-Page2.png			=>          *  OVERRIDE_faults_page.png
             *  OVERRIDE_ECU-Mod-Panel_Faults-Page3.png			=>          *  OVERRIDE_faults2_page.png
             *  OVERRIDE_ECU-Mod-Panel_Tuner-Page4.png			=>          *  OVERRIDE_tuner_page.png
             *  OVERRIDE_ECU-Mod-Panel-Turbocharger-Page5.png	=>          *  OVERRIDE_turbocharger_page.png
             *  OVERRIDE_ECU-Mod-Panel-Assistance-Page6.png		=>          *  OVERRIDE_assistance_page.png
             *  OVERRIDE_ECU-Mod-Panel-Airride-Page7.png		=>          *  OVERRIDE_airride_page.png
             *  OVERRIDE_Handbrake-Icon.png						=>          *  OVERRIDE_handbrake_icon.png
             *  OVERRIDE_Indicator-Left-Icon.png				=>          *  OVERRIDE_blinker_left_icon.png
             *  OVERRIDE_Indicator-Right-Icon.png				=>          *  OVERRIDE_blinker_right_icon.png
             *  OVERRIDE_LowBeam-Icon.png						=>          *  OVERRIDE_low_beam_icon.png
             *  OVERRIDE_HighBeam-Icon.png						=>          *  OVERRIDE_high_beam_icon.png
             *  OVERRIDE_Rpm-Needle.png							=>          *  OVERRIDE_needle_icon.png
             *  OVERRIDE_TurbineWheel.png						=>          *  OVERRIDE_turbine_icon.png
         *  
         *  Changed names of save files.
             *  ecu_mod_ABSModule_partSave.txt			=> abs_module_saveFile.txt
             *  ecu_mod_ESPModule_partSave.txt			=> esp_module_saveFile.txt
             *  ecu_mod_TCSModule_partSave.txt			=> tcs_module_saveFile.txt
             *  ecu_mod_CableHarness_partSave.txt		=> cable_harness_saveFile.txt
             *  ecu_mod_MountingPlate_partSave.txt		=> mounting_plate_saveFile.txt
             *  ecu_mod_ModShop_SaveFile.txt			=> mod_shop_saveFile.txt
             *  ecu_mod_SmartEngineModule_partSave.txt	=> smart_engine_module_saveFile.txt
             *  ecu_mod_CruiseControlPanel_partSave.txt	=> cruise_control_panel_saveFile.txt
             *  ecu_InfoPanel_partSave.txt				=> info_panel_saveFile.txt
             *  ecu_reverseCamera_saveFile.txt			=> reverse_camera_saveFile.txt
             *  ecu_rainLightSensorboard_saveFile.txt	=> rain_light_sensor_board_saveFile.txt
             *  ecu_mod_screwable_save.txt              => screwable_saveFile.txt
         *
         *  Added auto renamer for old file names. The auto renamer will automaticly rename all files to their new name (only if a file with the new name doesnt exist)
         *  A Message box is also displayed when files have been renamed showing each file that got renamed ex.: old name.xyz => new name.xyz
         *  Made some values on ecu panel only update after x seconds (clock for example is updated each (realtime) second).
         *  Fixed Clock on info panel not working "jumping around"
         */
        /* BUGS/Need to fix
         * Optimize code both turbo and ecu (only update when needed)
         * Optimize code both turbo and ecu (only have one method instead of two for small and big turbo)
         * improve fps when using info panel -> don't update each frame
         * EDU mod: adjust triggers to be at the same location as the part itself and smaller trigger area
         * ECU mod: add ERRor to display if something is wrong
         */

        public override string ID => "DonnerTech_ECU_Mod"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing ECUs"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.4"; //Version
        public override bool UseAssetsFolder => true;

        SaveFileRenamer saveFileRenamer;
        OverrideFileRenamer overrideFileRenamer;
        public AssetBundle assetBundle;
        public Logger logger;

        public float GetStep2RevRpm()
        {
            return info_panel_logic.GetStep2RevRpm();
        }

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
        private static PartBuySave partBuySave;

        public Keybind info_panel_arrowUp = new Keybind("info_panel_arrowUp", "Arrow Up", KeyCode.Keypad8);
        public Keybind info_panel_arrowDown = new Keybind("info_panel_arrowDown", "Arrow Down", KeyCode.Keypad2);
        public Keybind info_panel_circle = new Keybind("info_panel_circle", "Circle", KeyCode.KeypadEnter);
        public Keybind info_panel_cross = new Keybind("info_panel_cross", "Cross", KeyCode.KeypadPeriod);
        public Keybind info_panel_plus = new Keybind("info_panel_plus", "Plus", KeyCode.KeypadPlus);
        public Keybind info_panel_minus = new Keybind("info_panel_minus", "Minus", KeyCode.KeypadMinus);

        public Keybind programmer_ui_open = new Keybind("programmer_ui_open", "Open/Close", KeyCode.Keypad0);

        //FuelSystem
        public FuelSystem fuel_system;
        private GameObject carb_trigger;
        private FsmBool carb_installed;

        private GameObject twinCarb_trigger;
        private FsmBool twinCarb_installed;

        private GameObject racingCarb_trigger;
        private FsmBool racingCarb_installed;
        private FsmBool racingCarb_bolted;

        private GameObject pump_trigger;
        private FsmBool pump_installed;


        private FsmString playerCurrentVehicle;

        //Part logic
        public InfoPanel_Logic info_panel_logic { get; set; }
        public CruiseControl_Logic cruise_control_logic { get; set; }
        public SmartEngineModule_Logic smart_engine_module_logic { get; set; }
        public BoxLogic fuel_injectors_box_logic {get; set;}
        public BoxLogic throttle_bodies_box_logic { get; set; }

        public ReverseCamera_Logic reverse_camera_logic { get; set; }

        private Settings toggleSixGears = new Settings("toggleSixGears", "Enable/Disable SixGears Mod", false);
        private Settings toggleAWD = new Settings("toggleAWD", "Toggle All Wheel Drive", false);


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
        public SimplePart info_panel_part { get; set; }
        public SimplePart reverse_camera_part { get; set; }
        public SimplePart rain_light_sensor_board_part { get; set; }

        public SimplePart fuel_injector1_part { get; set; }
        public SimplePart fuel_injector2_part { get; set; }
        public SimplePart fuel_injector3_part { get; set; }
        public SimplePart fuel_injector4_part { get; set; }

        public SimplePart throttle_body1_part { get; set; }
        public SimplePart throttle_body2_part { get; set; }
        public SimplePart throttle_body3_part { get; set; }
        public SimplePart throttle_body4_part { get; set; }


        public SimplePart fuel_pump_cover_part { get; set; }
        public SimplePart fuel_injection_manifold_part { get; set; }
        public SimplePart fuel_rail_part { get; set; }
        public static GameObject fuel_injectors_box { get; set; }
        public static GameObject throttle_bodies_box { get; set; }

        public GameObject chip { get; set; }
        public SimplePart chip_programmer_part { get; set; }


        public static List<SimplePart> partsList;

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

        public Vector3 fuel_pump_cover_installLocation = new Vector3(-0.0515f, 0.105f, 0.006f);
        public Vector3 fuel_injection_manifold_installLocation = new Vector3(-0.009f, -0.0775f, 0.02f);
        public Vector3 fuel_rail_installLocation = new Vector3(0, 0.03f, 0.012f);
       


        private static GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;
        private FsmBool electricsOK;


        private const string logger_saveFile = "logger_saveFile.txt";
        private const string abs_module_saveFile = "abs_module_saveFile.txt";
        private const string esp_module_saveFile = "esp_module_saveFile.txt";
        private const string tcs_module_saveFile = "tcs_module_saveFile.txt";
        private const string cable_harness_saveFile = "cable_harness_saveFile.txt";
        private const string mounting_plate_saveFile = "mounting_plate_saveFile.txt";
        private const string mod_shop_saveFile = "mod_shop_saveFile.txt";
        private const string smart_engine_module_saveFile = "smart_engine_module_saveFile.txt";
        private const string cruise_control_panel_saveFile = "cruise_control_panel_saveFile.txt";
        private const string info_panel_saveFile = "info_panel_saveFile.txt";

        private const string reverse_camera_saveFile = "reverse_camera_saveFile.txt";
        private const string rain_light_sensor_board_saveFile = "rain_light_sensor_board_saveFile.txt";

        private const string fuel_injector1_saveFile = "fuel_injector1_saveFile.txt";
        private const string fuel_injector2_saveFile = "fuel_injector2_saveFile.txt";
        private const string fuel_injector3_saveFile = "fuel_injector3_saveFile.txt";
        private const string fuel_injector4_saveFile = "fuel_injector4_saveFile.txt";
        private const string fuel_injectors_box_saveFile = "fuel_injectors_box_saveFile.txt";

        private const string throttle_body1_saveFile = "throttle_body1_saveFile.txt";
        private const string throttle_body2_saveFile = "throttle_body2_saveFile.txt";
        private const string throttle_body3_saveFile = "throttle_body3_saveFile.txt";
        private const string throttle_body4_saveFile = "throttle_body4_saveFile.txt";
        private const string throttle_bodies_box_saveFile = "throttle_bodies_box_saveFile.txt";

        private const string fuel_pump_cover_saveFile = "fuel_pump_cover_saveFile.txt";
        private const string fuel_injection_manifold_saveFile = "fuel_injection_manifold_saveFile.txt";
        private const string fuel_rail_saveFile = "fuel_rail_saveFile.txt";
        private const string chip_programmer_saveFile = "chip_programmer_saveFile.txt";


        private const string screwable_saveFile = "screwable_saveFile.txt";

        private Settings resetPosSetting = new Settings("resetPos", "Reset uninstalled parts location", WorkAroundAction);
        private Settings debugCruiseControlSetting = new Settings("debugCruiseControl", "Enable/Disable", WorkAroundAction);

        private static void WorkAroundAction()
        {

        }

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

        //ECU InfoPanel
        private bool info_panel_workaroundChildDisableDone = false;

        private static AudioSource dashButtonAudioSource
        {
            get
            {
                return GameObject.Find("dash_button").GetComponent<AudioSource>();
            }
        }

        public bool useButtonDown
        {
            get { return cInput.GetKeyDown("Use"); }
        }

        public bool throttleButtonDown
        {
            get { return cInput.GetKey("Throttle"); }
        }
        public bool leftMouseDown
        {
            get { return Input.GetMouseButtonDown(0); }
        }
        public bool rightMouseDown
        {
            get { return Input.GetMouseButtonDown(1); }
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
            resetPosSetting.DoAction = PosReset;
            toggleAWD.DoAction = ToggleAWD;
            toggleSixGears.DoAction = ToggleSixGears;
            debugCruiseControlSetting.DoAction = SwitchCruiseControlDebug;

            logger = new Logger(this, logger_saveFile);
            if (!ModLoader.CheckSteam())
            {
                ModUI.ShowMessage("Cunt", "CUNT");
                ModConsole.Print("Cunt detected");
            }
            Keybind.AddHeader(this, "ECU-Panel Keybinds");
            Keybind.Add(this, info_panel_arrowUp);
            Keybind.Add(this, info_panel_arrowDown);
            Keybind.Add(this, info_panel_circle);
            Keybind.Add(this, info_panel_cross);
            Keybind.Add(this, info_panel_plus);
            Keybind.Add(this, info_panel_minus);
            Keybind.Add(this, programmer_ui_open);

            partsList = new List<SimplePart>();

            modAssetsFolder = ModLoader.GetModAssetsFolder(this);
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
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
            GameObject abs_module = (assetBundle.LoadAsset("ECU-Mod_ABS-Module.prefab") as GameObject);
            GameObject esp_module = (assetBundle.LoadAsset("ECU-Mod_ESP-Module.prefab") as GameObject);
            GameObject tcs_module = (assetBundle.LoadAsset("ECU-Mod_TCS-Module.prefab") as GameObject);
            GameObject cable_harness = (assetBundle.LoadAsset("ECU-Mod_Cable-Harness.prefab") as GameObject);
            GameObject mounting_plate = (assetBundle.LoadAsset("ECU-Mod_Mounting-Plate.prefab") as GameObject);
            GameObject smart_engine_module = (assetBundle.LoadAsset("ECU-Mod_SmartEngine-Module.prefab") as GameObject);
            GameObject cruise_control_panel = (assetBundle.LoadAsset("ECU-Mod_CruiseControl-Panel.prefab") as GameObject);
            GameObject info_panel = (assetBundle.LoadAsset("ECU-Mod_InfoPanel.prefab") as GameObject);
            
            GameObject reverse_camera = (assetBundle.LoadAsset("ECU-Mod_Reverse-Camera.prefab") as GameObject);
            GameObject rain_light_sensor_board = (assetBundle.LoadAsset("ECU-Mod_Rain_Light-Sensorboard.prefab") as GameObject);


            GameObject fuel_injector = (assetBundle.LoadAsset("fuel_injector.prefab") as GameObject);
            GameObject fuel_injector1 = GameObject.Instantiate(fuel_injector);
            GameObject fuel_injector2 = GameObject.Instantiate(fuel_injector);
            GameObject fuel_injector3 = GameObject.Instantiate(fuel_injector);
            GameObject fuel_injector4 = GameObject.Instantiate(fuel_injector);
            
            fuel_injectors_box =  GameObject.Instantiate((assetBundle.LoadAsset("fuel_injectors_box.prefab") as GameObject));
            fuel_injectors_box_logic = fuel_injectors_box.AddComponent<BoxLogic>();


            GameObject fuel_pump_cover = (assetBundle.LoadAsset("fuel_pump_cover.prefab") as GameObject);
            GameObject fuel_injection_manifold = (assetBundle.LoadAsset("fuel_injection_manifold.prefab") as GameObject);
            GameObject fuel_rail = (assetBundle.LoadAsset("fuel_rail.prefab") as GameObject);
            GameObject chip_programmer = (assetBundle.LoadAsset("chip_programmer.prefab") as GameObject);


            GameObject throttle_body = (assetBundle.LoadAsset("throttle_body.prefab") as GameObject);
            GameObject throttle_body1 = GameObject.Instantiate(throttle_body);
            GameObject throttle_body2 = GameObject.Instantiate(throttle_body);
            GameObject throttle_body3 = GameObject.Instantiate(throttle_body);
            GameObject throttle_body4 = GameObject.Instantiate(throttle_body);

            throttle_bodies_box = GameObject.Instantiate((assetBundle.LoadAsset("throttle_bodies_box.prefab") as GameObject));
            throttle_bodies_box_logic = throttle_bodies_box.AddComponent<BoxLogic>();

            chip = GameObject.Instantiate((assetBundle.LoadAsset("chip.prefab") as GameObject));
            chip.SetActive(false);
#if DEBUG
            GameObject awd_gearbox = (assetBundle.LoadAsset("AWD-Gearbox.prefab") as GameObject);
            GameObject awd_differential = (assetBundle.LoadAsset("AWD-Differential.prefab") as GameObject);
            GameObject awd_propshaft = (assetBundle.LoadAsset("AWD-Propshaft.prefab") as GameObject);
            GameObject airride_fl = (assetBundle.LoadAsset("Airride_FL.prefab") as GameObject);
            GameObject airride_fr = (assetBundle.LoadAsset("Airride_FL.prefab") as GameObject);
            SetObjectNameTagLayer(awd_gearbox, "AWD Gearbox");
            SetObjectNameTagLayer(awd_differential, "AWD Differential");
            SetObjectNameTagLayer(awd_propshaft, "AWD Propshaft");
            SetObjectNameTagLayer(airride_fl, "Airride FL");
            SetObjectNameTagLayer(airride_fr, "Airride FR");
#endif
            SetObjectNameTagLayer(abs_module, "ABS Module");
            SetObjectNameTagLayer(esp_module, "ESP Module");
            SetObjectNameTagLayer(tcs_module, "TCS Module");
            SetObjectNameTagLayer(cable_harness, "ECU Cable Harness");
            SetObjectNameTagLayer(mounting_plate, "ECU Mounting Plate");
            SetObjectNameTagLayer(smart_engine_module, "Smart Engine ECU");
            SetObjectNameTagLayer(cruise_control_panel, "Cruise Control Panel");
            SetObjectNameTagLayer(info_panel, "DonnerTech Info Panel");
            SetObjectNameTagLayer(reverse_camera, "Reverse Camera");
            SetObjectNameTagLayer(rain_light_sensor_board, "Rain & Light Sensorboard");

            SetObjectNameTagLayer(fuel_injector1, "Fuel Injector 1(Clone)");
            SetObjectNameTagLayer(fuel_injector2, "Fuel Injector 2(Clone)");
            SetObjectNameTagLayer(fuel_injector3, "Fuel Injector 3(Clone)");
            SetObjectNameTagLayer(fuel_injector4, "Fuel Injector 4(Clone)");
            SetObjectNameTagLayer(fuel_injectors_box, "Fuel Injectors(Clone)");

            SetObjectNameTagLayer(throttle_body1, "Throttle Body 1(Clone)");
            SetObjectNameTagLayer(throttle_body2, "Throttle Body 2(Clone)");
            SetObjectNameTagLayer(throttle_body3, "Throttle Body 3(Clone)");
            SetObjectNameTagLayer(throttle_body4, "Throttle Body 4(Clone)");
            SetObjectNameTagLayer(throttle_bodies_box, "Throttle Bodies(Clone)");

            SetObjectNameTagLayer(chip, "Chip");
            SetObjectNameTagLayer(chip_programmer, "Chip Programmer");

            SetObjectNameTagLayer(fuel_pump_cover, "Fuel Pump Cover");
            SetObjectNameTagLayer(fuel_injection_manifold, "Fuel Injection Manifold");
            SetObjectNameTagLayer(fuel_rail, "Fuel Rail");

            try
            {
                partBuySave = SaveLoad.DeserializeSaveFile<PartBuySave>(this, mod_shop_saveFile);
            }
            catch
            {
            }
            if (partBuySave == null)
            {

                partBuySave = new PartBuySave();
            }

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

            smart_engine_module_part = new SimplePart(
                SimplePart.LoadData(this, smart_engine_module_saveFile, partBuySave.boughtSmartEngineModule),
                smart_engine_module,
                satsuma,
                new Trigger("smart_engine_module_Trigger", satsuma, new Vector3(0.2398f, -0.28f, 0.104f), new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.02f, 0.13f), false),
                new Vector3(0.2398f, -0.248f, 0.104f),
                new Quaternion(0, 0, 0, 0)
            );
            smart_engine_module_part.SetDisassembleFunction(new Action(DisassembleSmartEngineModule));
            smart_engine_module_logic = smart_engine_module_part.rigidPart.AddComponent<SmartEngineModule_Logic>();
            
            abs_module_part = new AbsPart(
                SimplePart.LoadData(this, abs_module_saveFile, partBuySave.boughtABSModule),
                abs_module,
                satsuma,
                new Trigger("abs_module_Trigger", satsuma, new Vector3(0.254f, -0.28f, -0.155f), new Quaternion(0, 0, 0, 0), new Vector3(0.14f, 0.02f, 0.125f), false),
                new Vector3(0.254f, -0.248f, -0.155f),
                new Quaternion(0, 0, 0, 0)
            );
            esp_module_part = new EspPart(
                SimplePart.LoadData(this, esp_module_saveFile, partBuySave.boughtESPModule),
                esp_module,
                satsuma,
                 new Trigger("esp_module_Trigger", satsuma, new Vector3(0.288f, -0.28f, -0.0145f), new Quaternion(0, 0, 0, 0), new Vector3(0.21f, 0.02f, 0.125f), false),
                new Vector3(0.288f, -0.248f, -0.0145f),
                new Quaternion(0, 0, 0, 0)
            );
            tcs_module_part = new TcsPart(
                SimplePart.LoadData(this, tcs_module_saveFile, partBuySave.boughtTCSModule),
                tcs_module,
                satsuma,
                new Trigger("tcs_module_Trigger", satsuma, new Vector3(0.342f, -0.28f, 0.115f), new Quaternion(0, 0, 0, 0), new Vector3(0.104f, 0.02f, 0.104f), false),
                new Vector3(0.342f, -0.246f, 0.115f),
                new Quaternion(0, 0, 0, 0)
            );
            cable_harness_part = new SimplePart(
                SimplePart.LoadData(this, cable_harness_saveFile, partBuySave.boughtCableHarness),
                cable_harness,
                satsuma,
                new Trigger("cable_harness_Trigger", satsuma, new Vector3(0.423f, -0.28f, -0.0384f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.02f, 0.3f), false),
                new Vector3(0.388f, -0.245f, -0.007f),
                new Quaternion(0, 0, 0, 0)
            );
            mounting_plate_part = new SimplePart(
                SimplePart.LoadData(this, mounting_plate_saveFile, partBuySave.boughtMountingPlate),
                mounting_plate,
                satsuma,
                new Trigger("mounting_plate_Trigger", satsuma, new Vector3(0.31f, -0.28f, -0.038f), new Quaternion(0, 0, 0, 0), new Vector3(0.32f, 0.02f, 0.45f), false),
                new Vector3(0.3115f, -0.27f, -0.0393f),
                new Quaternion(0, 0, 0, 0)
            );

            cruise_control_panel_part = new SimplePart(
                SimplePart.LoadData(this, cruise_control_panel_saveFile, partBuySave.boughtCruiseControlPanel),
                cruise_control_panel,
                GameObject.Find("dashboard(Clone)"),
                new Trigger("cruise_control_panel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.46f, -0.095f, 0.08f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                new Vector3(0.5f, -0.095f, 0.08f),
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            cruise_control_logic = cruise_control_panel_part.rigidPart.AddComponent<CruiseControl_Logic>();


            info_panel_part = new SimplePart(
                SimplePart.LoadData(this, info_panel_saveFile, partBuySave.boughtInfoPanel),
                info_panel,
                GameObject.Find("dashboard(Clone)"),
                new Trigger("info_panel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.25f, -0.07f, -0.02f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                new Vector3(0.25f, -0.088f, -0.01f),
                new Quaternion { eulerAngles = new Vector3(0, 180, 180) }
            );

            info_panel_logic = info_panel_part.rigidPart.AddComponent<InfoPanel_Logic>();


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
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            fuel_injection_manifold_part.SetDisassembleFunction(new Action(DisassembleFuelInjectionManifold));


            fuel_injector1_part = new SimplePart(
            SimplePart.LoadData(this, fuel_injector1_saveFile, partBuySave.bought_fuel_injectors_box),
            fuel_injector1,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("fuel_injector1", fuel_injection_manifold_part.rigidPart, fuel_injector1_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                fuel_injector1_installLocation,
                new Quaternion { eulerAngles = new Vector3(30, 0, 0) }
            );
            fuel_injector2_part = new SimplePart(
            SimplePart.LoadData(this, fuel_injector2_saveFile, partBuySave.bought_fuel_injectors_box),
            fuel_injector2,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("fuel_injector2", fuel_injection_manifold_part.rigidPart, fuel_injector2_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                fuel_injector2_installLocation,
                new Quaternion { eulerAngles = new Vector3(30, 0, 0) }
            );
            fuel_injector3_part = new SimplePart(
            SimplePart.LoadData(this, fuel_injector3_saveFile, partBuySave.bought_fuel_injectors_box),
            fuel_injector3,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("fuel_injector3", fuel_injection_manifold_part.rigidPart, fuel_injector3_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                fuel_injector3_installLocation,
                new Quaternion { eulerAngles = new Vector3(30, 0, 0) }
            );
            fuel_injector4_part = new SimplePart(
                SimplePart.LoadData(this, fuel_injector4_saveFile, partBuySave.bought_fuel_injectors_box),
                fuel_injector4,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("fuel_injector4", fuel_injection_manifold_part.rigidPart, fuel_injector4_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                fuel_injector4_installLocation,
                new Quaternion { eulerAngles = new Vector3(30, 0, 0) }
            );

            fuel_pump_cover_part = new SimplePart(
                SimplePart.LoadData(this, fuel_pump_cover_saveFile, partBuySave.bought_fuel_pump_cover),
                fuel_pump_cover,
                GameObject.Find("block(Clone)"),
                new Trigger("fuel_pump_cover", GameObject.Find("block(Clone)"), fuel_pump_cover_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                fuel_pump_cover_installLocation,
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );

            fuel_rail_part = new SimplePart(
                SimplePart.LoadData(this, fuel_rail_saveFile, partBuySave.bought_fuel_rail),
                fuel_rail,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("fuel_rail", fuel_injection_manifold_part.rigidPart, fuel_rail_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                fuel_rail_installLocation,
                new Quaternion { eulerAngles = new Vector3(30, 0, 0) }
            );


            throttle_body1_part = new SimplePart(
                SimplePart.LoadData(this, throttle_body1_saveFile, partBuySave.bought_throttle_bodies_box),
                throttle_body1,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("throttle_body1", fuel_injection_manifold_part.rigidPart, throttle_body1_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                throttle_body1_installLocation,
                new Quaternion { eulerAngles = new Vector3(-40, 0, 0) }
            );

            throttle_body2_part = new SimplePart(
                SimplePart.LoadData(this, throttle_body2_saveFile, partBuySave.bought_throttle_bodies_box),
                throttle_body2,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("throttle_body2", fuel_injection_manifold_part.rigidPart, throttle_body2_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                throttle_body2_installLocation,
                new Quaternion { eulerAngles = new Vector3(-40, 0, 0) }
            );

            throttle_body3_part = new SimplePart(
                SimplePart.LoadData(this, throttle_body3_saveFile, partBuySave.bought_throttle_bodies_box),
                throttle_body3,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("throttle_body3", fuel_injection_manifold_part.rigidPart, throttle_body3_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                throttle_body3_installLocation,
                new Quaternion { eulerAngles = new Vector3(-40, 0, 0) }
            );
            throttle_body4_part = new SimplePart(
                SimplePart.LoadData(this, throttle_body4_saveFile, partBuySave.bought_throttle_bodies_box),
                throttle_body4,
                fuel_injection_manifold_part.rigidPart,
                new Trigger("throttle_body4", fuel_injection_manifold_part.rigidPart, throttle_body4_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                throttle_body4_installLocation,
                new Quaternion { eulerAngles = new Vector3(-40, 0, 0) }
            );
            GameObject test = new GameObject();
            chip_programmer_part = new SimplePart(
                SimplePart.LoadData(this, chip_programmer_saveFile, partBuySave.bought_chip_programmer),
                chip_programmer,
                test,
                new Trigger("chip_programmer", test, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                new Vector3(0, 0, 0),
                new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
                );

            throttle_bodies_box_logic.Init(this, new SimplePart[]
            {
                throttle_body1_part,
                throttle_body2_part,
                throttle_body3_part,
                throttle_body4_part,
            }, "Unpack throttle body");
            fuel_injectors_box_logic.Init(this, new SimplePart[]
            {
                fuel_injector1_part,
                fuel_injector2_part,
                fuel_injector3_part,
                fuel_injector4_part,
            }, "Unpack injector");


            partsList.Add(abs_module_part);
            partsList.Add(esp_module_part);
            partsList.Add(tcs_module_part);
            partsList.Add(cable_harness_part);
            partsList.Add(mounting_plate_part);
            partsList.Add(smart_engine_module_part);
            partsList.Add(cruise_control_panel_part);
            partsList.Add(info_panel_part);

            partsList.Add(rain_light_sensor_board_part);
            partsList.Add(reverse_camera_part);

            partsList.Add(fuel_injector1_part);
            partsList.Add(fuel_injector2_part);
            partsList.Add(fuel_injector3_part);
            partsList.Add(fuel_injector4_part);

            partsList.Add(throttle_body1_part);
            partsList.Add(throttle_body2_part);
            partsList.Add(throttle_body3_part);
            partsList.Add(throttle_body4_part);

            partsList.Add(fuel_pump_cover_part);
            partsList.Add(fuel_injection_manifold_part);
            partsList.Add(fuel_rail_part);
            partsList.Add(chip_programmer_part);

            TextMesh[] info_panel_TextMeshes = info_panel_part.activePart.GetComponentsInChildren<TextMesh>();
            foreach (TextMesh textMesh in info_panel_TextMeshes)
            {
                textMesh.gameObject.SetActive(false);
            }


            SpriteRenderer[] info_panel_SpriteRenderer = info_panel_part.activePart.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in info_panel_SpriteRenderer)
            {
                spriteRenderer.enabled = false;
            }
            if(info_panel_SpriteRenderer.Length > 0 && info_panel_TextMeshes.Length > 0)
            {
                info_panel_workaroundChildDisableDone = true;
            }

            ScrewablePart screwable;

            screwable = new ScrewablePart(screwListSave, this, abs_module_part.rigidPart,
                new Vector3[]{
                        new Vector3(0.0558f, -0.0025f, -0.0525f),
                        new Vector3(0.0558f, -0.0025f, 0.0525f),
                        new Vector3(-0.0558f, -0.0025f, 0.0525f),
                        new Vector3(-0.0558f, -0.0025f, -0.0525f),
                },
                new Vector3[]{
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                },
                new Vector3[]{
                        new Vector3(1.2f, 1.2f, 1.2f),
                        new Vector3(1.2f, 1.2f, 1.2f),
                        new Vector3(1.2f, 1.2f, 1.2f),
                        new Vector3(1.2f, 1.2f, 1.2f),
                }, 12, "screwable_screw2");
            abs_module_part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, esp_module_part.rigidPart,
                new Vector3[]{
                        new Vector3(0.0918f, -0.0025f, -0.053f),
                        new Vector3(0.0918f, -0.0025f, 0.0518f),
                        new Vector3(-0.0902f, -0.0025f, 0.0518f),
                        new Vector3(-0.0902f, -0.0025f, -0.053f),
                },
                new Vector3[]{
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                },
                new Vector3[]{
                        new Vector3(1.2f, 1.2f, 1.2f),
                        new Vector3(1.2f, 1.2f, 1.2f),
                        new Vector3(1.2f, 1.2f, 1.2f),
                        new Vector3(1.2f, 1.2f, 1.2f),
                }, 12, "screwable_screw2");
            esp_module_part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, tcs_module_part.rigidPart,
                new Vector3[]{
                        new Vector3(0.0378f, 0f, -0.0418f),
                        new Vector3(0.0378f, 0f, 0.0422f),
                        new Vector3(-0.039f, 0f, 0.0422f),
                        new Vector3(-0.039f, 0f, -0.0418f),
                },
                new Vector3[]{
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                },
                new Vector3[]{
                        new Vector3(1.2f, 1.2f, 1.2f),
                        new Vector3(1.2f, 1.2f, 1.2f),
                        new Vector3(1.2f, 1.2f, 1.2f),
                        new Vector3(1.2f, 1.2f, 1.2f),
                }, 12, "screwable_screw2");
            tcs_module_part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, smart_engine_module_part.rigidPart,
                new Vector3[]{
                        new Vector3(0.0232f, -0.0048f, -0.033f),
                        new Vector3(0.0232f, -0.0048f, 0.055f),
                        new Vector3(-0.044f, -0.0048f, 0.055f),
                        new Vector3(-0.044f, -0.0048f, -0.033f),
                },
                new Vector3[]{
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                },
                new Vector3[]{
                        new Vector3(1.1f, 1.1f, 1.1f),
                        new Vector3(1.1f, 1.1f, 1.1f),
                        new Vector3(1.1f, 1.1f, 1.1f),
                        new Vector3(1.1f, 1.1f, 1.1f),
                }, 11, "screwable_screw2");
            smart_engine_module_part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, mounting_plate_part.rigidPart,
                new Vector3[]{
                        new Vector3(-0.128f, 0.002f, -0.2068f),
                        new Vector3(-0.0018f, 0.002f, -0.2068f),
                        new Vector3(0.1245f, 0.002f, -0.2068f),
                        new Vector3(0.1245f, 0.002f, -0.0035f),
                        new Vector3(0.124f, 0.002f, 0.2f),
                },
                new Vector3[]{
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                        new Vector3(-90, 0, 0),
                },
                new Vector3[]{
                        new Vector3(1.5f, 1.5f, 1.5f),
                        new Vector3(1.5f, 1.5f, 1.5f),
                        new Vector3(1.5f, 1.5f, 1.5f),
                        new Vector3(1.5f, 1.5f, 1.5f),
                        new Vector3(1.5f, 1.5f, 1.5f),
                }, 15, "screwable_screw2");
            mounting_plate_part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, rain_light_sensor_board_part.rigidPart,
                new Vector3[]{
                    new Vector3(0.078f, 0.0055f, 0f),
                    new Vector3(-0.078f, 0.0055f, 0f),
                },
                new Vector3[]{
                    new Vector3(-90, 0, 0),
                    new Vector3(-90, 0, 0),
                },
                new Vector3[]{
                    new Vector3(0.5f, 0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f, 0.5f),
                }, 8, "screwable_screw2");
            rain_light_sensor_board_part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, reverse_camera_part.rigidPart,
                new Vector3[]{
                    new Vector3(0f, -0.015f, 0.0055f),
                },
                new Vector3[]{
                    new Vector3(0, 0, 0),
                },
                new Vector3[]{
                    new Vector3(0.5f, 0.5f, 0.5f),
                }, 5, "screwable_screw2");
            reverse_camera_part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, info_panel_part.rigidPart,
                new Vector3[]{
                    new Vector3(0f, -0.025f, -0.067f),
                },
                new Vector3[]{
                    new Vector3(180, 0, 0),
                },
                new Vector3[]{
                    new Vector3(0.8f, 0.8f, 0.8f),
                }, 8, "screwable_screw2");
            info_panel_part.SetScrewablePart(screwable);

            if (GameObject.Find("Shop for mods") != null)
            {
                ModsShop.ShopItem modsShopItem = GameObject.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();

                List<ProductInformation> shopItemss = new List<ProductInformation>();
           
                shopItemss.Add(new ProductInformation(abs_module_part.activePart, "ABS Module", 800, assetBundle.LoadAsset<Sprite>("ecu_abs_module_productImage.png"), abs_module_part.bought));
                shopItemss.Add(new ProductInformation(esp_module_part.activePart, "ESP Module", 1200, assetBundle.LoadAsset<Sprite>("ecu_esp_module_productImage.png"), esp_module_part.bought));
                shopItemss.Add(new ProductInformation(tcs_module_part.activePart, "TCS Module", 1800, assetBundle.LoadAsset<Sprite>("ecu_tcs_module_productImage.png"), tcs_module_part.bought));
                shopItemss.Add(new ProductInformation(cable_harness_part.activePart, "ECU Cable Harness", 300, assetBundle.LoadAsset<Sprite>("ecu_cable_harness_productImage.png"), cable_harness_part.bought));
                shopItemss.Add(new ProductInformation(mounting_plate_part.activePart, "ECU Mounting Plate", 100, assetBundle.LoadAsset<Sprite>("ecu_mounting_plate_productImage.png"), mounting_plate_part.bought));
                shopItemss.Add(new ProductInformation(smart_engine_module_part.activePart, "Smart Engine Module ECU", 4600, assetBundle.LoadAsset<Sprite>("ecu_smart_engine_module_productImage.png"), smart_engine_module_part.bought));
                shopItemss.Add(new ProductInformation(cruise_control_panel_part.activePart, "Cruise Control Panel with Controller", 2000, assetBundle.LoadAsset<Sprite>("ecu_cruise_control_productImage.png"), cruise_control_panel_part.bought));
                shopItemss.Add(new ProductInformation(info_panel_part.activePart, "ECU Info Panel", 4000, assetBundle.LoadAsset<Sprite>("ecu_info_panel_productImage.png"), info_panel_part.bought));
                shopItemss.Add(new ProductInformation(rain_light_sensor_board_part.activePart, "Rain & Light Sensorboard", 1000, assetBundle.LoadAsset<Sprite>("ecu_rainLightSensorboard_productImage.png"), rain_light_sensor_board_part.bought));
                shopItemss.Add(new ProductInformation(reverse_camera_part.activePart, "Reverse Camera", 1500, assetBundle.LoadAsset<Sprite>("ecu_reverseCamera_productImage.png"), reverse_camera_part.bought));
#if DEBUG
                shopItemss.Add(new ProductInformation(airride_fl_part.activePart, "Airride fl", 0, null, airride_fl_part.bought));
                shopItemss.Add(new ProductInformation(awd_gearbox_part.activePart, "AWD Gearbox", 0, null, awd_gearbox_part.bought));
                shopItemss.Add(new ProductInformation(awd_differential_part.activePart, "AWD Differential", 0, null, awd_differential_part.bought));
                shopItemss.Add(new ProductInformation(awd_propshaft_part.activePart, "AWD Propshaft", 0, null, awd_propshaft_part.bought));
#endif

                shopItemss.Add(new ProductInformation(fuel_injectors_box, "Fuel Injectors", 1, null, partBuySave.bought_fuel_injectors_box));
                shopItemss.Add(new ProductInformation(throttle_bodies_box, "Throttle Bodies", 1, null, partBuySave.bought_throttle_bodies_box));

                shopItemss.Add(new ProductInformation(fuel_pump_cover_part.activePart, "Fuel Pump Cover", 5, null, partBuySave.bought_fuel_pump_cover));
                shopItemss.Add(new ProductInformation(fuel_injection_manifold_part.activePart, "Fuel Injection Manifold", 6, null, partBuySave.bought_fuel_injection_manifold));
                shopItemss.Add(new ProductInformation(fuel_rail_part.activePart, "Fuel Rail", 7, null, partBuySave.bought_fuel_rail));
                shopItemss.Add(new ProductInformation(chip_programmer_part.activePart, "Chip Programmer", 7, null, partBuySave.bought_chip_programmer));

                //Add a way of buying the throttle bodies

                if (!partBuySave.bought_fuel_injectors_box)
                {
                    fuel_injector1_part.removePart();
                    fuel_injector2_part.removePart();
                    fuel_injector3_part.removePart();
                    fuel_injector4_part.removePart();

                    fuel_injector1_part.activePart.SetActive(false);
                    fuel_injector2_part.activePart.SetActive(false);
                    fuel_injector3_part.activePart.SetActive(false);
                    fuel_injector4_part.activePart.SetActive(false);
                }
                if (!partBuySave.bought_throttle_bodies_box)
                {
                    throttle_body1_part.removePart();
                    throttle_body2_part.removePart();
                    throttle_body3_part.removePart();
                    throttle_body4_part.removePart();

                    throttle_body1_part.activePart.SetActive(false);
                    throttle_body2_part.activePart.SetActive(false);
                    throttle_body3_part.activePart.SetActive(false);
                    throttle_body4_part.activePart.SetActive(false);
                }


                //Shop shop = new Shop(this, modsShopItem, assetBundle, partBuySave, shopItems);
                Shop shop = new Shop(this, modsShopItem, assetBundle, partBuySave, shopItemss);
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
            UnityEngine.Object.Destroy(abs_module);
            UnityEngine.Object.Destroy(esp_module);
            UnityEngine.Object.Destroy(tcs_module);
            UnityEngine.Object.Destroy(cable_harness);
            UnityEngine.Object.Destroy(mounting_plate);
            UnityEngine.Object.Destroy(smart_engine_module);
            UnityEngine.Object.Destroy(cruise_control_panel);
            UnityEngine.Object.Destroy(info_panel);
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

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");

            
            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " finished loading");
        }

        public void SetReverseCameraEnabled(bool enabled)
        {
            reverse_camera_logic.SetReverseCameraEnabled(enabled);


        }

        public void DisassembleFuelInjectionManifold()
        {
            fuel_injector1_part.removePart();
            fuel_injector2_part.removePart();
            fuel_injector3_part.removePart();
            fuel_injector4_part.removePart();

            fuel_rail_part.removePart();

            throttle_body1_part.removePart();
            throttle_body2_part.removePart();
            throttle_body3_part.removePart();
            throttle_body4_part.removePart();

            racingCarb_installed.Value = false;
            racingCarb_bolted.Value = false;
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
            Settings.AddButton(this, debugCruiseControlSetting, "DEBUG Cruise Control");
            Settings.AddCheckBox(this, toggleSixGears);
            Settings.AddCheckBox(this, toggleAWD);
            Settings.AddButton(this, resetPosSetting, "reset part location");
            Settings.AddHeader(this, "", Color.clear);
            
            Settings.AddText(this, "New Gear ratios + 5th & 6th gear\n" +
                "1.Gear: " + newGearRatio[2] + "\n" +
                "2.Gear: " + newGearRatio[3] + "\n" +
                "3.Gear: " + newGearRatio[4] + "\n" +
                "4.Gear: " + newGearRatio[5] + "\n" +
                "5.Gear: " + newGearRatio[6] + "\n" +
                "6.Gear: " + newGearRatio[7]
                );

        }
        public override void OnSave()
        {
            try
            {
                partsList.ForEach(delegate (SimplePart part)
                {
                    SaveLoad.SerializeSaveFile<PartSaveInfo>(this, part.getSaveInfo(), part.saveFile);
                });
                fuel_system.chip_parts.ForEach(delegate (ChipPart chip)
                {

                    SaveLoad.SerializeSaveFile<ChipSave>(this, chip.chipSave, chip.fuelMap_saveFile);
                    SaveLoad.SerializeSaveFile<PartSaveInfo>(this, chip.getSaveInfo(), chip.saveFile);
                });
                SaveLoad.SerializeSaveFile<PartBuySave>(this, partBuySave, mod_shop_saveFile);
            }
            catch(Exception ex)
            {
                //Logger
            }

            try
            {
                ScrewablePart.SaveScrews(this, new ScrewablePart[]
{
                    abs_module_part.GetScrewablePart(),
                    esp_module_part.GetScrewablePart(),
                    tcs_module_part.GetScrewablePart(),
                    smart_engine_module_part.GetScrewablePart(),
                    mounting_plate_part.GetScrewablePart(),
                    info_panel_part.GetScrewablePart(),
                    rain_light_sensor_board_part.GetScrewablePart(),
                    reverse_camera_part.GetScrewablePart(),
                }, screwable_saveFile);
            }
            catch
            {
                //Logger
            }

            try
            {

               // fuel_system.SaveChips();

            }
            catch (Exception ex)
            {
               //Logger 
            }
        }

        
        public override void OnGUI()
        {
            saveFileRenamer.GuiHandler();

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
            //InfoPanel scale workaround
            if (!info_panel_part.installed)
            {
                if (info_panel_part.activePart.transform.localScale.x < 1.5f)
                {
                    info_panel_part.activePart.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }
                if (!info_panel_workaroundChildDisableDone)
                {
                    TextMesh[] info_panel_TextMeshes = info_panel_part.activePart.GetComponentsInChildren<TextMesh>();
                    foreach (TextMesh textMesh in info_panel_TextMeshes)
                    {
                        textMesh.gameObject.SetActive(false);
                    }


                    SpriteRenderer[] info_panel_SpriteRenderer = info_panel_part.activePart.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer spriteRenderer in info_panel_SpriteRenderer)
                    {
                        spriteRenderer.enabled = false;
                    }
                    if (info_panel_SpriteRenderer.Length > 0 && info_panel_TextMeshes.Length > 0)
                    {
                        info_panel_workaroundChildDisableDone = true;
                    }
                }
            }
        }

        public GameObject SetObjectNameTagLayer(GameObject gameObject, string name)
        {
            gameObject.name = name;
            gameObject.tag = "PART";

            gameObject.layer = LayerMask.NameToLayer("Parts");
            return gameObject;
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
                if (fuel_injectors_box.activeSelf)
                {
                    fuel_injectors_box.transform.position = ModsShop.FleetariSpawnLocation.desk;
                }
                if (throttle_bodies_box.activeSelf)
                {
                    throttle_bodies_box.transform.position = ModsShop.FleetariSpawnLocation.desk;
                }
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
