using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
    public class Airride : InfoPanelPage
    {
        public enum Selection
        {
            All,
            Front,
            Rear,
            Left,
            Right,
            FrontLeft,
            FrontRight,
            RearLeft,
            RearRight,
        }
        public enum Action
        {
            Highest,
            Lowest,
            Increase,
            Decrease,
            None,
        }
        private Airride_Logic airrideLogic;

        public ModAudio airride_sound = new ModAudio();
        public AudioSource airride_audioSource;

        public ModAudio compressor_sound = new ModAudio();
        public AudioSource compressor_audioSource;

        public bool atMax = true;
        public bool atMin = false;

        public bool airride_playing = false;
        public bool compressor_playing = false;
        public Action action = Action.None;

        public void AirrideSound(bool playOrStop = true)
        {
            if (playOrStop)
            {
                //Play
                if (!airride_playing)
                {
                    airride_playing = true;
                    airride_sound.Play();
                }
                return;
            }
            //Stop
            airride_sound.Stop();
            airride_playing = false;
            return;
        }
        public void CompressorSound(bool playOrStop = true)
        {
            if (playOrStop)
            {
                //Play
                if (!compressor_playing)
                {
                    compressor_playing = true;
                    compressor_sound.Play();
                }
                return;
            }
            //Stop
            compressor_sound.Stop();
            compressor_playing = false;
            return;
        }
        public Airride(string pageName, DonnerTech_ECU_Mod mod, Dictionary<string, TextMesh> display_values) : base(mod, pageName, display_values)
        {
            airrideLogic = logic.panel_gameObject.AddComponent<Airride_Logic>();
            airrideLogic.Init(this, mod);

            airride_audioSource = mod.info_panel.part.rigidPart.AddComponent<AudioSource>();
            airride_sound.audioSource = airride_audioSource;

            compressor_audioSource = mod.info_panel.part.rigidPart.AddComponent<AudioSource>();
            compressor_sound.audioSource = compressor_audioSource;

            airride_sound.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), "airride_sound.wav"), true, false);
            compressor_sound.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), "compressor_sound.wav"), true, false);
        }
        public override string[] guiTexts => new string[]
        {
            "Increase Pressure",
            "Decrease Pressure",
            "Highest Pressure",
            "Lowest Pressure",
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
            "Switch selected option",
        };

        public override void DisplayValues()
        {
            display_values["value_1"].text = "UP";
            display_values["value_2"].text = "DOWN";
            display_values["value_3"].text = "HIGH";
            display_values["value_4"].text = "LOW";
            display_values["value_5"].text = mod.engineRunning ? "INFINITE" : "LOW";
            display_values["value_16"].text = "All";
        }

        public override void Handle()
        {
            logic.HandleTouchPresses(guiTexts, this);
            DisplayValues();
        }

        public override void Pressed_Display_Value(string value, GameObject gameObjectHit)
        {
            if (mod.engineRunning && action == Action.None)
            {
                switch (value)
                {
                    case "Increase Pressure":
                        action = Action.Increase;
                        break;
                    case "Decrease Pressure":
                        action = Action.Decrease;
                        break;
                    case "Highest Pressure":
                        action = Action.Highest;
                        break;
                    case "Lowest Pressure":
                        action = Action.Lowest;
                        break;
                    default:
                        playSound = false;
                        break;
                }
                playTouchSound(gameObjectHit);
            }

        }
    }
}
