using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    class Turbocharger : InfoPanelPage
    {
        private GameObject turbine;
        private PlayMakerFSM turbocharger_bigFSM;
        private FsmFloat turbocharger_big_rpm;
        private FsmFloat turbocharger_big_pressure;
        private FsmFloat turbocharger_big_max_boost;
        private FsmFloat turbocharger_big_wear;
        private FsmFloat turbocharger_big_exhaust_temp;
        private FsmFloat turbocharger_big_intake_temp;
        private FsmBool turbocharger_big_allInstalled;

        private PlayMakerFSM turbocharger_smallFSM;
        private FsmFloat turbocharger_small_rpm;
        private FsmFloat turbocharger_small_pressure;
        private FsmFloat turbocharger_small_max_boost;
        private FsmFloat turbocharger_small_wear;
        private FsmFloat turbocharger_small_exhaust_temp;
        private FsmFloat turbocharger_small_intake_temp;
        private FsmBool turbocharger_small_allInstalled;

        public Turbocharger(string pageName, DonnerTech_ECU_Mod mod, GameObject turbine, Dictionary<string, TextMesh> display_values) : base(mod, pageName,  display_values)
        {
            this.turbine = turbine;
            turbineUsed = true;
        }
        public override string[] guiTexts => new string[0];

        public override void DisplayValues()
        {
            turbine.transform.Rotate(0f, 0f, 40 * Time.deltaTime);

            if (turbocharger_bigFSM != null && turbocharger_big_allInstalled.Value)
            {
                if (turbocharger_big_pressure.Value >= 0f)
                {
                    display_values["value_1"].text = turbocharger_big_pressure.Value.ToString("0.00");
                }
                else
                {
                    display_values["value_1"].text = "0.00";
                }
                display_values["value_2"].text = turbocharger_big_max_boost.Value.ToString("0.00");
                display_values["value_14"].text = turbocharger_big_exhaust_temp.Value.ToString("000");
                display_values["value_15"].text = turbocharger_big_intake_temp.Value.ToString("000");
                display_values["value_16"].text = turbocharger_big_rpm.Value.ToString("0");
            }
            else if (turbocharger_smallFSM != null && turbocharger_small_allInstalled.Value)
            {
                if (turbocharger_small_pressure.Value >= 0f)
                {
                    display_values["value_1"].text = turbocharger_small_pressure.Value.ToString("0.00");
                }
                else
                {
                    display_values["value_1"].text = "0.00";
                }
                display_values["value_2"].text = turbocharger_small_max_boost.Value.ToString("0.00");
                display_values["value_14"].text = turbocharger_small_exhaust_temp.Value.ToString("000");
                display_values["value_15"].text = turbocharger_small_intake_temp.Value.ToString("000");
                display_values["value_16"].text = turbocharger_small_rpm.Value.ToString("0");
            }
        }

        public override void Handle()
        {
            if (ModLoader.IsModPresent("SatsumaTurboCharger"))
            {
                if(turbocharger_bigFSM == null)
                {
                    CheckTurboBigFsm();
                }
                if(turbocharger_smallFSM == null)
                {

                }
                DisplayValues();
            }
        }

        private void CheckTurboBigFsm()
        {
            try
            {
                GameObject racingTurbo = GameObject.Find("Racing Turbocharger(Clone)");
                if (racingTurbo != null)
                {
                    turbocharger_bigFSM = racingTurbo.GetComponent<PlayMakerFSM>();
                    if (turbocharger_bigFSM != null)
                    {
                        turbocharger_big_rpm = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Rpm");
                        turbocharger_big_pressure = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Pressure");
                        turbocharger_big_max_boost = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Max boost");
                        turbocharger_big_wear = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Wear");
                        turbocharger_big_exhaust_temp = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Exhaust temperature");
                        turbocharger_big_intake_temp = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Intake temperature");
                        turbocharger_big_allInstalled = turbocharger_bigFSM.FsmVariables.FindFsmBool("All installed");
                    }
                }

            }
            catch
            {

            }
        }
        private void CheckTurboSmallFsm()
        {
            try
            {
                GameObject gtTurbo = GameObject.Find("GT Turbocharger(Clone)");
                if (gtTurbo != null)
                {
                    turbocharger_smallFSM = gtTurbo.GetComponent<PlayMakerFSM>();
                    if (turbocharger_smallFSM != null)
                    {
                        turbocharger_small_rpm = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Rpm");
                        turbocharger_small_pressure = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Pressure");
                        turbocharger_small_max_boost = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Max boost");
                        turbocharger_small_wear = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Wear");
                        turbocharger_small_exhaust_temp = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Exhaust temperature");
                        turbocharger_small_intake_temp = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Intake temperature");
                        turbocharger_small_allInstalled = turbocharger_smallFSM.FsmVariables.FindFsmBool("All installed");
                    }
                }

            }
            catch
            {

            }
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
