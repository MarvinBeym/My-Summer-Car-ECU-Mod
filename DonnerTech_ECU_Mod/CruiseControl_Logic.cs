using MSCLoader;
using UnityEngine;
using HutongGames.PlayMaker;
using System;
using DonnerTech_ECU_Mod.fuelsystem;
using DonnerTech_ECU_Mod.part;
using MscModApi.Caching;
using MscModApi.Tools;

namespace DonnerTech_ECU_Mod
{
	public class CruiseControl_Logic : MonoBehaviour
	{
		private DonnerTech_ECU_Mod mod;

		private CruiseControlPanel cruiseControlPanel;
		private TextMesh cruiseControlText;

		private AudioSource dashButtonAudio;

		private RaycastHit hit;

		//Cruise control
		private int setCruiseControlSpeed = 0;
		private bool cruiseControlModuleEnabled = false;
		private GameObject switchMinus;
		private GameObject switchPlus;
		private GameObject switchSet;
		private GameObject switchReset;


		// Use this for initialization
		void Start()
		{
		}

		public void Init(CruiseControlPanel cruiseControlPanel)
		{
			this.cruiseControlPanel = cruiseControlPanel;

			switchMinus = cruiseControlPanel.transform.FindChild("ECU-Mod_CruiseControlPanel_Switch_Minus").gameObject;
			switchPlus = cruiseControlPanel.transform.FindChild("ECU-Mod_CruiseControlPanel_Switch_Plus").gameObject;
			switchSet = cruiseControlPanel.transform.FindChild("ECU-Mod_CruiseControlPanel_Switch_Set").gameObject;
			switchReset = cruiseControlPanel.transform.FindChild("ECU-Mod_CruiseControlPanel_Switch_Reset").gameObject;
			cruiseControlText = cruiseControlPanel.transform
				.FindChild("ECU-Mod_CruiseControlPanel_Set_Speed_Text")
				.GetComponent<TextMesh>();
		}
		void Update()
		{
			if (!CarH.hasPower || !cruiseControlPanel.conditionsFulfilled || !CarH.playerInCar)
			{
				setCruiseControlSpeed = 0;
				cruiseControlModuleEnabled = false;
				SetCruiseControlSpeedText("");
				return;
			}

			HandleButtonPresses();

			SetCruiseControlSpeedText(cruiseControlPanel.set.ToString());

			if (cruiseControlPanel.enabled
				&& (
					cruiseControlPanel.currentCarSpeed < CruiseControlPanel.MIN_SET
					|| CarH.carController.brakeInput > 0f
					|| CarH.carController.clutchInput > 0f
					|| CarH.carController.handbrakeInput > 0f
					|| CarH.drivetrain.gear <= 0
					)
				)
			{
				cruiseControlPanel.Reset();
				return;
			}

			if (!cruiseControlPanel.enabled)
			{
				return;
			}

			float cruiseControlThrottle = Map(
				(float)cruiseControlPanel.currentCarSpeed, 
				0,
				(float)cruiseControlPanel.set, 
				1f, 
				0.35f
				);
			CarH.drivetrain.idlethrottle = cruiseControlThrottle;

			ModConsole.Print(cruiseControlThrottle);
			return;
		}
		private float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
		{
			return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
		}

		private void HandleButtonPresses()
		{
			Action actionToPerform = null;
			string guiText = null;

			if (switchMinus.IsLookingAt())
			{
				actionToPerform = cruiseControlPanel.Decrease;
				guiText = "decrease cruise speed";
			}

			if (switchPlus.IsLookingAt())
			{
				actionToPerform = cruiseControlPanel.Increase;
				guiText = "increase cruise speed";
			}

			if (switchReset.IsLookingAt())
			{
				actionToPerform = cruiseControlPanel.Reset;
				guiText = "reset/disable cruise control";
			}

			if (switchSet.IsLookingAt())
			{
				actionToPerform = cruiseControlPanel.Set;
				guiText = "set/enable cruise control";
			}

			if (actionToPerform != null && guiText != null)
			{
				UserInteraction.GuiInteraction(guiText);
				if (UserInteraction.UseButtonDown)
				{
					actionToPerform.Invoke();
					cruiseControlPanel.gameObject.PlayTouch();
				}
			}
		}

		private void SetCruiseControlSpeedText(string toSet)
		{
			cruiseControlText.text = toSet;
		}

		public Color textColor
		{
			get => cruiseControlText.color;
			set => cruiseControlText.color = value;
		}
	}
}