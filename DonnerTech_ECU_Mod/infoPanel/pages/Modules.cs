using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using UnityEngine;
using static DonnerTech_ECU_Mod.SmartEngineModule_Logic;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    public class Modules : InfoPanelPage
    {
        private GameObject needle;
        private SmartEngineModule_Logic smartEngineLogic;

        private FsmInt odometerKM;


        public Modules(string pageName, GameObject needle, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanelBaseInfo)
        {
            this.smartEngineLogic = mod.smart_engine_module_logic;
            this.needle = needle;
            needleUsed = true;

            GameObject odometer = Game.Find("dashboard meters(Clone)/Gauges/Odometer");
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

        public override void PressedButton(InfoPanel_Logic.PressedButton pressedButton, string action)
        {
            switch (action)
            {
                case "Select 2Step RPM":
                    switch (pressedButton)
                    {
                        case InfoPanel_Logic.PressedButton.Plus:
                            smartEngineLogic.twoStepRpm.Value += 100;
                            break;
                        case InfoPanel_Logic.PressedButton.Minus:
                            smartEngineLogic.twoStepRpm.Value -= 100;
                            break;
                    }
                    if (smartEngineLogic.twoStepRpm.Value >= 10000) { smartEngineLogic.twoStepRpm.Value = 10000; }
                    if (smartEngineLogic.twoStepRpm.Value <= 2000) { smartEngineLogic.twoStepRpm.Value = 2000; }
                    break;
            }
        }

        public override void DisplayValues()
        {
            display_values["value_1"].text = smartEngineLogic.absModuleEnabled.Value.ToOnOff();
            display_values["value_2"].text = smartEngineLogic.espModuleEnabled.Value.ToOnOff();
            display_values["value_3"].text = smartEngineLogic.tcsModuleEnabled.Value.ToOnOff();
            display_values["value_4"].text = smartEngineLogic.twoStepModuleEnabled.Value.ToOnOff();
            display_values["value_13"].text = smartEngineLogic.alsModuleEnabled.Value.ToOnOff();
            display_values["value_16"].text = smartEngineLogic.twoStepRpm.Value.ToString();
            if (logic.GetSelectedSetting() == "Select 2Step RPM")
            {
                display_values["value_16"].color = Color.green;
            }
            else
            {
                display_values["value_16"].color = Color.white;
            }

            display_values["value_km"].text = odometerKM.Value.ToString();
            display_values["value_kmh"].text = Convert.ToInt32(CarH.drivetrain.differentialSpeed).ToString();
            display_values["value_gear"].text = GearToString();


            needle.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(-1f), 0));
        }

        public override void Pressed_Display_Value(string value, GameObject gameObjectHit)
        {
            switch (value)
            {
                case "Enable ABS":
                    smartEngineLogic.ToggleModule(Module.Abs);
                    break;
                case "Enable ESP":
                    smartEngineLogic.ToggleModule(Module.Esp);
                    break;
                case "Enable TCS":
                    smartEngineLogic.ToggleModule(Module.Tcs);
                    break;
                case "Enable 2StepRevLimiter":
                    smartEngineLogic.ToggleModule(Module.TwoStep);
                    break;
                case "Enable Antilag":
                    smartEngineLogic.ToggleModule(Module.Als);
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
            Helper.PlayTouchSound(gameObjectHit);
        }
    }
}
