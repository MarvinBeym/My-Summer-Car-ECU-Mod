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

namespace DonnerTech_ECU_Mod
{
    public class DonnerTech_ECU_Mod : Mod
    {

        /*  ToDo:
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
        

        AssetBundle assetBundle;

        public float GetStep2RevRpm()
        {
            return ecu_InfoPanel_Logic.GetStep2RevRpm();
        }

        public static ScrewablePart ecu_mod_absModule_screwable;
        public static ScrewablePart ecu_mod_espModule_Part_screwable;
        public static ScrewablePart ecu_mod_tcsModule_Part_screwable;
        public static ScrewablePart ecu_mod_smartEngineModule_Part_screwable;
        public static ScrewablePart ecu_mod_mountingPlate_Part_screwable;
        public static ScrewablePart ecu_infoPanel_screwable;
        public static ScrewablePart ecu_reverseCamera_screwable;
        public static ScrewablePart ecu_rainLightSensorboard_screwable;

#if DEBUG
        private static SimplePart ecu_airride_fl_part;
        //private static ECU_Airride_FR_Part ecu_airride_fr_part;
        public static SimplePart awd_gearbox_part;
        public static SimplePart awd_propshaft_part;
        public static SimplePart awd_differential_part;
        public Vector3 ecu_airride_fl_installLocation = new Vector3(0, 0, 0);
        public Vector3 awd_gearbox_part_installLocation = new Vector3(0, 0, 0);
        public Vector3 awd_differential_part_installLocation = new Vector3(0, -0.33f, -1.16f);
        public Vector3 awd_propshaft_part_installLocation = new Vector3(-0.05f, -0.3f, -0.105f);
        private const string ecu_airride_fl_saveFile = "ecu_airride_fl_saveFile";
        private const string ecu_airride_fr_saveFile = "ecu_airride_fr_saveFile";
        private const string awd_gearbox_saveFile = "awd_gearbox_partSave.txt";
        private const string awd_propshaft_saveFile = "awd_propshaft_partSave.txt";
        private const string awd_differential_saveFile = "awd_differntial_partSave.txt";
#endif
        private TextMesh cruiseControlText;
        private static PartBuySave partBuySave;

        public Keybind ecu_panel_ArrowUp = new Keybind("ecu_panel_ArrowUp", "Arrow Up", KeyCode.Keypad8);
        public Keybind ecu_panel_ArrowDown = new Keybind("ecu_panel_ArrowDown", "Arrow Down", KeyCode.Keypad2);
        public Keybind ecu_panel_Circle = new Keybind("ecu_panel_Circle", "Circle", KeyCode.KeypadEnter);
        public Keybind ecu_panel_Cross = new Keybind("ecu_panel_Cross", "Cross", KeyCode.KeypadPeriod);
        public Keybind ecu_panel_Plus = new Keybind("ecu_panel_Plus", "Plus", KeyCode.KeypadPlus);
        public Keybind ecu_panel_Minus = new Keybind("ecu_panel_Minus", "Minus", KeyCode.KeypadMinus);




        private FsmString playerCurrentVehicle;

        //Part logic
        private ECU_InfoPanel_Logic ecu_InfoPanel_Logic;
        private ECU_CruiseControl_Logic ecu_CruiseControl_Logic;
        private ECU_SmartEngineModule_Logic ecu_smartEngineModule_logic;
        private ECU_ReverseCamera_Logic ecu_reverseCamera_logic;

        private static Settings toggleSixGears = new Settings("toggleSixGears", "Enable/Disable SixGears Mod", false, new Action(ToggleSixGears));
        private static Settings toggleAWD = new Settings("toggleAWD", "Toggle All Wheel Drive", false, new Action(ToggleAWD));
        


        public static bool absModuleEnabled = false;
        public static bool espModuleEnabled = false;
        public static bool tcsModuleEnabled = false;

        private static string modAssetsFolder;
        private RaycastHit hit;

        private static AbsPart ecu_mod_absModule_Part; //Had more logic
        private static SimplePart ecu_mod_espModule_Part;
        private static TcsPart ecu_mod_tcsModule_Part; //Had more logic
        private static SimplePart ecu_mod_cableHarness_Part;
        private static SimplePart ecu_mod_mountingPlate_Part;
        private static SimplePart ecu_mod_smartEngineModule_Part;
        private static SimplePart ecu_mod_cruiseControlPanel_Part;
        private static SimplePart ecu_InfoPanel_Part;
        private static SimplePart ecu_reverseCamera_part;
        private static SimplePart ecu_rainLightSensorboard_part;

        private static List<Part> partsList;

        public Vector3 ecu_reverseCamera_part_installLocation = new Vector3(0, -0.343f, -0.157f);
        public Vector3 ecu_rainLightSensorboard_part_installLocation = new Vector3(-0.0015f, 0.086f, 0.1235f);

        private static GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;
        private FsmBool electricsOK;

        private const string ecu_mod_ABSModule_SaveFile = "ecu_mod_ABSModule_partSave.txt";
        private const string ecu_mod_ESPModule_SaveFile = "ecu_mod_ESPModule_partSave.txt";
        private const string ecu_mod_TCSModule_SaveFile = "ecu_mod_TCSModule_partSave.txt";
        private const string ecu_mod_CableHarness_SaveFile = "ecu_mod_CableHarness_partSave.txt";
        private const string ecu_mod_MountingPlate_SaveFile = "ecu_mod_MountingPlate_partSave.txt";
        private const string ecu_mod_ModShop_SaveFile = "ecu_mod_ModShop_SaveFile.txt";
        private const string ecu_mod_SmartEngineModule_SaveFile = "ecu_mod_SmartEngineModule_partSave.txt";
        private const string ecu_mod_cruiseControlPanel_SaveFile = "ecu_mod_CruiseControlPanel_partSave.txt";
        private const string ecu_InfoPanel_SaveFile = "ecu_InfoPanel_partSave.txt";

        private const string ecu_reverseCamera_saveFile = "ecu_reverseCamera_saveFile.txt";
        private const string ecu_rainLightSensorboard_saveFile = "ecu_rainLightSensorboard_saveFile.txt";

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
        private bool ecu_InfoPanel_workaroundChildDisableDone = false;

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
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, ecu_mod_ABSModule_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, ecu_mod_ESPModule_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, ecu_mod_TCSModule_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, ecu_mod_CableHarness_SaveFile);
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, ecu_mod_MountingPlate_SaveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_mod_ModShop_SaveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_mod_SmartEngineModule_SaveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_mod_cruiseControlPanel_SaveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_InfoPanel_SaveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_reverseCamera_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_rainLightSensorboard_saveFile);

#if DEBUG
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_airride_fl_saveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_airride_fr_saveFile);
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
            Keybind.Add(this, ecu_panel_ArrowUp);
            Keybind.Add(this, ecu_panel_ArrowDown);
            Keybind.Add(this, ecu_panel_Circle);
            Keybind.Add(this, ecu_panel_Cross);
            Keybind.Add(this, ecu_panel_Plus);
            Keybind.Add(this, ecu_panel_Minus);

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
            GameObject ecu_mod_ABSModule = (assetBundle.LoadAsset("ECU-Mod_ABS-Module.prefab") as GameObject);
            GameObject ecu_mod_ESPModule = (assetBundle.LoadAsset("ECU-Mod_ESP-Module.prefab") as GameObject);
            GameObject ecu_mod_TCSModule = (assetBundle.LoadAsset("ECU-Mod_TCS-Module.prefab") as GameObject);
            GameObject ecu_mod_CableHarness = (assetBundle.LoadAsset("ECU-Mod_Cable-Harness.prefab") as GameObject);
            GameObject ecu_mod_MountingPlate = (assetBundle.LoadAsset("ECU-Mod_Mounting-Plate.prefab") as GameObject);
            GameObject ecu_mod_SmartEngineModule = (assetBundle.LoadAsset("ECU-Mod_SmartEngine-Module.prefab") as GameObject);
            GameObject ecu_mod_CruiseControlPanel = (assetBundle.LoadAsset("ECU-Mod_CruiseControl-Panel.prefab") as GameObject);
            GameObject ecu_InfoPanel = (assetBundle.LoadAsset("ECU-Mod_InfoPanel.prefab") as GameObject);
            GameObject ecu_reverseCamera = (assetBundle.LoadAsset("ECU-Mod_Reverse-Camera.prefab") as GameObject);
            GameObject ecu_rainLightSensorboard = (assetBundle.LoadAsset("ECU-Mod_Rain_Light-Sensorboard.prefab") as GameObject);

#if DEBUG
            GameObject awd_gearbox = (assetBundle.LoadAsset("AWD-Gearbox.prefab") as GameObject);
            GameObject awd_differential = (assetBundle.LoadAsset("AWD-Differential.prefab") as GameObject);
            GameObject awd_propshaft = (assetBundle.LoadAsset("AWD-Propshaft.prefab") as GameObject);
            GameObject ecu_airride_fl = (assetBundle.LoadAsset("Airride_FL.prefab") as GameObject);
            GameObject ecu_airride_fr = (assetBundle.LoadAsset("Airride_FL.prefab") as GameObject);
            SetObjectNameTagLayer(awd_gearbox, "AWD Gearbox");
            SetObjectNameTagLayer(awd_differential, "AWD Differential");
            SetObjectNameTagLayer(awd_propshaft, "AWD Propshaft");
            SetObjectNameTagLayer(ecu_airride_fl, "Airride FL");
            SetObjectNameTagLayer(ecu_airride_fr, "Airride FR");

            PartSaveInfo ecu_airride_fl_saveInfo = this.loadSaveData(ecu_airride_fl_saveFile, false);
            PartSaveInfo awd_gearbox_saveInfo = this.loadSaveData(awd_gearbox_saveFile, false);
            PartSaveInfo awd_differential_saveInfo = this.loadSaveData(awd_differential_saveFile, false);
            PartSaveInfo awd_propshaft_saveInfo = this.loadSaveData(awd_propshaft_saveFile, false);
#endif
            SetObjectNameTagLayer(ecu_mod_ABSModule, "ABS Module");
            SetObjectNameTagLayer(ecu_mod_ABSModule, "ABS Module");
            SetObjectNameTagLayer(ecu_mod_ESPModule, "ESP Module");
            SetObjectNameTagLayer(ecu_mod_TCSModule, "TCS Module");
            SetObjectNameTagLayer(ecu_mod_CableHarness, "ECU Cable Harness");
            SetObjectNameTagLayer(ecu_mod_MountingPlate, "ECU Mounting Plate");
            SetObjectNameTagLayer(ecu_mod_SmartEngineModule, "Smart Engine ECU");
            SetObjectNameTagLayer(ecu_mod_CruiseControlPanel, "Cruise Control Panel");
            SetObjectNameTagLayer(ecu_InfoPanel, "DonnerTech Info Panel");
            SetObjectNameTagLayer(ecu_reverseCamera, "Reverse Camera");
            SetObjectNameTagLayer(ecu_rainLightSensorboard, "Rain & Light Sensorboard");

            try
            {
                partBuySave = SaveLoad.DeserializeSaveFile<PartBuySave>(this, ecu_mod_ModShop_SaveFile);
            }
            catch
            {
            }
            if (partBuySave == null)
            {

                partBuySave = new PartBuySave();
            }

            PartSaveInfo ecu_mod_ABSModule_SaveInfo = this.loadSaveData(ecu_mod_ABSModule_SaveFile, partBuySave.boughtABSModule);
            PartSaveInfo ecu_mod_ESPModule_SaveInfo = this.loadSaveData(ecu_mod_ESPModule_SaveFile, partBuySave.boughtESPModule);
            PartSaveInfo ecu_mod_TCSModule_SaveInfo = this.loadSaveData(ecu_mod_TCSModule_SaveFile, partBuySave.boughtTCSModule);
            PartSaveInfo ecu_mod_CableHarness_SaveInfo = this.loadSaveData(ecu_mod_CableHarness_SaveFile, partBuySave.boughtCableHarness);
            PartSaveInfo ecu_mod_MountingPlate_SaveInfo = this.loadSaveData(ecu_mod_MountingPlate_SaveFile, partBuySave.boughtMountingPlate);
            PartSaveInfo ecu_mod_SmartEngineModule_SaveInfo = this.loadSaveData(ecu_mod_SmartEngineModule_SaveFile, partBuySave.boughtSmartEngineModule);
            PartSaveInfo ecu_mod_CruiseControllPanel_SaveInfo = this.loadSaveData(ecu_mod_cruiseControlPanel_SaveFile, partBuySave.boughtCruiseControlPanel);
            PartSaveInfo ecu_InfoPanel_SaveInfo = this.loadSaveData(ecu_InfoPanel_SaveFile, partBuySave.boughtInfoPanel);
            PartSaveInfo ecu_reverseCamera_saveInfo = this.loadSaveData(ecu_reverseCamera_saveFile, partBuySave.bought_reverseCamera);
            PartSaveInfo ecu_rainLightSensorboard_saveInfo = this.loadSaveData(ecu_rainLightSensorboard_saveFile, partBuySave.bought_rainLightSensorboard);
            SortedList<String, Screws> screwListSave = ScrewablePart.LoadScrews(this, "ecu_mod_screwable_save.txt");

#if DEBUG
            ecu_airride_fl_part = new SimplePart(
                    ecu_airride_fl_saveInfo,
                    ecu_airride_fl,
                    GameObject.Find("Chassis/FL"),
                    new Trigger("ecu_airride_fl_trigger", GameObject.Find("Chassis/FL"), ecu_airride_fl_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    ecu_airride_fl_installLocation,
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
                partsList.Add(ecu_airride_fl_part);
#endif

            ecu_mod_absModule_Part = new AbsPart(
                ecu_mod_ABSModule_SaveInfo,
                ecu_mod_ABSModule,
                satsuma,
                new Trigger("ecu_mod_ABSModule_Trigger", satsuma, new Vector3(0.254f, -0.28f, -0.155f), new Quaternion(0, 0, 0, 0), new Vector3(0.14f, 0.02f, 0.125f), false),
                new Vector3(0.254f, -0.248f, -0.155f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_espModule_Part = new SimplePart(
                ecu_mod_ESPModule_SaveInfo,
                ecu_mod_ESPModule,
                satsuma,
                 new Trigger("ecu_mod_ESPModule_Trigger", satsuma, new Vector3(0.288f, -0.28f, -0.0145f), new Quaternion(0, 0, 0, 0), new Vector3(0.21f, 0.02f, 0.125f), false),
                new Vector3(0.288f, -0.248f, -0.0145f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_tcsModule_Part = new TcsPart(
                ecu_mod_TCSModule_SaveInfo,
                ecu_mod_TCSModule,
                satsuma,
                new Trigger("ecu_mod_TCSModule_Trigger", satsuma, new Vector3(0.342f, -0.28f, 0.115f), new Quaternion(0, 0, 0, 0), new Vector3(0.104f, 0.02f, 0.104f), false),
                new Vector3(0.342f, -0.246f, 0.115f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_cableHarness_Part = new SimplePart(
                ecu_mod_CableHarness_SaveInfo,
                ecu_mod_CableHarness,
                satsuma,
                new Trigger("ecu_mod_CableHarness_Trigger", satsuma, new Vector3(0.423f, -0.28f, -0.0384f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.02f, 0.3f), false),
                new Vector3(0.388f, -0.245f, -0.007f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_mountingPlate_Part = new SimplePart(
                ecu_mod_MountingPlate_SaveInfo,
                ecu_mod_MountingPlate,
                satsuma,
                new Trigger("ecu_mod_MountingPlate_Trigger", satsuma, new Vector3(0.31f, -0.28f, -0.038f), new Quaternion(0, 0, 0, 0), new Vector3(0.32f, 0.02f, 0.45f), false),
                new Vector3(0.3115f, -0.27f, -0.0393f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_smartEngineModule_Part = new SimplePart(
                ecu_mod_SmartEngineModule_SaveInfo,
                ecu_mod_SmartEngineModule,
                satsuma,
                new Trigger("ecu_mod_SmartEngineModule_Trigger", satsuma, new Vector3(0.2398f, -0.28f, 0.104f), new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.02f, 0.13f), false),
                new Vector3(0.2398f, -0.248f, 0.104f),
                new Quaternion(0, 0, 0, 0)
            );

            ecu_smartEngineModule_logic = ecu_mod_smartEngineModule_Part.rigidPart.AddComponent<ECU_SmartEngineModule_Logic>();

            ecu_mod_cruiseControlPanel_Part = new SimplePart(
                    ecu_mod_CruiseControllPanel_SaveInfo,
                    ecu_mod_CruiseControlPanel,
                    GameObject.Find("dashboard(Clone)"),
                    new Trigger("ecu_mod_CruiseControlPanel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.46f, -0.095f, 0.08f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    new Vector3(0.5f, -0.095f, 0.08f),
                    new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
                );
            ecu_CruiseControl_Logic = ecu_mod_cruiseControlPanel_Part.rigidPart.AddComponent<ECU_CruiseControl_Logic>();


            ecu_InfoPanel_Part = new SimplePart(
                    ecu_InfoPanel_SaveInfo,
                    ecu_InfoPanel,
                    GameObject.Find("dashboard(Clone)"),
                    new Trigger("ecu_InfoPanel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.25f, -0.07f, -0.02f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    new Vector3(0.25f, -0.088f, -0.01f),
                    new Quaternion { eulerAngles = new Vector3(0, 180, 180) }
                );

            ecu_InfoPanel_Logic = ecu_InfoPanel_Part.rigidPart.AddComponent<ECU_InfoPanel_Logic>();


            ecu_rainLightSensorboard_part = new SimplePart(
                    ecu_rainLightSensorboard_saveInfo,
                    ecu_rainLightSensorboard,
                    GameObject.Find("dashboard(Clone)"),
                    new Trigger("ecu_rainLightSensorboard_trigger", GameObject.Find("dashboard(Clone)"), ecu_rainLightSensorboard_part_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    ecu_rainLightSensorboard_part_installLocation,
                    new Quaternion { eulerAngles = new Vector3(90, 0, 0) }
                );

            ecu_reverseCamera_part = new SimplePart(
                    ecu_reverseCamera_saveInfo,
                    ecu_reverseCamera,
                    GameObject.Find("bootlid(Clone)"),
                    new Trigger("ecu_reverseCamera_trigger", GameObject.Find("bootlid(Clone)"), ecu_reverseCamera_part_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    ecu_reverseCamera_part_installLocation,
                    new Quaternion { eulerAngles = new Vector3(120, 0, 0) }
                );
            ecu_reverseCamera_logic = ecu_reverseCamera_part.rigidPart.AddComponent<ECU_ReverseCamera_Logic>();

            partsList.Add(ecu_mod_absModule_Part);
            partsList.Add(ecu_mod_espModule_Part);
            partsList.Add(ecu_mod_tcsModule_Part);
            partsList.Add(ecu_mod_cableHarness_Part);
            partsList.Add(ecu_mod_mountingPlate_Part);
            partsList.Add(ecu_mod_smartEngineModule_Part);
            partsList.Add(ecu_mod_cruiseControlPanel_Part);
            partsList.Add(ecu_InfoPanel_Part);
            partsList.Add(ecu_reverseCamera_part);
            partsList.Add(ecu_rainLightSensorboard_part);

            TextMesh[] ecu_InfoPanel_TextMeshes = ecu_InfoPanel_Part.activePart.GetComponentsInChildren<TextMesh>();
            foreach (TextMesh textMesh in ecu_InfoPanel_TextMeshes)
            {
                textMesh.gameObject.SetActive(false);
            }


            SpriteRenderer[] ecu_InfoPanel_SpriteRenderer = ecu_InfoPanel_Part.activePart.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in ecu_InfoPanel_SpriteRenderer)
            {
                spriteRenderer.enabled = false;
            }
            if(ecu_InfoPanel_SpriteRenderer.Length > 0 && ecu_InfoPanel_TextMeshes.Length > 0)
            {
                ecu_InfoPanel_workaroundChildDisableDone = true;
            }

            ScrewablePart screwable;

            screwable = new ScrewablePart(screwListSave, this, ecu_mod_absModule_Part.rigidPart,
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
            ecu_mod_absModule_Part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, ecu_mod_espModule_Part.rigidPart,
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
            ecu_mod_espModule_Part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, ecu_mod_tcsModule_Part.rigidPart,
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
            ecu_mod_tcsModule_Part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, ecu_mod_smartEngineModule_Part.rigidPart,
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
            ecu_mod_smartEngineModule_Part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, ecu_mod_mountingPlate_Part.rigidPart,
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
            ecu_mod_mountingPlate_Part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, ecu_rainLightSensorboard_part.rigidPart,
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
            ecu_rainLightSensorboard_part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, ecu_reverseCamera_part.rigidPart,
                new Vector3[]{
                    new Vector3(0f, -0.015f, 0.0055f),
                },
                new Vector3[]{
                    new Vector3(0, 0, 0),
                },
                new Vector3[]{
                    new Vector3(0.5f, 0.5f, 0.5f),
                }, 5, "screwable_screw2");
            ecu_reverseCamera_part.SetScrewablePart(screwable);

            screwable = new ScrewablePart(screwListSave, this, ecu_InfoPanel_Part.rigidPart,
                new Vector3[]{
                    new Vector3(0f, -0.025f, -0.067f),
                },
                new Vector3[]{
                    new Vector3(180, 0, 0),
                },
                new Vector3[]{
                    new Vector3(0.8f, 0.8f, 0.8f),
                }, 8, "screwable_screw2");
            ecu_InfoPanel_Part.SetScrewablePart(screwable);

            if (GameObject.Find("Shop for mods") != null)
            {
                ModsShop.ShopItem modsShopItem = GameObject.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();
                
                IDictionary<string, Part> shopItems = new Dictionary<string, Part>();
                shopItems["ABS Module ECU"] = ecu_mod_absModule_Part;
                shopItems["ESP Module ECU"] = ecu_mod_espModule_Part;
                shopItems["TCS Module ECU"] = ecu_mod_tcsModule_Part;
                shopItems["ECU Cable Harness"] = ecu_mod_cableHarness_Part;
                shopItems["ECU Mounting Plate"] = ecu_mod_mountingPlate_Part;
                shopItems["Smart Engine Module ECU"] = ecu_mod_smartEngineModule_Part;
                shopItems["Cruise Control Panel with Controller"] = ecu_mod_cruiseControlPanel_Part;
                shopItems["ECU Info Panel"] = ecu_InfoPanel_Part;
                shopItems["Rain & Light Sensorboard"] = ecu_rainLightSensorboard_part;
                shopItems["Reverse Camera"] = ecu_reverseCamera_part;
#if DEBUG
                shopItems["Airride FL"] = ecu_airride_fl_part;
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
            UnityEngine.Object.Destroy(ecu_mod_ABSModule);
            UnityEngine.Object.Destroy(ecu_mod_ESPModule);
            UnityEngine.Object.Destroy(ecu_mod_TCSModule);
            UnityEngine.Object.Destroy(ecu_mod_CableHarness);
            UnityEngine.Object.Destroy(ecu_mod_MountingPlate);
            UnityEngine.Object.Destroy(ecu_mod_SmartEngineModule);
            UnityEngine.Object.Destroy(ecu_mod_CruiseControlPanel);
            UnityEngine.Object.Destroy(ecu_InfoPanel);
            UnityEngine.Object.Destroy(ecu_rainLightSensorboard);
            UnityEngine.Object.Destroy(ecu_reverseCamera);

            UnityEngine.Object.Destroy(ecu_airride_fl);
            UnityEngine.Object.Destroy(awd_gearbox);

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");
            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " finished loading");
        }

        public bool GetReverseCameraInstalledScrewed()
        {
            return (ecu_reverseCamera_part.installed && ecu_reverseCamera_part.GetScrewablePart().partFixed);
        }

        internal bool GetInfoPanelScrewed()
        {
            return ecu_InfoPanel_Part.GetScrewablePart().partFixed;
        }
        public void SetReverseCameraEnabled(bool enabled)
        {
            ecu_reverseCamera_logic.SetReverseCameraEnabled(enabled);


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
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_mod_absModule_Part.getSaveInfo(), ecu_mod_ABSModule_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_mod_espModule_Part.getSaveInfo(), ecu_mod_ESPModule_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_mod_tcsModule_Part.getSaveInfo(), ecu_mod_TCSModule_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_mod_cableHarness_Part.getSaveInfo(), ecu_mod_CableHarness_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_mod_mountingPlate_Part.getSaveInfo(), ecu_mod_MountingPlate_SaveFile);
                SaveLoad.SerializeSaveFile<PartBuySave>(this, partBuySave, ecu_mod_ModShop_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_mod_smartEngineModule_Part.getSaveInfo(), ecu_mod_SmartEngineModule_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_mod_cruiseControlPanel_Part.getSaveInfo(), ecu_mod_cruiseControlPanel_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_InfoPanel_Part.getSaveInfo(), ecu_InfoPanel_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_rainLightSensorboard_part.getSaveInfo(), ecu_rainLightSensorboard_saveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, ecu_reverseCamera_part.getSaveInfo(), ecu_reverseCamera_saveFile);
#if DEBUG
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, awd_gearbox_part.getSaveInfo(), awd_gearbox_saveFile);
#endif
                ScrewablePart.SaveScrews(this, new ScrewablePart[]
                {
                    ecu_mod_absModule_Part.GetScrewablePart(),
                    ecu_mod_espModule_Part.GetScrewablePart(),
                    ecu_mod_tcsModule_Part.GetScrewablePart(),
                    ecu_mod_smartEngineModule_Part.GetScrewablePart(),
                    ecu_mod_mountingPlate_Part.GetScrewablePart(),
                    ecu_InfoPanel_Part.GetScrewablePart(),
                    ecu_rainLightSensorboard_part.GetScrewablePart(),
                    ecu_reverseCamera_part.GetScrewablePart(),
                }, "ecu_mod_screwable_save.txt");
            }
            catch (System.Exception ex)
            {
                ModConsole.Error("<b>[ECU_MOD]</b> - an error occured while attempting to save see: " + ex.ToString());
            }
        }

        public override void OnGUI()
        {
            if (cruiseControlDebugEnabled)
            {
                GUI.Label(new Rect(20, 400, 500, 100), "------------------------------------");
                GUI.Label(new Rect(20, 420, 500, 100), "true = correct value for cruise control to work");
                GUI.Label(new Rect(20, 440, 500, 100), "false = conition needed to have cruise control working");
                GUI.Label(new Rect(20, 460, 500, 100), "Gear not R: " + (satsumaDriveTrain.gear != 0));
                GUI.Label(new Rect(20, 480, 500, 100), "cruise control panel installed: " + ecu_mod_cruiseControlPanel_Part.installed);
                GUI.Label(new Rect(20, 500, 500, 100), "cruise control enabled: " + cruiseControlModuleEnabled);
                GUI.Label(new Rect(20, 520, 500, 100), "mounting plate installed: " + ecu_mod_mountingPlate_Part.installed);
                GUI.Label(new Rect(20, 540, 500, 100), "mounting plate screwed in: " + ecu_mod_mountingPlate_Part.GetScrewablePart().partFixed);
                GUI.Label(new Rect(20, 560, 500, 100), "smart engine module installed: " + ecu_mod_smartEngineModule_Part.installed);
                GUI.Label(new Rect(20, 580, 500, 100), "smart engine module screwed in: " + ecu_mod_smartEngineModule_Part.GetScrewablePart().partFixed);
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
            if (!ecu_InfoPanel_Part.installed)
            {
                if (ecu_InfoPanel_Part.activePart.transform.localScale.x < 1.5f)
                {
                    ecu_InfoPanel_Part.activePart.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }
                if (!ecu_InfoPanel_workaroundChildDisableDone)
                {
                    TextMesh[] ecu_InfoPanel_TextMeshes = ecu_InfoPanel_Part.activePart.GetComponentsInChildren<TextMesh>();
                    foreach (TextMesh textMesh in ecu_InfoPanel_TextMeshes)
                    {
                        textMesh.gameObject.SetActive(false);
                    }


                    SpriteRenderer[] ecu_InfoPanel_SpriteRenderer = ecu_InfoPanel_Part.activePart.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer spriteRenderer in ecu_InfoPanel_SpriteRenderer)
                    {
                        spriteRenderer.enabled = false;
                    }
                    if (ecu_InfoPanel_SpriteRenderer.Length > 0 && ecu_InfoPanel_TextMeshes.Length > 0)
                    {
                        ecu_InfoPanel_workaroundChildDisableDone = true;
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
            if (ecu_mod_absModule_Part.installed & ecu_mod_absModule_Part.GetScrewablePart().partFixed)
            {
                satsuma.GetComponent<CarController>().ABS = !satsuma.GetComponent<CarController>().ABS;
                absModuleEnabled = satsuma.GetComponent<CarController>().ABS;
            }
            else
            {
                satsuma.GetComponent<CarController>().ABS = false;
                absModuleEnabled = false;
            }
            

        }
        
        public static void ToggleESP()
        {
            if (ecu_mod_espModule_Part.installed && ecu_mod_espModule_Part.GetScrewablePart().partFixed)
            {
                satsuma.GetComponent<CarController>().ESP = !satsuma.GetComponent<CarController>().ESP;
                espModuleEnabled = satsuma.GetComponent<CarController>().ESP;
            }
            else
            {
                satsuma.GetComponent<CarController>().ESP = false;
                espModuleEnabled = false;
            }
            
        }
        public static void ToggleTCS()
        {
            if (ecu_mod_tcsModule_Part.installed && ecu_mod_tcsModule_Part.GetScrewablePart().partFixed)
            {
                satsuma.GetComponent<CarController>().TCS = !satsuma.GetComponent<CarController>().TCS;
                tcsModuleEnabled = satsuma.GetComponent<CarController>().TCS;
            }
            else
            {
                satsuma.GetComponent<CarController>().TCS = false;
                tcsModuleEnabled = false;
            }
            
        }
        public void ToggleALS()
        {
            ecu_smartEngineModule_logic.ToggleALS();
        }
        public void Toggle2StepRevLimiter()
        {
            ecu_smartEngineModule_logic.ToggleStep2RevLimiter();
        }

        private static void PosReset()
        {
            try
            {
                foreach (Part part in DonnerTech_ECU_Mod.partsList)
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
            return (ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part.GetScrewablePart().partFixed);
        }

        public bool GetAbsModuleEnabled()
        {
            return absModuleEnabled;
        }
        public bool GetEspModuleEnabled()
        {
            return espModuleEnabled;
        }
        public bool GetTcsModuleEnabled()
        {
            return tcsModuleEnabled;
        }
        public bool GetAlsModuleEnabled()
        {
            return ecu_smartEngineModule_logic.GetAlsEnabled();
        }
        public bool GetStep2RevModuleEnabled()
        {
            return ecu_smartEngineModule_logic.GetStep2RevLimiterEnabled();
        }

        public void SetAbsModuleEnabled(bool absModuleEnabled)
        {
            DonnerTech_ECU_Mod.absModuleEnabled = absModuleEnabled;
        }
        public void SetEspModuleEnabled(bool espModuleEnabled)
        {
            DonnerTech_ECU_Mod.espModuleEnabled = espModuleEnabled;
        }
        public void SetTcsModuleEnabled(bool tcsModuleEnabled)
        {
            DonnerTech_ECU_Mod.tcsModuleEnabled = tcsModuleEnabled;
        }
        public void SetAlsModuleEnabled(bool enabled)
        {
            ecu_smartEngineModule_logic.SetAlsModuleEnabled(enabled);
        }
        public void SetStep2RevModuleEnabled(bool enabled)
        {
            ecu_smartEngineModule_logic.SetStep2RevModuleEnabled(enabled);
        }


        public AbsPart GetABSModule_Part()
        {
            return ecu_mod_absModule_Part;
        }
        public SimplePart GetESPModule_Part()
        {
            return ecu_mod_espModule_Part;
        }
        public TcsPart GetTCSModule_Part()
        {
            return ecu_mod_tcsModule_Part;
        }
        public SimplePart GetSmartEngineModule_Part()
        {
            return ecu_mod_smartEngineModule_Part;
        }
        public SimplePart GetCableHarness_Part()
        {
            return ecu_mod_cableHarness_Part;
        }
        public SimplePart GetMountingPlate_Part()
        {
            return ecu_mod_mountingPlate_Part;
        }

        public ScrewablePart GetABSModule_Screwable()
        {
            return ecu_mod_absModule_Part.GetScrewablePart();
        }
        public ScrewablePart GetESPModule_Screwable()
        {
            return ecu_mod_espModule_Part.GetScrewablePart();
        }
        public ScrewablePart GetTCSModule_Screwable()
        {
            return ecu_mod_tcsModule_Part.GetScrewablePart();
        }
        public ScrewablePart GetSmartEngineModule_Screwable()
        {
            return ecu_mod_smartEngineModule_Part.GetScrewablePart();
        }
        public ScrewablePart GetMountingPlate_Screwable()
        {
            return ecu_mod_mountingPlate_Part.GetScrewablePart();
        }

    }
}
