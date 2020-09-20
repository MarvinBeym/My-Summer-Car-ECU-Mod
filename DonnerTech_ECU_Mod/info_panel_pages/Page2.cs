using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    class Page2 : InfoPanelPage
    {
        public const int page = 2;
        private DonnerTech_ECU_Mod mod;
        private GameObject needle;
        private Dictionary<string, TextMesh> display_values;
        private InfoPanel_Logic logic;

        private FsmFloat wearAlternator;
        private FsmFloat wearClutch;
        private FsmFloat wearCrankshaft;
        private FsmFloat wearFuelpump;
        private FsmFloat wearGearbox;
        private FsmFloat wearHeadgasket;
        private FsmFloat wearRockershaft;
        private FsmFloat wearStarter;
        private FsmFloat wearPiston1;
        private FsmFloat wearPiston2;
        private FsmFloat wearPiston3;
        private FsmFloat wearPiston4;
        private FsmFloat wearWaterpump;
        private FsmFloat wearHeadlightBulbLeft;
        private FsmFloat wearHeadlightBulbRight;

        public Page2(DonnerTech_ECU_Mod mod, InfoPanel_Logic logic, GameObject needle, Dictionary<string, TextMesh> display_values)
        {
            this.mod = mod;
            this.needle = needle;
            this.display_values = display_values;
            this.logic = logic;

            PlayMakerFSM mechanicalWear = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/MechanicalWear").GetComponent<PlayMakerFSM>();

            wearAlternator = mechanicalWear.FsmVariables.FindFsmFloat("WearAlternator");
            wearClutch = mechanicalWear.FsmVariables.FindFsmFloat("WearClutch");
            wearCrankshaft = mechanicalWear.FsmVariables.FindFsmFloat("WearCrankshaft");
            wearFuelpump = mechanicalWear.FsmVariables.FindFsmFloat("WearFuelpump");
            wearGearbox = mechanicalWear.FsmVariables.FindFsmFloat("WearGearbox");
            wearHeadgasket = mechanicalWear.FsmVariables.FindFsmFloat("WearHeadgasket");
            wearRockershaft = mechanicalWear.FsmVariables.FindFsmFloat("WearRockershaft");
            wearStarter = mechanicalWear.FsmVariables.FindFsmFloat("WearStarter");
            wearPiston1 = mechanicalWear.FsmVariables.FindFsmFloat("WearPiston1");
            wearPiston2 = mechanicalWear.FsmVariables.FindFsmFloat("WearPiston2");
            wearPiston3 = mechanicalWear.FsmVariables.FindFsmFloat("WearPiston3");
            wearPiston4 = mechanicalWear.FsmVariables.FindFsmFloat("WearPiston4");
            wearWaterpump = mechanicalWear.FsmVariables.FindFsmFloat("WearWaterpump");

            PlayMakerFSM headlightBulbLeftFSM = GameObject.Find("HeadlightBulbLeft").GetComponent<PlayMakerFSM>();
            PlayMakerFSM headlightBulbRightFSM = GameObject.Find("HeadlightBulbRight").GetComponent<PlayMakerFSM>();
            wearHeadlightBulbLeft = headlightBulbLeftFSM.FsmVariables.FindFsmFloat("Wear");
            wearHeadlightBulbRight = headlightBulbRightFSM.FsmVariables.FindFsmFloat("Wear");
        }
        public override string[] guiTexts => new string[0];

        public override void DisplayValues()
        {
            display_values["value_1"].text = ConvertFloatToWear(wearAlternator.Value);
            display_values["value_2"].text = ConvertFloatToWear(wearClutch.Value);
            display_values["value_3"].text = ConvertFloatToWear(wearCrankshaft.Value);
            display_values["value_4"].text = ConvertFloatToWear(wearFuelpump.Value);
            display_values["value_5"].text = ConvertFloatToWear(wearGearbox.Value);
            display_values["value_6"].text = ConvertFloatToWear(wearHeadgasket.Value);
            display_values["value_7"].text = ConvertFloatToWear(wearRockershaft.Value);
            display_values["value_8"].text = ConvertFloatToWear(wearStarter.Value);
            display_values["value_9"].text = ConvertFloatToWear(wearPiston1.Value);
            display_values["value_10"].text = ConvertFloatToWear(wearPiston2.Value);
            display_values["value_11"].text = ConvertFloatToWear(wearPiston3.Value);
            display_values["value_12"].text = ConvertFloatToWear(wearPiston4.Value);
            display_values["value_13"].text = ConvertFloatToWear(wearWaterpump.Value);
            display_values["value_14"].text = ConvertFloatToWear(wearHeadlightBulbLeft.Value);
            display_values["value_15"].text = ConvertFloatToWear(wearHeadlightBulbRight.Value);
        }

        public override void Handle()
        {
            DisplayValues();
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
