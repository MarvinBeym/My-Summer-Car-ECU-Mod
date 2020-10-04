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

        private FuelSystem fuel_system;


        public float[,] fuelMap;

        private Drivetrain satsumaDriveTrain;

        // Use this for initialization
        void Start()
        {
            this.satsumaDriveTrain = fuel_system.satsumaDriveTrain;
        }

       
        void Update()
        {
            if (fuel_system.allInstalled && fuel_system.mod.smart_engine_module_part.InstalledScrewed())
            {


                for (int index = 0; index < fuel_system.chip_parts.Count; index++)
                {
                    ChipPart part = fuel_system.chip_parts[index];
                    if (part.installed && part.chipSave.chipProgrammed)
                    {
                        fuelMap = part.chipSave.map;
                        break;
                    }
                }

                if (fuelMap != null && satsumaDriveTrain.rpm > 0)
                {

                    int mapRpmIndex = GetRPMIndex(Convert.ToInt32(satsumaDriveTrain.rpm));
                    int mapThrottleIndex = GetThrottleIndex((int)(fuel_system.axisCarController.throttle) * 100);
                    fuel_system.racingCarb_adjustAverage.Value = fuelMap[mapThrottleIndex, mapRpmIndex];
                }
                else
                {
                    //Prevent engine from starting/running
                    //Maybe show error on info panel
                }
            }

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

        public void Init(FuelSystem fuel_system)
        {
            this.fuel_system = fuel_system;
        }
    }
}