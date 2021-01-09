using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    class Faults2 : InfoPanelPage
    {
        private FsmBool radiator_installed;
        private FsmBool race_radiator_installed;
        private FsmBool oilpan_installed;
        private FsmBool fueltank_installed;
        private FsmBool brakeMasterCylinder_installed;
        private FsmBool clutchMasterCylinder_installed;
        private FsmBool sparkplug1_installed;
        private FsmBool sparkplug2_installed;
        private FsmBool sparkplug3_installed;
        private FsmBool sparkplug4_installed;
        private FsmFloat waterLevelRadiator;
        private FsmFloat waterLevelRaceRadiator;
        private FsmFloat oilLevel;
        private FsmFloat oilContamination;
        private FsmFloat oilGrade;
        private FsmFloat fuelLevel;

        private FsmFloat brakeFluidF;
        private FsmFloat brakeFluidR;
        private FsmFloat clutchFluidLevel;
        private FsmFloat wearSpark1;
        private FsmFloat wearSpark2;
        private FsmFloat wearSpark3;
        private FsmFloat wearSpark4;

        public Faults2(string pageName, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanelBaseInfo)
        {
            PlayMakerFSM race_radiatorFSM = Game.Find("Racing Radiator").GetComponent<PlayMakerFSM>();
            PlayMakerFSM radiatorFSM = Game.Find("Radiator").GetComponent<PlayMakerFSM>();
            PlayMakerFSM oilpanFSM = Game.Find("Oilpan").GetComponent<PlayMakerFSM>();
            PlayMakerFSM brakeMasterCylinderFSM = Game.Find("BrakeMasterCylinder").GetComponent<PlayMakerFSM>();
            PlayMakerFSM clutchMasterCylinderFSM = Game.Find("ClutchMasterCylinder").GetComponent<PlayMakerFSM>();
            PlayMakerFSM sparkPlug1FSM = Game.Find("Sparkplug1").GetComponent<PlayMakerFSM>();
            PlayMakerFSM sparkPlug2FSM = Game.Find("Sparkplug2").GetComponent<PlayMakerFSM>();
            PlayMakerFSM sparkPlug3FSM = Game.Find("Sparkplug3").GetComponent<PlayMakerFSM>();
            PlayMakerFSM sparkPlug4FSM = Game.Find("Sparkplug4").GetComponent<PlayMakerFSM>();
            PlayMakerFSM[] dataBaseMechanicsFSMs = Game.Find("Database/DatabaseMechanics").GetComponentsInChildren<PlayMakerFSM>(true);
            foreach (PlayMakerFSM fsm in dataBaseMechanicsFSMs)
            {
                if (fsm.name == "FuelTank")
                {
                    fuelLevel = fsm.FsmVariables.FindFsmFloat("FuelLevel");
                    fueltank_installed = fsm.FsmVariables.FindFsmBool("Installed");
                    break;
                }
            }

            radiator_installed = radiatorFSM.FsmVariables.FindFsmBool("Installed");
            race_radiator_installed = race_radiatorFSM.FsmVariables.FindFsmBool("Installed");
            oilpan_installed = oilpanFSM.FsmVariables.FindFsmBool("Installed");
            brakeMasterCylinder_installed = brakeMasterCylinderFSM.FsmVariables.FindFsmBool("Installed");
            clutchMasterCylinder_installed = clutchMasterCylinderFSM.FsmVariables.FindFsmBool("Installed");
            sparkplug1_installed = sparkPlug1FSM.FsmVariables.FindFsmBool("Installed");
            sparkplug2_installed = sparkPlug2FSM.FsmVariables.FindFsmBool("Installed");
            sparkplug3_installed = sparkPlug3FSM.FsmVariables.FindFsmBool("Installed");
            sparkplug4_installed = sparkPlug4FSM.FsmVariables.FindFsmBool("Installed");
            waterLevelRadiator = radiatorFSM.FsmVariables.FindFsmFloat("Water");
            waterLevelRaceRadiator = race_radiatorFSM.FsmVariables.FindFsmFloat("Water");
            oilLevel = oilpanFSM.FsmVariables.FindFsmFloat("Oil");
            oilContamination = oilpanFSM.FsmVariables.FindFsmFloat("OilContamination");
            oilGrade = oilpanFSM.FsmVariables.FindFsmFloat("OilGrade");
            brakeFluidF = brakeMasterCylinderFSM.FsmVariables.FindFsmFloat("BrakeFluidF");
            brakeFluidR = brakeMasterCylinderFSM.FsmVariables.FindFsmFloat("BrakeFluidR");
            clutchFluidLevel = clutchMasterCylinderFSM.FsmVariables.FindFsmFloat("ClutchFluid");
            wearSpark1 = sparkPlug1FSM.FsmVariables.FindFsmFloat("Wear");
            wearSpark2 = sparkPlug2FSM.FsmVariables.FindFsmFloat("Wear");
            wearSpark3 = sparkPlug3FSM.FsmVariables.FindFsmFloat("Wear");
            wearSpark4 = sparkPlug4FSM.FsmVariables.FindFsmFloat("Wear");
        }
        public override string[] guiTexts => new string[0];

        public override void DisplayValues()
        {
            if (radiator_installed.Value)
            {
                display_values["value_1"].text = ConvertFloatToPercantage(0.01f, 5.4f, waterLevelRadiator.Value);
            }
            else if (race_radiator_installed.Value)
            {
                display_values["value_1"].text = ConvertFloatToPercantage(0.01f, 7, waterLevelRaceRadiator.Value);
            }

            if (oilpan_installed.Value)
            {
                display_values["value_2"].text = ConvertFloatToPercantage(0.01f, 3, oilLevel.Value);
                display_values["value_3"].text = ConvertFloatToPercantage(1, 100, oilContamination.Value);
                display_values["value_4"].text = oilGrade.Value.ToString("00.00");
            }

            if (clutchMasterCylinder_installed.Value)
            {
                display_values["value_5"].text = ConvertFloatToPercantage(0, 0.5f, clutchFluidLevel.Value);
            }

            if (fueltank_installed.Value)
            {
                display_values["value_6"].text = ConvertFloatToPercantage(0, 36, fuelLevel.Value);
            }

            if (brakeMasterCylinder_installed.Value)
            {
                display_values["value_7"].text = ConvertFloatToPercantage(0, 1, brakeFluidF.Value);
                display_values["value_8"].text = ConvertFloatToPercantage(0, 1, brakeFluidR.Value);
            }

            if (sparkplug1_installed.Value)
            {
                display_values["value_9"].text = ConvertFloatToPercantage(0, 100, wearSpark1.Value);
            }

            if (sparkplug2_installed.Value)
            {
                display_values["value_10"].text = ConvertFloatToPercantage(0, 100, wearSpark2.Value);
            }

            if (sparkplug3_installed.Value)
            {
                display_values["value_11"].text = ConvertFloatToPercantage(0, 100, wearSpark3.Value);
            }

            if (sparkplug4_installed.Value)
            {
                display_values["value_12"].text = ConvertFloatToPercantage(0, 100, wearSpark4.Value);
            }
        }

        public override void Handle()
        {
            DisplayValues();
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
