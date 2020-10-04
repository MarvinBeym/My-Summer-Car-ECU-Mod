﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    class Page6 : InfoPanelPage
    {
        public const int page = 6;
        private DonnerTech_ECU_Mod mod;
        private GameObject needle;
        private Dictionary<string, TextMesh> display_values;
        private InfoPanel_Logic logic;

        public Page6(DonnerTech_ECU_Mod mod, InfoPanel_Logic logic, GameObject needle, Dictionary<string, TextMesh> display_values)
        {
            this.mod = mod;
            this.needle = needle;
            this.display_values = display_values;
            this.logic = logic;
        }
        public override string[] guiTexts => new string[]
        {
            "Enable Rainsensor",
            "Enable Lightsensor",
            "Select Shift Indicator green line",
            "Select Shift Indicator red line",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
        };

        public override void DisplayValues()
        {
            display_values["value_1"].text = BoolToOnOffString(logic.rainsensor_enabled);
            display_values["value_2"].text = BoolToOnOffString(logic.lightsensor_enabled);
            display_values["value_3"].text = logic.shift_indicator_greenLine.ToString();
            display_values["value_4"].text = logic.shift_indicator_redLine.ToString();
            switch (logic.GetSelectedSetting())
            {
                case "Select Shift Indicator green line":
                    display_values["value_3"].color = Color.green;
                    display_values["value_4"].color = Color.white;
                    break;
                case "Select Shift Indicator red line":
                    display_values["value_3"].color = Color.white;
                    display_values["value_4"].color = Color.green;
                    break;
                default:
                    display_values["value_3"].color = Color.white;
                    display_values["value_4"].color = Color.white;
                    break;

            }
        }

        public override void Handle()
        {
            logic.HandleTouchPresses(guiTexts, this);
            DisplayValues();
        }

        public override void Pressed_Display_Value(string value, GameObject gameObjectHit)
        {
            switch (value)
            {
                case "Enable Rainsensor":
                    logic.rainsensor_enabled = !logic.rainsensor_enabled;
                    break;
                case "Enable Lightsensor":
                    logic.lightsensor_enabled = !logic.lightsensor_enabled;
                    break;
                case "Select Shift Indicator green line":
                    if (logic.GetSelectedSetting() == "Select Shift Indicator green line")
                    {
                        logic.SetSelectedSetting("");
                        break;
                    }
                    logic.SetSelectedSetting("Select Shift Indicator green line");
                    break;
                case "Select Shift Indicator red line":
                    if (logic.GetSelectedSetting() == "Select Shift Indicator red line")
                    {
                        logic.SetSelectedSetting("");
                        break;
                    }
                    logic.SetSelectedSetting("Select Shift Indicator red line");
                    break;
            }
            playTouchSound(gameObjectHit);
        }
    }
}