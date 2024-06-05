using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class FuelRail : DerivablePart
	{
		protected override string partId => "fuel_rail";
		protected override string partName => "Fuel Rail";
		protected override Vector3 partInstallPosition => new Vector3(0, 0.03f, 0.012f);
		protected override Vector3 partInstallRotation => new Vector3(30, 0, 0);

		public FuelRail(FuelInjectionManifold parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
		}
	}
}