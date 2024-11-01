using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part.Module
{
	public abstract class ModulePart : DerivablePart
	{
		public abstract bool enabled { get; set; }


		protected ModulePart(DerivablePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
		}

		protected ModulePart(GamePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
		}

		public void Toggle()
		{
			enabled = !enabled;
		}
	}
}