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

namespace DonnerTech_ECU_Mod
{
    public class DonnerTech_ECU_Mod : Mod
    {
        
        /*  TODO:
         *  Add Turbocharger ECU
         *  ADD RevLimiter function
         *  ADD Antilag function
         *  ADD change turbocharger boost function
         *  ADD s2Rev Stage 2 revlimiter
         *  ADD CruiseControll
         *  DONE: Make parts plop off when mounting plate is removed
         *           *  change air/fuel ratio when launch controll and antilag
         *           
         */

        /*  Changelog (v1.4)
         *  fixed problem with TV-Mode or camera changing mods
         *  fixed stage2RevLimiter/Launch controll can always be enabled
         *  fixed stage2RevLimiter/Launch controll won't disable when smart engine controller module is removed
         *  added ModConsole output when Mod starts loading
         *  fixed some als module bugs
         *  fixed some stage2 bugs
         *  when als is enabled and is backfiring. Air fuel ratio will be richer (more fuel -> backfire)
         *  some optimization
         *  added SixGears and AWD
         *  also fixed AWD (awd can now always be toggled) (won't set it on startup you have to toggle the checkbox once after game is loaded)
         *  SixGears sets itself on startup based on the last Mod Settings checkbox ticked
         *  fixed Smart Engine Controll Module product image
         *  
         */

            

        /* BUGS
         * fix smart engine module product picture
         * 

         * 
         */

        public override string ID => "DonnerTech_ECU_Mod"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing ECUs"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.1.4"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => true;

        AssetBundle assetBundle;
        private static GameObject ecu_mod_ABSModule = new GameObject();
        private static GameObject ecu_mod_ESPModule = new GameObject();
        private static GameObject ecu_mod_TCSModule = new GameObject();
        private static GameObject ecu_mod_CableHarness = new GameObject();
        private static GameObject ecu_mod_MountingPlate = new GameObject();
        private static GameObject ecu_mod_ControllPanel = new GameObject();
        private static GameObject ecu_mod_SmartEngineModule = new GameObject();

        private static PartBuySave partBuySave;
        private Trigger ecu_mod_ABSModule_Trigger;
        private Trigger ecu_mod_ESPModule_Trigger;
        private Trigger ecu_mod_TCSModule_Trigger;
        private Trigger ecu_mod_CableHarness_Trigger;
        private Trigger ecu_mod_MountingPlate_Trigger;
        private Trigger ecu_mod_ControllPanel_Trigger;
        private Trigger ecu_mod_SmartEngineModule_Trigger;

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
        private bool cracked = false;
        private string[] crackedMSCLoaderHashes = { "4e5af1f010743d8f48e74ea7472fed0e153bfd48", "9db4a94cede70acefb91a3862ee99f06e1987d15", "cdc72e09bb7dbc1e67e7dd84a394d6f8bad5c38c" };
        private string computedSHA1;

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

        private Settings resetPosSetting = new Settings("resetPos", "Reset uninstalled parts location", new Action(DonnerTech_ECU_Mod.PosReset));
        private static AudioSource backFireLoop;
        private static ModAudio backFire_loop = new ModAudio();
        private bool engineBackfiring = false;
        private float originalChokeValue;
        private static bool sixGearsEnabled;


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
        }

        public override void OnLoad()
        {
            ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " started loaded");

            string gamePath = Directory.GetCurrentDirectory();
            string mscLoaderDLLPath = Path.Combine(gamePath, "mysummercar_Data\\Managed\\MSCLoader.dll");
            //string modAssetsFolderPath = ModLoader.GetModAssetsFolder(this);
            //string mscLoaderDLLPath = modAssetsFolderPath.Replace("Mods\\Assets\\DonnerTech_ECU_Mod", "mysummercar_Data\\Managed\\MSCLoader.dll");
            if (File.Exists(mscLoaderDLLPath))
            {
                computedSHA1 = CalculateSHA1(mscLoaderDLLPath);
            }
            else
            {
                computedSHA1 = "none";
            }

