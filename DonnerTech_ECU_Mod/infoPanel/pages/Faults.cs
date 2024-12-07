using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonnerTech_ECU_Mod.part;
using MscModApi.Caching;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
	class Faults : InfoPanelPage
	{
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

		public Faults(string pageName, InfoPanel infoPanel, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanel, infoPanelBaseInfo)
		{
			PlayMakerFSM mechanicalWear = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/MechanicalWear")
				.GetComponent<PlayMakerFSM>();

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

			PlayMakerFSM headlightBulbLeftFSM = Cache.Find("HeadlightBulbLeft").GetComponent<PlayMakerFSM>();
			PlayMakerFSM headlightBulbRightFSM = Cache.Find("HeadlightBulbRight").GetComponent<PlayMakerFSM>();
			wearHeadlightBulbLeft = headlightBulbLeftFSM.FsmVariables.FindFsmFloat("Wear");
			wearHeadlightBulbRight = headlightBulbRightFSM.FsmVariables.FindFsmFloat("Wear");
		}

		public override string[] guiTexts => new string[0];

		public override void DisplayValues()
		{
			infoPanel
				.SetDisplayValue(InfoPanel.VALUE_1, ConvertFloatToWear(wearAlternator.Value))
				.SetDisplayValue(InfoPanel.VALUE_2, ConvertFloatToWear(wearClutch.Value))
				.SetDisplayValue(InfoPanel.VALUE_3, ConvertFloatToWear(wearCrankshaft.Value))
				.SetDisplayValue(InfoPanel.VALUE_4, ConvertFloatToWear(wearFuelpump.Value))
				.SetDisplayValue(InfoPanel.VALUE_5, ConvertFloatToWear(wearGearbox.Value))
				.SetDisplayValue(InfoPanel.VALUE_6, ConvertFloatToWear(wearHeadgasket.Value))
				.SetDisplayValue(InfoPanel.VALUE_7, ConvertFloatToWear(wearRockershaft.Value))
				.SetDisplayValue(InfoPanel.VALUE_8, ConvertFloatToWear(wearStarter.Value))
				.SetDisplayValue(InfoPanel.VALUE_9, ConvertFloatToWear(wearPiston1.Value))
				.SetDisplayValue(InfoPanel.VALUE_10, ConvertFloatToWear(wearPiston2.Value))
				.SetDisplayValue(InfoPanel.VALUE_11, ConvertFloatToWear(wearPiston3.Value))
				.SetDisplayValue(InfoPanel.VALUE_12, ConvertFloatToWear(wearPiston4.Value))
				.SetDisplayValue(InfoPanel.VALUE_13, ConvertFloatToWear(wearWaterpump.Value))
				.SetDisplayValue(InfoPanel.VALUE_14, ConvertFloatToWear(wearHeadlightBulbLeft.Value))
				.SetDisplayValue(InfoPanel.VALUE_15, ConvertFloatToWear(wearHeadlightBulbRight.Value));
		}

		public override void Handle()
		{
			DisplayValues();
		}

		public override void Pressed_Display_Value(string value)
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