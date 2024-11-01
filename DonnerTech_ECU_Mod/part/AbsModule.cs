using MscModApi.Caching;
using MscModApi.Parts;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class AbsModule : Module.ModulePart
	{
		protected override string partId => "abs_module";
		protected override string partName => "ABS Module";
		protected override Vector3 partInstallPosition => new Vector3(0.058f, 0.022f, 0.116f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);


		public AbsModule(MountingPlate parent) : base(parent)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(0.0558f, 0.0115f, -0.0525f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.0558f, 0.0115f, 0.0525f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.0558f, 0.0115f, 0.0525f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.0558f, 0.0115f, -0.0525f), new Vector3(-90, 0, 0))
			}, 0.8f, 8);
		}

		public override bool enabled
		{
			get => CarH.carController.ABS;
			set
			{
				if (!installedOnCar || !bolted)
				{
					value = false;
				}
				CarH.carController.ABS = value;
			}
		}
	}
}