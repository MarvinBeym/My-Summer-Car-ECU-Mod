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
         */    

        /*  Changelog (v1.1)
         *  parts will now be uninstalled when mounting plate is removed.
         *  fixed some bugs
         *  Added new Controll Panel with switches instead of buttons
         *  switches will now also change position to be more realistic
         *  
         *  added Stage2 revlimiter/Launch controll (best with awd)
         *  added antilag (only switches)
         *  fixed state of switch resetting to off when power of car is turned off
         *  module will now be enabled when switch is on "ON" but power is off when power is switched back on
         *  changed Mounting plate to add space for Smart Engine Module ECU (will now handle everything that has to do with the engine) = expensive part
         *  changed Cable Harness to only black
         *  changed Cable Harness to add plug for Smart Engine Module ECU
         */


            //Check: module function stays on when removing part while driving
            //Check: switch resets when part is removed
            //Check: color of light doesn't change back to green when part is back installed
    public override string ID => "DonnerTech_ECU_Mod"; //Your mod ID (unique)
        public override string Name => "DonnerTechRacing ECUs"; //You mod name
        public override string Author => "DonnerPlays"; //Your Username
        public override string Version => "1.0.2"; //Version

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

        public static bool absModuleEnabled = false;
        public static bool espModuleEnabled = false;
        public static bool tcsModuleEnabled = false;
        public static bool alsModuleEnabled = false;
        public static bool stage2revModuleEnabled = false;
        public static bool stage2revSwitchEnabled = false;


        private static ECU_MOD_ABSModule_Part ecu_mod_absModule_Part;
        private static ECU_MOD_ESPModule_Part ecu_mod_espModule_Part;
        private static ECU_MOD_TCSModule_Part ecu_mod_tcsModule_Part;
        private static ECU_MOD_CableHarness_Part ecu_mod_cableHarness_Part;
        private static ECU_MOD_MountingPlate_Part ecu_mod_mountingPlate_Part;
        private static ECU_MOD_ControllPanel_Part ecu_mod_controllPanel_Part;
        private static ECU_MOD_SmartEngineModule_Part ecu_mod_smartEngineModule_Part;

        private static GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;


        private const string ecu_mod_ABSModule_SaveFile = "ecu_mod_ABSModule_partSave.txt";
        private const string ecu_mod_ESPModule_SaveFile = "ecu_mod_ESPModule_partSave.txt";
        private const string ecu_mod_TCSModule_SaveFile = "ecu_mod_TCSModule_partSave.txt";
        private const string ecu_mod_CableHarness_SaveFile = "ecu_mod_CableHarness_partSave.txt";
        private const string ecu_mod_MountingPlate_SaveFile = "ecu_mod_MountingPlate_partSave.txt";
        private const string ecu_mod_ControllPanel_SaveFile = "ecu_mod_ControllPanel_partSave.txt";
        private const string ecu_mod_ModShop_SaveFile = "ecu_mod_ModShop_SaveFile.txt";
        private const string ecu_mod_SmartEngineModule_SaveFile = "ecu_mod_SmartEngineModule_partSave.txt";

        private Settings resetPosSetting = new Settings("resetPos", "Reset uninstalled parts location", new Action(DonnerTech_ECU_Mod.PosReset));

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
            if (ModLoader.CheckSteam() == false)
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
            else
            {
                satsuma = GameObject.Find("SATSUMA(557kg, 248)");
                satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
                satsumaCarController = satsuma.GetComponent<CarController>();
                satsumaAxles = satsuma.GetComponent<Axles>();

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
                        productIcon = assetBundle.LoadAsset<Sprite>("ControllPanel_ProductImage.png"),
                        productPrice = 4600
                    };
                    if (!DonnerTech_ECU_Mod.partBuySave.boughtSmartEngineModule)
                    {
                        ModConsole.Print("Test");
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
            Settings.AddButton(this, resetPosSetting, "reset part location");
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
            if (ModLoader.CheckSteam() == true)
            {
                CheckPartsInstalledTrigger();
                RaycastHit hit;

                if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate && DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_cableHarness_Part.installed && ecu_mod_controllPanel_Part.installed && ecu_mod_mountingPlate_Part.installed && hasPower)
                {
                    GameObject moduleLight = null;
                    if (DonnerTech_ECU_Mod.partBuySave.boughtABSModule)
                    {
                        if (ecu_mod_absModule_Part.installed && absModuleEnabled == false)
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ABS-Light", Color.red);
                        }
                        else if (ecu_mod_absModule_Part.installed && absModuleEnabled == true)
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ABS-Light", Color.green);
                        }
                        else
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ABS-Light", new Color(1f, 1f, 1f, 1f));
                        }
                    }

                    if (DonnerTech_ECU_Mod.partBuySave.boughtESPModule)
                    {
                        if (ecu_mod_espModule_Part.installed && espModuleEnabled == false)
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ESP-Light", Color.red);
                        }
                        else if (ecu_mod_espModule_Part.installed && espModuleEnabled == true)
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ESP-Light", Color.green);
                        }
                        else
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ESP-Light", new Color(1f, 1f, 1f, 1f));
                        }
                    }

                    if (DonnerTech_ECU_Mod.partBuySave.boughtTCSModule)
                    {
                        if (ecu_mod_tcsModule_Part.installed && tcsModuleEnabled == false)
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_TCS-Light", Color.red);
                        }
                        else if (ecu_mod_tcsModule_Part.installed && tcsModuleEnabled == true)
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_TCS-Light", Color.green);
                        }
                        else
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_TCS-Light", new Color(1f, 1f, 1f, 1f));
                        }
                    }
                    if (DonnerTech_ECU_Mod.partBuySave.boughtSmartEngineModule)
                    {
                        if (ecu_mod_smartEngineModule_Part.installed && stage2revSwitchEnabled == false)
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_s2Rev-Light", Color.red);
                        }
                        else if (ecu_mod_absModule_Part.installed && stage2revSwitchEnabled == true)
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_s2Rev-Light", Color.green);
                        }
                        else
                        {
                            ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_s2Rev-Light", new Color(1f, 1f, 1f, 1f));
                        }

                        if (stage2revSwitchEnabled && satsumaDriveTrain.velo > 5.5f)
                        {
                            ToggleStage2RevSwitch();
                        }

                        if (stage2revSwitchEnabled && satsumaDriveTrain.velo < 5.5f)
                        {
                            satsumaDriveTrain.revLimiterTime = 0;
                        }
                        else
                        {
                            satsumaDriveTrain.revLimiterTime = 0.2f;
                        }
                    }

                }
                else if(DonnerTech_ECU_Mod.partBuySave.boughtControllPanel || (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && !hasPower))
                {
                    ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ABS-Light", new Color(1f, 1f, 1f, 1f));
                    ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_ESP-Light", new Color(1f, 1f, 1f, 1f));
                    ChangeColorOfLight("ECU-Mod_Controll-Panel_v2_TCS-Light", new Color(1f, 1f, 1f, 1f));
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
                }




                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1f, 1 << LayerMask.NameToLayer("DontCollide")))
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

                                /*
                                if (gameObjectHit.name == "ECU-Mod_Controll-Panel_v2_ESP-Switch_OFF")
                                {
                                    gameObjectHit.GetComponentInChildren<MeshRenderer>().enabled = false;
                                    GameObject.Find("ECU-Mod_Controll-Panel_v2_ESP-Switch_ON").GetComponentInChildren<MeshRenderer>().enabled = true;
                                }
                                else if(gameObjectHit.name == "ECU-Mod_Controll-Panel_v2_ESP-Switch_ON")
                                {
                                    gameObjectHit.GetComponentInChildren<MeshRenderer>().enabled = false;
                                    GameObject.Find("ECU-Mod_Controll-Panel_v2_TCS-Switch_OFF").GetComponentInChildren<MeshRenderer>().enabled = true;
                                }

                                if (gameObjectHit.name == "ECU-Mod_Controll-Panel_v2_TCS-Switch_OFF")
                                {
                                    gameObjectHit.GetComponentInChildren<MeshRenderer>().enabled = false;
                                    GameObject.Find("ECU-Mod_Controll-Panel_v2_TCS-Switch_ON").GetComponentInChildren<MeshRenderer>().enabled = true;
                                }
                                else if(gameObjectHit.name == "ECU-Mod_Controll-Panel_v2_TCS-Switch_ON")
                                {
                                    GameObject.Find("ECU-Mod_Controll-Panel_v2_TCS-Switch_OFF").GetComponentInChildren<MeshRenderer>().enabled = true;
                                }
                                */
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
                ecu_mod_ABSModule_Trigger.triggerGameObject.SetActive(true);
                ecu_mod_ESPModule_Trigger.triggerGameObject.SetActive(true);
                ecu_mod_TCSModule_Trigger.triggerGameObject.SetActive(true);
                ecu_mod_CableHarness_Trigger.triggerGameObject.SetActive(true);
                ecu_mod_SmartEngineModule_Trigger.triggerGameObject.SetActive(true);
            }
            else
            {
                if(ecu_mod_absModule_Part.installed)
                {
                    if (absModuleEnabled)
                    {
                        ToggleABS();
                    }
                    ecu_mod_absModule_Part.removePart();
                }
                if (ecu_mod_espModule_Part.installed)
                {
                    if (espModuleEnabled)
                    {
                        ToggleESP();
                    }
                    ecu_mod_espModule_Part.removePart();
                }
                if (ecu_mod_tcsModule_Part.installed)
                {
                    if (tcsModuleEnabled)
                    {
                        ToggleTCS();
                    }
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

                ecu_mod_ABSModule_Trigger.triggerGameObject.SetActive(false);
                ecu_mod_ESPModule_Trigger.triggerGameObject.SetActive(false);
                ecu_mod_TCSModule_Trigger.triggerGameObject.SetActive(false);
                ecu_mod_CableHarness_Trigger.triggerGameObject.SetActive(false);
                ecu_mod_SmartEngineModule_Trigger.triggerGameObject.SetActive(false);
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
            moduleLight.GetComponent<MeshRenderer>().material.color = color;
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
                
            }
        }

        private void ToggleALSSwitch()
        {
            if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_controllPanel_Part.installed)
            {
                GameObject alsSwitch = GameObject.Find("ECU-Mod_Controll-Panel_v2_ALS-Switch");
                if (alsModuleEnabled)
                {
                    alsSwitch.transform.localPosition = new Vector3(0f, 0f, 0f);
                    alsSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, 0f) };
                }
                else
                {
                    alsSwitch.transform.localPosition = new Vector3(-0.03855515f, -0.01058602f, 4.656613e-10f);
                    alsSwitch.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0f, 0f, -180f) };
                }
                alsModuleEnabled = !alsModuleEnabled;
            }
        }

        public static void ToggleStage2Rev()
        {
            if (DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && DonnerTech_ECU_Mod.partBuySave.boughtMountingPlate && DonnerTech_ECU_Mod.partBuySave.boughtControllPanel && ecu_mod_cableHarness_Part.installed && ecu_mod_controllPanel_Part.installed && ecu_mod_mountingPlate_Part.installed && hasPower)
            {
                if(stage2revSwitchEnabled && satsumaDriveTrain.velo < 5.5f)
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
                ToggleStage2Rev();
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

    }
}
