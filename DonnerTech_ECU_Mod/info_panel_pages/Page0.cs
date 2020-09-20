using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    class Page0 : InfoPanelPage
    {
        public const int page = 0;
        private DonnerTech_ECU_Mod mod;
        private GameObject needle;
        private Dictionary<string, TextMesh> display_values;
        private InfoPanel_Logic logic;

        private FsmInt odometerKM;
        private FsmFloat coolantPressurePSI;
        private FsmFloat coolantTemp;
        private FsmFloat clockHours;
        private FsmFloat clockMinutes;
        private FsmFloat voltage;
        private FsmFloat afRatio;

        private float timerFuel = 0;
        private float fuelConsumption = 0;
        private float oldValue = 0;
        private float consumptionCounter = 0;
        private FsmFloat fuelLeft;

        public Page0(DonnerTech_ECU_Mod mod, InfoPanel_Logic logic, GameObject needle, Dictionary<string, TextMesh> display_values)
        {
            this.mod = mod;
            this.needle = needle;
            this.display_values = display_values;
            this.logic = logic;

            this.afRatio = logic.afRatio;

            GameObject dataBaseMechanics = GameObject.Find("Database/DatabaseMechanics");
            PlayMakerFSM[] dataBaseMechanicsFSMs = dataBaseMechanics.GetComponentsInChildren<PlayMakerFSM>();

            GameObject odometer = GameObject.Find("dashboard meters(Clone)/Gauges/Odometer");
            PlayMakerFSM odometerFSM = odometer.GetComponentInChildren<PlayMakerFSM>();
            odometerKM = odometerFSM.FsmVariables.FindFsmInt("OdometerReading");

            GameObject electrics = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Electrics");
            PlayMakerFSM electricsFSM = electrics.GetComponent<PlayMakerFSM>();
            voltage = electricsFSM.FsmVariables.FindFsmFloat("Volts");

            
            GameObject cooling = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Cooling").gameObject;
            PlayMakerFSM coolingFSM = PlayMakerFSM.FindFsmOnGameObject(cooling, "Cooling");
            coolantTemp = coolingFSM.FsmVariables.FindFsmFloat("CoolantTemp");
            coolantPressurePSI = coolingFSM.FsmVariables.FindFsmFloat("WaterPressurePSI");

            clockHours = PlayMakerGlobals.Instance.Variables.FindFsmFloat("TimeRotationHour");
            clockMinutes = PlayMakerGlobals.Instance.Variables.FindFsmFloat("TimeRotationMinute");


            foreach (PlayMakerFSM fsm in dataBaseMechanicsFSMs)
            {
                if (fsm.name == "FuelTank")
                {
                    fuelLeft = fsm.FsmVariables.FindFsmFloat("FuelLevel");
                    break;
                }
            }
        }
        public override string[] guiTexts => new string[0];

        public override void DisplayValues()
        {
            float[] fuelCalculated = FuelKMCalculate();
            
            display_values["value_1"].text = Convert.ToInt32(satsumaDriveTrain.rpm).ToString();
            display_values["value_2"].text = Convert.ToInt32(coolantTemp.Value).ToString();
            if (afRatio != null)
            {
                display_values["value_3"].text = afRatio.Value.ToString("00.00");
            }
            
            display_values["value_4"].text = Convert.ToInt32(coolantPressurePSI.Value).ToString();
            display_values["value_13"].text = ConvertToDigitalTime(clockHours.Value, clockMinutes.Value);
            display_values["value_14"].text = voltage.Value.ToString("00 .0") + "V";
            if (fuelCalculated != null && fuelCalculated[1] >= 0 && fuelCalculated[1] <= 9000)
            {
                if (fuelCalculated[1] >= 0 && fuelCalculated[1] <= 9000)
                {
                    display_values["value_15"].text = Convert.ToInt32(fuelCalculated[1]).ToString();
                    display_values["value_16"].text = Convert.ToInt32(fuelCalculated[0]).ToString();
                }
            }
            display_values["value_km"].text = odometerKM.Value.ToString();
            display_values["value_kmh"].text = Convert.ToInt32(satsumaDriveTrain.differentialSpeed).ToString();

            display_values["value_gear"].text = GearToString();

            needle.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(-1f), 0));
        }

        public override void Handle()
        {
            logic.HandleTouchPresses(guiTexts, this);
            DisplayValues();
        }

        private string ConvertToDigitalTime(float hours, float minutes)
        {
            string hour = ((360 - hours) / 30f + 2f).ToString("00");
            string minute = (Mathf.FloorToInt((360f - minutes) / 6f)).ToString("00");
            return (hour + ":" + minute);
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
            switch (value)
            {
                
            }
            playTouchSound(gameObjectHit);
        }
    }
}
