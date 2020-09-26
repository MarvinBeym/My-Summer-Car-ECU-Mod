using HutongGames.PlayMaker;
using MSCLoader;
using System;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class SmartEngineModule_Logic : MonoBehaviour
    {
        private DonnerTech_ECU_Mod mod;

        //Car
        private static GameObject satsuma;
        private static Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;

        private FsmString playerCurrentVehicle;

        private PlayMakerFSM smart_engine_moduleFSM;
        private FsmBool smart_engine_module_allInstalled;
        
        public FsmBool absModule_enabled { get; set; }
        public FsmBool espModule_enabled { get; set; }
        public FsmBool tcsModule_enabled { get; set; }
        public FsmBool alsModule_enabled { get; set; }
        public FsmBool step2RevLimiterModule_enabled { get; set; }
       

        void Start()
        {
            System.Collections.Generic.List<Mod> mods = ModLoader.LoadedMods;
            Mod[] modsArr = mods.ToArray();
            foreach (Mod mod in modsArr)
            {
                if (mod.Name == "DonnerTechRacing ECUs")
                {
                    this.mod = (DonnerTech_ECU_Mod)mod;
                    break;
                }
            }

            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
            satsumaAxles = satsuma.GetComponent<Axles>();

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");
            satsumaDriveTrain.maxRPM = 8500;

            smart_engine_moduleFSM = this.gameObject.AddComponent<PlayMakerFSM>();
            smart_engine_moduleFSM.FsmName = "Smart Engine Module";


            smart_engine_module_allInstalled = new FsmBool("All installed");
            alsModule_enabled = new FsmBool("ALS Enabled");
            step2RevLimiterModule_enabled = new FsmBool("Step2RevLimiter Enabled");

            smart_engine_moduleFSM.FsmVariables.BoolVariables = new FsmBool[]
            {
                smart_engine_module_allInstalled,
                absModule_enabled,
                espModule_enabled,
                tcsModule_enabled,
                alsModule_enabled,
                step2RevLimiterModule_enabled
            };
        }


        void Update()
        {
            if (mod.smart_engine_module_part.InstalledScrewed() && !step2RevLimiterModule_enabled.Value && satsumaDriveTrain.maxRPM != 8500)
            {
                satsumaDriveTrain.maxRPM = 8500;
            }

            if (mod.hasPower)
            {
                if(mod.cable_harness_part.installed && mod.mounting_plate_part.installed && mod.mounting_plate_part.InstalledScrewed())
                {
                    if (playerCurrentVehicle.Value == "Satsuma")
                    {
                        if (step2RevLimiterModule_enabled.Value)
                        {
                            HandleStep2RevLimiterModule();
                        }

                        if (alsModule_enabled.Value)
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

        public void ToggleABS()
        {
            if (mod.abs_module_part.InstalledScrewed())
            {
                satsuma.GetComponent<CarController>().ABS = !satsuma.GetComponent<CarController>().ABS;
                absModule_enabled.Value = satsuma.GetComponent<CarController>().ABS;
            }
            else
            {
                satsuma.GetComponent<CarController>().ABS = false;
                absModule_enabled.Value = false;
            }


        }
        public void ToggleESP()
        {
            if (mod.esp_module_part.InstalledScrewed())
            {
                satsuma.GetComponent<CarController>().ESP = !satsuma.GetComponent<CarController>().ESP;
                espModule_enabled.Value = satsuma.GetComponent<CarController>().ESP;
            }
            else
            {
                satsuma.GetComponent<CarController>().ESP = false;
                espModule_enabled.Value = false;
            }

        }
        public void ToggleTCS()
        {
            if (mod.tcs_module_part.InstalledScrewed())
            {
                satsuma.GetComponent<CarController>().TCS = !satsuma.GetComponent<CarController>().TCS;
                tcsModule_enabled.Value = satsuma.GetComponent<CarController>().TCS;
            }
            else
            {
                satsuma.GetComponent<CarController>().TCS = false;
                tcsModule_enabled.Value = false;
            }

        }

        public void ToggleALS()
        {
            alsModule_enabled.Value = !alsModule_enabled.Value;
        }
        public void Toggle2StepRevLimiter()
        {
            step2RevLimiterModule_enabled.Value = !step2RevLimiterModule_enabled.Value;
        }

        public void HandleStep2RevLimiterModule()
        {
            if(step2RevLimiterModule_enabled.Value && satsumaDriveTrain.velo < 3.5f)
            {
                satsumaDriveTrain.revLimiterTime = 0;
                satsumaDriveTrain.maxRPM = mod.GetStep2RevRpm();
            }
            else
            {
                satsumaDriveTrain.revLimiterTime = 0.2f;
                if (step2RevLimiterModule_enabled.Value)
                {
                    Toggle2StepRevLimiter();
                }
            }
        }
    }
}