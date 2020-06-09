using HutongGames.PlayMaker;
using MSCLoader;
using System;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class ECU_SmartEngineModule_Logic : MonoBehaviour
    {
        private Mod mainMod;
        private DonnerTech_ECU_Mod donnerTech_ecu_mod;

        //Car
        private static GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;

        private FsmString playerCurrentVehicle;

        
        private bool alsModule_enabled = false;
        private bool step2RevLimiterModule_enabled = false;

        private PlayMakerFSM smart_engine_moduleFSM;
        private FsmBool smart_engine_module_allInstalled;
        private FsmBool smart_engine_module_alsModuleEnabled;
        private FsmBool smart_engine_module_step2RevLimiterModuleEnabled;

        void Start()
        {
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
            satsumaAxles = satsuma.GetComponent<Axles>();

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");
            satsumaDriveTrain.maxRPM = 8500;

            smart_engine_moduleFSM = this.gameObject.AddComponent<PlayMakerFSM>();
            smart_engine_moduleFSM.FsmName = "Smart Engine Module";


            smart_engine_module_allInstalled = new FsmBool("All installed");
            smart_engine_module_alsModuleEnabled = new FsmBool("ALS Enabled");
            smart_engine_module_step2RevLimiterModuleEnabled = new FsmBool("Step2RevLimiter Enabled");

            smart_engine_moduleFSM.FsmVariables.BoolVariables = new FsmBool[]
            {
                smart_engine_module_alsModuleEnabled,
                smart_engine_module_allInstalled,
                smart_engine_module_step2RevLimiterModuleEnabled
            };
        }


        void Update()
        {
            System.Collections.Generic.List<Mod> mods = ModLoader.LoadedMods;
            Mod[] modsArr = mods.ToArray();
            foreach (Mod mod in modsArr)
            {
                if (mod.Name == "DonnerTechRacing ECUs")
                {
                    mainMod = mod;
                    break;
                }
            }
            donnerTech_ecu_mod = (DonnerTech_ECU_Mod)mainMod;

            if (donnerTech_ecu_mod.GetSmartEngineModuleInstalledFixed() && !step2RevLimiterModule_enabled && satsumaDriveTrain.maxRPM != 8500)
            {
                satsumaDriveTrain.maxRPM = 8500;
            }

            if (hasPower)
            {
                if(donnerTech_ecu_mod.GetCableHarness_Part().installed && donnerTech_ecu_mod.GetMountingPlate_Part().installed && donnerTech_ecu_mod.GetMountingPlate_Screwable().partFixed)
                {
                    if (playerCurrentVehicle.Value == "Satsuma")
                    {
                        if (step2RevLimiterModule_enabled)
                        {
                            HandleStep2RevLimiterModule();
                        }

                        if (alsModule_enabled)
                        {
                            HandleALSModuleLogic();
                        }
                    }
                }
            }
        }

        private void HandleALSModuleLogic()
        {
            /*
            timeSinceLastBackFire += Time.deltaTime;
            ModCommunication modCommunication = ecu_mod_smartEngineModule_Part.rigidPart.GetComponent<ModCommunication>();
            if (useThrottleButton && satsumaDriveTrain.rpm > 4000 && !als_backfire_allowed)
            {
                als_backfire_allowed = true;
            }
            else if (!useThrottleButton && satsumaDriveTrain.rpm > 4000 && als_backfire_allowed)
            {
                als_backfire_allowed = false;
                modCommunication.alsEnabled = true;
                timeSinceLastBackFire = 0;
            }

            if (alsModuleEnabled && timeSinceLastBackFire >= 0.5)
            {
                modCommunication.alsEnabled = false;
            }
            */
        }

        private void HandleStep2RevLimiterModule()
        {
            if(step2RevLimiterModule_enabled && satsumaDriveTrain.velo < 3.5f)
            {
                satsumaDriveTrain.revLimiterTime = 0;
                satsumaDriveTrain.maxRPM = donnerTech_ecu_mod.GetStep2RevRpm();
            }
            else
            {
                satsumaDriveTrain.revLimiterTime = 0.2f;
                if (step2RevLimiterModule_enabled)
                {
                    ToggleStep2RevLimiter();
                }
            }
        }

        public void ToggleALS()
        {
            alsModule_enabled = !alsModule_enabled;
            smart_engine_module_alsModuleEnabled.Value = alsModule_enabled;
        }
        public void ToggleStep2RevLimiter()
        {
            step2RevLimiterModule_enabled = !step2RevLimiterModule_enabled;
            smart_engine_module_step2RevLimiterModuleEnabled.Value = step2RevLimiterModule_enabled;
            if (!step2RevLimiterModule_enabled)
            {
                satsumaDriveTrain.maxRPM = 8500;
            }
        }
        public bool GetAlsEnabled()
        {
            return alsModule_enabled;
        }
        public bool GetStep2RevLimiterEnabled()
        {
            return step2RevLimiterModule_enabled;
        }

        public void SetAlsModuleEnabled(bool enabled)
        {
            this.alsModule_enabled = enabled;
        }

        public void SetStep2RevModuleEnabled(bool enabled)
        {
            this.step2RevLimiterModule_enabled = enabled;
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
    }
}