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

namespace DonnerTech_ECU_Mod
{
    public class DonnerTech_ECU_Mod : Mod
    {

        /*  ToDo:
         *  Add auto renaming of old file names to new file names (incase someone old installs mod
         *  Add logic that uses Page classes
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
         *
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

        private static bool cruiseControlDebugEnabled = false;
        SaveFileRenamer saveFileRenamer;
        OverrideFileRenamer overrideFileRenamer;
        AssetBundle assetBundle;

        public float GetStep2RevRpm()
        {
            return info_panel_logic.GetStep2RevRpm();
        }

        public static ScrewablePart abs_module_screwable;
        public static ScrewablePart esp_module_Part_screwable;
        public static ScrewablePart tcs_module_Part_screwable;
        public static ScrewablePart smart_engineModule_Part_screwable;
        public static ScrewablePart mounting_plate_Part_screwable;
        public static ScrewablePart info_panel_screwable;
        public static ScrewablePart reverse_camera_screwable;
        public static ScrewablePart rain_light_sensor_board_screwable;

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
        private TextMesh cruiseControlText;
        private static PartBuySave partBuySave;

        public Keybind info_panel_arrowUp = new Keybind("info_panel_arrowUp", "Arrow Up", KeyCode.Keypad8);
        public Keybind info_panel_arrowDown = new Keybind("info_panel_arrowDown", "Arrow Down", KeyCode.Keypad2);
        public Keybind info_panel_circle = new Keybind("info_panel_circle", "Circle", KeyCode.KeypadEnter);
        public Keybind info_panel_cross = new Keybind("info_panel_cross", "Cross", KeyCode.KeypadPeriod);
        public Keybind info_panel_plus = new Keybind("info_panel_plus", "Plus", KeyCode.KeypadPlus);
        public Keybind info_panel_minus = new Keybind("info_panel_minus", "Minus", KeyCode.KeypadMinus);




        private FsmString playerCurrentVehicle;

        //Part logic
        private InfoPanel_Logic info_panel_logic;
        private CruiseControl_Logic cruise_control_logic;
        private SmartEngineModule_Logic smart_engine_module_logic;
        private ReverseCamera_Logic reverse_camera_logic;

        private static Settings toggleSixGears = new Settings("toggleSixGears", "Enable/Disable SixGears Mod", false, new Action(ToggleSixGears));
        private static Settings toggleAWD = new Settings("toggleAWD", "Toggle All Wheel Drive", false, new Action(ToggleAWD));
        


        public static bool abs_module_enabled = false;
        public static bool esp_module_enabled = false;
        public static bool tcs_module_enabled = false;

        private static string modAssetsFolder;
        private RaycastHit hit;

        private static AbsPart abs_module_part;
        private static SimplePart esp_module_part;
        private static TcsPart tcs_module_part;
        private static SimplePart cable_harness_part;
        private static SimplePart mounting_plate_part;
        private static SimplePart smart_engine_module_part;
        private static SimplePart cruise_control_panel_part;
        private static SimplePart info_panel_part;
        private static SimplePart reverse_camera_part;
        private static SimplePart rain_light_sensor_board_part;

        private static List<Part> partsList;

        public Vector3 reverse_camera_installLocation = new Vector3(0, -0.343f, -0.157f);
        public Vector3 rain_light_sensor_board_installLocation = new Vector3(-0.0015f, 0.086f, 0.1235f);

        private static GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;
        private FsmBool electricsOK;

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

        private const string screwable_saveFile = "screwable_saveFile.txt";

        private Settings resetPosSetting = new Settings("resetPos", "Reset uninstalled parts location", new Action(PosReset));
        private Settings debugCruiseControlSetting = new Settings("debugCruiseControl", "Enable/Disable", SwitchCruiseControlDebug);

        private int setCruiseControlSpeed = 0;
        private bool cruiseControlModuleEnabled = false;


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

        internal static bool useButtonDown
        {
            get
            {
                return cInput.GetKeyDown("Use");
            }
        }

        internal static bool useThrottleButton
        {
            get
            {
                return cInput.GetKey("Throttle");
            }
        }

        private PartSaveInfo loadSaveData(string saveFile, bool installed)
        {
            if (!installed)
            {
                return null;
            }
            try
            {
                return SaveLoad.DeserializeSaveFile<PartSaveInfo>(this, saveFile);
            }
            catch (System.NullReferenceException)
            {
                return null;
            }
        }


        public override void OnNewGame()
        {
            // Called once, when starting a New Game, you can reset your saves here
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, abs_module_saveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, esp_module_saveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, tcs_module_saveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, cable_harness_saveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, mounting_plate_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, mod_shop_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, smart_engine_module_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, cruise_control_panel_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, info_panel_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, reverse_camera_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, rain_light_sensor_board_saveFile);

#if DEBUG
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, airride_fl_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, airride_fr_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, awd_gearbox_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, awd_differential_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, awd_propshaft_saveFile);
#endif
        }
        public override void OnLoad()
        {
            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " started loading");
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

            partsList = new List<Part>();

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

            PartSaveInfo airride_fl_saveInfo = this.loadSaveData(airride_fl_saveFile, false);
            PartSaveInfo awd_gearbox_saveInfo = this.loadSaveData(awd_gearbox_saveFile, false);
            PartSaveInfo awd_differential_saveInfo = this.loadSaveData(awd_differential_saveFile, false);
            PartSaveInfo awd_propshaft_saveInfo = this.loadSaveData(awd_propshaft_saveFile, false);
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

            PartSaveInfo abs_module_saveInfo = this.loadSaveData(abs_module_saveFile, partBuySave.boughtABSModule);
            PartSaveInfo esp_module_saveInfo = this.loadSaveData(esp_module_saveFile, partBuySave.boughtESPModule);
            PartSaveInfo tcs_module_saveInfo = this.loadSaveData(tcs_module_saveFile, partBuySave.boughtTCSModule);
            PartSaveInfo cable_harness_saveInfo = this.loadSaveData(cable_harness_saveFile, partBuySave.boughtCableHarness);
            PartSaveInfo mounting_plate_saveInfo = this.loadSaveData(mounting_plate_saveFile, partBuySave.boughtMountingPlate);
            PartSaveInfo smart_engine_module_saveInfo = this.loadSaveData(smart_engine_module_saveFile, partBuySave.boughtSmartEngineModule);
            PartSaveInfo cruise_control_panel_saveInfo = this.loadSaveData(cruise_control_panel_saveFile, partBuySave.boughtCruiseControlPanel);
            PartSaveInfo info_panel_saveInfo = this.loadSaveData(info_panel_saveFile, partBuySave.boughtInfoPanel);
            PartSaveInfo reverse_camera_saveInfo = this.loadSaveData(reverse_camera_saveFile, partBuySave.bought_reverseCamera);
            PartSaveInfo rain_light_sensor_board_saveInfo = this.loadSaveData(rain_light_sensor_board_saveFile, partBuySave.bought_rainLightSensorboard);
            SortedList<String, Screws> screwListSave = ScrewablePart.LoadScrews(this, screwable_saveFile);

#if DEBUG
            airride_fl_part = new SimplePart(
                    airride_fl_saveInfo,
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
                    awd_gearbox_saveInfo,
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
                    awd_differential_saveInfo,
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
                    awd_propshaft_saveInfo,
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
#endif

            abs_module_part = new AbsPart(
                abs_module_saveInfo,
                abs_module,
                satsuma,
                new Trigger("abs_module_Trigger", satsuma, new Vector3(0.254f, -0.28f, -0.155f), new Quaternion(0, 0, 0, 0), new Vector3(0.14f, 0.02f, 0.125f), false),
                new Vector3(0.254f, -0.248f, -0.155f),
                new Quaternion(0, 0, 0, 0)
            );
            esp_module_part = new SimplePart(
                esp_module_saveInfo,
                esp_module,
                satsuma,
                 new Trigger("esp_module_Trigger", satsuma, new Vector3(0.288f, -0.28f, -0.0145f), new Quaternion(0, 0, 0, 0), new Vector3(0.21f, 0.02f, 0.125f), false),
                new Vector3(0.288f, -0.248f, -0.0145f),
                new Quaternion(0, 0, 0, 0)
            );
            tcs_module_part = new TcsPart(
                tcs_module_saveInfo,
                tcs_module,
                satsuma,
                new Trigger("tcs_module_Trigger", satsuma, new Vector3(0.342f, -0.28f, 0.115f), new Quaternion(0, 0, 0, 0), new Vector3(0.104f, 0.02f, 0.104f), false),
                new Vector3(0.342f, -0.246f, 0.115f),
                new Quaternion(0, 0, 0, 0)
            );
            cable_harness_part = new SimplePart(
                cable_harness_saveInfo,
                cable_harness,
                satsuma,
                new Trigger("cable_harness_Trigger", satsuma, new Vector3(0.423f, -0.28f, -0.0384f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.02f, 0.3f), false),
                new Vector3(0.388f, -0.245f, -0.007f),
                new Quaternion(0, 0, 0, 0)
            );
            mounting_plate_part = new SimplePart(
                mounting_plate_saveInfo,
                mounting_plate,
                satsuma,
                new Trigger("mounting_plate_Trigger", satsuma, new Vector3(0.31f, -0.28f, -0.038f), new Quaternion(0, 0, 0, 0), new Vector3(0.32f, 0.02f, 0.45f), false),
                new Vector3(0.3115f, -0.27f, -0.0393f),
                new Quaternion(0, 0, 0, 0)
            );
            smart_engine_module_part = new SimplePart(
                smart_engine_module_saveInfo,
                smart_engine_module,
                satsuma,
                new Trigger("smart_engine_module_Trigger", satsuma, new Vector3(0.2398f, -0.28f, 0.104f), new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.02f, 0.13f), false),
                new Vector3(0.2398f, -0.248f, 0.104f),
                new Quaternion(0, 0, 0, 0)
            );

            smart_engine_module_logic = smart_engine_module_part.rigidPart.AddComponent<SmartEngineModule_Logic>();

            cruise_control_panel_part = new SimplePart(
                cruise_control_panel_saveInfo,
                cruise_control_panel,
                GameObject.Find("dashboard(Clone)"),
                new Trigger("cruise_control_panel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.46f, -0.095f, 0.08f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                new Vector3(0.5f, -0.095f, 0.08f),
                new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
            );
            cruise_control_logic = cruise_control_panel_part.rigidPart.AddComponent<CruiseControl_Logic>();


            info_panel_part = new SimplePart(
                info_panel_saveInfo,
                info_panel,
                GameObject.Find("dashboard(Clone)"),
                new Trigger("info_panel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.25f, -0.07f, -0.02f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                new Vector3(0.25f, -0.088f, -0.01f),
                new Quaternion { eulerAngles = new Vector3(0, 180, 180) }
            );

            info_panel_logic = info_panel_part.rigidPart.AddComponent<InfoPanel_Logic>();


            rain_light_sensor_board_part = new SimplePart(
                    rain_light_sensor_board_saveInfo,
                    rain_light_sensor_board,
                    GameObject.Find("dashboard(Clone)"),
                    new Trigger("rain_light_sensor_board_trigger", GameObject.Find("dashboard(Clone)"), rain_light_sensor_board_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    rain_light_sensor_board_installLocation,
                    new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
                );

            reverse_camera_part = new SimplePart(
                    reverse_camera_saveInfo,
                    reverse_camera,
                    GameObject.Find("bootlid(Clone)"),
                    new Trigger("reverse_camera_trigger", GameObject.Find("bootlid(Clone)"), reverse_camera_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    reverse_camera_installLocation,
                    new Quaternion { eulerAngles = new Vector3(120, 0, 0) }
                );
            reverse_camera_logic = reverse_camera_part.rigidPart.AddComponent<ReverseCamera_Logic>();

            partsList.Add(abs_module_part);
            partsList.Add(esp_module_part);
            partsList.Add(tcs_module_part);
            partsList.Add(cable_harness_part);
            partsList.Add(mounting_plate_part);
            partsList.Add(smart_engine_module_part);
            partsList.Add(cruise_control_panel_part);
            partsList.Add(info_panel_part);
            partsList.Add(reverse_camera_part);
            partsList.Add(rain_light_sensor_board_part);

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
                
                IDictionary<string, Part> shopItems = new Dictionary<string, Part>();
                shopItems["ABS Module ECU"] = abs_module_part;
                shopItems["ESP Module ECU"] = esp_module_part;
                shopItems["TCS Module ECU"] = tcs_module_part;
                shopItems["ECU Cable Harness"] = cable_harness_part;
                shopItems["ECU Mounting Plate"] = mounting_plate_part;
                shopItems["Smart Engine Module ECU"] = smart_engine_module_part;
                shopItems["Cruise Control Panel with Controller"] = cruise_control_panel_part;
                shopItems["ECU Info Panel"] = info_panel_part;
                shopItems["Rain & Light Sensorboard"] = rain_light_sensor_board_part;
                shopItems["Reverse Camera"] = reverse_camera_part;
#if DEBUG
                shopItems["Airride FL"] = airride_fl_part;
                shopItems["AWD Gearbox"] = awd_gearbox_part;
                shopItems["AWD Differential"] = awd_differential_part;
                shopItems["AWD Propshaft"] = awd_propshaft_part;
#endif
                Shop shop = new Shop(this, modsShopItem, assetBundle, partBuySave, shopItems);


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

            UnityEngine.Object.Destroy(airride_fl);
            UnityEngine.Object.Destroy(awd_gearbox);

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");
            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " finished loading");
        }

        public bool GetReverseCameraInstalledScrewed()
        {
            return (reverse_camera_part.installed && reverse_camera_part.GetScrewablePart().partFixed);
        }

        internal bool GetInfoPanelScrewed()
        {
            return info_panel_part.GetScrewablePart().partFixed;
        }
        public void SetReverseCameraEnabled(bool enabled)
        {
            reverse_camera_logic.SetReverseCameraEnabled(enabled);


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
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, abs_module_part.getSaveInfo(), abs_module_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, esp_module_part.getSaveInfo(), esp_module_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, tcs_module_part.getSaveInfo(), tcs_module_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, cable_harness_part.getSaveInfo(), cable_harness_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, mounting_plate_part.getSaveInfo(), mounting_plate_saveFile);
                SaveLoad.SerializeSaveFile<PartBuySave>(this, partBuySave, mod_shop_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, smart_engine_module_part.getSaveInfo(), smart_engine_module_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, cruise_control_panel_part.getSaveInfo(), cruise_control_panel_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, info_panel_part.getSaveInfo(), info_panel_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, rain_light_sensor_board_part.getSaveInfo(), rain_light_sensor_board_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, reverse_camera_part.getSaveInfo(), reverse_camera_saveFile);
#if DEBUG
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, awd_gearbox_part.getSaveInfo(), awd_gearbox_saveFile);
#endif
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
            catch (System.Exception ex)
            {
                ModConsole.Error("<b>[ECU_MOD]</b> - an error occured while attempting to save see: " + ex.ToString());
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
                GUI.Label(new Rect(20, 520, 500, 100), "mounting plate installed: " + mounting_plate_part.installed);
                GUI.Label(new Rect(20, 540, 500, 100), "mounting plate screwed in: " + mounting_plate_part.GetScrewablePart().partFixed);
                GUI.Label(new Rect(20, 560, 500, 100), "smart engine module installed: " + smart_engine_module_part.installed);
                GUI.Label(new Rect(20, 580, 500, 100), "smart engine module screwed in: " + smart_engine_module_part.GetScrewablePart().partFixed);
                GUI.Label(new Rect(20, 600, 500, 100), "not on throttle: " + (satsumaCarController.throttleInput <= 0f));
                GUI.Label(new Rect(20, 620, 500, 100), "speed above 20km/h: " + (satsumaDriveTrain.differentialSpeed >= 20f));
                GUI.Label(new Rect(20, 640, 500, 100), "brake not pressed: " + (!satsumaCarController.brakeKey));
                GUI.Label(new Rect(20, 660, 500, 100), "clutch not pressed: " + (!cInput.GetKey("Clutch")));
                GUI.Label(new Rect(20, 680, 500, 100), "handbrake not pressed: " + (satsumaCarController.handbrakeInput <= 0f));
                GUI.Label(new Rect(20, 700, 500, 100), "set cruise control speed: " + setCruiseControlSpeed);
                GUI.Label(new Rect(20, 720, 500, 100), "car electricity on: " + hasPower);
                GUI.Label(new Rect(20, 740, 500, 100), "current speed: " + satsumaDriveTrain.differentialSpeed);
                GUI.Label(new Rect(20, 760, 500, 100), "------------------------------------");
            }
        }



        public override void Update()
        {
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

        private GameObject SetObjectNameTagLayer(GameObject gameObject, string name)
        {
            gameObject.name = name;
            gameObject.tag = "PART";

            gameObject.layer = LayerMask.NameToLayer("Parts");
            return gameObject;
        }

        private bool hasPower
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

        public static void ToggleABS()
        {
            if (abs_module_part.installed & abs_module_part.GetScrewablePart().partFixed)
            {
                satsuma.GetComponent<CarController>().ABS = !satsuma.GetComponent<CarController>().ABS;
                abs_module_enabled = satsuma.GetComponent<CarController>().ABS;
            }
            else
            {
                satsuma.GetComponent<CarController>().ABS = false;
                abs_module_enabled = false;
            }
            

        }
        
        public static void ToggleESP()
        {
            if (esp_module_part.installed && esp_module_part.GetScrewablePart().partFixed)
            {
                satsuma.GetComponent<CarController>().ESP = !satsuma.GetComponent<CarController>().ESP;
                esp_module_enabled = satsuma.GetComponent<CarController>().ESP;
            }
            else
            {
                satsuma.GetComponent<CarController>().ESP = false;
                esp_module_enabled = false;
            }
            
        }
        public static void ToggleTCS()
        {
            if (tcs_module_part.installed && tcs_module_part.GetScrewablePart().partFixed)
            {
                satsuma.GetComponent<CarController>().TCS = !satsuma.GetComponent<CarController>().TCS;
                tcs_module_enabled = satsuma.GetComponent<CarController>().TCS;
            }
            else
            {
                satsuma.GetComponent<CarController>().TCS = false;
                tcs_module_enabled = false;
            }
            
        }
        public void ToggleALS()
        {
            smart_engine_module_logic.ToggleALS();
        }
        public void Toggle2StepRevLimiter()
        {
            smart_engine_module_logic.ToggleStep2RevLimiter();
        }

        private static void PosReset()
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
            }
            catch (Exception ex)
            {
                ModConsole.Error("Error while trying to save parts");
            }
        }

        private static void ToggleSixGears()
        {
            if (toggleSixGears.Value is bool value)
            {
                if (value)
                {
                    satsumaDriveTrain.gearRatios = newGearRatio;
                    return;
                }
                satsumaDriveTrain.gearRatios = originalGearRatios;
                return;
            }
        }
        private static void ToggleAWD()
        {
            if (toggleAWD.Value is bool value)
            {
                
                if (value == true)
                {
                    satsumaDriveTrain.SetTransmission(Drivetrain.Transmissions.AWD);
                }
                else if (value == false)
                {
                    satsumaDriveTrain.SetTransmission(Drivetrain.Transmissions.FWD);
                }
            }
        }

        private static void SwitchCruiseControlDebug()
        {
            cruiseControlDebugEnabled = !cruiseControlDebugEnabled;
        }
        public bool GetSmartEngineModuleInstalledFixed()
        {
            return (smart_engine_module_part.installed && smart_engine_module_part.GetScrewablePart().partFixed);
        }

        public bool GetAbsModuleEnabled()
        {
            return abs_module_enabled;
        }
        public bool GetEspModuleEnabled()
        {
            return esp_module_enabled;
        }
        public bool GetTcsModuleEnabled()
        {
            return tcs_module_enabled;
        }
        public bool GetAlsModuleEnabled()
        {
            return smart_engine_module_logic.GetAlsEnabled();
        }
        public bool GetStep2RevModuleEnabled()
        {
            return smart_engine_module_logic.GetStep2RevLimiterEnabled();
        }

        public void SetAbsModuleEnabled(bool abs_module_enabled)
        {
            DonnerTech_ECU_Mod.abs_module_enabled = abs_module_enabled;
        }
        public void SetEspModuleEnabled(bool esp_module_enabled)
        {
            DonnerTech_ECU_Mod.esp_module_enabled = esp_module_enabled;
        }
        public void SetTcsModuleEnabled(bool tcs_module_enabled)
        {
            DonnerTech_ECU_Mod.tcs_module_enabled = tcs_module_enabled;
        }
        public void SetAlsModuleEnabled(bool enabled)
        {
            smart_engine_module_logic.SetAlsModuleEnabled(enabled);
        }
        public void SetStep2RevModuleEnabled(bool enabled)
        {
            smart_engine_module_logic.SetStep2RevModuleEnabled(enabled);
        }


        public AbsPart GetABSModule_Part()
        {
            return abs_module_part;
        }
        public SimplePart GetESPModule_Part()
        {
            return esp_module_part;
        }
        public TcsPart GetTCSModule_Part()
        {
            return tcs_module_part;
        }
        public SimplePart GetSmartEngineModule_Part()
        {
            return smart_engine_module_part;
        }
        public SimplePart GetCableHarness_Part()
        {
            return cable_harness_part;
        }
        public SimplePart GetMountingPlate_Part()
        {
            return mounting_plate_part;
        }

        public ScrewablePart GetABSModule_Screwable()
        {
            return abs_module_part.GetScrewablePart();
        }
        public ScrewablePart GetESPModule_Screwable()
        {
            return esp_module_part.GetScrewablePart();
        }
        public ScrewablePart GetTCSModule_Screwable()
        {
            return tcs_module_part.GetScrewablePart();
        }
        public ScrewablePart GetSmartEngineModule_Screwable()
        {
            return smart_engine_module_part.GetScrewablePart();
        }
        public ScrewablePart GetMountingPlate_Screwable()
        {
            return mounting_plate_part.GetScrewablePart();
        }

    }
}
