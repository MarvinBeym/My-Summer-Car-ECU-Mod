using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DonnerTech_ECU_Mod.part;
using MscModApi.Caching;
using MscModApi.Parts.EventSystem;
using MscModApi.Tools;
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
		public bool allow_sound = true;
		public Action action = Action.None;

		public void AirrideSound(bool playOrStop = true)
		{
			if (playOrStop)
			{
				//Play
				if (allow_sound)
				{
					if (!airride_playing)
					{
						airride_playing = true;
						airride_sound.Play();
					}
				}
				else
				{
					allow_sound = true;
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
				if (allow_sound)
				{
					if (!compressor_playing)
					{
						compressor_playing = true;
						compressor_sound.Play();
					}
				}
				else
				{
					allow_sound = true;
				}

				return;
			}

			//Stop
			compressor_sound.Stop();
			compressor_playing = false;
			return;
		}

		public Airride(string pageName, InfoPanel infoPanel, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanel, infoPanelBaseInfo)
		{
			airrideLogic = infoPanel.AddEventBehaviour<Airride_Logic>(PartEvent.Type.Install);
			airrideLogic.Init(this, mod);

			airride_audioSource = CarH.satsuma.AddComponent<AudioSource>();
			airride_sound.audioSource = airride_audioSource;

			compressor_audioSource = CarH.satsuma.AddComponent<AudioSource>();
			compressor_sound.audioSource = compressor_audioSource;

			airride_sound.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), "airride_sound.wav"), true,
				false);
			compressor_sound.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), "compressor_sound.wav"),
				true, false);
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
			infoPanel
				.SetDisplayValue(InfoPanel.VALUE_1, "UP")
				.SetDisplayValue(InfoPanel.VALUE_2, "DOWN")
				.SetDisplayValue(InfoPanel.VALUE_3, "HIGH")
				.SetDisplayValue(InfoPanel.VALUE_4, "LOW")
				.SetDisplayValue(InfoPanel.VALUE_13, CarH.running ? "INFINITE" : "LOW")
				.SetDisplayValue(InfoPanel.VALUE_16, "All");
		}

		public override void Handle()
		{
			logic.HandleTouchPresses(guiTexts, this);
			DisplayValues();
		}

		public override void Pressed_Display_Value(string value)
		{
			switch (value)
			{
				case "Increase Pressure":
					action = Action.Increase;
					allow_sound = false;
					break;
				case "Decrease Pressure":
					action = Action.Decrease;
					allow_sound = false;
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
		}
	}
}