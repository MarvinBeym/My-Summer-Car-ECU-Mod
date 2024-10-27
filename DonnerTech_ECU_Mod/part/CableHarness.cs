using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class CableHarness : DerivablePart
	{
		protected override string partId => "cable_harness";
		protected override string partName => "ECU Cable Harness";
		protected override Vector3 partInstallPosition => new Vector3(-0.117f, 0.0102f, -0.024f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public CableHarness(MountingPlate parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
		}
	}
}