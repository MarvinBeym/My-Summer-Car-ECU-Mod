using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class MountingPlate : DerivablePart
	{
		protected override string partId => "mounting_plate";
		protected override string partName => "ECU Mounting Plate";
		protected override Vector3 partInstallPosition => new Vector3(0.3115f, -0.276f, -0.0393f);
		protected override Vector3 partInstallRotation => new Vector3(0, 180, 0);

		public MountingPlate(GamePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(-0.1240f, 0.018f, 0.0040f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.1240f, 0.018f, 0.2070f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.0020f, 0.018f, 0.2070f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(0.1280f, 0.018f, 0.2070f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.1240f, 0.018f, -0.2000f), new Vector3(-90, 0, 0))
			}, 1.2f);
		}
	}
}