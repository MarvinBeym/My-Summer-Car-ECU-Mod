using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part.Module
{
	public abstract class Module : DerivablePart
	{
		public abstract bool enabled { get; set; }


		protected Module(DerivablePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
		}

		protected Module(GamePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
		}

		public void Toggle()
		{
			enabled = !enabled;
		}
	}
}