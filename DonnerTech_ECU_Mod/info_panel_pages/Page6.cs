using System;
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
            "",
            "",
        };

        public override void DisplayValues()
        {
            display_values["value_1"].text = BoolToOnOffString(logic.rainsensor_enabled);
            display_values["value_2"].text = BoolToOnOffString(logic.lightsensor_enabled);

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
                    {
                        logic.rainsensor_enabled = !logic.rainsensor_enabled;
                        break;
                    }
                case "Enable Lightsensor":
                    {
                        logic.lightsensor_enabled = !logic.lightsensor_enabled;
                        break;
                    }
            }
            playTouchSound(gameObjectHit);
        }
    }
}
