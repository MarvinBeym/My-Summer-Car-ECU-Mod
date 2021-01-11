using HutongGames.PlayMaker;
using MSCLoader;
using Parts;
using System;
using Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class SmartEngineModule_Logic : MonoBehaviour
    {
        private DonnerTech_ECU_Mod mod;

        private FsmString playerCurrentVehicle;

        private PlayMakerFSM smart_engine_moduleFSM;
        private FsmBool smart_engine_module_allInstalled;
        private AdvPart part;
        private AdvPart absModulePart;
        private AdvPart espModulePart;
        private AdvPart tcsModulePart;

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

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");
            CarH.drivetrain.maxRPM = 8500;

            smart_engine_moduleFSM = this.gameObject.AddComponent<PlayMakerFSM>();
            smart_engine_moduleFSM.FsmName = "Smart Engine Module";


            smart_engine_module_allInstalled = new FsmBool("All installed");
            alsModule_enabled = new FsmBool("ALS Enabled");
            step2RevLimiterModule_enabled = new FsmBool("Step2RevLimiter Enabled");

            smart_engine_moduleFSM.FsmVariables.BoolVariables = new FsmBool[]
            {
                smart_engine_module_allInstalled,
                alsModule_enabled,
                step2RevLimiterModule_enabled
            };
        }


        void Update()
        {
            if (mod.smart_engine_module_part.InstalledScrewed() && !step2RevLimiterModule_enabled.Value && CarH.drivetrain.maxRPM != 8500)
            {
                CarH.drivetrain.maxRPM = 8500;
            }

            if (CarH.hasPower)
            {
                if(mod.cable_harness_part.installed && mod.mounting_plate_part.installed && mod.mounting_plate_part.InstalledScrewed())
                {
                    if (playerCurrentVehicle.Value == "Satsuma")
                    {
                        if (mod.step2RevLimiterModule_enabled.Value)
                        {
                            HandleStep2RevLimiterModule();
                        }

                        if (mod.alsModule_enabled.Value)
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
            if (useThrottleButton && CarH.drivetrain.rpm > 4000 && !als_backfire_allowed)
            {
                als_backfire_allowed = true;
            }
            else if (!useThrottleButton && CarH.drivetrain.rpm > 4000 && als_backfire_allowed)
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
                CarH.carController.ABS = !CarH.carController.ABS;
                mod.absModule_enabled.Value = CarH.carController.ABS;
            }
            else
            {
                CarH.carController.ABS = false;
                mod.absModule_enabled.Value = false;
            }


        }
        public void ToggleESP()
        {
            if (mod.esp_module_part.InstalledScrewed())
            {
                CarH.carController.ESP = !CarH.carController.ESP;
                mod.espModule_enabled.Value = CarH.carController.ESP;
            }
            else
            {
                CarH.carController.ESP = false;
                mod.espModule_enabled.Value = false;
            }

        }
        public void ToggleTCS()
        {
            if (mod.tcs_module_part.InstalledScrewed())
            {
                CarH.carController.TCS = !CarH.carController.TCS;
                mod.tcsModule_enabled.Value = CarH.carController.TCS;
            }
            else
            {
                CarH.carController.TCS = false;
                mod.tcsModule_enabled.Value = false;
            }

        }

        public void ToggleALS()
        {
            mod.alsModule_enabled.Value = !mod.alsModule_enabled.Value;
            alsModule_enabled.Value = !alsModule_enabled.Value;
        }
        public void Toggle2StepRevLimiter()
        {
            mod.step2RevLimiterModule_enabled.Value = !mod.step2RevLimiterModule_enabled.Value;
            step2RevLimiterModule_enabled.Value = !step2RevLimiterModule_enabled.Value;
        }

        public void HandleStep2RevLimiterModule()
        {
            if(mod.step2RevLimiterModule_enabled.Value && CarH.drivetrain.velo < 3.5f)
            {
                CarH.drivetrain.revLimiterTime = 0;
                CarH.drivetrain.maxRPM = mod.step2_rpm.Value;
            }
            else
            {
                CarH.drivetrain.revLimiterTime = 0.2f;
                if (mod.step2RevLimiterModule_enabled.Value)
                {
                    Toggle2StepRevLimiter();
                }
            }
        }

        internal void Init(AdvPart smart_engine_module_part, AdvPart absModulePart, AdvPart espModulePart, AdvPart tcsModulePart)
        {
            part = smart_engine_module_part;
            part.AddOnAssembleAction(OnAssemble);
            part.AddOnDisassembleAction(OnDisassemble);

            this.absModulePart = absModulePart;
            this.espModulePart = espModulePart;
            this.tcsModulePart = tcsModulePart;
        }

        private void OnDisassemble()
        {
            SetAbs(false);
            SetEsp(false);
            SetTcs(false);
        }
        public void SetAbs(bool newStatus)
        {
            if (absModulePart.InstalledScrewed())
            {
                CarH.carController.ABS = newStatus;
            }
            else
            {
                CarH.carController.ABS = false;
            }
        }

        public void SetEsp(bool newStatus)
        {
            if (espModulePart.InstalledScrewed())
            {
                CarH.carController.ESP = newStatus;
            }
            else
            {
                CarH.carController.ESP = false;
            }
        }

        public void SetTcs(bool newStatus)
        {
            if (tcsModulePart.InstalledScrewed())
            {
                CarH.carController.TCS = newStatus;
            }
            else
            {
                CarH.carController.TCS = false;
            }
        }

        private void OnAssemble()
        {
            throw new NotImplementedException();
        }
    }
}