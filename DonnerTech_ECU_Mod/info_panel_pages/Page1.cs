using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    public class Page1 : InfoPanelPage
    {
        public const int page = 1; 
        private DonnerTech_ECU_Mod mod;
        private GameObject needle;
        private Dictionary<string, TextMesh> display_values;
        private InfoPanel_Logic logic;
        private SmartEngineModule_Logic smartEngineLogic;

        private FsmInt odometerKM;


        public Page1(DonnerTech_ECU_Mod mod, InfoPanel_Logic logic, GameObject needle, Dictionary<string, TextMesh> display_values)
        {
            this.mod = mod;
            this.smartEngineLogic = mod.smart_engine_module_logic;
            this.needle = needle;
            this.display_values = display_values;
            this.logic = logic;

            GameObject odometer = GameObject.Find("dashboard meters(Clone)/Gauges/Odometer");
            PlayMakerFSM odometerFSM = odometer.GetComponentInChildren<PlayMakerFSM>();
            this.odometerKM = odometerFSM.FsmVariables.FindFsmInt("OdometerReading");
        }
        public override string[] guiTexts => new string[]
        {
            "Enable ABS",
            "Enable ESP",
            "Enable TCS",
            "Enable 2StepRevLimiter",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "Enable Antilag",
            "",
            "",
            "Select 2Step RPM",
        };

        public override void Handle()
        {
            logic.HandleTouchPresses(guiTexts, this);
            DisplayValues();
        }

        public override void DisplayValues()
        {
            
            display_values["value_1"].text = BoolToOnOffString(smartEngineLogic.absModule_enabled.Value);
            display_values["value_2"].text = BoolToOnOffString(smartEngineLogic.espModule_enabled.Value);
            display_values["value_3"].text = BoolToOnOffString(smartEngineLogic.tcsModule_enabled.Value);
            display_values["value_4"].text = BoolToOnOffString(smartEngineLogic.step2RevLimiterModule_enabled.Value);
            display_values["value_13"].text = BoolToOnOffString(smartEngineLogic.alsModule_enabled.Value);
            display_values["value_16"].text = logic.GetStep2RevRpm().ToString();
            if (logic.GetSelectedSetting() == "Select 2Step RPM")
            {
                display_values["value_16"].color = Color.green;
            }
            else
            {
                display_values["value_16"].color = Color.white;
            }

            display_values["value_km"].text = odometerKM.Value.ToString();
            display_values["value_kmh"].text = Convert.ToInt32(satsumaDriveTrain.differentialSpeed).ToString();
            display_values["value_gear"].text = GearToString();


            needle.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(-1f), 0));
        }

        public override void Pressed_Display_Value(string value, GameObject gameObjectHit)
        {
            switch (value)
            {
                case "Enable ABS":
                    smartEngineLogic.ToggleABS();
                    break;
                case "Enable ESP":
                    smartEngineLogic.ToggleESP();
                    break;
                case "Enable TCS":
                    smartEngineLogic.ToggleTCS();
                    break;
                case "Enable 2StepRevLimiter":
                    smartEngineLogic.Toggle2StepRevLimiter();
                    break;
                case "Enable Antilag":
                    smartEngineLogic.ToggleALS();
                    break;
                case "Select 2Step RPM":
                    if (logic.GetSelectedSetting() == "Select 2Step RPM")
                    {
                        logic.SetSelectedSetting("");
                        break;
                    }
                    logic.SetSelectedSetting("Select 2Step RPM");
                    break;
            }
            playTouchSound(gameObjectHit);
        }
    }
}