            for (int i = 0; i < crackedMSCLoaderHashes.Length; i++)
            {
                if (computedSHA1 == crackedMSCLoaderHashes[i] == true)
                {
                    cracked = true;
                    break;
                }
            }
            if (ModLoader.CheckSteam() == false || cracked)
            {
                if(ModLoader.CheckSteam() == false)
                {
                    ModConsole.Warning("You are not running a legit version of 'My Summer Car' from Steam");
                    ModConsole.Warning("Please support the developer of the game!");
                    ModConsole.Warning("This mod will not work if your version of the game is not legit");
                    ModConsole.Warning("Other cause: you started the .exe instead of through steam");
                    ModUI.ShowMessage(
                        "You are running a version of 'My Summer Car' without Steam.\n" +
                        "Either it is a pirated copy of the game or you started the .exe of the game.\n" +
                        "Please buy the game and support Developing.\n\n" + "You had enough time to test the game.",
                        "Illegal copy of Game Detected - Mod was disabled");
                }
                if(cracked)
                {
                    ModConsole.Warning("You are running a modified version of the 'MSC ModLoader");
                    ModConsole.Warning("This version might add dangerous stuff which could potentially delete files on your pc or do something else");
                    ModConsole.Warning("Please use the original version of the ModLoader made by @piotrulos");
                    ModUI.ShowMessage(
                        "You are running a modified version of the 'MSC ModLoader.\n" +
                        "This version might add dangerous stuff which could potentially delete files on your pc or do something else.\n" +
                        "Please use the original version made by @piotrulos.",
                        "DANGEROUS/MODIFIED version of ModLoader found - Mod will disable!");
                }
            }
            else
            {
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

                DonnerTech_ECU_Mod.ecu_mod_ABSModule.name = "ABS Module";
                DonnerTech_ECU_Mod.ecu_mod_ESPModule.name = "ESP Module";
                DonnerTech_ECU_Mod.ecu_mod_TCSModule.name = "TCS Module";
                DonnerTech_ECU_Mod.ecu_mod_CableHarness.name = "ECU Cable Harness";
                DonnerTech_ECU_Mod.ecu_mod_MountingPlate.name = "ECU Mounting Plate";
                DonnerTech_ECU_Mod.ecu_mod_ControllPanel.name = "ECU Controll Panel";
                DonnerTech_ECU_Mod.ecu_mod_SmartEngineModule.name = "Smart Engine ECU";

                DonnerTech_ECU_Mod.ecu_mod_ABSModule.tag = "PART";
                DonnerTech_ECU_Mod.ecu_mod_ESPModule.tag = "PART";
                DonnerTech_ECU_Mod.ecu_mod_TCSModule.tag = "PART";
                DonnerTech_ECU_Mod.ecu_mod_CableHarness.tag = "PART";
                DonnerTech_ECU_Mod.ecu_mod_MountingPlate.tag = "PART";
                DonnerTech_ECU_Mod.ecu_mod_ControllPanel.tag = "PART";
                DonnerTech_ECU_Mod.ecu_mod_SmartEngineModule.tag = "PART";

                DonnerTech_ECU_Mod.ecu_mod_ABSModule.layer = LayerMask.NameToLayer("Parts");
                DonnerTech_ECU_Mod.ecu_mod_ESPModule.layer = LayerMask.NameToLayer("Parts");
                DonnerTech_ECU_Mod.ecu_mod_TCSModule.layer = LayerMask.NameToLayer("Parts");
                DonnerTech_ECU_Mod.ecu_mod_CableHarness.layer = LayerMask.NameToLayer("Parts");
                DonnerTech_ECU_Mod.ecu_mod_MountingPlate.layer = LayerMask.NameToLayer("Parts");
                DonnerTech_ECU_Mod.ecu_mod_ControllPanel.layer = LayerMask.NameToLayer("Parts");
                DonnerTech_ECU_Mod.ecu_mod_SmartEngineModule.layer = LayerMask.NameToLayer("Parts");


                ecu_mod_ABSModule_Trigger = new Trigger("ECU_MOD_ABSModule_Trigger", satsuma, new Vector3(0.254f, -0.28f, -0.155f), new Quaternion(0, 0, 0, 0), new Vector3(0.14f, 0.02f, 0.125f), false);
                ecu_mod_ESPModule_Trigger = new Trigger("ECU_MOD_ESPModule_Trigger", satsuma, new Vector3(0.288f, -0.28f, -0.0145f), new Quaternion(0, 0, 0, 0), new Vector3(0.21f, 0.02f, 0.125f), false);
                ecu_mod_TCSModule_Trigger = new Trigger("ECU_MOD_TCSModule_Trigger", satsuma, new Vector3(0.342f, -0.28f, 0.115f), new Quaternion(0, 0, 0, 0), new Vector3(0.104f, 0.02f, 0.104f), false);
                ecu_mod_CableHarness_Trigger = new Trigger("ECU_MOD_CableHarness_Trigger", satsuma, new Vector3(0.423f, -0.28f, -0.0384f), new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.02f, 0.3f), false);
                ecu_mod_MountingPlate_Trigger = new Trigger("ECU_MOD_MountingPlate_Trigger", satsuma, new Vector3(0.31f, -0.28f, -0.038f), new Quaternion(0, 0, 0, 0), new Vector3(0.32f, 0.02f, 0.45f), false);
                ecu_mod_ControllPanel_Trigger = new Trigger("ECU_MOD_ControllPanel_Trigger", GameObject.Find("dashboard(Clone)"), new Vector3(0.4f, -0.042f, -0.12f), new Quaternion(0, 0, 0, 0), new Vector3(0.2f, 0.02f, 0.08f), false);
                ecu_mod_SmartEngineModule_Trigger = new Trigger("ECU_MOD_SmartEngineModule_Trigger", satsuma, new Vector3(0.2398f, -0.28f, 0.104f), new Quaternion(0, 0, 0, 0), new Vector3(0.15f, 0.02f, 0.13f), false);

