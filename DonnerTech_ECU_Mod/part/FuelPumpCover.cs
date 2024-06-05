using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class FuelPumpCover : DerivablePart
	{
		protected override string partId => "fuel_pump_cover";
		protected override string partName => "Fuel Pump Cover";
		protected override Vector3 partInstallPosition => new Vector3(-0.0515f, 0.105f, 0.006f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		public FuelPumpCover(GamePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(-0.02f, 0.003f, -0.0230f), new Vector3(0, 180, 0), Screw.Type.Nut),
				new Screw(new Vector3(0.018f, 0.003f, -0.0230f), new Vector3(0, 180, 0), Screw.Type.Nut)
			}, 0.6f, 7);
		}
	}
}