using UnityEngine;
using System.Collections;
using DonnerTech_ECU_Mod;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using DonnerTech_ECU_Mod.fuelsystem;
using System;
using MSCLoader;

namespace DonnerTech_ECU_Mod
{
    public class FuelSystemLogic : MonoBehaviour
    {

        private DonnerTech_ECU_Mod mod;
        private FuelSystem fuel_system;
        private bool startAssistEnabled = false;

        public float[,] fuelMap;

        private Drivetrain satsumaDriveTrain;
        private Transform throttle_body1_valve;
        private Transform throttle_body2_valve;
        private Transform throttle_body3_valve;
        private Transform throttle_body4_valve;

        // Use this for initialization
        void Start()
        {
            throttle_body1_valve = fuel_system.throttle_body1_part.rigidPart.transform.FindChild("Butterfly-Valve");
            throttle_body2_valve = fuel_system.throttle_body2_part.rigidPart.transform.FindChild("Butterfly-Valve");
            throttle_body3_valve = fuel_system.throttle_body3_part.rigidPart.transform.FindChild("Butterfly-Valve");
            throttle_body4_valve = fuel_system.throttle_body4_part.rigidPart.transform.FindChild("Butterfly-Valve");
            this.satsumaDriveTrain = fuel_system.satsumaDriveTrain;
        }

       
        void Update()
        {
            if (mod.hasPower)
            {
                if (
                    (bool)mod.settingThrottleBodieTurning.Value
                    && fuel_system.throttle_body1_part.InstalledScrewed()
                    && fuel_system.throttle_body2_part.InstalledScrewed()
                    && fuel_system.throttle_body3_part.InstalledScrewed()
                    && fuel_system.throttle_body4_part.InstalledScrewed()
                    )
                {
                    HandleThrottleBodyMovement();
                }

                if (fuel_system.allInstalled && mod.smart_engine_module_part.InstalledScrewed())
                {


                    for (int index = 0; index < fuel_system.chip_parts.Count; index++)
                    {
                        ChipPart part = fuel_system.chip_parts[index];
                        if (part.installed && part.chipSave.chipProgrammed)
                        {
                            fuelMap = part.chipSave.map;
                            startAssistEnabled = part.chipSave.startAssistEnabled;
                            fuel_system.distributor_sparkAngle.Value = part.chipSave.sparkAngle;
                            break;
                        }
                    }

                    if(startAssistEnabled)
                    {
                        if(satsumaDriveTrain.differentialSpeed < 15 && (satsumaDriveTrain.gear == 2 || satsumaDriveTrain.gear == 0))
                        {
                            satsumaDriveTrain.canStall = false;
                        }
                        else
                        {
                            satsumaDriveTrain.canStall = true;
                        }
                    }

                    if (fuelMap != null && satsumaDriveTrain.rpm > 0)
                    {
                        int mapRpmIndex = GetRPMIndex(Convert.ToInt32(satsumaDriveTrain.rpm));
                        int mapThrottleIndex = GetThrottleIndex((int)(mod.axisCarController.throttle) * 100);
                        fuel_system.racingCarb_adjustAverage.Value = fuelMap[mapThrottleIndex, mapRpmIndex];
                    }
                    else
                    {
                        fuel_system.allInstalled_applied = false;
                    }
                }
            }
        }

        private void HandleThrottleBodyMovement()
        {
            throttle_body1_valve.localRotation = new Quaternion { eulerAngles = new Vector3(Mathf.Clamp(mod.axisCarController.throttle * 4, 0, 1) * -90, 0, 0) };
            throttle_body2_valve.localRotation = new Quaternion { eulerAngles = new Vector3(Mathf.Clamp(mod.axisCarController.throttle * 4, 0, 1) * -90, 0, 0) };
            throttle_body3_valve.localRotation = new Quaternion { eulerAngles = new Vector3(Mathf.Clamp(mod.axisCarController.throttle * 4, 0, 1) * -90, 0, 0) };
            throttle_body4_valve.localRotation = new Quaternion { eulerAngles = new Vector3(Mathf.Clamp(mod.axisCarController.throttle * 4, 0, 1) * -90, 0, 0) };
        }

        private int GetThrottleIndex(int throttle)
        {
            switch (throttle)
            {
                case int t when t <= 0:
                    return 0;
                case int t when t <= 5:
                    return 1;
                case int t when t <= 25:
                    return 2;
                case int t when t <= 30:
                    return 3;
                case int t when t <= 40:
                    return 4;
                case int t when t <= 45:
                    return 5;
                case int t when t <= 50:
                    return 6;
                case int t when t <= 55:
                    return 7;
                case int t when t <= 60:
                    return 8;
                case int t when t <= 65:
                    return 9;
                case int t when t <= 75:
                    return 10;
                case int t when t <= 80:
                    return 11;
                case int t when t <= 95:
                    return 12;
                default:
                    return 13;
            }
        }

        private int GetRPMIndex(int rpm)
        {
            switch (rpm)
            {
                case int r when r <= 500:
                    return 0;
                case int r when r <= 1000:
                    return 1;
                case int r when r <= 1500:
                    return 2;
                case int r when r <= 2000:
                    return 3;
                case int r when r <= 2500:
                    return 4;
                case int r when r <= 3000:
                    return 5;
                case int r when r <= 3500:
                    return 6;
                case int r when r <= 4000:
                    return 7;
                case int r when r <= 4500:
                    return 8;
                case int r when r <= 5000:
                    return 9;
                case int r when r <= 5500:
                    return 10;
                case int r when r <= 6000:
                    return 11;
                case int r when r <= 6500:
                    return 12;
                case int r when r <= 7000:
                    return 13;
                case int r when r <= 7500:
                    return 14;
                case int r when r <= 8000:
                    return 15;
                default:
                    return 16;
            }
        }

        public void Init(FuelSystem fuel_system, DonnerTech_ECU_Mod mod)
        {
            this.mod = mod;
            this.fuel_system = fuel_system;
        }
    }
}