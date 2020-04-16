using HutongGames.PlayMaker;
using ModApi.Attachable;
using ModApi;
using MSCLoader;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;
using ModsShop;
using System.Security.Cryptography;
using ScrewablePartAPI;
using System.Collections.Generic;

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
         *           
         */

        /*  Changelog (v1.2.1)
         *  small bug fix.
         *  added cruise control debug information -> button is in Mod Setting. Everything that if false HAS TO BE TRUE!
         *  
         */
        /* BUGS
         * 
         */

        public override string ID => "DonnerTech_ECU_Mod"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing ECUs"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.2.1"; //Version
        public override bool UseAssetsFolder => true;

        private static bool cruiseControlDebugEnabled = false;



        AssetBundle assetBundle;
        private static GameObject ecu_mod_ABSModule = new GameObject();
        private static GameObject ecu_mod_ESPModule = new GameObject();
        private static GameObject ecu_mod_TCSModule = new GameObject();
        private static GameObject ecu_mod_CableHarness = new GameObject();
        private static GameObject ecu_mod_MountingPlate = new GameObject();
        private static GameObject ecu_mod_ControllPanel = new GameObject();
        private static GameObject ecu_mod_SmartEngineModule = new GameObject();
        private static GameObject ecu_mod_CruiseControlPanel = new GameObject();
        private TextMesh cruiseControlText;
        private static PartBuySave partBuySave;
        private Trigger ecu_mod_ABSModule_Trigger;
        private Trigger ecu_mod_ESPModule_Trigger;
        private Trigger ecu_mod_TCSModule_Trigger;
        private Trigger ecu_mod_CableHarness_Trigger;
        private Trigger ecu_mod_MountingPlate_Trigger;
        private Trigger ecu_mod_ControllPanel_Trigger;
        private Trigger ecu_mod_SmartEngineModule_Trigger;
        private Trigger ecu_mod_CruiseControlPanel_Trigger;

        private static Settings toggleSixGears = new Settings("toggleSixGears", "Enable/Disable SixGears Mod", false, new Action(ToggleSixGears));
        private static Settings toggleAWD = new Settings("toggleAWD", "Toggle All Wheel Drive", false, new Action(ToggleAWD));


        private Color absLightColor;
        private Color espLightColor;
        private Color tcsLightColor;
        private Color alsLightColor;
        private Color stage2RevLimiterLightColor;


        public static bool absModuleEnabled = false;
        public static bool espModuleEnabled = false;
        public static bool tcsModuleEnabled = false;
        public static bool alsModuleEnabled = false;
        public static bool alsSwitchEnabled = false;
        public static bool stage2revModuleEnabled = false;
        public static bool stage2revSwitchEnabled = false;


        private static string modAssetsFolder;
        private RaycastHit hit;
       
        private AudioSource backFire;
        private ModAudio backfire_once = new ModAudio();
        private float timeSinceLastBackFire;

        private static ECU_MOD_ABSModule_Part ecu_mod_absModule_Part;
        private static ECU_MOD_ESPModule_Part ecu_mod_espModule_Part;
        private static ECU_MOD_TCSModule_Part ecu_mod_tcsModule_Part;
        private static ECU_MOD_CableHarness_Part ecu_mod_cableHarness_Part;
        private static ECU_MOD_MountingPlate_Part ecu_mod_mountingPlate_Part;
        private static ECU_MOD_ControllPanel_Part ecu_mod_controllPanel_Part;
        private static ECU_MOD_SmartEngineModule_Part ecu_mod_smartEngineModule_Part;
        private static ECU_MOD_CruiseControlPanel_Part ecu_mod_cruiseControlPanel_Part;

        public static ScrewablePart ecu_mod_absModule_screwable;
        public static ScrewablePart ecu_mod_espModule_Part_screwable;
        public static ScrewablePart ecu_mod_tcsModule_Part_screwable;
        public static ScrewablePart ecu_mod_smartEngineModule_Part_screwable;
        public static ScrewablePart ecu_mod_mountingPlate_Part_screwable;
        //public static ScrewablePart ecu_mod_controllPanel_Part_screwable;



        private static GameObject satsuma;
        private GameObject carChoke;
        private FsmFloat chokeValue;
        private static Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;
        PlayMakerFSM chokeFSM;
        //private FsmFloat mixture;

        private const string ecu_mod_ABSModule_SaveFile = "ecu_mod_ABSModule_partSave.txt";
        private const string ecu_mod_ESPModule_SaveFile = "ecu_mod_ESPModule_partSave.txt";
        private const string ecu_mod_TCSModule_SaveFile = "ecu_mod_TCSModule_partSave.txt";
        private const string ecu_mod_CableHarness_SaveFile = "ecu_mod_CableHarness_partSave.txt";
        private const string ecu_mod_MountingPlate_SaveFile = "ecu_mod_MountingPlate_partSave.txt";
        private const string ecu_mod_ControllPanel_SaveFile = "ecu_mod_ControllPanel_partSave.txt";
        private const string ecu_mod_ModShop_SaveFile = "ecu_mod_ModShop_SaveFile.txt";
        private const string ecu_mod_SmartEngineModule_SaveFile = "ecu_mod_SmartEngineModule_partSave.txt";
        private const string ecu_mod_cruiseControlPanel_SaveFile = "ecu_mod_CruiseControlPanel_partSave.txt";

        private Settings resetPosSetting = new Settings("resetPos", "Reset uninstalled parts location", new Action(DonnerTech_ECU_Mod.PosReset));
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
            SaveLoad.SerializeSaveFile<PartSaveInfo>(this, null, ecu_mod_ControllPanel_SaveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_mod_ModShop_SaveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_mod_SmartEngineModule_SaveFile);
            SaveLoad.SerializeSaveFile<PartBuySave>(this, null, ecu_mod_cruiseControlPanel_SaveFile);
        }
        public override void OnLoad()
        {
            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " started loaded");

            modAssetsFolder = ModLoader.GetModAssetsFolder(this);
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
            satsumaAxles = satsuma.GetComponent<Axles>();
            originalGearRatios = satsumaDriveTrain.gearRatios;

            ToggleSixGears();
            ToggleAWD();

            //mixture = satsuma.transform.GetChild(13).GetChild(1).GetChild(3).gameObject.GetComponents<PlayMakerFSM>()[1].FsmVariables.FloatVariables[16];



            assetBundle = LoadAssets.LoadBundle(this, "ecu-mod.unity3d");
            DonnerTech_ECU_Mod.ecu_mod_ABSModule = (assetBundle.LoadAsset("ECU-Mod_ABS-Module.prefab") as GameObject);
            DonnerTech_ECU_Mod.ecu_mod_ESPModule = (assetBundle.LoadAsset("ECU-Mod_ESP-Module.prefab") as GameObject);
            DonnerTech_ECU_Mod.ecu_mod_TCSModule = (assetBundle.LoadAsset("ECU-Mod_TCS-Module.prefab") as GameObject);
            DonnerTech_ECU_Mod.ecu_mod_CableHarness = (assetBundle.LoadAsset("ECU-Mod_Cable-Harness_v2.prefab") as GameObject);
            DonnerTech_ECU_Mod.ecu_mod_MountingPlate = (assetBundle.LoadAsset("ECU-Mod_Mounting-Plate_v2.prefab") as GameObject);
            DonnerTech_ECU_Mod.ecu_mod_ControllPanel = (assetBundle.LoadAsset("ECU-Mod_Controll-Panel_v2.prefab") as GameObject);
            DonnerTech_ECU_Mod.ecu_mod_SmartEngineModule = (assetBundle.LoadAsset("ECU-Mod_SmartEngine-Module.prefab") as GameObject);
            ecu_mod_CruiseControlPanel = (assetBundle.LoadAsset("ECU-Mod_CruiseControl-Panel.prefab") as GameObject);

            

            DonnerTech_ECU_Mod.ecu_mod_ABSModule.name = "ABS Module";
            DonnerTech_ECU_Mod.ecu_mod_ESPModule.name = "ESP Module";
            DonnerTech_ECU_Mod.ecu_mod_TCSModule.name = "TCS Module";
            DonnerTech_ECU_Mod.ecu_mod_CableHarness.name = "ECU Cable Harness";
            DonnerTech_ECU_Mod.ecu_mod_MountingPlate.name = "ECU Mounting Plate";
            DonnerTech_ECU_Mod.ecu_mod_ControllPanel.name = "ECU Control Panel";
            DonnerTech_ECU_Mod.ecu_mod_SmartEngineModule.name = "Smart Engine ECU";
            ecu_mod_CruiseControlPanel.name = "Cruise Control Panel";

            DonnerTech_ECU_Mod.ecu_mod_ABSModule.tag = "PART";
            DonnerTech_ECU_Mod.ecu_mod_ESPModule.tag = "PART";
            DonnerTech_ECU_Mod.ecu_mod_TCSModule.tag = "PART";
            DonnerTech_ECU_Mod.ecu_mod_CableHarness.tag = "PART";
            DonnerTech_ECU_Mod.ecu_mod_MountingPlate.tag = "PART";
            DonnerTech_ECU_Mod.ecu_mod_ControllPanel.tag = "PART";
            DonnerTech_ECU_Mod.ecu_mod_SmartEngineModule.tag = "PART";
            ecu_mod_CruiseControlPanel.tag = "PART";

            DonnerTech_ECU_Mod.ecu_mod_ABSModule.layer = LayerMask.NameToLayer("Parts");
            DonnerTech_ECU_Mod.ecu_mod_ESPModule.layer = LayerMask.NameToLayer("Parts");
            DonnerTech_ECU_Mod.ecu_mod_TCSModule.layer = LayerMask.NameToLayer("Parts");
            DonnerTech_ECU_Mod.ecu_mod_CableHarness.layer = LayerMask.NameToLayer("Parts");
            DonnerTech_ECU_Mod.ecu_mod_MountingPlate.layer = LayerMask.NameToLayer("Parts");
            DonnerTech_ECU_Mod.ecu_mod_ControllPanel.layer = LayerMask.NameToLayer("Parts");
            DonnerTech_ECU_Mod.ecu_mod_SmartEngineModule.layer = LayerMask.NameToLayer("Parts");
            ecu_mod_CruiseControlPanel.layer = LayerMask.NameToLayer("Parts");

            ecu_mod_SmartEngineModule.AddComponent<ModCommunication>();



            ecu_mod_ABSModule_Trigger = new Trigger("ECU_MOD_ABSModule_Trigger", satsuma, new Vector3(0.254f, -0.28f, -0.155f), new Quaternion(0, 0, 0, 0), new Vector3(0.14f, 0.02f, 0.125f), false);
            ecu_mod_ESPModule_Trigger = new Trigger("ECU_MOD_ESPModule_Trigger", satsuma, new Vector3(0.288f, -0.28f, -0.0145f), new Quaternion(0, 0, 0, 0), new Vector3(0.21f, 0.02f, 0.125f), false);
            ecu_mod_TCSModule_Trigger = new Trigger("ECU_MOD_TCSModule_Trigger", satsuma, new Vector3(0.342f, -0.28f, 0.115f), new Quaternion(0, 0, 0, 0), new Vector3(0.104f, 0.02f, 0.104f), false);
            ecu_mod_CableHarness_Trigger = new Trigger("ECU_MOD_CableHarness_Trigger", satsuma, new Vector3(0.423f, -0.28f, -0.0384f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.02f, 0.3f), false);
            ecu_mod_MountingPlate_Trigger = new Trigger("ECU_MOD_MountingPlate_Trigger", satsuma, new Vector3(0.31f, -0.28f, -0.038f), new Quaternion(0, 0, 0, 0), new Vector3(0.32f, 0.02f, 0.45f), false);
            ecu_mod_ControllPanel_Trigger = new Trigger("ECU_MOD_ControllPanel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.4f, -0.042f, -0.12f), new Quaternion(0, 0, 0, 0), new Vector3(0.2f, 0.02f, 0.08f), false);
            ecu_mod_SmartEngineModule_Trigger = new Trigger("ECU_MOD_SmartEngineModule_Trigger", satsuma, new Vector3(0.2398f, -0.28f, 0.104f), new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.02f, 0.13f), false);
            ecu_mod_CruiseControlPanel_Trigger = new Trigger("ECU_MOD_CruiseControllPanel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.46f, -0.095f, 0.08f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false);

            PartSaveInfo ecu_mod_ABSModule_SaveInfo = null;
            PartSaveInfo ecu_mod_ESPModule_SaveInfo = null;
            PartSaveInfo ecu_mod_TCSModule_SaveInfo = null;
            PartSaveInfo ecu_mod_CableHarness_SaveInfo = null;
            PartSaveInfo ecu_mod_MountingPlate_SaveInfo = null;
            PartSaveInfo ecu_mod_ControllPanel_SaveInfo = null;
            PartSaveInfo ecu_mod_SmartEngineModule_SaveInfo = null;
            PartSaveInfo ecu_mod_CruiseControllPanel_SaveInfo = null;

            ecu_mod_ABSModule_SaveInfo = this.loadSaveData(ecu_mod_ABSModule_SaveFile);
            ecu_mod_ESPModule_SaveInfo = this.loadSaveData(ecu_mod_ESPModule_SaveFile);
            ecu_mod_TCSModule_SaveInfo = this.loadSaveData(ecu_mod_TCSModule_SaveFile);
            ecu_mod_CableHarness_SaveInfo = this.loadSaveData(ecu_mod_CableHarness_SaveFile);
            ecu_mod_MountingPlate_SaveInfo = this.loadSaveData(ecu_mod_MountingPlate_SaveFile);
            ecu_mod_ControllPanel_SaveInfo = this.loadSaveData(ecu_mod_ControllPanel_SaveFile);
            ecu_mod_SmartEngineModule_SaveInfo = this.loadSaveData(ecu_mod_SmartEngineModule_SaveFile);
            ecu_mod_CruiseControllPanel_SaveInfo = this.loadSaveData(ecu_mod_cruiseControlPanel_SaveFile);
            try
            {
                DonnerTech_ECU_Mod.partBuySave = SaveLoad.DeserializeSaveFile<PartBuySave>(this, ecu_mod_ModShop_SaveFile);
            }
            catch
            {
            }
            if (DonnerTech_ECU_Mod.partBuySave == null)
            {
                DonnerTech_ECU_Mod.partBuySave = new PartBuySave
                {
                    boughtABSModule = false,
                    boughtESPModule = false,
                    boughtTCSModule = false,
                    boughtCableHarness = false,
                    boughtMountingPlate = false,
                    boughtControllPanel = false,
                    boughtSmartEngineModule = false,
                    boughtCruiseControlPanel = false
                };
            }
            if (!DonnerTech_ECU_Mod.partBuySave.boughtABSModule)
            {
                ecu_mod_ABSModule_SaveInfo = null;
            }
            if (!DonnerTech_ECU_Mod.partBuySave.boughtESPModule)
            {
                ecu_mod_ESPModule_SaveInfo = null;
            }
            if (!DonnerTech_ECU_Mod.partBuySave.boughtTCSModule)
            {
                ecu_mod_TCSModule_SaveInfo = null;
            }
            if (!DonnerTech_ECU_Mod.partBuySave.boughtCableHarness)
            {
                ecu_mod_CableHarness_SaveInfo = null;
            }
            if (!DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate)
            {
                ecu_mod_MountingPlate_SaveInfo = null;
            }
            if (!DonnerTech_ECU_Mod.partBuySave.boughtControllPanel)
            {
                ecu_mod_ControllPanel_SaveInfo = null;
            }
            if (!DonnerTech_ECU_Mod.partBuySave.boughtSmartEngineModule)
            {
                ecu_mod_SmartEngineModule_SaveInfo = null;
            }
            if (!partBuySave.boughtCruiseControlPanel)
            {
                ecu_mod_CruiseControllPanel_SaveInfo = null;
            }

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
            ecu_mod_controllPanel_Part = new ECU_MOD_ControllPanel_Part(
                ecu_mod_ControllPanel_SaveInfo,
                ecu_mod_ControllPanel,
                GameObject.Find("dashboard(Clone)"),
                ecu_mod_ControllPanel_Trigger,
                new Vector3(0.4f, -0.042f, -0.12f),
                new Quaternion
                {
                    eulerAngles = new Vector3(90, 0, 0)
                }
            );
            ecu_mod_smartEngineModule_Part = new ECU_MOD_SmartEngineModule_Part(
                ecu_mod_SmartEngineModule_SaveInfo,
                ecu_mod_SmartEngineModule,
                satsuma,
                ecu_mod_SmartEngineModule_Trigger,
                new Vector3(0.2398f, -0.248f, 0.104f),
                new Quaternion(0, 0, 0, 0)
            );
            ecu_mod_cruiseControlPanel_Part = new ECU_MOD_CruiseControlPanel_Part(
                    ecu_mod_CruiseControllPanel_SaveInfo,
                    ecu_mod_CruiseControlPanel,
                    GameObject.Find("dashboard(Clone)"),
                    ecu_mod_CruiseControlPanel_Trigger,
                    new Vector3(0.46f, -0.095f, 0.08f),
                    new Quaternion
                    {
                        eulerAngles = new Vector3(90, 0, 0)
                    }
                );
            cruiseControlText = ecu_mod_cruiseControlPanel_Part.rigidPart.GetComponentInChildren<TextMesh>();

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
            /*
            ecu_mod_controllPanel_Part_screwable = new ScrewablePart(screwListSave, this, ecu_mod_absModule_Part.rigidPart,
                new Vector3[]{

                },
                new Vector3[]{

                },
                new Vector3[]{

                }, 8, "screwable_screw2");
            */


            cruiseControlText = GameObject.Find("ECU-Mod_CruiseControlPanel_Set_Speed_Text").GetComponent<TextMesh>();

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
                    productIcon = assetBundle.LoadAsset<Sprite>("ABSModule_ProductImage.png"),
                    productPrice = 800
                };
                if (!DonnerTech_ECU_Mod.partBuySave.boughtABSModule)
                {
                    shop.Add(this, absModuleProduct, ModsShop.ShopType.Fleetari, PurchaseMadeABS, ecu_mod_absModule_Part.activePart);
                    ecu_mod_absModule_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails espModuleProduct = new ModsShop.ProductDetails
                {
                    productName = "ESP Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ESPModule_ProductImage.png"),

                    productPrice = 1200
                };
                if (!DonnerTech_ECU_Mod.partBuySave.boughtESPModule)
                {
                    shop.Add(this, espModuleProduct, ModsShop.ShopType.Fleetari, PurchaseMadeESP, ecu_mod_espModule_Part.activePart);
                    ecu_mod_espModule_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails tcsModuleProduct = new ModsShop.ProductDetails
                {
                    productName = "TCS Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("TCSModule_ProductImage.png"),
                    productPrice = 1800
                };
                if (!DonnerTech_ECU_Mod.partBuySave.boughtTCSModule)
                {
                    shop.Add(this, tcsModuleProduct, ModsShop.ShopType.Fleetari, PurchaseMadeTCS, ecu_mod_tcsModule_Part.activePart);
                    ecu_mod_tcsModule_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails cableHarnessProduct = new ModsShop.ProductDetails
                {
                    productName = "ECU Cable Harness",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("CableHarness_ProductImage.png"),
                    productPrice = 300
                };
                if (!DonnerTech_ECU_Mod.partBuySave.boughtCableHarness)
                {
                    shop.Add(this, cableHarnessProduct, ModsShop.ShopType.Fleetari, PurchaseMadeCableHarness, ecu_mod_cableHarness_Part.activePart);
                    ecu_mod_cableHarness_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails mountingPlateProduct = new ModsShop.ProductDetails
                {
                    productName = "ECU Mounting Plate",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("MountingPlate_ProductImage.png"),
                    productPrice = 100
                };
                if (!DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate)
                {
                    shop.Add(this, mountingPlateProduct, ModsShop.ShopType.Fleetari, PurchaseMadeMountingPlate, ecu_mod_mountingPlate_Part.activePart);
                    ecu_mod_mountingPlate_Part.activePart.SetActive(false);
                }

                ProductDetails controllPanelProduct = new ModsShop.ProductDetails
                {
                    productName = "ECU Controll Panel",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ControllPanel_ProductImage.png"),
                    productPrice = 300
                };
                if (!DonnerTech_ECU_Mod.partBuySave.boughtControllPanel)
                {
                    shop.Add(this, controllPanelProduct, ModsShop.ShopType.Fleetari, PurchaseMadeControllPanel, ecu_mod_controllPanel_Part.activePart);
                    ecu_mod_controllPanel_Part.activePart.SetActive(false);
                }
                ModsShop.ProductDetails smartEngineModuleProduct = new ModsShop.ProductDetails
                {
                    productName = "Smart Engine Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("SmartEngineControllModule_ProductImage.png"),
                    productPrice = 4600
                };
                if (!DonnerTech_ECU_Mod.partBuySave.boughtSmartEngineModule)
                {
                    shop.Add(this, smartEngineModuleProduct, ModsShop.ShopType.Fleetari, PurchageMadeSmartEngineModule, ecu_mod_smartEngineModule_Part.activePart);
                    ecu_mod_smartEngineModule_Part.activePart.SetActive(false);
                }

                ModsShop.ProductDetails cruiseControlPanelProduct = new ModsShop.ProductDetails
                {
                    productName = "Cruise Control Panel with Controller",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("CruiseControlPanel_ProductImage.png"),
                    productPrice = 2000
                };
                if (!DonnerTech_ECU_Mod.partBuySave.boughtCruiseControlPanel)
                {
                    shop.Add(this, cruiseControlPanelProduct, ModsShop.ShopType.Fleetari, PurchaseMadeCruiseControllPanel, ecu_mod_cruiseControlPanel_Part.activePart);
                    ecu_mod_cruiseControlPanel_Part.activePart.SetActive(false);
                }
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
            UnityEngine.Object.Destroy(DonnerTech_ECU_Mod.ecu_mod_ABSModule);
            UnityEngine.Object.Destroy(DonnerTech_ECU_Mod.ecu_mod_ABSModule);
            UnityEngine.Object.Destroy(DonnerTech_ECU_Mod.ecu_mod_ESPModule);
            UnityEngine.Object.Destroy(DonnerTech_ECU_Mod.ecu_mod_TCSModule);
            UnityEngine.Object.Destroy(DonnerTech_ECU_Mod.ecu_mod_CableHarness);
            UnityEngine.Object.Destroy(DonnerTech_ECU_Mod.ecu_mod_MountingPlate);
            UnityEngine.Object.Destroy(DonnerTech_ECU_Mod.ecu_mod_ControllPanel);
            UnityEngine.Object.Destroy(DonnerTech_ECU_Mod.ecu_mod_SmartEngineModule);
            UnityEngine.Object.Destroy(ecu_mod_CruiseControlPanel);

            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " loaded");
        }

        public void PurchaseMadeABS(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position =ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtABSModule = true;
        }
        public void PurchaseMadeESP(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position =ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtESPModule = true;
        }
        public void PurchaseMadeTCS(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtTCSModule = true;
        }
        public void PurchaseMadeCableHarness(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtCableHarness = true;
        }
        public void PurchaseMadeMountingPlate(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate = true;
        }
        public void PurchaseMadeControllPanel(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtControllPanel = true;
        }
        private void PurchageMadeSmartEngineModule(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtSmartEngineModule = true;
        }

        private void PurchaseMadeCruiseControllPanel(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtCruiseControlPanel = true;
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
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, DonnerTech_ECU_Mod.ecu_mod_absModule_Part.getSaveInfo(), ecu_mod_ABSModule_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, DonnerTech_ECU_Mod.ecu_mod_espModule_Part.getSaveInfo(), ecu_mod_ESPModule_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, DonnerTech_ECU_Mod.ecu_mod_tcsModule_Part.getSaveInfo(), ecu_mod_TCSModule_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, DonnerTech_ECU_Mod.ecu_mod_cableHarness_Part.getSaveInfo(), ecu_mod_CableHarness_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, DonnerTech_ECU_Mod.ecu_mod_mountingPlate_Part.getSaveInfo(), ecu_mod_MountingPlate_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, DonnerTech_ECU_Mod.ecu_mod_controllPanel_Part.getSaveInfo(), ecu_mod_ControllPanel_SaveFile);
                SaveLoad.SerializeSaveFile<PartBuySave>(this, DonnerTech_ECU_Mod.partBuySave, ecu_mod_ModShop_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, DonnerTech_ECU_Mod.ecu_mod_smartEngineModule_Part.getSaveInfo(), ecu_mod_SmartEngineModule_SaveFile);
                SaveLoad.SerializeSaveFile<PartSaveInfo>(this, DonnerTech_ECU_Mod.ecu_mod_cruiseControlPanel_Part.getSaveInfo(), ecu_mod_cruiseControlPanel_SaveFile);

                ScrewablePart.SaveScrews(this, new ScrewablePart[]
                {
                    ecu_mod_absModule_screwable,
                    ecu_mod_espModule_Part_screwable,
                    ecu_mod_tcsModule_Part_screwable,
                    ecu_mod_smartEngineModule_Part_screwable,
                    ecu_mod_mountingPlate_Part_screwable,
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
                GUI.Label(new Rect(20, 240, 500, 100), "------------------------------------");
                GUI.Label(new Rect(20, 260, 500, 100), "true = correct value for cruise control to work");
                GUI.Label(new Rect(20, 280, 500, 100), "false = conition needed to have cruise control working");
                GUI.Label(new Rect(20, 300, 500, 100), "Gear not R: " + (satsumaDriveTrain.gear != 0));
                GUI.Label(new Rect(20, 320, 500, 100), "cruise control panel installed: " + ecu_mod_cruiseControlPanel_Part.installed);
                GUI.Label(new Rect(20, 340, 500, 100), "cruise control enabled: " + cruiseControlModuleEnabled);
                GUI.Label(new Rect(20, 360, 500, 100), "mounting plate installed: " + ecu_mod_mountingPlate_Part.installed);
                GUI.Label(new Rect(20, 380, 500, 100), "mounting plate screwed in: " + ecu_mod_mountingPlate_Part_screwable.partFixed);
                GUI.Label(new Rect(20, 400, 500, 100), "smart engine module installed: " + ecu_mod_smartEngineModule_Part.installed);
                GUI.Label(new Rect(20, 420, 500, 100), "smart engine module screwed in: " + ecu_mod_smartEngineModule_Part_screwable.partFixed);
                GUI.Label(new Rect(20, 440, 500, 100), "not on throttle: " + (satsumaCarController.throttleInput <= 0f));
                GUI.Label(new Rect(20, 460, 500, 100), "speed above 20km/h: " + (satsumaDriveTrain.differentialSpeed >= 20f));
                GUI.Label(new Rect(20, 480, 500, 100), "brake not pressed: " + (satsumaCarController.brakeInput <= 0f));
                GUI.Label(new Rect(20, 500, 500, 100), "clutch not pressed: " + (satsumaCarController.clutchInput <= 0f));
                GUI.Label(new Rect(20, 520, 500, 100), "handbrake not pressed: " + (satsumaCarController.handbrakeInput <= 0f));
                GUI.Label(new Rect(20, 540, 500, 100), "set cruise control speed: " + setCruiseControlSpeed);
                GUI.Label(new Rect(20, 560, 500, 100), "car electricity on: " + hasPower);
                GUI.Label(new Rect(20, 580, 500, 100), "------------------------------------");
            }

            if (satsumaDriveTrain.gear != 0 && ecu_mod_cruiseControlPanel_Part.installed && cruiseControlModuleEnabled && ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed && ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed && satsumaCarController.throttleInput <= 0f)
            {
                float valueToThrottle = 0f;
                if (satsumaDriveTrain.differentialSpeed <= setCruiseControlSpeed)
                {
                    valueToThrottle = (satsumaDriveTrain.differentialSpeed - (setCruiseControlSpeed * 2.2f)) / ((-setCruiseControlSpeed * 2.2f));
                }
                else
                {
                    valueToThrottle = 0f;
                }
                satsumaCarController.throttle = valueToThrottle;
                if (satsumaDriveTrain.differentialSpeed < 19f || satsumaCarController.brakeInput > 0f || satsumaCarController.clutchInput > 0f || satsumaCarController.handbrakeInput > 0f)
                {

                    ResetCruiseControl();
                }
            }
            else if (cruiseControlModuleEnabled && satsumaCarController.throttleInput <= 0f)
            {
                ResetCruiseControl();
                setCruiseControlSpeed = 0;

            }

        }

        private void SetCruiseControlSpeedText(string toSet)
        {
            cruiseControlText.text = toSet;
        }
        private void SetCruiseControlSpeedTextColor(Color colorToSet)
        {
            cruiseControlText.color = colorToSet;
        }


        public override void Update()
        {
            ecu_mod_absModule_screwable.DetectScrewing();
            ecu_mod_espModule_Part_screwable.DetectScrewing();
            ecu_mod_tcsModule_Part_screwable.DetectScrewing();
            ecu_mod_smartEngineModule_Part_screwable.DetectScrewing();
            ecu_mod_mountingPlate_Part_screwable.DetectScrewing();
            //ecu_mod_controllPanel_Part_screwable.DetectScrewing();

            CheckPartsInstalledTrigger();


            if (ecu_mod_cableHarness_Part.installed && ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed)
            {
                if (ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed)
                {
                    if (ecu_mod_cruiseControlPanel_Part.installed && hasPower)
                    {
                        SetCruiseControlSpeedText(setCruiseControlSpeed.ToString());
                    }
                    else if (!hasPower)
                    {
                        setCruiseControlSpeed = 0;
                        cruiseControlModuleEnabled = false;
                        SetCruiseControlSpeedText("");
                    }
                    else
                    {
                        SetCruiseControlSpeedText("");
                    }
                }
                if (!hasPower)
                {
                    setCruiseControlSpeed = 0;
                    cruiseControlModuleEnabled = false;
                    SetCruiseControlSpeedText("");
                }

                if (ecu_mod_controllPanel_Part.installed && hasPower)
                {
                    if (DonnerTech_ECU_Mod.partBuySave.boughtABSModule)
                    {
                        if (ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed)
                        {
                            if (ecu_mod_absModule_Part.installed && ecu_mod_absModule_screwable.partFixed && absModuleEnabled == false)
                            {
                                absLightColor = Color.red;
                            }
                            else if (ecu_mod_absModule_Part.installed && ecu_mod_absModule_screwable.partFixed && absModuleEnabled == true)
                            {
                                absLightColor = Color.green;
                            }
                            else
                            {
                                absLightColor = new Color(1f, 1f, 1f, 1f);
                            }
                        }
                        else
                        {
                            absLightColor = new Color(1f, 1f, 1f, 1f);
                        }
                        ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ABS-Light", absLightColor);
                    }
                    if (DonnerTech_ECU_Mod.partBuySave.boughtESPModule)
                    {
                        if (ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed)
                        {
                            if (ecu_mod_espModule_Part.installed && ecu_mod_espModule_Part_screwable.partFixed && espModuleEnabled == false)
                            {
                                espLightColor = Color.red;
                            }
                            else if (ecu_mod_espModule_Part.installed && ecu_mod_espModule_Part_screwable.partFixed && espModuleEnabled == true)
                            {
                                espLightColor = Color.green;
                            }
                            else
                            {
                                espLightColor = new Color(1f, 1f, 1f, 1f);
                            }
                        }
                        else
                        {
                            espLightColor = new Color(1f, 1f, 1f, 1f);
                        }
                        ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ESP-Light", espLightColor);
                    }

                    if (DonnerTech_ECU_Mod.partBuySave.boughtTCSModule)
                    {
                        if (ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed)
                        {
                            if (ecu_mod_tcsModule_Part.installed && ecu_mod_tcsModule_Part_screwable.partFixed && tcsModuleEnabled == false)
                            {
                                tcsLightColor = Color.red;
                            }
                            else if (ecu_mod_tcsModule_Part.installed && ecu_mod_tcsModule_Part_screwable.partFixed && tcsModuleEnabled == true)
                            {
                                tcsLightColor = Color.green;
                            }
                            else
                            {
                                tcsLightColor = new Color(1f, 1f, 1f, 1f);
                            }
                        }
                        else
                        {
                            tcsLightColor = new Color(1f, 1f, 1f, 1f);
                        }
                        ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_TCS-Light", tcsLightColor);
                    }
                    if (DonnerTech_ECU_Mod.partBuySave.boughtSmartEngineModule)
                    {
                        if (ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed)
                        {
                            if (ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed && alsModuleEnabled == false)
                            {
                                alsLightColor = Color.red;
                            }
                            else if (ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed && alsModuleEnabled == true)
                            {
                                alsLightColor = Color.green;
                            }
                            else
                            {
                                alsLightColor = new Color(1f, 1f, 1f, 1f);
                            }
                        }
                        else
                        {
                            alsLightColor = new Color(1f, 1f, 1f, 1f);
                        }
                        ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ALS-Light", alsLightColor);

                        if (ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed)
                        {
                            if (ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed && stage2revSwitchEnabled == false)
                            {
                                stage2RevLimiterLightColor = Color.red;
                            }
                            else if (ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed && stage2revSwitchEnabled == true)
                            {
                                stage2RevLimiterLightColor = Color.green;
                            }
                            else
                            {
                                stage2RevLimiterLightColor = new Color(1f, 1f, 1f, 1f);
                            }
                        }
                        else
                        {
                            stage2RevLimiterLightColor = new Color(1f, 1f, 1f, 1f);
                        }
                        ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_s2Rev-Light", stage2RevLimiterLightColor);

                        if (stage2revModuleEnabled && alsModuleEnabled)
                        {
                            ToggleALSSwitch();
                        }

                        if (stage2revModuleEnabled && stage2revSwitchEnabled && satsumaDriveTrain.velo > 3.5f)
                        {
                            ToggleStage2RevSwitch();
                        }

                        if (stage2revModuleEnabled && satsumaDriveTrain.velo < 3.5f)
                        {
                            satsumaDriveTrain.revLimiterTime = 0;
                        }
                        else
                        {
                            satsumaDriveTrain.revLimiterTime = 0.2f;
                        }

                        if (ecu_mod_smartEngineModule_Part.installed)
                        {
                            if (alsModuleEnabled)
                            {
                                if (hasPower)
                                {
                                    if (FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle").Value == "Satsuma")
                                    {
                                        ModCommunication modCommunication = ecu_mod_smartEngineModule_Part.rigidPart.GetComponent<ModCommunication>();

                                        if (cInput.GetKey("Use") && satsumaDriveTrain.rpm >= 400 && (!useThrottleButton && !cruiseControlModuleEnabled))
                                        {
                                            modCommunication.alsEnabled = true;

                                            if (carChoke == null)
                                            {
                                                carChoke = GameObject.Find("Choke");
                                            }
                                            else
                                            {
                                                chokeFSM = PlayMakerFSM.FindFsmOnGameObject(carChoke, "Choke");
                                                if (chokeFSM.FsmVariables.FloatVariables[0].Value != 0.5f)
                                                {
                                                    originalChokeValue = chokeFSM.FsmVariables.FloatVariables[0].Value;
                                                }

                                                chokeFSM.FsmVariables.FloatVariables[0].Value = 0.5f;

                                            }
                                            if (backFireLoop == null)
                                            {
                                                CreateBackfireLoop();
                                            }
                                            else if (!backFireLoop.isPlaying)
                                            {
                                                backFireLoop.Play();
                                            }
                                        }
                                        else
                                        {
                                            modCommunication.alsEnabled = false;
                                            if (chokeFSM != null)
                                            {
                                                chokeFSM.FsmVariables.FloatVariables[0].Value = originalChokeValue;
                                            }

                                            if (backFireLoop != null && backFireLoop.isPlaying)
                                            {
                                                backFireLoop.Stop();
                                            }

                                        }
                                    }
                                    else if (backFireLoop != null && backFireLoop.isPlaying)
                                    {
                                        backFireLoop.Stop();
                                    }
                                }
                                else if (backFireLoop != null && backFireLoop.isPlaying)
                                {
                                    backFireLoop.Stop();
                                }




                            }
                            timeSinceLastBackFire += Time.deltaTime;
                            TriggerBackFire();
                        }
                    }
                }
            }
            else if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel || (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && !hasPower))
            {
                if (backFireLoop != null && backFireLoop.isPlaying)
                {
                    backFireLoop.Stop();
                }
                ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ABS-Light", new Color(1f, 1f, 1f, 1f));
                ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ESP-Light", new Color(1f, 1f, 1f, 1f));
                ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_TCS-Light", new Color(1f, 1f, 1f, 1f));
                ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ALS-Light", new Color(1f, 1f, 1f, 1f));
                ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_s2Rev-Light", new Color(1f, 1f, 1f, 1f));
            }

            if (hasPower)
            {
                if (satsumaDriveTrain.gear != 0 && ecu_mod_cruiseControlPanel_Part.installed && cruiseControlModuleEnabled && ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed && ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed && satsumaCarController.throttleInput <= 0f)
                {
                    float valueToThrottle = 0f;
                    if (satsumaDriveTrain.differentialSpeed <= setCruiseControlSpeed)
                    {
                        valueToThrottle = (satsumaDriveTrain.differentialSpeed - (setCruiseControlSpeed * 2.2f)) / ((-setCruiseControlSpeed * 2.2f));
                    }
                    else
                    {
                        valueToThrottle = 0f;
                    }
                    satsumaCarController.throttle = valueToThrottle;
                    if (satsumaDriveTrain.differentialSpeed < 19f || satsumaCarController.brakeInput > 0f || satsumaCarController.clutchInput > 0f || satsumaCarController.handbrakeInput > 0f)
                    {
                        
                        ResetCruiseControl();
                    }
                }
                else if (cruiseControlModuleEnabled && satsumaCarController.throttleInput <= 0f)
                {
                    ResetCruiseControl();
                    setCruiseControlSpeed = 0;
                    
                }

                if (ecu_mod_mountingPlate_Part.installed && ecu_mod_mountingPlate_Part_screwable.partFixed)
                {
                    if ((DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ABS != absModuleEnabled) && ecu_mod_absModule_Part.installed && ecu_mod_absModule_screwable.partFixed)
                    {
                        ToggleABS();
                    }
                    if ((DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ESP != espModuleEnabled) && ecu_mod_espModule_Part.installed && ecu_mod_espModule_Part_screwable.partFixed)
                    {
                        ToggleESP();
                    }
                    if ((DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().TCS != tcsModuleEnabled) && ecu_mod_tcsModule_Part.installed && ecu_mod_tcsModule_Part_screwable.partFixed)
                    {
                        ToggleTCS();
                    }
                    if ((stage2revModuleEnabled != stage2revSwitchEnabled) && ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed)
                    {
                        ToggleStage2Rev();
                    }
                    if ((alsModuleEnabled != alsSwitchEnabled) && ecu_mod_smartEngineModule_Part.installed && ecu_mod_smartEngineModule_Part_screwable.partFixed)
                    {
                        ToggleALS();
                    }
                }

            }

            /*
            if (satsumaDriveTrain.rpm >= 5800)
            {
                if (backFireLoop == null)
                {
                    CreateBackfireLoop();
                }
                else if (backFireLoop.isPlaying == false)
                    backFireLoop.Play();
            }
            else
            {
                backFireLoop.Stop();
            }
            */
            if (Camera.main != null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1f, 1 << LayerMask.NameToLayer("DontCollide")) != false)
                {
                    GameObject gameObjectHit;
                    bool foundObject = false;
                    string guiText = "";
                    gameObjectHit = hit.collider?.gameObject;
                    if (gameObjectHit != null)
                    {

                        Action actionToPerform = null;
                        //Control panel
                        if (gameObjectHit.name == "ECU-Mod_Controll-Panel_v2_ABS-Switch")
                        {
                            foundObject = true;
                            actionToPerform = ToggleABSSwitch;
                            guiText = "toggle ABS";
                        }
                        if (gameObjectHit.name == "ECU-Mod_Controll-Panel_v2_ESP-Switch")
                        {
                            foundObject = true;
                            actionToPerform = ToggleESPSwitch;
                            guiText = "toggle ESP";
                        }
                        if (gameObjectHit.name == "ECU-Mod_Controll-Panel_v2_TCS-Switch")
                        {
                            foundObject = true;
                            actionToPerform = ToggleTCSSwitch;
                            guiText = "toggle TCS";
                        }
                        if (gameObjectHit.name == "ECU-Mod_Controll-Panel_v2_ALS-Switch")
                        {
                            foundObject = true;
                            actionToPerform = ToggleALSSwitch;
                            guiText = "toggle ALS";
                        }
                        if (gameObjectHit.name == "ECU-Mod_Controll-Panel_v2_s2Rev-Switch")
                        {
                            foundObject = true;
                            actionToPerform = ToggleStage2RevSwitch;
                            guiText = "toggle Stage2 RevLimiter";
                        }

                        //CruiseControl Panel
                        if(gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Minus")
                        {
                            foundObject = true;
                            actionToPerform = DecreaseCruiseControl;
                            guiText = "decrease cruise speed";
                        }
                        if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Plus")
                        {
                            foundObject = true;
                            actionToPerform = IncreaseCruiseControl;
                            guiText = "increase cruise speed";
                        }
                        if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Set")
                        {
                            foundObject = true;
                            actionToPerform = SetCruiseControl;
                            guiText = "set/enable cruise control";
                        }
                        if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Reset")
                        {
                            foundObject = true;
                            actionToPerform = ResetCruiseControl;
                            guiText = "reset/disable cruise control";
                        }

                        if (foundObject)
                        {
                            ModClient.guiInteract(guiText);
                            if (useButtonDown)
                            {
                                actionToPerform.Invoke();
                                AudioSource audio = dashButtonAudioSource;
                                audio.transform.position = gameObjectHit.transform.position;
                                audio.Play();
                            }
                        }
                    }
                }
            }
        }

        private void DecreaseCruiseControl()
        {
            if(setCruiseControlSpeed >= 2)
            {
                setCruiseControlSpeed -= 2;
            }
            
        }
        private void IncreaseCruiseControl()
        {
            setCruiseControlSpeed += 2;
        }
        private void SetCruiseControl()
        {
            if(satsumaDriveTrain.differentialSpeed >= 20 && setCruiseControlSpeed >= 20f)
            {
                SetCruiseControlSpeedTextColor(Color.green);
                cruiseControlModuleEnabled = true;
            }
            
        }
        private void ResetCruiseControl()
        {
            SetCruiseControlSpeedTextColor(Color.white);
            cruiseControlModuleEnabled = false;
        }



        private void CheckPartsInstalledTrigger()
        {
            try
            {
                if (ecu_mod_mountingPlate_Part.installed)
                {
                    if (ecu_mod_ABSModule_Trigger.triggerGameObject.activeSelf == false)
                    {
                        ecu_mod_ABSModule_Trigger.triggerGameObject.SetActive(true);
                    }
                    if (ecu_mod_ESPModule_Trigger.triggerGameObject.activeSelf == false)
                    {
                        ecu_mod_ESPModule_Trigger.triggerGameObject.SetActive(true);
                    }
                    if (ecu_mod_TCSModule_Trigger.triggerGameObject.activeSelf == false)
                    {
                        ecu_mod_TCSModule_Trigger.triggerGameObject.SetActive(true);
                    }
                    if (ecu_mod_CableHarness_Trigger.triggerGameObject.activeSelf == false)
                    {
                        ecu_mod_CableHarness_Trigger.triggerGameObject.SetActive(true);
                    }
                    if (ecu_mod_SmartEngineModule_Trigger.triggerGameObject.activeSelf == false)
                    {
                        ecu_mod_SmartEngineModule_Trigger.triggerGameObject.SetActive(true);
                    }
                }
                else
                {
                    if (ecu_mod_absModule_Part.installed)
                    {
                        ecu_mod_absModule_Part.removePart();
                    }
                    if (ecu_mod_espModule_Part.installed)
                    {
                        ecu_mod_espModule_Part.removePart();
                    }
                    if (ecu_mod_tcsModule_Part.installed)
                    {
                        ecu_mod_tcsModule_Part.removePart();
                    }
                    if (ecu_mod_cableHarness_Part.installed)
                    {
                        ecu_mod_cableHarness_Part.removePart();
                    }
                    if (ecu_mod_smartEngineModule_Part.installed)
                    {
                        ecu_mod_smartEngineModule_Part.removePart();
                    }

                    if (ecu_mod_ABSModule_Trigger.triggerGameObject.activeSelf == true)
                    {
                        ecu_mod_ABSModule_Trigger.triggerGameObject.SetActive(false);
                    }
                    if (ecu_mod_ESPModule_Trigger.triggerGameObject.activeSelf == true)
                    {
                        ecu_mod_ESPModule_Trigger.triggerGameObject.SetActive(false);
                    }
                    if (ecu_mod_TCSModule_Trigger.triggerGameObject.activeSelf == true)
                    {
                        ecu_mod_TCSModule_Trigger.triggerGameObject.SetActive(false);
                    }
                    if (ecu_mod_CableHarness_Trigger.triggerGameObject.activeSelf == true)
                    {
                        ecu_mod_CableHarness_Trigger.triggerGameObject.SetActive(false);
                    }
                    if (ecu_mod_SmartEngineModule_Trigger.triggerGameObject.activeSelf == true)
                    {
                        ecu_mod_SmartEngineModule_Trigger.triggerGameObject.SetActive(false);
                    }
                }
            }
            catch
            {

            }

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

        private void ChangeColorOfLight(string lightGameObjectName, Color color)
        {
            GameObject moduleLight = GameObject.Find(lightGameObjectName);
            if(moduleLight != null && moduleLight.GetComponent<MeshRenderer>().material.color != color)
            {
                moduleLight.GetComponent<MeshRenderer>().material.color = color;
            }
        }

        public static void ToggleABS()
        {
            if (DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate && DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_cableHarness_Part.installed && ecu_mod_controllPanel_Part.installed && ecu_mod_mountingPlate_Part.installed && hasPower)
            {
                satsuma.GetComponent<CarController>().ABS = !DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ABS;
            }
        }
        private void ToggleABSSwitch()
        {
            if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_controllPanel_Part.installed)
            {
                GameObject absSwitch = GameObject.Find("ECU-Mod_Controll-Panel_v2_ABS-Switch");
                if (absModuleEnabled && ecu_mod_absModule_Part.installed)
                {
                    absSwitch.transform.localPosition = new Vector3(0f, 0f, 0f);
                    absSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, 0f) };
                }
                else if (ecu_mod_absModule_Part.installed)
                {
                    absSwitch.transform.localPosition = new Vector3(0.1066675f, -0.01057142f, 4.656613e-10f);
                    absSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, -180f) };
                }
                absModuleEnabled = !absModuleEnabled;
            }            
        }

        public static void ToggleESP()
        {
            if (DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate && DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_cableHarness_Part.installed && ecu_mod_controllPanel_Part.installed && ecu_mod_mountingPlate_Part.installed && hasPower)
            {
                DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ESP = !DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ESP;
            }
        }

        void ToggleESPSwitch()
        {
            if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_controllPanel_Part.installed)
            {
                GameObject espSwitch = GameObject.Find("ECU-Mod_Controll-Panel_v2_ESP-Switch");
                if (espModuleEnabled)
                {
                    espSwitch.transform.localPosition = new Vector3(0f, 0f, 0f);
                    espSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, 0f) };
                }
                else
                {
                    espSwitch.transform.localPosition = new Vector3(0.05664825f, -0.01059383f, 4.656613e-10f);
                    espSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, -180f) };
                }
                espModuleEnabled = !espModuleEnabled;
            }    
        }

        
        public static void ToggleTCS()
        {
            
            if (DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate && DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_cableHarness_Part.installed && ecu_mod_controllPanel_Part.installed && ecu_mod_mountingPlate_Part.installed && hasPower)
            {
                DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().TCS = !DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().TCS;
            }
        }

        void ToggleTCSSwitch()
        {
            if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_controllPanel_Part.installed)
            {
                GameObject tcsSwitch = GameObject.Find("ECU-Mod_Controll-Panel_v2_TCS-Switch");
                if (tcsModuleEnabled)
                {
                    tcsSwitch.transform.localPosition = new Vector3(0f, 0f, 0f);
                    tcsSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, 0f) };
                }
                else
                {
                    tcsSwitch.transform.localPosition = new Vector3(0.0092659f, -0.01062297f, 4.656613e-10f);
                    tcsSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, -180f) };
                }
                tcsModuleEnabled = !tcsModuleEnabled;
            }
        }


        public  static void ToggleALS()
        {
            
            if (DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate && DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_cableHarness_Part.installed && ecu_mod_controllPanel_Part.installed && ecu_mod_mountingPlate_Part.installed && hasPower)
            {
                if (ecu_mod_smartEngineModule_Part.installed)
                {
                    alsModuleEnabled = !alsModuleEnabled;
                }
                if (!alsModuleEnabled && backFireLoop != null && backFireLoop.isPlaying)
                {
                    backFireLoop.Stop();
                }
            }
        }

        private void ToggleALSSwitch()
        {
            if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_controllPanel_Part.installed)
            {
                GameObject alsSwitch = GameObject.Find("ECU-Mod_Controll-Panel_v2_ALS-Switch");
                if (alsSwitchEnabled)
                {
                    alsSwitch.transform.localPosition = new Vector3(0f, 0f, 0f);
                    alsSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, 0f) };
                }
                else
                {
                    alsSwitch.transform.localPosition = new Vector3(-0.03855515f, -0.01058602f, 4.656613e-10f);
                    alsSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, -180f) };
                }
                alsSwitchEnabled = !alsSwitchEnabled;
            }
        }

        public static void ToggleStage2Rev()
        {
            stage2revModuleEnabled = !stage2revModuleEnabled;
            if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate && DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_cableHarness_Part.installed && ecu_mod_controllPanel_Part.installed && ecu_mod_mountingPlate_Part.installed && hasPower)
            {
                if(satsumaDriveTrain.velo < 3.5f && stage2revModuleEnabled)
                {
                    satsumaDriveTrain.maxRPM = satsumaDriveTrain.maxPowerRPM;
                }
                else
                {
                    satsumaDriveTrain.maxRPM = 8400;
                }
            }
        }

        private static void ToggleStage2RevSwitch()
        {
            if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_controllPanel_Part.installed)
            {
                GameObject stage2revSwitch = GameObject.Find("ECU-Mod_Controll-Panel_v2_s2Rev-Switch");
                if (stage2revSwitchEnabled)
                {
                    stage2revSwitch.transform.localPosition = new Vector3(0f, 0f, 0f);
                    stage2revSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, 0f) };
                }
                else
                {
                    stage2revSwitch.transform.localPosition = new Vector3(-0.09786987f, -0.01061337f, 4.656613e-10f);
                    stage2revSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, -180f) };
                }
                stage2revSwitchEnabled = !stage2revSwitchEnabled;
            }
        }



        private static void PosReset()
        {

            if (!DonnerTech_ECU_Mod.ecu_mod_absModule_Part.installed)
            {
                ecu_mod_absModule_Part.activePart.transform.position = ecu_mod_absModule_Part.defaultPartSaveInfo.position;
            }
            if (!DonnerTech_ECU_Mod.ecu_mod_espModule_Part.installed)
            {
                ecu_mod_espModule_Part.activePart.transform.position = ecu_mod_espModule_Part.defaultPartSaveInfo.position;
            }
            if (!DonnerTech_ECU_Mod.ecu_mod_tcsModule_Part.installed)
            {
                ecu_mod_tcsModule_Part.activePart.transform.position = ecu_mod_tcsModule_Part.defaultPartSaveInfo.position;
            }
            if (!DonnerTech_ECU_Mod.ecu_mod_cableHarness_Part.installed)
            {
                ecu_mod_cableHarness_Part.activePart.transform.position = ecu_mod_cableHarness_Part.defaultPartSaveInfo.position;
            }
            if (!DonnerTech_ECU_Mod.ecu_mod_mountingPlate_Part.installed)
            {
                ecu_mod_mountingPlate_Part.activePart.transform.position = ecu_mod_mountingPlate_Part.defaultPartSaveInfo.position;
            }
            if (!DonnerTech_ECU_Mod.ecu_mod_controllPanel_Part.installed)
            {
                ecu_mod_controllPanel_Part.activePart.transform.position = ecu_mod_controllPanel_Part.defaultPartSaveInfo.position;
            }
            if (!DonnerTech_ECU_Mod.ecu_mod_smartEngineModule_Part.installed)
            {
                ecu_mod_smartEngineModule_Part.activePart.transform.position = ecu_mod_smartEngineModule_Part.defaultPartSaveInfo.position;
            }
            if (!ecu_mod_cruiseControlPanel_Part.installed)
            {
                ecu_mod_cruiseControlPanel_Part.activePart.transform.position = ecu_mod_cruiseControlPanel_Part.defaultPartSaveInfo.position;
            }
           
        }

        private static void CreateBackfireLoop()
        {
            if(GameObject.Find("racing muffler(Clone)") != null)
            {
                backFireLoop = GameObject.Find("racing muffler(Clone)").AddComponent<AudioSource>();
                backFire_loop.audioSource = backFireLoop;
                backFire_loop.LoadAudioFromFile(Path.Combine(modAssetsFolder, "backFire_loop.wav"), true, false);
                backFireLoop.volume = 0.3f;
                //backFireLoop.rolloffMode = AudioRolloffMode.Linear;
                backFireLoop.minDistance = 1;
                backFireLoop.maxDistance = 40;
                backFireLoop.spatialBlend = 0.5f;
                backFireLoop.loop = true;
                backFire_loop.Play();
            }
            else
            {
                backFireLoop = null;
            }
            
        }

        private void TriggerBackFire()
        {
            if (stage2revSwitchEnabled && (satsumaDriveTrain.velo >= 0.25f && satsumaDriveTrain.velo <= 0.5f))
            {
                if(backFire == null)
                {
                    CreateBackfire();
                }
                else
                {
                    backFire.Play();
                }
            }
        }


        public void CreateBackfire()
        {
            backFire = satsuma.AddComponent<AudioSource>();
            backfire_once.audioSource = backFire;
            backFire.rolloffMode = AudioRolloffMode.Linear;
            backFire.pitch = 1f;
            backFire.volume = 5f;
            backfire_once.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(this), "backFire_once.wav"), true, false);
            backFire.Play();
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
    }
}
