using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class ChipProgrammer : DerivablePart
	{
		protected override string partId => "chip_programmer";
		protected override string partName => "Chip Programmer";
		protected override Vector3 partInstallPosition => new Vector3(0, 0, 0);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		public ChipProgrammer() : base(NullGamePart.GetInstance(), DonnerTech_ECU_Mod.partBaseInfo)
		{
		}
	}
}