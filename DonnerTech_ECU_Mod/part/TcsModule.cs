using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class TcsModule : Module.ModulePart
	{
		protected override string partId => "tcs_module";
		protected override string partName => "TCS Module";
		protected override Vector3 partInstallPosition => new Vector3(-0.03f, 0.0235f, -0.154f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public TcsModule(MountingPlate parent) : base(parent)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(0.0388f, 0.0150f, -0.0418f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.0388f, 0.0150f, 0.0422f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.0382f, 0.0150f, 0.0422f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.0382f, 0.0150f, -0.0418f), new Vector3(-90, 0, 0))
			}, 0.8f, 8);
		}

		public override bool enabled
		{
			get => CarH.carController.TCS;
			set => CarH.carController.TCS = value;
		}
	}
}