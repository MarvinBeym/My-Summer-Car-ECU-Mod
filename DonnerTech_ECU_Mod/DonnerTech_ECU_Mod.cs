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

        /*  TODO:
         *  Add Turbocharger ECU
         *  ADD RevLimiter function
         *  ADD Antilag function
         *  ADD change turbocharger boost function
         *  DONE: ADD s2Rev Stage 2 revlimiter
         *  ADD CruiseControll
         *  DONE: Make parts plop off when mounting plate is removed
         *  change air/fuel ratio when launch controll and antilag
         *  Save state of modules and load them
         */

        /*  Changelog (v1.3.1)
         *  Added limits for 2step revlimiter (max 10k, min 2k)
         */
        /* BUGS
         * Eco AutoTune runs Race tune too
         * Move screw of InfoPanel to not be visible when screwed in
         */

        public override string ID => "DonnerTech_ECU_Mod"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing ECUs"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.3"; //Version
        public override bool UseAssetsFolder => true;

        private static bool cruiseControlDebugEnabled = false;
        

        AssetBundle assetBundle;

        public float GetStep2RevRpm()
        {
            return ecu_InfoPanel_Logic.GetStep2RevRpm();
        }

        private static GameObject ecu_mod_ABSModule = new GameObject();
        private static GameObject ecu_mod_ESPModule = new GameObject();
        private static GameObject ecu_mod_TCSModule = new GameObject();
        private static GameObject ecu_mod_CableHarness = new GameObject();
        private static GameObject ecu_mod_MountingPlate = new GameObject();
        private static GameObject ecu_mod_SmartEngineModule = new GameObject();
        private static GameObject ecu_mod_CruiseControlPanel = new GameObject();
        private static GameObject ecu_InfoPanel = new GameObject();
#if DEBUG
        private static GameObject awd_gearbox = new GameObject();
        private static GameObject awd_propshaft = new GameObject();
        private static GameObject awd_differential = new GameObject();
#endif
        private TextMesh cruiseControlText;
        private static PartBuySave partBuySave;
        private Trigger ecu_mod_ABSModule_Trigger;
        private Trigger ecu_mod_ESPModule_Trigger;
        private Trigger ecu_mod_TCSModule_Trigger;
        private Trigger ecu_mod_CableHarness_Trigger;
        private Trigger ecu_mod_MountingPlate_Trigger;
        private Trigger ecu_mod_SmartEngineModule_Trigger;
        private Trigger ecu_mod_CruiseControlPanel_Trigger;
        private Trigger ecu_InfoPanel_Trigger;

        private Trigger awd_gearbox_trigger;
        private Trigger awd_propshaft_trigger;
        private Trigger awd_differential_trigger;

        private FsmString playerCurrentVehicle;

        //Part logic
        private ECU_InfoPanel_Logic ecu_InfoPanel_Logic;
        private ECU_CruiseControl_Logic ecu_CruiseControl_Logic;
        private ECU_SmartEngineModule_Logic ecu_smartEngineModule_logic;

        private static Settings toggleSixGears = new Settings("toggleSixGears", "Enable/Disable SixGears Mod", false, new Action(ToggleSixGears));
        private static Settings toggleAWD = new Settings("toggleAWD", "Toggle All Wheel Drive", false, new Action(ToggleAWD));
        


        public static bool absModuleEnabled = false;
        public static bool espModuleEnabled = false;
        public static bool tcsModuleEnabled = false;

        private GameObject headLightBeams;

        private static string modAssetsFolder;
        private RaycastHit hit;

        private static ECU_MOD_ABSModule_Part ecu_mod_absModule_Part;
        private static ECU_MOD_ESPModule_Part ecu_mod_espModule_Part;
        private static ECU_MOD_TCSModule_Part ecu_mod_tcsModule_Part;
        private static ECU_MOD_CableHarness_Part ecu_mod_cableHarness_Part;
        private static ECU_MOD_MountingPlate_Part ecu_mod_mountingPlate_Part;
        private static ECU_MOD_SmartEngineModule_Part ecu_mod_smartEngineModule_Part;
        private static ECU_MOD_CruiseControlPanel_Part ecu_mod_cruiseControlPanel_Part;
        private static ECU_InfoPanel_Part ecu_InfoPanel_Part;

        private static List<Part> partsList;

#if DEBUG
        public static AWD_Gearbox_Part awd_gearbox_part;
        public static AWD_PropShaft_Part awd_propshaft_part;
        public static AWD_Differential_Part awd_differential_part;
