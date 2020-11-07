using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    public class Modules : InfoPanelPage
    {
        private GameObject needle;
        private SmartEngineModule_Logic smartEngineLogic;

        private FsmInt odometerKM;


        public Modules(string pageName, DonnerTech_ECU_Mod mod, GameObject needle, Dictionary<string, TextMesh> display_values) : base(mod, pageName,  display_values)
        {
            this.smartEngineLogic = mod.smart_engine_module_logic;
            this.needle = needle;
            needleUsed = true;

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
            display_values["value_1"].text = BoolToOnOffString(mod.absModule_enabled.Value);
            display_values["value_2"].text = BoolToOnOffString(mod.espModule_enabled.Value);
            display_values["value_3"].text = BoolToOnOffString(mod.tcsModule_enabled.Value);
            display_values["value_4"].text = BoolToOnOffString(mod.step2RevLimiterModule_enabled.Value);
            display_values["value_13"].text = BoolToOnOffString(mod.alsModule_enabled.Value);
            display_values["value_16"].text = mod.step2_rpm.Value.ToString();
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
