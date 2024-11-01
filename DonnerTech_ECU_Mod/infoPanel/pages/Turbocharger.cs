using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonnerTech_ECU_Mod.part;
using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
	class Turbocharger : InfoPanelPage
	{
		private GameObject turbine;
		private readonly bool modInstalled;

		private GameObject gtTurboGameObject;
		private GameObject racingTurboGameObject;
		private GameObject installedTurbo;
		private FsmFloat setBoost;
		private FsmInt exhaustTemp;
		private FsmInt intakeTemp;
		private FsmInt rpm;
		private FsmFloat boost;

		public Turbocharger(string pageName, InfoPanel infoPanel, GameObject turbine, InfoPanelBaseInfo infoPanelBaseInfo) : base(pageName, infoPanel, infoPanelBaseInfo)
		{
			this.turbine = turbine;
			turbineUsed = true;

			modInstalled = ModLoader.IsModPresent("SatsumaTurboCharger");

			if (modInstalled)
			{

			}
		}

		public override string[] guiTexts => new string[0];

		public override void DisplayValues()
		{
			if (installedTurbo == null)
			{
				display_values["value_1"].text = "---";
				display_values["value_2"].text = "---";
				display_values["value_14"].text = "---";
				display_values["value_15"].text = "---";
				display_values["value_16"].text = "---";
				return;
			}

			turbine.transform.Rotate(0f, 0f, 40 * Time.deltaTime);

			display_values["value_1"].text = boost.Value.ToString("0.00");
			display_values["value_2"].text = setBoost.Value.ToString("0.00");
			display_values["value_14"].text = exhaustTemp.Value > 0f ? exhaustTemp.Value.ToString("000") : "---"; //ToDo: requires implementation on turbo mod side
			display_values["value_15"].text = intakeTemp.Value > 0f ? intakeTemp.Value.ToString("000") : "---"; //ToDo: requires implementation on turbo mod side
			display_values["value_16"].text = rpm.Value.ToString("0");
		}

		public override void Handle()
		{
			if (modInstalled)
			{
				if (gtTurboGameObject == null)
				{
					gtTurboGameObject = Cache.Find("GT Turbo(Clone)");
				}

				if (racingTurboGameObject == null)
				{
					racingTurboGameObject = Cache.Find("Racing Turbo(Clone)");
				}

				bool installedTurboChanged = false;
				if (gtTurboGameObject != null && gtTurboGameObject.transform.root == CarH.satsuma.transform && installedTurbo != gtTurboGameObject)
				{
					installedTurbo = gtTurboGameObject;
					installedTurboChanged = true;
				}

				if (racingTurboGameObject != null && racingTurboGameObject.transform.root == CarH.satsuma.transform && installedTurbo != gtTurboGameObject)
				{
					installedTurbo = racingTurboGameObject;
					installedTurboChanged = true;
				}

				if (installedTurboChanged)
				{
					PlayMakerFSM fsmPartData = installedTurbo.GetPlayMaker(FsmPartData.FsmName);
					setBoost = fsmPartData.GetVariable<FsmFloat>("setBoost");
					exhaustTemp = fsmPartData.GetVariable<FsmInt>("exhaustTemp");
					intakeTemp = fsmPartData.GetVariable<FsmInt>("intakeTemp");
					rpm = fsmPartData.GetVariable<FsmInt>("rpm");
					boost = fsmPartData.GetVariable<FsmFloat>("boost");
				}

				if (
					gtTurboGameObject != null && gtTurboGameObject.transform.root != CarH.satsuma.transform
				    && racingTurboGameObject != null && racingTurboGameObject.transform.root != CarH.satsuma.transform
				   )
				{
					installedTurbo = null;
				}

				DisplayValues();
			}
		}

		public override void Pressed_Display_Value(string value)
		{
		}
	}
}