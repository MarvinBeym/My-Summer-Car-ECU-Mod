using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class EspModule : Module.Module
	{
		protected override string partId => "esp_module";
		protected override string partName => "ESP Module";
		protected override Vector3 partInstallPosition => new Vector3(0.0235f, 0.023f, -0.0245f);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public EspModule(MountingPlate parent) : base(parent)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(0.09f, 0.0120f, -0.052f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.09f, 0.0120f, 0.0528f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.092f, 0.0120f, 0.0528f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.092f, 0.0120f, -0.052f), new Vector3(-90, 0, 0))
			}, 0.8f, 8);
		}


		public override bool enabled
		{
			get => CarH.carController.ESP;
			set => CarH.carController.ESP = value;
		}
	}
}