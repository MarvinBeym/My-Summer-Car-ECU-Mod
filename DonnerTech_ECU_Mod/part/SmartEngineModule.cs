using MscModApi.Parts;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class SmartEngineModule : DerivablePart
	{
		protected override string partId => "smart_engine_module";
		protected override string partName => "Smart Engine ECU";
		protected override Vector3 partInstallPosition => new Vector3(0.072f, 0.024f, -0.1425f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public SmartEngineModule_Logic logic;

		public SmartEngineModule(
			MountingPlate parent,
			AbsModule absModule,
			EspModule espModule,
			TcsModule tcsModule
		) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(-0.028f, 0.01f, 0.039f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.049f, 0.01f, 0.039f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.049f, 0.01f, -0.0625f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.028f, 0.01f, -0.0625f), new Vector3(-90, 0, 0))
			}, 0.8f, 8);

			logic = AddEventBehaviour<SmartEngineModule_Logic>(PartEvent.Type.Install);
			logic.Init(this, absModule, espModule, tcsModule);
		}
	}
}