                PartSaveInfo ecu_mod_ABSModule_SaveInfo = null;
                PartSaveInfo ecu_mod_ESPModule_SaveInfo = null;
                PartSaveInfo ecu_mod_TCSModule_SaveInfo = null;
                PartSaveInfo ecu_mod_CableHarness_SaveInfo = null;
                PartSaveInfo ecu_mod_MountingPlate_SaveInfo = null;
                PartSaveInfo ecu_mod_ControllPanel_SaveInfo = null;
                PartSaveInfo ecu_mod_SmartEngineModule_SaveInfo = null;

                ecu_mod_ABSModule_SaveInfo = this.loadSaveData(ecu_mod_ABSModule_SaveFile);
                ecu_mod_ESPModule_SaveInfo = this.loadSaveData(ecu_mod_ESPModule_SaveFile);
                ecu_mod_TCSModule_SaveInfo = this.loadSaveData(ecu_mod_TCSModule_SaveFile);
                ecu_mod_CableHarness_SaveInfo = this.loadSaveData(ecu_mod_CableHarness_SaveFile);
                ecu_mod_MountingPlate_SaveInfo = this.loadSaveData(ecu_mod_MountingPlate_SaveFile);
                ecu_mod_ControllPanel_SaveInfo = this.loadSaveData(ecu_mod_ControllPanel_SaveFile);
                ecu_mod_SmartEngineModule_SaveInfo = this.loadSaveData(ecu_mod_SmartEngineModule_SaveFile);
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
                        boughtSmartEngineModule = false
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
                        shop.Add(this, absModuleProduct, ModsShop.ShopType.Teimo, PurchaseMadeABS, ecu_mod_absModule_Part.activePart);
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
                        shop.Add(this, espModuleProduct, ModsShop.ShopType.Teimo, PurchaseMadeESP, ecu_mod_espModule_Part.activePart);
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
                        shop.Add(this, tcsModuleProduct, ModsShop.ShopType.Teimo, PurchaseMadeTCS, ecu_mod_tcsModule_Part.activePart);
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
                        shop.Add(this, cableHarnessProduct, ModsShop.ShopType.Teimo, PurchaseMadeCableHarness, ecu_mod_cableHarness_Part.activePart);
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
                        shop.Add(this, mountingPlateProduct, ModsShop.ShopType.Teimo, PurchaseMadeMountingPlate, ecu_mod_mountingPlate_Part.activePart);
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
                        shop.Add(this, controllPanelProduct, ModsShop.ShopType.Teimo, PurchaseMadeControllPanel, ecu_mod_controllPanel_Part.activePart);
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
                        shop.Add(this, smartEngineModuleProduct, ModsShop.ShopType.Teimo, PurchageMadeSmartEngineModule, ecu_mod_smartEngineModule_Part.activePart);
                        ecu_mod_smartEngineModule_Part.activePart.SetActive(false);
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

                

