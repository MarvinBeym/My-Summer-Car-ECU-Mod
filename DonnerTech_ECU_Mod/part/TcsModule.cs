using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class TcsModule : DerivablePart
	{
		protected override string partId => "tcs_module";
		protected override string partName => "TCS Module";
		protected override Vector3 partInstallPosition => new Vector3(-0.03f, 0.0235f, -0.154f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public TcsModule(MountingPlate parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(0.0388f, 0.0150f, -0.0418f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.0388f, 0.0150f, 0.0422f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.0382f, 0.0150f, 0.0422f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.0382f, 0.0150f, -0.0418f), new Vector3(-90, 0, 0))
			}, 0.8f, 8);
		}
	}
}