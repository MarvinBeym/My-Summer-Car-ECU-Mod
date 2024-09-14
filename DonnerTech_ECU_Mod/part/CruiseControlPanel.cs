using DonnerTech_ECU_Mod.fuelsystem;
using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class CruiseControlPanel : DerivablePart
	{
		protected override string partId => "cruise_control_panel";
		protected override string partName => "Cruise Control Panel";
		protected override Vector3 partInstallPosition => new Vector3(0.5f, -0.095f, 0.08f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

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

		public bool functional => smartEngineModule.bolted && cableHarness.bolted && mountingPlate.bolted;

	}
}