#endif
        public static ScrewablePart ecu_mod_absModule_screwable;
        public static ScrewablePart ecu_mod_espModule_Part_screwable;
        public static ScrewablePart ecu_mod_tcsModule_Part_screwable;
        public static ScrewablePart ecu_mod_smartEngineModule_Part_screwable;
        public static ScrewablePart ecu_mod_mountingPlate_Part_screwable;
        public static ScrewablePart ecu_infoPanel_screwable;

        public static Vector3 awd_gearbox_part_spawnLocation = new Vector3(0, 0, 0);
        public static Vector3 awd_differential_part_spawnLocation = new Vector3(0, 0, 0);
        public static Vector3 awd_propshaft_part_spawnLocation = new Vector3(0, 0, 0);
#if DEBUG
        public Vector3 awd_gearbox_part_installLocation = new Vector3(0, 0, 0);
        public Vector3 awd_differential_part_installLocation = new Vector3(0, -0.33f, -1.16f);
        public Vector3 awd_propshaft_part_installLocation = new Vector3(-0.05f, -0.3f, -0.105f);
#endif
        private static GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;

        private const string ecu_mod_ABSModule_SaveFile = "ecu_mod_ABSModule_partSave.txt";
        private const string ecu_mod_ESPModule_SaveFile = "ecu_mod_ESPModule_partSave.txt";
        private const string ecu_mod_TCSModule_SaveFile = "ecu_mod_TCSModule_partSave.txt";
        private const string ecu_mod_CableHarness_SaveFile = "ecu_mod_CableHarness_partSave.txt";
        private const string ecu_mod_MountingPlate_SaveFile = "ecu_mod_MountingPlate_partSave.txt";
        private const string ecu_mod_ModShop_SaveFile = "ecu_mod_ModShop_SaveFile.txt";
        private const string ecu_mod_SmartEngineModule_SaveFile = "ecu_mod_SmartEngineModule_partSave.txt";
        private const string ecu_mod_cruiseControlPanel_SaveFile = "ecu_mod_CruiseControlPanel_partSave.txt";
        private const string ecu_InfoPanel_SaveFile = "ecu_InfoPanel_partSave.txt";
#if DEBUG
        private const string awd_gearbox_saveFile = "awd_gearbox_partSave.txt";
        private const string awd_propshaft_saveFile = "awd_propshaft_partSave.txt";
        private const string awd_differential_saveFile = "awd_differntial_partSave.txt";
