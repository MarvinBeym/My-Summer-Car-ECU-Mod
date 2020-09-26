using MSCLoader;
using UnityEngine;
using HutongGames.PlayMaker;
using ScrewablePartAPI;
using System;
using ModApi;

namespace DonnerTech_ECU_Mod
{
    public class CruiseControl_Logic : MonoBehaviour
    {
        private DonnerTech_ECU_Mod mod;

        private GameObject cruiseControlPanel;
        private TextMesh cruiseControlText;

        private AudioSource dashButtonAudio;

        private bool allPartsFixed
        {
            get { return (mod.smart_engine_module_part.InstalledScrewed() && mod.cable_harness_part.InstalledScrewed() && mod.mounting_plate_part.InstalledScrewed()); }
        }


        //Car
        private GameObject satsuma;
        private Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;

        private RaycastHit hit;

        //Cruise control
        private int setCruiseControlSpeed = 0;
        private bool cruiseControlModuleEnabled = false;

        // Use this for initialization
        void Start()
        {
            System.Collections.Generic.List<Mod> mods = ModLoader.LoadedMods;
            Mod[] modsArr = mods.ToArray();
            foreach (Mod mod in modsArr)
            {
                if (mod.Name == "DonnerTechRacing ECUs")
                {
                    this.mod = (DonnerTech_ECU_Mod)mod;
                    break;
                }
            }

            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();

            cruiseControlPanel = this.gameObject;
            cruiseControlText = cruiseControlPanel.GetComponentInChildren<TextMesh>();
        }



        // Update is called once per frame
        void Update()
        {
            if (mod.hasPower && allPartsFixed)
            {
                HandleButtonPresses();

                SetCruiseControlSpeedText(setCruiseControlSpeed.ToString());
                if (satsumaDriveTrain.gear != 0 && cruiseControlModuleEnabled && satsumaCarController.throttleInput <= 0f)
                {
                    float valueToThrottle = 0f;
                    if (satsumaDriveTrain.differentialSpeed >= (setCruiseControlSpeed - 0.5) && satsumaDriveTrain.differentialSpeed <= (setCruiseControlSpeed + 0.5))
                    {
                        valueToThrottle = 0.5f;
                    }
                    else if (satsumaDriveTrain.differentialSpeed < (setCruiseControlSpeed - 0.5))
                    {
                        valueToThrottle = 1f;
                    }
                    else if (satsumaDriveTrain.differentialSpeed >= (setCruiseControlSpeed + 0.5f))
                    {
                        valueToThrottle = 0f;
                    }
                    else if (satsumaDriveTrain.differentialSpeed >= setCruiseControlSpeed)
                    {
                        valueToThrottle = 0.3f;
                    }
                    satsumaDriveTrain.idlethrottle = valueToThrottle;
                    if (satsumaDriveTrain.differentialSpeed < 19f || satsumaCarController.brakeInput > 0f || satsumaCarController.clutchInput > 0f || satsumaCarController.handbrakeInput > 0f)
                    {

                        ResetCruiseControl();
                    }
                }
                else if (cruiseControlModuleEnabled && satsumaCarController.throttleInput <= 0f)
                {
                    ResetCruiseControl();
                    setCruiseControlSpeed = 0;

                }
            }
            else if (!mod.hasPower)
            {
                setCruiseControlSpeed = 0;
                cruiseControlModuleEnabled = false;
                SetCruiseControlSpeedText("");
            }
        }

        private void HandleButtonPresses()
        {
            if (Camera.main != null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1f, 1 << LayerMask.NameToLayer("DontCollide")) != false)
                {
                    GameObject gameObjectHit;
                    bool foundObject = false;
                    string guiText = "";
                    gameObjectHit = hit.collider?.gameObject;
                    if (gameObjectHit != null)
                    {
                        Action actionToPerform = null;
                        //CruiseControl Panel
                        if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Minus")
                        {
                            foundObject = true;
                            actionToPerform = DecreaseCruiseControl;
                            guiText = "decrease cruise speed";
                        }
                        if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Plus")
                        {
                            foundObject = true;
                            actionToPerform = IncreaseCruiseControl;
                            guiText = "increase cruise speed";
                        }
                        if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Set")
                        {
                            foundObject = true;
                            actionToPerform = SetCruiseControl;
                            guiText = "set/enable cruise control";
                        }
                        if (gameObjectHit.name == "ECU-Mod_CruiseControlPanel_Switch_Reset")
                        {
                            foundObject = true;
                            actionToPerform = ResetCruiseControl;
                            guiText = "reset/disable cruise control";
                        }

                        if (foundObject)
                        {
                            ModClient.guiInteract(guiText);
                            if (mod.useButtonDown)
                            {
                                actionToPerform.Invoke();
                                AudioSource audio = dashButtonAudioSource;
                                audio.transform.position = gameObjectHit.transform.position;
                                audio.Play();
                            }
                        }
                    }
                }
            }
        }

        private void DecreaseCruiseControl()
        {
            if (setCruiseControlSpeed > 20)
            {
                setCruiseControlSpeed -= 2;
            }

        }
        private void IncreaseCruiseControl()
        {
            setCruiseControlSpeed += 2;
        }
        private void SetCruiseControl()
        {
            if (satsumaDriveTrain.differentialSpeed >= 20)
            {
                int speedToSet = Convert.ToInt32(satsumaDriveTrain.differentialSpeed);
                if (speedToSet % 2 != 0)
                {
                    speedToSet--;
                }
                setCruiseControlSpeed = speedToSet;

                SetCruiseControlSpeedTextColor(Color.green);
                cruiseControlModuleEnabled = true;
            }
        }
        private void ResetCruiseControl()
        {
            if (!cruiseControlModuleEnabled)
            {
                setCruiseControlSpeed = 0;
            }
            SetCruiseControlSpeedTextColor(Color.white);
            cruiseControlModuleEnabled = false;
        }

        private AudioSource dashButtonAudioSource
        {
            get
            {
                if (dashButtonAudio == null)
                {
                    dashButtonAudio =  GameObject.Find("dash_button").GetComponent<AudioSource>();
                }
                return dashButtonAudio;
            }
        }


        private void SetCruiseControlSpeedText(string toSet)
        {
            cruiseControlText.text = toSet;
        }
        private void SetCruiseControlSpeedTextColor(Color colorToSet)
        {
            cruiseControlText.color = colorToSet;
        }
    }
}