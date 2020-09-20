using HutongGames.PlayMaker;
using ModApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    public abstract class InfoPanelPage
    {
        public GameObject satsuma;
        public Drivetrain satsumaDriveTrain;
        public CarController satsumaCarController;
        public Axles satsumaAxles;
        public bool playSound = true;
        private const float needle_maxAngle = 270;
        private const float needle_minAngle = 0;
        private const float needle_maxRPM = 9000;
        private RaycastHit hit;

        public InfoPanelPage()
        {
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
            satsumaAxles = satsuma.GetComponent<Axles>();
        }


        public void playTouchSound(GameObject gameObjectHit)
        {
            if (playSound)
            {
                AudioSource audio = dashButtonAudioSource;
                audio.transform.position = gameObjectHit.transform.position;
                audio.Play();
            }
        }

        private AudioSource dashButtonAudioSource
        {
            get
            {
                return GameObject.Find("dash_button").GetComponent<AudioSource>();
            }
        }

        public string BoolToOnOffString(bool value)
        {
            if (value)
            {
                return "ON";
            }
            return "OFF";
        }
        public string GearToString()
        {
            switch (satsumaDriveTrain.gear)
            {
                case 0:
                    return "R";
                case 1:
                    return "N";
                default:
                    return (satsumaDriveTrain.gear - 1).ToString();
            }
        }
        public float GetRPMRotation(float rpmOverride)
        {
            if (rpmOverride >= 0)
            {
                float totalAngleSize = needle_minAngle - needle_maxAngle;
                float rpmNormalized = rpmOverride / needle_maxRPM;
                return needle_minAngle - rpmNormalized * totalAngleSize;
            }
            else
            {
                float totalAngleSize = needle_minAngle - needle_maxAngle;
                float rpmNormalized = satsumaDriveTrain.rpm / needle_maxRPM;
                return needle_minAngle - rpmNormalized * totalAngleSize;
            }
        }

        public abstract string[] guiTexts { get; }
        public abstract void Handle();
        public abstract void Pressed_Display_Value(string value, GameObject gameObjectHit);
        public abstract void DisplayValues();
        protected string ConvertFloatToWear(float value)
        {
            switch (value)
            {
                case float v when v >= 80:
                    return "Good";
                case float v when v >= 60:
                    return "Warning";
                default:
                    return "DANGER";
            }
        }
        protected string ConvertFloatToPercantage(float min, float max, float value)
        {
            float calculatedPercentage = ((value - min) * 100) / (max - min);
            int intPercentage = Convert.ToInt32(calculatedPercentage);

            return intPercentage.ToString("000") + "%";
        }
    }
}