#endif
        private Settings resetPosSetting = new Settings("resetPos", "Reset uninstalled parts location", new Action(PosReset));
        private Settings debugCruiseControlSetting = new Settings("debugCruiseControl", "Enable/Disable", SwitchCruiseControlDebug);

        private static AudioSource backFireLoop;
        private static ModAudio backFire_loop = new ModAudio();
        private bool engineBackfiring = false;
        private float originalChokeValue;
        private static bool sixGearsEnabled;

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

        private PartSaveInfo loadSaveData(string saveFile)
        {
            try
            {
                return SaveLoad.DeserializeSaveFile<PartSaveInfo>(this, saveFile);
            }
            catch (System.NullReferenceException)
            {
                // no save file exists.. //loading default save data.

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
#if DEBUG
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
            ecu_mod_ABSModule = (assetBundle.LoadAsset("ECU-Mod_ABS-Module.prefab") as GameObject);
            ecu_mod_ESPModule = (assetBundle.LoadAsset("ECU-Mod_ESP-Module.prefab") as GameObject);
            ecu_mod_TCSModule = (assetBundle.LoadAsset("ECU-Mod_TCS-Module.prefab") as GameObject);
            ecu_mod_CableHarness = (assetBundle.LoadAsset("ECU-Mod_Cable-Harness.prefab") as GameObject);
            ecu_mod_MountingPlate = (assetBundle.LoadAsset("ECU-Mod_Mounting-Plate.prefab") as GameObject);
            ecu_mod_SmartEngineModule = (assetBundle.LoadAsset("ECU-Mod_SmartEngine-Module.prefab") as GameObject);
            ecu_mod_CruiseControlPanel = (assetBundle.LoadAsset("ECU-Mod_CruiseControl-Panel.prefab") as GameObject);
            ecu_InfoPanel = (assetBundle.LoadAsset("ECU-Mod_InfoPanel.prefab") as GameObject);
#if DEBUG
            awd_gearbox = (assetBundle.LoadAsset("AWD-Gearbox.prefab") as GameObject);
            awd_differential = (assetBundle.LoadAsset("AWD-Differential.prefab") as GameObject);
            awd_propshaft = (assetBundle.LoadAsset("AWD-Propshaft.prefab") as GameObject);
#endif
            ecu_mod_ABSModule.name = "ABS Module";
            ecu_mod_ESPModule.name = "ESP Module";
            ecu_mod_TCSModule.name = "TCS Module";
            ecu_mod_CableHarness.name = "ECU Cable Harness";
            ecu_mod_MountingPlate.name = "ECU Mounting Plate";
            ecu_mod_SmartEngineModule.name = "Smart Engine ECU";
            ecu_mod_CruiseControlPanel.name = "Cruise Control Panel";
            ecu_InfoPanel.name = "DonnerTech Info Panel";
#if DEBUG
            awd_gearbox.name = "AWD Gearbox";
            awd_differential.name = "AWD Differential";
            awd_propshaft.name = "AWD Propshaft";
#endif
            ecu_mod_ABSModule.tag = "PART";
            ecu_mod_ESPModule.tag = "PART";
            ecu_mod_TCSModule.tag = "PART";
            ecu_mod_CableHarness.tag = "PART";
            ecu_mod_MountingPlate.tag = "PART";
            ecu_mod_SmartEngineModule.tag = "PART";
            ecu_mod_CruiseControlPanel.tag = "PART";
            ecu_InfoPanel.tag = "PART";
#if DEBUG
            awd_gearbox.tag = "PART";
            awd_differential.tag = "PART";
            awd_propshaft.tag = "PART";
#endif
            ecu_mod_ABSModule.layer = LayerMask.NameToLayer("Parts");
            ecu_mod_ESPModule.layer = LayerMask.NameToLayer("Parts");
            ecu_mod_TCSModule.layer = LayerMask.NameToLayer("Parts");
            ecu_mod_CableHarness.layer = LayerMask.NameToLayer("Parts");
            ecu_mod_MountingPlate.layer = LayerMask.NameToLayer("Parts");
            ecu_mod_SmartEngineModule.layer = LayerMask.NameToLayer("Parts");
            ecu_mod_CruiseControlPanel.layer = LayerMask.NameToLayer("Parts");
            ecu_InfoPanel.layer = LayerMask.NameToLayer("Parts");
#if DEBUG
            awd_gearbox.layer = LayerMask.NameToLayer("Parts");
            awd_differential.layer = LayerMask.NameToLayer("DontCollide");
            awd_propshaft.layer = LayerMask.NameToLayer("Parts");
#endif



            ecu_mod_ABSModule_Trigger = new Trigger("ECU_MOD_ABSModule_Trigger", satsuma, new Vector3(0.254f, -0.28f, -0.155f), new Quaternion(0, 0, 0, 0), new Vector3(0.14f, 0.02f, 0.125f), false);
            ecu_mod_ESPModule_Trigger = new Trigger("ECU_MOD_ESPModule_Trigger", satsuma, new Vector3(0.288f, -0.28f, -0.0145f), new Quaternion(0, 0, 0, 0), new Vector3(0.21f, 0.02f, 0.125f), false);
            ecu_mod_TCSModule_Trigger = new Trigger("ECU_MOD_TCSModule_Trigger", satsuma, new Vector3(0.342f, -0.28f, 0.115f), new Quaternion(0, 0, 0, 0), new Vector3(0.104f, 0.02f, 0.104f), false);
            ecu_mod_CableHarness_Trigger = new Trigger("ECU_MOD_CableHarness_Trigger", satsuma, new Vector3(0.423f, -0.28f, -0.0384f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.02f, 0.3f), false);
            ecu_mod_MountingPlate_Trigger = new Trigger("ECU_MOD_MountingPlate_Trigger", satsuma, new Vector3(0.31f, -0.28f, -0.038f), new Quaternion(0, 0, 0, 0), new Vector3(0.32f, 0.02f, 0.45f), false);
            ecu_mod_SmartEngineModule_Trigger = new Trigger("ECU_MOD_SmartEngineModule_Trigger", satsuma, new Vector3(0.2398f, -0.28f, 0.104f), new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.02f, 0.13f), false);
            ecu_mod_CruiseControlPanel_Trigger = new Trigger("ECU_MOD_CruiseControllPanel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.46f, -0.095f, 0.08f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
            ecu_InfoPanel_Trigger = new Trigger("ECU_MOD_InfoPanel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.25f, -0.07f, -0.02f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
#if DEBUG
            awd_gearbox_trigger = new Trigger("ECU_MOD_AWD_Gearbox_Trigger", GameObject.Find("pivot_gearbox"), awd_gearbox_part_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
            awd_differential_trigger = new Trigger("ECU_MOD_AWD_Differential_Trigger", satsuma, awd_differential_part_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
            awd_propshaft_trigger = new Trigger("ECU_MOD_AWD_Propshaft_Trigger", satsuma, awd_propshaft_part_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);
#endif
            PartSaveInfo ecu_mod_ABSModule_SaveInfo = null;
            PartSaveInfo ecu_mod_ESPModule_SaveInfo = null;
            PartSaveInfo ecu_mod_TCSModule_SaveInfo = null;
            PartSaveInfo ecu_mod_CableHarness_SaveInfo = null;
            PartSaveInfo ecu_mod_MountingPlate_SaveInfo = null;
            PartSaveInfo ecu_mod_SmartEngineModule_SaveInfo = null;
            PartSaveInfo ecu_mod_CruiseControllPanel_SaveInfo = null;
            PartSaveInfo ecu_InfoPanel_SaveInfo = null;
#if DEBUG
            PartSaveInfo awd_gearbox_saveInfo = null;
            PartSaveInfo awd_differential_saveInfo = null;
            PartSaveInfo awd_propshaft_saveInfo = null;
#endif
            ecu_mod_ABSModule_SaveInfo = this.loadSaveData(ecu_mod_ABSModule_SaveFile);
            ecu_mod_ESPModule_SaveInfo = this.loadSaveData(ecu_mod_ESPModule_SaveFile);
            ecu_mod_TCSModule_SaveInfo = this.loadSaveData(ecu_mod_TCSModule_SaveFile);
            ecu_mod_CableHarness_SaveInfo = this.loadSaveData(ecu_mod_CableHarness_SaveFile);
            ecu_mod_MountingPlate_SaveInfo = this.loadSaveData(ecu_mod_MountingPlate_SaveFile);
            ecu_mod_SmartEngineModule_SaveInfo = this.loadSaveData(ecu_mod_SmartEngineModule_SaveFile);
            ecu_mod_CruiseControllPanel_SaveInfo = this.loadSaveData(ecu_mod_cruiseControlPanel_SaveFile);
            ecu_InfoPanel_SaveInfo = this.loadSaveData(ecu_InfoPanel_SaveFile);

#if DEBUG
            awd_gearbox_saveInfo = this.loadSaveData(awd_gearbox_saveFile);
            awd_differential_saveInfo = this.loadSaveData(awd_differential_saveFile);
            awd_propshaft_saveInfo = this.loadSaveData(awd_propshaft_saveFile);
#endif
            try
            {
                partBuySave = SaveLoad.DeserializeSaveFile<PartBuySave>(this, ecu_mod_ModShop_SaveFile);
            }
            catch
            {
            }
            if (partBuySave == null)
            {

                partBuySave = new PartBuySave
                {
                    boughtABSModule = false,
                    boughtESPModule = false,
                    boughtTCSModule = false,
                    boughtCableHarness = false,
                    boughtMountingPlate = false,
                    boughtSmartEngineModule = false,
                    boughtCruiseControlPanel = false,
                    boughtInfoPanel = false,
                    boughtAwdGearbox = false,
                    boughtAwdDifferential = false,
                    boughtAwdPropshaft = false
                };
            }
            if (!partBuySave.boughtABSModule)
            {
                ecu_mod_ABSModule_SaveInfo = null;
            }
            if (!partBuySave.boughtESPModule)
            {
                ecu_mod_ESPModule_SaveInfo = null;
            }
            if (!partBuySave.boughtTCSModule)
            {
                ecu_mod_TCSModule_SaveInfo = null;
            }
            if (!partBuySave.boughtCableHarness)
            {
                ecu_mod_CableHarness_SaveInfo = null;
            }
            if (!partBuySave.boughtMountingPlate)
            {
                ecu_mod_MountingPlate_SaveInfo = null;
            }
            if (!partBuySave.boughtSmartEngineModule)
            {
                ecu_mod_SmartEngineModule_SaveInfo = null;
            }
            if (!partBuySave.boughtCruiseControlPanel)
            {
                ecu_mod_CruiseControllPanel_SaveInfo = null;
            }
            if (!partBuySave.boughtInfoPanel)
            {
                ecu_InfoPanel_SaveInfo = null;
            }

#if DEBUG
            if (!partBuySave.boughtAwdGearbox)
            {
                awd_gearbox_saveInfo = null;
            }
            if (!partBuySave.boughtAwdDifferential)
            {
                awd_differential_saveInfo = null;
            }
            if (!partBuySave.boughtAwdPropshaft)
            {
                awd_propshaft_saveInfo = null;
            }
#endif
            ecu_mod_absModule_Part = new ECU_MOD_ABSModule_Part(
                ecu_mod_ABSModule_SaveInfo,
                ecu_mod_ABSModule,
                satsuma,
                ecu_mod_ABSModule_Trigger,
                new Vector3(0.254f, -0.248f, -0.155f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_espModule_Part = new ECU_MOD_ESPModule_Part(
                ecu_mod_ESPModule_SaveInfo,
                ecu_mod_ESPModule,
                satsuma,
                ecu_mod_ESPModule_Trigger,
                new Vector3(0.288f, -0.248f, -0.0145f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_tcsModule_Part = new ECU_MOD_TCSModule_Part(
                ecu_mod_TCSModule_SaveInfo,
                ecu_mod_TCSModule,
                satsuma,
                ecu_mod_TCSModule_Trigger,
                new Vector3(0.342f, -0.246f, 0.115f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_cableHarness_Part = new ECU_MOD_CableHarness_Part(
                ecu_mod_CableHarness_SaveInfo,
                ecu_mod_CableHarness,
                satsuma,
                ecu_mod_CableHarness_Trigger,
                new Vector3(0.388f, -0.245f, -0.007f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_mountingPlate_Part = new ECU_MOD_MountingPlate_Part(
                ecu_mod_MountingPlate_SaveInfo,
                ecu_mod_MountingPlate,
                satsuma,
                ecu_mod_MountingPlate_Trigger,
                new Vector3(0.3115f, -0.27f, -0.0393f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_smartEngineModule_Part = new ECU_MOD_SmartEngineModule_Part(
                ecu_mod_SmartEngineModule_SaveInfo,
                ecu_mod_SmartEngineModule,
                satsuma,
                ecu_mod_SmartEngineModule_Trigger,
                new Vector3(0.2398f, -0.248f, 0.104f),
                new Quaternion(0, 0, 0, 0)
            );

            ecu_smartEngineModule_logic = ecu_mod_smartEngineModule_Part.rigidPart.AddComponent<ECU_SmartEngineModule_Logic>();

            ecu_mod_cruiseControlPanel_Part = new ECU_MOD_CruiseControlPanel_Part(
                    ecu_mod_CruiseControllPanel_SaveInfo,
                    ecu_mod_CruiseControlPanel,
                    GameObject.Find("dashboard(Clone)"),
                    ecu_mod_CruiseControlPanel_Trigger,
                    new Vector3(0.5f, -0.095f, 0.08f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(90, 0, 0)
                    }
                );
            ecu_CruiseControl_Logic = ecu_mod_cruiseControlPanel_Part.rigidPart.AddComponent<ECU_CruiseControl_Logic>();

            ecu_InfoPanel_Part = new ECU_InfoPanel_Part(
                    ecu_InfoPanel_SaveInfo,
                    ecu_InfoPanel,
                    GameObject.Find("dashboard(Clone)"),
                    ecu_InfoPanel_Trigger,
                    new Vector3(0.25f, -0.088f, -0.01f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 180, 180)
                    }
                );
            ecu_InfoPanel_Logic = ecu_InfoPanel_Part.rigidPart.AddComponent<ECU_InfoPanel_Logic>();

#if DEBUG
                awd_gearbox_part = new AWD_Gearbox_Part(
                    awd_gearbox_saveInfo,
                    awd_gearbox,
                    GameObject.Find("pivot_gearbox"),
                    awd_gearbox_trigger,
                    awd_gearbox_part_installLocation,
                    new Quaternion
                    {
                        eulerAngles = new Vector3(90, 0, 0)
                    }
                );
                awd_differential_part = new AWD_Differential_Part(
                    awd_differential_saveInfo,
                    awd_differential,
                    satsuma,
                    awd_differential_trigger,
                    awd_differential_part_installLocation,
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 0, 0)
                    }
                );
                awd_differential_part.rigidPart.layer = LayerMask.NameToLayer("Datsun");

                awd_propshaft_part = new AWD_PropShaft_Part(
                    awd_propshaft_saveInfo,
                    awd_propshaft,
                    satsuma,
                    awd_propshaft_trigger,
                    awd_propshaft_part_installLocation,
                    new Quaternion
                    {
                        eulerAngles = new Vector3(0, 180, 0)
                    }
                );
#endif
            partsList.Add(ecu_mod_absModule_Part);
            partsList.Add(ecu_mod_espModule_Part);
            partsList.Add(ecu_mod_tcsModule_Part);
            partsList.Add(ecu_mod_cableHarness_Part);
            partsList.Add(ecu_mod_mountingPlate_Part);
            partsList.Add(ecu_mod_smartEngineModule_Part);
            partsList.Add(ecu_mod_cruiseControlPanel_Part);
            partsList.Add(ecu_InfoPanel_Part);

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

            

            SortedList<String, Screws> screwListSave = ScrewablePart.LoadScrews(this, "ecu_mod_screwable_save.txt");
            ecu_mod_absModule_screwable = new ScrewablePart(screwListSave, this, ecu_mod_absModule_Part.rigidPart,
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
            ecu_mod_espModule_Part_screwable = new ScrewablePart(screwListSave, this, ecu_mod_espModule_Part.rigidPart,
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
            ecu_mod_tcsModule_Part_screwable = new ScrewablePart(screwListSave, this, ecu_mod_tcsModule_Part.rigidPart,
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
            ecu_mod_smartEngineModule_Part_screwable = new ScrewablePart(screwListSave, this, ecu_mod_smartEngineModule_Part.rigidPart,
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

            ecu_mod_mountingPlate_Part_screwable = new ScrewablePart(screwListSave, this, ecu_mod_mountingPlate_Part.rigidPart,
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
            ecu_infoPanel_screwable = new ScrewablePart(screwListSave, this, ecu_InfoPanel_Part.rigidPart,
                new Vector3[]{
                        new Vector3(0f, -0.025f, -0.067f),
                },
                new Vector3[]{
                        new Vector3(180, 0, 0),
                },
                new Vector3[]{
                        new Vector3(0.8f, 0.8f, 0.8f),
                }, 8, "screwable_screw2");




            if (GameObject.Find("Shop for mods") != null)
            {
                ModsShop.ShopItem shop;
                shop = GameObject.Find("Shop for mods").GetComponent<ModsShop.ShopItem>();
                //Create product
                ModsShop.ProductDetails absModuleProduct = new ModsShop.ProductDetails
                {
                    productName = "ABS Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_abs_module_productImage.png"),
                    productPrice = 800
                };
                if (!partBuySave.boughtABSModule)
                {
                    shop.Add(this, absModuleProduct, ModsShop.ShopType.Fleetari, PurchaseMadeABS, ecu_mod_absModule_Part.activePart);
                    ecu_mod_absModule_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails espModuleProduct = new ModsShop.ProductDetails
                {
                    productName = "ESP Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_esp_module_productImage.png"),

                    productPrice = 1200
                };
                if (!partBuySave.boughtESPModule)
                {
                    shop.Add(this, espModuleProduct, ModsShop.ShopType.Fleetari, PurchaseMadeESP, ecu_mod_espModule_Part.activePart);
                    ecu_mod_espModule_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails tcsModuleProduct = new ModsShop.ProductDetails
                {
                    productName = "TCS Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_tcs_module_productImage.png"),
                    productPrice = 1800
                };
                if (!partBuySave.boughtTCSModule)
                {
                    shop.Add(this, tcsModuleProduct, ModsShop.ShopType.Fleetari, PurchaseMadeTCS, ecu_mod_tcsModule_Part.activePart);
                    ecu_mod_tcsModule_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails cableHarnessProduct = new ModsShop.ProductDetails
                {
                    productName = "ECU Cable Harness",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_cable_harness_productImage.png"),
                    productPrice = 300
                };
                if (!partBuySave.boughtCableHarness)
                {
                    shop.Add(this, cableHarnessProduct, ModsShop.ShopType.Fleetari, PurchaseMadeCableHarness, ecu_mod_cableHarness_Part.activePart);
                    ecu_mod_cableHarness_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails mountingPlateProduct = new ModsShop.ProductDetails
                {
                    productName = "ECU Mounting Plate",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_mounting_plate_productImage.png"),
                    productPrice = 100
                };
                if (!partBuySave.boughtMountingPlate)
                {
                    shop.Add(this, mountingPlateProduct, ModsShop.ShopType.Fleetari, PurchaseMadeMountingPlate, ecu_mod_mountingPlate_Part.activePart);
                    ecu_mod_mountingPlate_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails smartEngineModuleProduct = new ModsShop.ProductDetails
                {
                    productName = "Smart Engine Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_smart_engine_module_productImage.png"),
                    productPrice = 4600
                };
                if (!partBuySave.boughtSmartEngineModule)
                {
                    shop.Add(this, smartEngineModuleProduct, ModsShop.ShopType.Fleetari, PurchageMadeSmartEngineModule, ecu_mod_smartEngineModule_Part.activePart);
                    ecu_mod_smartEngineModule_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails cruiseControlPanelProduct = new ModsShop.ProductDetails
                {
                    productName = "Cruise Control Panel with Controller",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_cruise_control_productImage.png"),
                    productPrice = 2000
                };
                if (!partBuySave.boughtCruiseControlPanel)
                {
                    shop.Add(this, cruiseControlPanelProduct, ModsShop.ShopType.Fleetari, PurchaseMadeCruiseControllPanel, ecu_mod_cruiseControlPanel_Part.activePart);
                    ecu_mod_cruiseControlPanel_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails ecu_InfoPanel_Product = new ModsShop.ProductDetails
                {
                    productName = "ECU Info Panel",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_info_panel_productImage.png"),
                    productPrice = 4000
                };
                if (!partBuySave.boughtInfoPanel)
                {
                    shop.Add(this, ecu_InfoPanel_Product, ModsShop.ShopType.Fleetari, PurchaseMadeInfoPanel, ecu_InfoPanel_Part.activePart);
                    ecu_InfoPanel_Part.activePart.SetActive(false);
                }

#if DEBUG
                ModsShop.ProductDetails awd_gearbox_product = new ModsShop.ProductDetails
                {
                    productName = "AWD Gearbox",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    //productIcon = assetBundle.LoadAsset<Sprite>("CruiseControlPanel_ProductImage.png"),
                    productPrice = 4000
                };
                if (!partBuySave.boughtAwdGearbox)
                {
                    shop.Add(this, awd_gearbox_product, ModsShop.ShopType.Fleetari, PurchaseMadeAWDGearbox, awd_gearbox_part.activePart);
                    awd_gearbox_part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails awd_differential_product = new ModsShop.ProductDetails
                {
                    productName = "AWD Differential",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    //productIcon = assetBundle.LoadAsset<Sprite>("CruiseControlPanel_ProductImage.png"),
                    productPrice = 4000
                };
                if (!partBuySave.boughtAwdDifferential)
                {
                    shop.Add(this, awd_differential_product, ModsShop.ShopType.Fleetari, PurchaseMadeAWDDifferential, awd_differential_part.activePart);
                    awd_differential_part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails awd_propshaft_product = new ModsShop.ProductDetails
                {
                    productName = "AWD Propshaft",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    //productIcon = assetBundle.LoadAsset<Sprite>("CruiseControlPanel_ProductImage.png"),
                    productPrice = 4000
                };
                if (!partBuySave.boughtAwdPropshaft)
                {
                    shop.Add(this, awd_propshaft_product, ModsShop.ShopType.Fleetari, PurchaseMadeAWDPropshaft, awd_propshaft_part.activePart);
                    awd_propshaft_part.activePart.SetActive(false);
                }
#endif
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
            UnityEngine.Object.Destroy(ecu_mod_ABSModule);
            UnityEngine.Object.Destroy(ecu_mod_ESPModule);
            UnityEngine.Object.Destroy(ecu_mod_TCSModule);
            UnityEngine.Object.Destroy(ecu_mod_CableHarness);
            UnityEngine.Object.Destroy(ecu_mod_MountingPlate);
            UnityEngine.Object.Destroy(ecu_mod_SmartEngineModule);
            UnityEngine.Object.Destroy(ecu_mod_CruiseControlPanel);
            UnityEngine.Object.Destroy(ecu_InfoPanel);

#if DEBUG
            UnityEngine.Object.Destroy(awd_gearbox);
#endif
            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");
            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " finished loading");
        }

        internal bool GetInfoPanelScrewed()
        {
            return ecu_infoPanel_screwable.partFixed;
        }

        public void PurchaseMadeABS(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position =ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtABSModule = true;
        }
        public void PurchaseMadeESP(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position =ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtESPModule = true;
        }
        public void PurchaseMadeTCS(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtTCSModule = true;
        }
        public void PurchaseMadeCableHarness(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtCableHarness = true;
        }
        public void PurchaseMadeMountingPlate(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtMountingPlate = true;
        }
        private void PurchageMadeSmartEngineModule(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtSmartEngineModule = true;
        }

        private void PurchaseMadeCruiseControllPanel(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtCruiseControlPanel = true;
        }

        private void PurchaseMadeInfoPanel(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtInfoPanel = true;
        }

        private void PurchaseMadeAWDGearbox(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtAwdGearbox = true;
        }
        private void PurchaseMadeAWDDifferential(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtAwdDifferential = true;
        }
        private void PurchaseMadeAWDPropshaft(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            partBuySave.boughtAwdPropshaft = true;
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
#if DEBUG
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, awd_gearbox_part.getSaveInfo(), awd_gearbox_saveFile);
#endif
                ScrewablePart.SaveScrews(this, new ScrewablePart[]
                {
                    ecu_mod_absModule_screwable,
                    ecu_mod_espModule_Part_screwable,
                    ecu_mod_tcsModule_Part_screwable,
                    ecu_mod_smartEngineModule_Part_screwable,
                    ecu_mod_mountingPlate_Part_screwable,
                    ecu_infoPanel_screwable,
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
                GUI.Label(new Rect(20, 540, 500, 100), "mounting plate screwed in: " + ecu_mod_mountingPlate_Part_screwable.partFixed);
                GUI.Label(new Rect(20, 560, 500, 100), "smart engine module installed: " + ecu_mod_smartEngineModule_Part.installed);
                GUI.Label(new Rect(20, 580, 500, 100), "smart engine module screwed in: " + ecu_mod_smartEngineModule_Part_screwable.partFixed);
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
            /*
            //CheckPartsInstalledTrigger();
            if (hasPower)
            {
                if (ecu_mod_cableHarness_Part.installed && ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed && ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed)
                {
                    if(playerCurrentVehicle.Value == "Satsuma")
                    {
                        if (stage2revModuleEnabled)
                        {
                            HandleStage2RevLimiterModule();
                        }

                        if (alsModuleEnabled)
                        {
                            HandleALSModuleLogic();
                        }
                    }
                }
            }
            */
        }
        private static bool hasPower
        {
            get
            {
                GameObject carElectrics = GameObject.Find("SATSUMA(557kg, 248)/Electricity");
                PlayMakerFSM carElectricsPower = PlayMakerFSM.FindFsmOnGameObject(carElectrics, "Power");
                return carElectricsPower.FsmVariables.FindFsmBool("ElectricsOK").Value;
            }
        }

        public Keybind ecu_panel_ArrowUp = new Keybind("ecu_panel_ArrowUp", "Arrow Up", KeyCode.Keypad8);
        public Keybind ecu_panel_ArrowDown = new Keybind("ecu_panel_ArrowDown", "Arrow Down", KeyCode.Keypad2);
        public Keybind ecu_panel_Circle = new Keybind("ecu_panel_Circle", "Circle", KeyCode.KeypadEnter);
        public Keybind ecu_panel_Cross = new Keybind("ecu_panel_Cross", "Cross", KeyCode.KeypadPeriod);
        public Keybind ecu_panel_Plus = new Keybind("ecu_panel_Plus", "Plus", KeyCode.KeypadPlus);
        public Keybind ecu_panel_Minus = new Keybind("ecu_panel_Minus", "Minus", KeyCode.KeypadMinus);
        public static void ToggleABS()
        {
            if (ecu_mod_absModule_Part.installed & ecu_mod_absModule_screwable.partFixed)
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
            if (ecu_mod_espModule_Part.installed && ecu_mod_espModule_Part_screwable.partFixed)
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
            if (ecu_mod_tcsModule_Part.installed && ecu_mod_tcsModule_Part_screwable.partFixed)
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
                ModConsole.Error(ex.StackTrace);
            }
        }

        private static void ToggleSixGears()
        {
            if (toggleSixGears.Value is bool value)
            {
                if (value == true)
                {
                    sixGearsEnabled = true;
                    satsumaDriveTrain.gearRatios = newGearRatio;
                }
                else if (value == false)
                {
                    sixGearsEnabled = false;
                    satsumaDriveTrain.gearRatios = originalGearRatios;
                }
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
            return (ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed);
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


        public ECU_MOD_ABSModule_Part GetABSModule_Part()
        {
            return DonnerTech_ECU_Mod.ecu_mod_absModule_Part;
        }
        public ECU_MOD_ESPModule_Part GetESPModule_Part()
        {
            return DonnerTech_ECU_Mod.ecu_mod_espModule_Part;
        }
        public ECU_MOD_TCSModule_Part GetTCSModule_Part()
        {
            return DonnerTech_ECU_Mod.ecu_mod_tcsModule_Part;
        }
        public ECU_MOD_SmartEngineModule_Part GetSmartEngineModule_Part()
        {
            return DonnerTech_ECU_Mod.ecu_mod_smartEngineModule_Part;
        }
        public ECU_MOD_CableHarness_Part GetCableHarness_Part()
        {
            return DonnerTech_ECU_Mod.ecu_mod_cableHarness_Part;
        }
        public ECU_MOD_MountingPlate_Part GetMountingPlate_Part()
        {
            return DonnerTech_ECU_Mod.ecu_mod_mountingPlate_Part;
        }

        public ScrewablePart GetABSModule_Screwable()
        {
            return DonnerTech_ECU_Mod.ecu_mod_absModule_screwable;
        }
        public ScrewablePart GetESPModule_Screwable()
        {
            return DonnerTech_ECU_Mod.ecu_mod_espModule_Part_screwable;
        }
        public ScrewablePart GetTCSModule_Screwable()
        {
            return DonnerTech_ECU_Mod.ecu_mod_tcsModule_Part_screwable;
        }
        public ScrewablePart GetSmartEngineModule_Screwable()
        {
            return DonnerTech_ECU_Mod.ecu_mod_smartEngineModule_Part_screwable;
        }
        public ScrewablePart GetMountingPlate_Screwable()
        {
            return DonnerTech_ECU_Mod.ecu_mod_mountingPlate_Part_screwable;
        }

    }
}
