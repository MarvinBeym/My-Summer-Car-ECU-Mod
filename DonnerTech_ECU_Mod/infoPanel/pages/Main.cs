using DonnerTech_ECU_Mod.timers;
using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    class Main : InfoPanelPage
    {
        private GameObject needle;

        private FsmInt odometerKM;
        private FsmFloat coolantPressurePSI;
        private FsmFloat coolantTemp;
        private FsmFloat clockHours;
        private FsmFloat voltage;

        private FsmBool carbInstalled;
        private FsmBool racingCarbInstalled;
        private FsmBool twinCarbInstalled;

        private FsmFloat afRatioCarb;
        private FsmFloat afRatioRacingCarb;
        private FsmFloat afRatioTwinCarb;

        private float timerFuel = 0;
        private float fuelConsumption = 0;
        private float oldValue = 0;
        private float consumptionCounter = 0;
        private FsmFloat fuelLeft;

        private Timer clockUpdateTimer;
        private Timer gearUpdateTimer;
        private Timer odometerUpdateTimer;
        private Timer voltageUpdateTimer;

        public Main(string pageName, GameObject needle, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanelBaseInfo)
        {
            this.needle = needle;
            needleUsed = true;

            PlayMakerFSM carbFSM = Game.Find("Carburator").GetComponent<PlayMakerFSM>();
            PlayMakerFSM racingCarbFSM = Game.Find("Racing Carburators").GetComponent<PlayMakerFSM>();
            PlayMakerFSM twinCarb = Game.Find("Twin Carburators").GetComponent<PlayMakerFSM>();

            carbInstalled = carbFSM.FsmVariables.FindFsmBool("Installed");
            racingCarbInstalled = racingCarbFSM.FsmVariables.FindFsmBool("Installed");
            twinCarbInstalled = twinCarb.FsmVariables.FindFsmBool("Installed");

            afRatioCarb = carbFSM.FsmVariables.FindFsmFloat("IdleAdjust");
            afRatioRacingCarb = racingCarbFSM.FsmVariables.FindFsmFloat("AdjustAverage");
            afRatioTwinCarb = twinCarb.FsmVariables.FindFsmFloat("IdleAdjust");

            GameObject dataBaseMechanics = Game.Find("Database/DatabaseMechanics");
            PlayMakerFSM[] dataBaseMechanicsFSMs = dataBaseMechanics.GetComponentsInChildren<PlayMakerFSM>(true);

            GameObject odometer = Game.Find("dashboard meters(Clone)/Gauges/Odometer");
            PlayMakerFSM odometerFSM = odometer.GetComponentInChildren<PlayMakerFSM>();
            odometerKM = odometerFSM.FsmVariables.FindFsmInt("OdometerReading");

            GameObject electrics = Game.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Electrics");
            PlayMakerFSM electricsFSM = electrics.GetComponent<PlayMakerFSM>();
            voltage = electricsFSM.FsmVariables.FindFsmFloat("Volts");

            
            GameObject cooling = Game.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Cooling").gameObject;
            PlayMakerFSM coolingFSM = PlayMakerFSM.FindFsmOnGameObject(cooling, "Cooling");
            coolantTemp = coolingFSM.FsmVariables.FindFsmFloat("CoolantTemp");
            coolantPressurePSI = coolingFSM.FsmVariables.FindFsmFloat("WaterPressurePSI");

            clockHours = PlayMakerGlobals.Instance.Variables.FindFsmFloat("TimeRotationHour");


            foreach (PlayMakerFSM fsm in dataBaseMechanicsFSMs)
            {
                if (fsm.name == "FuelTank")
                {
                    fuelLeft = fsm.FsmVariables.FindFsmFloat("FuelLevel");
                    break;
                }
            }
            InitTimers();
        }
        public override string[] guiTexts => new string[0];

        public void InitTimers()
        {
            clockUpdateTimer = new Timer(delegate () { display_values["value_13"].text = ConvertToDigitalTime(clockHours.Value); }, 1);
            gearUpdateTimer = new Timer(delegate () { display_values["value_gear"].text = GearToString(); }, 0.1f);
            odometerUpdateTimer = new Timer(delegate () { display_values["value_km"].text = odometerKM.Value.ToString(); }, 1.0f);
            voltageUpdateTimer = new Timer(delegate () { display_values["value_14"].text = voltage.Value.ToString("00 .0") + "V"; }, 0.2f);
        }

        public override void DisplayValues()
        {
            float[] fuelCalculated = FuelKMCalculate();
            
            display_values["value_1"].text = Convert.ToInt32(CarH.drivetrain.rpm).ToString();
            display_values["value_2"].text = Convert.ToInt32(coolantTemp.Value).ToString();
            if (carbInstalled.Value)
            {
                display_values["value_3"].text = afRatioCarb.Value.ToString("00.00");
            }
            else if (racingCarbInstalled.Value)
            {
                display_values["value_3"].text = afRatioRacingCarb.Value.ToString("00.00");
            }
            else if (twinCarbInstalled.Value)
            {
                display_values["value_3"].text = afRatioTwinCarb.Value.ToString("00.00");
            }
            else
            {
                display_values["value_3"].text = "00.00";
            }
            
            display_values["value_4"].text = Convert.ToInt32(coolantPressurePSI.Value).ToString();

            if (fuelCalculated != null && fuelCalculated[1] >= 0 && fuelCalculated[1] <= 9000)
            {
                if (fuelCalculated[1] >= 0 && fuelCalculated[1] <= 9000)
                {
                    display_values["value_15"].text = Convert.ToInt32(fuelCalculated[1]).ToString();
                    display_values["value_16"].text = Convert.ToInt32(fuelCalculated[0]).ToString();
                }
            }
            
            display_values["value_kmh"].text = Convert.ToInt32(CarH.drivetrain.differentialSpeed).ToString();

            clockUpdateTimer.Call();
            voltageUpdateTimer.Call();
            odometerUpdateTimer.Call();
            gearUpdateTimer.Call();

            needle.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(-1f), 0));
        }
        
        public override void Handle()
        {
            logic.HandleTouchPresses(guiTexts, this);
            DisplayValues();
        }

        private string ConvertToDigitalTime(float hours)
        {
            if (hours > 0)
            {
                float calculatedHours = ((360 - hours) / 30) + 2f;
                if (hours > 180)
                {
                    calculatedHours += 12;
                }

                float calculatedMinutes = calculatedHours * 60f;
                TimeSpan ts = TimeSpan.FromMinutes(calculatedMinutes);
                return new DateTime(ts.Ticks).ToString("HH:mm");
            }
            else
            {
                return display_values["value_13"].text;
            }
        }

        private float[] FuelKMCalculate()
        {
            timerFuel += Time.deltaTime;

            oldValue -= fuelLeft.Value;
            if (oldValue > 0)
            {
                fuelConsumption = oldValue;
                consumptionCounter += fuelConsumption;
            }
            oldValue = fuelLeft.Value;

            if (timerFuel >= 1f)
            {
                float Lper100km = Mathf.Clamp((consumptionCounter * 60) * 60, 0, 7);
                float kmLeft = (fuelLeft.Value / Lper100km) * 100;
                timerFuel = 0;
                consumptionCounter = 0;
                return new float[] { Lper100km, kmLeft };
            }
            return null;

        }

        public override void Pressed_Display_Value(string value, GameObject gameObjectHit)
        {
            /*
            switch (value)
            {
                
            }
            playTouchSound(gameObjectHit);
            */
        }
    }
}
