using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    class Page7 : InfoPanelPage
    {
        public const int page = 7;
        private DonnerTech_ECU_Mod mod;
        private GameObject needle;
        private Dictionary<string, TextMesh> display_values;
        private InfoPanel_Logic logic;

        public Page7(DonnerTech_ECU_Mod mod, InfoPanel_Logic logic, GameObject needle, Dictionary<string, TextMesh> display_values)
        {
            this.mod = mod;
            this.needle = needle;
            this.display_values = display_values;
            this.logic = logic;
        }
        public override string[] guiTexts => new string[]
        {
            "",
            "Lowest Pressure",
            "Highest Pressure",
            "Default Pressure",
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
            display_values["value_2"].text = "Lowest";
            display_values["value_3"].text = "Highest";
            display_values["value_4"].text = "Default";
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
                case "Lowest Pressure":
                    //ecu_airride_logic.increaseAirride(true, true, 0.05f);
                   // ecu_airride_operation = "to lowest";
                    break;
                case "Highest Pressure":
                  //  ecu_airride_operation = "to highest";
                    break;
                case "Default Pressure":
                  //  ecu_airride_operation = "to default";
                    break;
                default:
                    playSound = false;
                    break;
            }
            playTouchSound(gameObjectHit);
        }
    }
}
