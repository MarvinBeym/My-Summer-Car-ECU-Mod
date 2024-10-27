using System;
using DonnerTech_ECU_Mod.fuelsystem;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class CruiseControlPanel : DerivablePart
	{
		public const int INCREMENT = 5;
		public const int DECREMENT = 5;
		public const int MIN_SET = 30;

		protected override string partId => "cruise_control_panel";
		protected override string partName => "Cruise Control Panel";
		protected override Vector3 partInstallPosition => new Vector3(0.5f, -0.095f, 0.08f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		public bool enabled = false;
		public int set = 30;

		private SmartEngineModule smartEngineModule;
		private CableHarness cableHarness;
		private MountingPlate mountingPlate;

		public CruiseControl_Logic cruise_control_logic;

		public CruiseControlPanel(GamePart parent, SmartEngineModule smartEngineModule, CableHarness cableHarness, MountingPlate mountingPlate, FuelSystem fuelSystem) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			//FuelSystem not used in checks until known to work fully with new ReplacementGamePart logic.

			this.smartEngineModule = smartEngineModule;
			this.cableHarness = cableHarness;
			this.mountingPlate = mountingPlate;

			AddScrews(new[]
			{
				new Screw(new Vector3(0f, 0.012f, -0.025f), new Vector3(-90, 0, 0)),
			}, 0.8f, 10);

			cruise_control_logic = AddEventBehaviour<CruiseControl_Logic>(PartEvent.Type.Install);
			cruise_control_logic.Init(this);
		}

		public void Increase()
		{
			set += INCREMENT;
		}

		public void Decrease()
		{
			set -= DECREMENT;
			if (set <= MIN_SET)
			{
				set = MIN_SET;
			}
		}

		public void Reset()
		{
			enabled = !enabled;

			cruise_control_logic.textColor = enabled ? Color.white : Color.green;
		}

		public void Set()
		{
			set = 5 * (int)Math.Round(currentCarSpeed / 5.0);
			if (set <= MIN_SET)
			{
				set = MIN_SET;
			}
			enabled = true;
			cruise_control_logic.textColor = Color.green;
		}

		public int currentCarSpeed => (int) CarH.drivetrain.differentialSpeed;

		public bool conditionsFulfilled => smartEngineModule.bolted && cableHarness.bolted && mountingPlate.bolted;
	}
}