                ModConsole.Print("DonnerTechRacing ECUs Mod [ v" + this.Version + "]" + " loaded");
            }

        }

        public void PurchaseMadeABS(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.TeimoSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtABSModule = true;
        }
        public void PurchaseMadeESP(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.TeimoSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtESPModule = true;
        }
        public void PurchaseMadeTCS(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.TeimoSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtTCSModule = true;
        }
        public void PurchaseMadeCableHarness(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.TeimoSpawnLocation.outsideRamp;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtCableHarness = true;
        }
        public void PurchaseMadeMountingPlate(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.TeimoSpawnLocation.outsideRamp;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate = true;
        }
        public void PurchaseMadeControllPanel(ModsShop.PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.TeimoSpawnLocation.floor;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtControllPanel = true;
        }
        private void PurchageMadeSmartEngineModule(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.TeimoSpawnLocation.desk;
            item.gameObject.SetActive(true);
            DonnerTech_ECU_Mod.partBuySave.boughtSmartEngineModule = true;
        }






        public override void ModSettings()
        {
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

            }
            catch (System.Exception ex)
            {
                ModConsole.Error("<b>[ECU_MOD]</b> - an error occured while attempting to save part info.. see: " + ex.ToString());
            }
        }

        public override void OnGUI()
        {
        }

        public override void Update()
        {
            if (ModLoader.CheckSteam() == true || cracked == false)
            {
                CheckPartsInstalledTrigger();

                if (ecu_mod_cableHarness_Part.installed && ecu_mod_controllPanel_Part.installed && ecu_mod_mountingPlate_Part.installed && hasPower)
                {
                    if (DonnerTech_ECU_Mod.partBuySave.boughtABSModule)
                    {
                        if (ecu_mod_absModule_Part.installed && absModuleEnabled == false)
                        {
                            absLightColor = Color.red;
                        }
                        else if (ecu_mod_absModule_Part.installed && absModuleEnabled == true)
                        {
                            absLightColor = Color.green;
                        }
                        else
                        {
                            absLightColor = new Color(1f, 1f, 1f, 1f);
                        }
                        ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ABS-Light", absLightColor);
                    }
                    if (DonnerTech_ECU_Mod.partBuySave.boughtESPModule)
                    {
                        if (ecu_mod_espModule_Part.installed && espModuleEnabled == false)
                        {
                            espLightColor = Color.red;
                        }
                        else if (ecu_mod_espModule_Part.installed && espModuleEnabled == true)
                        {
                            espLightColor = Color.green;
                        }
                        else
                        {
                            espLightColor = new Color(1f, 1f, 1f, 1f);
                        }
                        ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ESP-Light", espLightColor);
                    }

                    if (DonnerTech_ECU_Mod.partBuySave.boughtTCSModule)
                    {
                        if (ecu_mod_tcsModule_Part.installed && tcsModuleEnabled == false)
                        {
                            tcsLightColor = Color.red;
                        }
                        else if (ecu_mod_tcsModule_Part.installed && tcsModuleEnabled == true)
                        {
                            tcsLightColor = Color.green;
                        }
                        else
                        {
                            tcsLightColor = new Color(1f, 1f, 1f, 1f);

                        }
                        ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_TCS-Light", tcsLightColor);
                    }
                    if (DonnerTech_ECU_Mod.partBuySave.boughtSmartEngineModule)
                    {
                        if (ecu_mod_smartEngineModule_Part.installed && alsModuleEnabled == false)
                        {
                            alsLightColor = Color.red;
                        }
                        else if (ecu_mod_smartEngineModule_Part.installed && alsModuleEnabled == true)
                        {
                            alsLightColor = Color.green;
                        }
                        else
                        {
                            alsLightColor = new Color(1f, 1f, 1f, 1f);
                        }
                        ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ALS-Light", alsLightColor);


                        if (ecu_mod_smartEngineModule_Part.installed && stage2revSwitchEnabled == false)
                        {
                            stage2RevLimiterLightColor = Color.red;
                        }
                        else if (ecu_mod_smartEngineModule_Part.installed && stage2revSwitchEnabled == true)
                        {
                            stage2RevLimiterLightColor = Color.green;
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
                                    if(FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle").Value == "Satsuma")
                                    {
                                        if (cInput.GetKey("Clutch") && satsumaDriveTrain.rpm >= 400)
                                        {
                                            satsumaCarController.throttle = 1f;
                                            if (satsumaDriveTrain.rpm >= 6500)
                                            {
                                                carChoke = GameObject.Find("Choke");
                                                if (carChoke != null)
                                                {

                                                    chokeFSM = PlayMakerFSM.FindFsmOnGameObject(carChoke, "Choke");
                                                    if(chokeFSM.FsmVariables.FloatVariables[0].Value != 0.5f)
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
                                            else if (backFireLoop != null && backFireLoop.isPlaying)
                                            {
                                                backFireLoop.Stop();
                                            }
                                            
                                        }
                                        else 
                                        {
                                            if(chokeFSM != null)
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
                else if(DonnerTech_ECU_Mod.partBuySave.boughtControllPanel || (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && !hasPower))
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
                    if ((DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ABS != absModuleEnabled) && ecu_mod_absModule_Part.installed)
                    {
                        ToggleABS();
                    }

                    if ((DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ESP != espModuleEnabled) && ecu_mod_espModule_Part.installed)
                    {
                        ToggleESP();
                    }


                    if((DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().TCS != tcsModuleEnabled) && ecu_mod_tcsModule_Part.installed)
                    {
                        ToggleTCS();
                    }
                    if((stage2revModuleEnabled != stage2revSwitchEnabled) && ecu_mod_smartEngineModule_Part.installed)
                    {
                        ToggleStage2Rev();
                    }
                    if ((alsModuleEnabled != alsSwitchEnabled) && ecu_mod_smartEngineModule_Part.installed)
                    {
                        ToggleALS();
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
            else
            {

            }
        }
        private void CheckPartsInstalledTrigger()
        {
            if (ecu_mod_mountingPlate_Part.installed)
            {
                if(ecu_mod_ABSModule_Trigger.triggerGameObject.activeSelf == false){
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
                if(ecu_mod_absModule_Part.installed)
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
                if (ecu_mod_absModule_Part.installed)
                {
                    DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ABS = !DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ABS;
                }
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
                if (ecu_mod_espModule_Part.installed)
                {
                    DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ESP = !DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().ESP;
                }
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
                if (ecu_mod_tcsModule_Part.installed)
                {
                    DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().TCS = !DonnerTech_ECU_Mod.satsuma.GetComponent<CarController>().TCS;
                }
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

        private static string CalculateSHA1(string filename)
        {
            using (var sha1 = SHA1.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
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
    }
}
