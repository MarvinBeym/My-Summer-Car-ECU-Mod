using MSCLoader;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class ECU_Airride_Logic : MonoBehaviour
    {
        private GameObject wheelFL, wheelFR, wheelRL, wheelRR;
        private PlayMakerFSM suspension;
        private float originalFrontWheelYPos, originalRearWheelYPos;
        private float ecu_airride_travelRally_min = 0.09f;
        private float ecu_airride_travelRally_max = 0.17f;
        private float ecu_airride_travelRally_default = 0.15f;

        // Use this for initialization
        void Start()
        {
            suspension = suspension = GameObject.Find("Suspension").GetComponent<PlayMakerFSM>();
            wheelFL = GameObject.Find("");
            wheelFR = GameObject.Find("");
            wheelRL = GameObject.Find("");
            wheelRR = GameObject.Find("");
        }

        // Update is called once per frame
        void Update()
        {
            /*
            if (ecu_airride_operation != "")
            {
                if (ecu_airride_operation == "to lowest")
                {
                    float value = Mathf.SmoothStep(25000, 10000, counter);
                    float value2 = Mathf.SmoothStep(ecu_airride_wheelPosRally_default, ecu_airride_wheelPosRally_min, counter);
                    rallyFrontRate.Value = 10000;
                    rallyRearRate.Value = 10000;
                    wheelPosRally.Value = ecu_airride_wheelPosRally_min;
                    if (rallyFrontRate.Value == 0)
                    {
                        counter = 0;
                        ecu_airride_operation = "";
                    }
                }
                else if (ecu_airride_operation == "to highest")
                {

                    float value = Mathf.SmoothStep(10000, 25000, counter);
                    float value2 = Mathf.SmoothStep(ecu_airride_wheelPosRally_min, ecu_airride_wheelPosRally_default, counter);
                    counter += 0.02f;
                    rallyFrontRate.Value = 25000;
                    rallyRearRate.Value = 25000;
                    wheelPosRally.Value = value2;
                    if (wheelPosRally.Value == ecu_airride_wheelPosRally_default)
                    {
                        counter = 0;
                        ecu_airride_operation = "";
                    }
                }
                else if (ecu_airride_operation == "to default")
                {
                    wheelPosRally.Value = ecu_airride_wheelPosRally_default;
                }
                else if (ecu_airride_operation == "increase")
                {
                    ModConsole.Print("tt");
                    if (wheelPosRally.Value < ecu_airride_wheelPosRally_max)
                    {
                        wheelPosRally.Value -= 0.005f;
                    }
                }
                else if (ecu_airride_operation == "decrease")
                {
                    if (wheelPosRally.Value > ecu_airride_wheelPosRally_min)
                    {
                        wheelPosRally.Value =
                    }
                }
            }
            */
        }

        public void decreaseAirride(bool front, bool back, float amount)
        {
            if(front && back)
            {
                
            }
            else if(front && !back)
            {

            }
            else if(!front && back)
            {

            }
        }
        public void increaseAirride(bool front, bool back, float amount)
        {
            if (front && back)
            {

            }
            else if (front && !back)
            {

            }
            else if (!front && back)
            {

            }
        }
    }
}