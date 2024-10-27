using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class ElectricFuelPump : DerivablePart
	{
		protected override string partId => "electric_fuel_pump";
		protected override string partName => "Electric Fuel Pump";
		protected override Vector3 partInstallPosition => new Vector3(-0.0822f, 0.125f, 0.9965f);
		protected override Vector3 partInstallRotation => new Vector3(0, 180, 0);

		public ElectricFuelPump(SatsumaGamePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(0f, 0.04f, -0.0030f), new Vector3(0, 180, 0)),
				new Screw(new Vector3(0f, -0.04f, -0.0030f), new Vector3(0, 180, 0))
			}, 0.6f, 8);

			transform.FindChild("fuelLine-1").GetComponent<Renderer>().enabled = true;
			transform.FindChild("fuelLine-2").GetComponent<Renderer>().enabled = true;
		}
	}
}